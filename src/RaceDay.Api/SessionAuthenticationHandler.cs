using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace RaceDay.Api;

/// <summary>Authenticates browser/API requests that carry the RaceDay server session cookie.</summary>
public sealed class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "RaceDaySession";
    public const string UserIdKey = "RaceDay.UserId";
    public const string RoleKey = "RaceDay.Role";
    public const string UsernameKey = "RaceDay.Username";

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Context.Session.GetString(UserIdKey);
        var role = Context.Session.GetString(RoleKey);
        var username = Context.Session.GetString(UsernameKey);
        if (!int.TryParse(userId, out _) || string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(username))
            return Task.FromResult(AuthenticateResult.NoResult());

        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
