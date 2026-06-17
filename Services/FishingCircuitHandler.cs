using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace CyberPetApp.Services;

/// <summary>P2-5：Circuit 断开时清理 <see cref="FishingSessionRegistry"/> 占用以及联机小船和在线追踪状态。</summary>
public sealed class FishingCircuitHandler : CircuitHandler
{
    private readonly CircuitSessionContext _context;
    private readonly OnlineTracker _tracker;
    private readonly BoatSessionManager _boatSessionManager;

    public FishingCircuitHandler(
        CircuitSessionContext context, 
        OnlineTracker tracker, 
        BoatSessionManager boatSessionManager)
    {
        _context = context;
        _tracker = tracker;
        _boatSessionManager = boatSessionManager;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _context.CircuitId = circuit.Id;
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_context.PlayerId != Guid.Empty)
        {
            // 下线时自动退出任何联机小船
            _boatSessionManager.LeaveSession(_context.PlayerId);
        }
        _tracker.UnregisterOnline(circuit.Id);
        FishingSessionRegistry.StopByCircuit(circuit.Id);
        GameSessionRegistry.RemoveByCircuit(circuit.Id);
        return Task.CompletedTask;
    }
}
