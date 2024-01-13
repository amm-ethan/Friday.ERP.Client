using System.Net;
using System.Net.Http.Headers;
using Friday.ERP.Shared;
using MudBlazor;
using Newtonsoft.Json;
using Toolbelt.Blazor;

namespace Friday.ERP.Client.Utilities;

public class HttpInterceptorService(HttpClientInterceptor interceptor, TokenService tokenService, ISnackbar snackbar)
{
    public void RegisterEvent()
    {
        interceptor.BeforeSendAsync += InterceptBeforeHttpAsync;
        interceptor.AfterSendAsync += InterceptResponse;
    }

    private async Task InterceptBeforeHttpAsync(object sender, HttpClientInterceptorEventArgs args)
    {
        var absPath = args.Request.RequestUri!.AbsoluteUri;
        if (!absPath.Contains("login") && !absPath.Contains("refresh"))
        {
            var token = await tokenService.GetAccessToken();
            if (!string.IsNullOrEmpty(token))
                args.Request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            else
                args.Cancel = true;
        }
    }

    private async Task InterceptResponse(object sender, HttpClientInterceptorEventArgs args)
    {
        var absPath = args.Request.RequestUri!.AbsoluteUri;
        if (!absPath.Contains("refresh"))
            if (args.Response?.StatusCode is not null && !args.Response.IsSuccessStatusCode)
            {
                var statusCode = args.Response?.StatusCode;
                if (statusCode == HttpStatusCode.InternalServerError)
                {
                    snackbar.Add("Unhandled Error. Contact Your System Admin.", Severity.Error);
                    return;
                }

                var content = await args.Response?.Content.ReadAsStringAsync()!;
                var responseObject = JsonConvert.DeserializeObject<ErrorResponseDto<string>>(content);

                switch (statusCode)
                {
                    case HttpStatusCode.UnprocessableEntity:
                    {
                        snackbar.Add(responseObject!.ErrorType, Severity.Warning);
                        foreach (var error in responseObject.ErrorDetail!)
                            snackbar.Add(error, Severity.Warning);
                        break;
                    }
                    case HttpStatusCode.BadRequest:
                    {
                        snackbar.Add(responseObject!.ErrorType, Severity.Warning);
                        foreach (var error in responseObject.ErrorDetail!)
                            snackbar.Add(error, Severity.Warning);
                        break;
                    }
                    case HttpStatusCode.NotFound:
                    {
                        snackbar.Add(responseObject!.ErrorType, Severity.Warning);
                        foreach (var error in responseObject.ErrorDetail!)
                            snackbar.Add(error, Severity.Info);
                        break;
                    }
                    case HttpStatusCode.Unauthorized:
                        snackbar.Add("Unauthorized.", Severity.Info);
                        break;
                    default:
                        snackbar.Add("Unhandled Error. Contact Your System Admin.", Severity.Error);
                        break;
                }
            }
    }

    public void DisposeEvent()
    {
        interceptor.BeforeSendAsync -= InterceptBeforeHttpAsync;
        interceptor.AfterSendAsync -= InterceptResponse;
    }
}