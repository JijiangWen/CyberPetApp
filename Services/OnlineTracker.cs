using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CyberPetApp.Services;

public class OnlineTracker
{
    private readonly ConcurrentDictionary<string, (Guid PlayerId, string Username)> _onlineCircuits = new();

    public event Action? OnOnlineListChanged;

    public void RegisterOnline(string circuitId, Guid playerId, string username)
    {
        if (string.IsNullOrEmpty(circuitId)) return;
        _onlineCircuits[circuitId] = (playerId, username);
        OnOnlineListChanged?.Invoke();
    }

    public void UnregisterOnline(string circuitId)
    {
        if (string.IsNullOrEmpty(circuitId)) return;
        _onlineCircuits.TryRemove(circuitId, out _);
        OnOnlineListChanged?.Invoke();
    }

    public Dictionary<Guid, string> GetOnlinePlayers()
    {
        var dict = new Dictionary<Guid, string>();
        foreach (var val in _onlineCircuits.Values)
        {
            dict[val.PlayerId] = val.Username;
        }
        return dict;
    }
}
