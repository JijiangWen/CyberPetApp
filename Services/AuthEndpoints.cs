using Microsoft.AspNetCore.Mvc;

namespace CyberPetApp.Services;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/register", async ([FromForm] string username, [FromForm] string password, [FromForm] string? confirmPassword, AuthService auth) =>
        {
            if (password != confirmPassword)
                return Results.Redirect("/register?error=" + Uri.EscapeDataString("两次密码不一致"));

            var result = await auth.RegisterAsync(username, password);
            return result.Success
                ? Results.Redirect("/")
                : Results.Redirect("/register?error=" + Uri.EscapeDataString(result.Error!));
        }).DisableAntiforgery();

        app.MapPost("/api/auth/login", async ([FromForm] string username, [FromForm] string password, AuthService auth) =>
        {
            var result = await auth.LoginAsync(username, password);
            return result.Success
                ? Results.Redirect("/")
                : Results.Redirect("/login?error=" + Uri.EscapeDataString(result.Error!));
        }).DisableAntiforgery();

        app.MapPost("/api/auth/logout", async (AuthService auth) =>
        {
            await auth.LogoutAsync();
            return Results.Redirect("/login");
        }).DisableAntiforgery();
    }
}
