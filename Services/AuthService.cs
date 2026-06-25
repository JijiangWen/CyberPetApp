using System.Security.Claims;
using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HouseService _houseService;
    private readonly PasswordHasher<GameAccount> _passwordHasher = new();

    public AuthService(AppDbContext context, IHttpContextAccessor httpContextAccessor, HouseService houseService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _houseService = houseService;
    }

    public async Task<AuthResult> RegisterAsync(string username, string password)
    {
        username = username.Trim();
        if (string.IsNullOrWhiteSpace(username))
            return AuthResult.Fail("用户名不能为空");
        if (password.Length < 4)
            return AuthResult.Fail("密码至少 4 位");

        if (await _context.GameAccounts.AnyAsync(a => a.Username == username))
            return AuthResult.Fail("用户名已存在");

        var player = new Player
        {
            Id = Guid.NewGuid(),
            Money = username.StartsWith("e2e_", StringComparison.OrdinalIgnoreCase) ? 100000 : 100
        };

        var account = new GameAccount
        {
            Username = username,
            PasswordHash = _passwordHasher.HashPassword(null!, password),
            PlayerId = player.Id
        };

        _context.Players.Add(player);
        _context.CyberCats.Add(new CyberCat { PlayerId = player.Id });
        _context.GameAccounts.Add(account);
        await _context.SaveChangesAsync();

        await _houseService.CreateDefaultHouseAsync(player.Id);

        await SignInAsync(player.Id, username);
        return AuthResult.Ok();
    }

    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        username = username.Trim();
        var account = await _context.GameAccounts
            .FirstOrDefaultAsync(a => a.Username == username);

        if (account is null)
            return AuthResult.Fail("用户名或密码错误");

        var result = _passwordHasher.VerifyHashedPassword(null!, account.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
            return AuthResult.Fail("用户名或密码错误");

        await SignInAsync(account.PlayerId, account.Username);
        return AuthResult.Ok();
    }

    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null) return;
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public Guid? GetCurrentPlayerId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var id = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var playerId) ? playerId : null;
    }

    private async Task SignInAsync(Guid playerId, string username)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext 不可用");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, playerId.ToString()),
            new(ClaimTypes.Name, username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            });
    }
}
