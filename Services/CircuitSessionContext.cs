namespace CyberPetApp.Services;

/// <summary>当前 Blazor Circuit 标识，由 <see cref="FishingCircuitHandler"/> 在连接打开时写入。</summary>
public class CircuitSessionContext
{
    public string? CircuitId { get; set; }
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = "";
}
