using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

public class DevelopmentAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DevelopmentAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "devuser"), new Claim(ClaimTypes.Role, "Administrator") };
        var identity = new ClaimsIdentity(claims, "Development");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Development");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
