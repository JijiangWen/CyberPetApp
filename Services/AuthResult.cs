namespace CyberPetApp.Services;

public class AuthResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static AuthResult Ok() => new() { Success = true };
    public static AuthResult Fail(string error) => new() { Success = false, Error = error };
}
