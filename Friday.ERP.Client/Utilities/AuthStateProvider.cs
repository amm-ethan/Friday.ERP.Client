using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Friday.ERP.Client.Utilities;

public class AuthStateProvider(ILocalStorageService localStorage) : AuthenticationStateProvider
{
    private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await localStorage.GetItemAsync<string>("accessToken");

        if (string.IsNullOrWhiteSpace(accessToken))
            return _anonymous;

        var claims = new List<Claim>();
        claims.AddRange(JwtParser.ParseClaimsFromJwt(accessToken));

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));
        return new AuthenticationState(authenticatedUser);
    }

    public void NotifyUserAuthentication(string accessToken)
    {
        var claims = new List<Claim>();
        claims.AddRange(JwtParser.ParseClaimsFromJwt(accessToken));

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(_anonymous);
        NotifyAuthenticationStateChanged(authState);
    }
}