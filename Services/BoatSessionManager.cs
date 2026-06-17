using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CyberPetApp.Models;

namespace CyberPetApp.Services;

public enum BoatSessionStatus
{
    Lobby,      // 正在大厅招募/等待船员
    Fishing     // 正在出海钓鱼中
}

/// <summary>
/// 内存中的船只联机房间
/// </summary>
public class BoatSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
    public string BoatType { get; set; } = "wood_boat";
    public string BoatName { get; set; } = "";
    public int MaxCapacity { get; set; } = 4;
    public BoatSessionStatus Status { get; set; } = BoatSessionStatus.Lobby;

    /// <summary>同船玩家列表：Key = PlayerId, Value = PlayerUsername</summary>
    public ConcurrentDictionary<Guid, string> Members { get; set; } = new();

    /// <summary>船上玩家的最新钓鱼动态事件日志（实时广播）</summary>
    public List<string> ActionLogs { get; set; } = new();
}

/// <summary>
/// 邀请函
/// </summary>
public class BoatInvitation
{
    public Guid InvitationId { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public Guid HostId { get; set; }
    public string HostName { get; set; } = "";
    public Guid InviteeId { get; set; }
    public string InviteeName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsExpired => (DateTime.UtcNow - CreatedAt).TotalSeconds > 60; // 60秒过期
}

public class BoatSessionManager
{
    // 所有活跃船只房间 (SessionId -> Session)
    private readonly ConcurrentDictionary<Guid, BoatSession> _activeSessions = new();
    
    // 当前在线玩家与其关联的房间 (PlayerId -> SessionId)
    private readonly ConcurrentDictionary<Guid, Guid> _playerSessionMap = new();

    // 活跃的组队邀请列表 (InvitationId -> Invitation)
    private readonly ConcurrentDictionary<Guid, BoatInvitation> _pendingInvitations = new();

    // 实时更新事件：当房间状态改变时，通知相关客户端刷新 UI
    public event Action<Guid>? OnSessionUpdated;
    
    // 实时通知事件：当有新的邀请发送过来时，通知受邀者
    public event Action<Guid, BoatInvitation>? OnInvitationReceived;

    /// <summary>
    /// 获取玩家当前加入的房间
    /// </summary>
    public BoatSession? GetSessionForPlayer(Guid playerId)
    {
        if (_playerSessionMap.TryGetValue(playerId, out var sessionId))
        {
            return _activeSessions.TryGetValue(sessionId, out var session) ? session : null;
        }
        return null;
    }

    /// <summary>
    /// 获取当前所有在线且未加入其他船只的玩家（用于邀请列表）
    /// </summary>
    /// <param name="excludePlayerId">排除自己</param>
    /// <param name="currentOnlinePlayers">在线玩家列表</param>
    public List<(Guid Id, string Name)> GetAvailablePlayers(Guid excludePlayerId, Dictionary<Guid, string> currentOnlinePlayers)
    {
        return currentOnlinePlayers
            .Where(kv => kv.Key != excludePlayerId && !_playerSessionMap.ContainsKey(kv.Key))
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    /// <summary>
    /// 船长创建房间
    /// </summary>
    public BoatSession CreateSession(Guid ownerId, string ownerName, PlayerBoat boat)
    {
        // 离开之前的房间
        LeaveSession(ownerId);

        var session = new BoatSession
        {
            OwnerId = ownerId,
            OwnerName = ownerName,
            BoatType = boat.BoatType,
            BoatName = boat.CustomName,
            MaxCapacity = boat.MaxCapacity,
            Status = BoatSessionStatus.Lobby
        };
        session.Members.TryAdd(ownerId, ownerName);
        
        _activeSessions[session.SessionId] = session;
        _playerSessionMap[ownerId] = session.SessionId;

        session.ActionLogs.Add($"[系统] 船长 {ownerName} 启航创建了 {session.BoatName}！");
        NotifyUpdate(session.SessionId);
        return session;
    }

    /// <summary>
    /// 船长向目标玩家发送组队邀请
    /// </summary>
    public bool SendInvitation(Guid sessionId, Guid hostId, string hostName, Guid inviteeId, string inviteeName)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session)) return false;
        if (session.Members.Count >= session.MaxCapacity) return false;

        // 清理同一人的历史过期邀请
        var expiredKeys = _pendingInvitations.Where(kv => kv.Value.InviteeId == inviteeId && kv.Value.IsExpired).Select(kv => kv.Key);
        foreach (var key in expiredKeys) _pendingInvitations.TryRemove(key, out _);

        var invite = new BoatInvitation
        {
            SessionId = sessionId,
            HostId = hostId,
            HostName = hostName,
            InviteeId = inviteeId,
            InviteeName = inviteeName
        };

        _pendingInvitations[invite.InvitationId] = invite;
        
        // 实时通知受邀者
        OnInvitationReceived?.Invoke(inviteeId, invite);
        return true;
    }

    /// <summary>
    /// 获取玩家当前收到的所有未过期邀请
    /// </summary>
    public List<BoatInvitation> GetInvitationsForPlayer(Guid playerId)
    {
        return _pendingInvitations.Values
            .Where(i => i.InviteeId == playerId && !i.IsExpired)
            .ToList();
    }

    /// <summary>
    /// 玩家接受邀请加入房间
    /// </summary>
    public bool AcceptInvitation(Guid inviteId, Guid playerId, string playerName)
    {
        if (!_pendingInvitations.TryRemove(inviteId, out var invite) || invite.IsExpired) return false;

        if (!_activeSessions.TryGetValue(invite.SessionId, out var session)) return false;

        if (session.Members.Count >= session.MaxCapacity) return false;

        // 先退出可能存在的旧房间
        LeaveSession(playerId);

        session.Members.TryAdd(playerId, playerName);
        _playerSessionMap[playerId] = session.SessionId;

        session.ActionLogs.Add($"[系统] 玩家 {playerName} 踏上了小船！");
        NotifyUpdate(session.SessionId);
        return true;
    }

    /// <summary>
    /// 拒绝邀请
    /// </summary>
    public void DeclineInvitation(Guid inviteId)
    {
        _pendingInvitations.TryRemove(inviteId, out _);
    }

    /// <summary>
    /// 船长开始船钓
    /// </summary>
    public void StartFishing(Guid sessionId)
    {
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            session.Status = BoatSessionStatus.Fishing;
            session.ActionLogs.Add($"[系统] 汽笛长鸣，小船开赴深海，船钓正式开始！");
            NotifyUpdate(sessionId);
        }
    }

    /// <summary>
    /// 船长解散或结束船钓回到大厅
    /// </summary>
    public void StopFishing(Guid sessionId)
    {
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            session.Status = BoatSessionStatus.Lobby;
            session.ActionLogs.Add($"[系统] 船钓结束，小船驶回港湾。");
            NotifyUpdate(sessionId);
        }
    }

    /// <summary>
    /// 玩家主动离开或下线清理
    /// </summary>
    public void LeaveSession(Guid playerId)
    {
        if (!_playerSessionMap.TryRemove(playerId, out var sessionId)) return;

        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            session.Members.TryRemove(playerId, out var name);
            session.ActionLogs.Add($"[系统] 玩家 {name ?? "???"} 离开了小船。");

            // 如果船长走了，则解散房间，所有成员强制踢出
            if (session.OwnerId == playerId)
            {
                session.ActionLogs.Add($"[系统] 船长离开了房间，小船已解散。");
                foreach (var memberId in session.Members.Keys)
                {
                    _playerSessionMap.TryRemove(memberId, out _);
                }
                _activeSessions.TryRemove(sessionId, out _);
                NotifyUpdate(sessionId);
            }
            else
            {
                NotifyUpdate(sessionId);
            }
        }
    }

    /// <summary>
    /// 船员钓鱼成功上报，向房间内广播日志
    /// </summary>
    public void BroadcastCatch(Guid sessionId, string playerName, string fishName, FishRarity rarity, double weight)
    {
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            string rarityText = rarity switch
            {
                FishRarity.Rare => "【稀有】",
                FishRarity.Epic => "【史诗】",
                FishRarity.Legendary => "【传说🌟】",
                _ => ""
            };
            session.ActionLogs.Add($"[鱼获] 玩家 {playerName} 成功钓起 {rarityText}{fishName} ({weight:F2} kg)！");
            if (session.ActionLogs.Count > 50) session.ActionLogs.RemoveAt(0); // 限制日志大小
            NotifyUpdate(sessionId);
        }
    }

    private void NotifyUpdate(Guid sessionId)
    {
        OnSessionUpdated?.Invoke(sessionId);
    }
}
