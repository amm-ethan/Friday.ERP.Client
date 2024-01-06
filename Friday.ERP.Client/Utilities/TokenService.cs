using Blazored.LocalStorage;
using Friday.ERP.Client.Data;
using Microsoft.AspNetCore.Components.Authorization;

namespace Friday.ERP.Client.Utilities;

public class TokenService(
    AuthenticationStateProvider authProvider,
    IHttpRepository httpRepo,
    ILocalStorageService localStorage)
{
    public async Task<string?> GetAccessToken()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var exp = user.FindFirst(c => c.Type.Equals("exp"))!.Value;
        var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).LocalDateTime;
        var timeNow = DateTime.Now;
        if (expTime > timeNow) return await localStorage.GetItemAsync<string>("accessToken");
        var accessToken = await httpRepo.GetRefreshToken();
        return accessToken;
    }
}