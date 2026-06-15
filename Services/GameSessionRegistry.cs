using System;
using System.Collections.Concurrent;

namespace CyberPetApp.Services;

/// <summary>
/// 全站单会话防护：同一 PlayerId 仅允许一个活跃会话。
/// 当有新会话登录时，旧会话将被挤下线。
/// </summary>
public static class GameSessionRegistry
{
    private static readonly ConcurrentDictionary<Guid, string> ActiveSessions = new();

    /// <summary>
    /// 注册/抢占游戏会话。
    /// </summary>
    public static void Register(Guid playerId, string circuitId)
    {
        ActiveSessions[playerId] = circuitId;
    }

    /// <summary>
    /// 检查当前会话是否依然有效（未被抢占）。
    /// </summary>
    public static bool IsSessionValid(Guid playerId, string circuitId)
    {
        if (ActiveSessions.TryGetValue(playerId, out var registeredId))
        {
            return registeredId == circuitId;
        }
        // 如果没有注册，说明可以注册（这种情况通常是初次进入或意外清理，视作有效）
        return true;
    }

    /// <summary>
    /// 移除特定的会话。
    /// </summary>
    public static void Remove(Guid playerId, string circuitId)
    {
        if (ActiveSessions.TryGetValue(playerId, out var existing) && existing == circuitId)
            ActiveSessions.TryRemove(playerId, out _);
    }

    /// <summary>
    /// 断开 Blazor Circuit 时移除该会话占用的全部游戏锁。
    /// </summary>
    public static void RemoveByCircuit(string circuitId)
    {
        if (string.IsNullOrEmpty(circuitId)) return;
        foreach (var kv in ActiveSessions.ToArray())
        {
            if (kv.Value == circuitId)
                ActiveSessions.TryRemove(kv.Key, out _);
        }
    }
}
