using System.Collections.Concurrent;

namespace CyberPetApp.Services;

/// <summary>
/// 轻量多标签页防护：同一 PlayerId 只允许一个 circuit 挂机钓鱼。
/// </summary>
public static class FishingSessionRegistry
{
    private static readonly ConcurrentDictionary<Guid, string> ActiveSessions = new();

    /// <summary>尝试开始钓鱼，返回是否成功占用。</summary>
    /// <param name="playerId">玩家 ID</param>
    /// <param name="circuitId">会话 ID（通常是窗口 URL）</param>
    /// <returns>true=成功占用；false=其他窗口已在钓鱼</returns>
    public static bool TryStart(Guid playerId, string circuitId) =>
        ActiveSessions.TryAdd(playerId, circuitId);

    public static void Stop(Guid playerId, string circuitId)
    {
        if (ActiveSessions.TryGetValue(playerId, out var existing) && existing == circuitId)
            ActiveSessions.TryRemove(playerId, out _);
    }

    /// <summary>断开 Blazor Circuit 时移除该会话占用的全部钓鱼锁。</summary>
    public static void StopByCircuit(string circuitId)
    {
        if (string.IsNullOrEmpty(circuitId)) return;
        foreach (var kv in ActiveSessions.ToArray())
        {
            if (kv.Value == circuitId)
                ActiveSessions.TryRemove(kv.Key, out _);
        }
    }
}
