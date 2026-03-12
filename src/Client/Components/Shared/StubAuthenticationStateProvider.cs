using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Client.Components.Shared;

public sealed class StubAuthenticationStateProvider : AuthenticationStateProvider
{
    private string _username = "devuser";
    private string[] _roles = Array.Empty<string>();

    public void SetUser(string username, params string[] roles)
    {
        _username = username;
        _roles = roles ?? Array.Empty<string>();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, _username),
            new Claim(ClaimTypes.NameIdentifier, _username),
        }.Concat(_roles.Select(r => new Claim(ClaimTypes.Role, r))), "Stub");
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(principal));
    }
}
