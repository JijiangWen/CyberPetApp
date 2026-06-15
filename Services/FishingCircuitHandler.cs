using Microsoft.AspNetCore.Components.Server.Circuits;

namespace CyberPetApp.Services;

/// <summary>P2-5：Circuit 断开时清理 <see cref="FishingSessionRegistry"/> 占用。</summary>
public sealed class FishingCircuitHandler : CircuitHandler
{
    private readonly CircuitSessionContext _context;

    public FishingCircuitHandler(CircuitSessionContext context) => _context = context;

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _context.CircuitId = circuit.Id;
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        FishingSessionRegistry.StopByCircuit(circuit.Id);
        GameSessionRegistry.RemoveByCircuit(circuit.Id);
        return Task.CompletedTask;
    }
}
