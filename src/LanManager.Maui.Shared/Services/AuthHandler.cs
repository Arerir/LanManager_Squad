namespace LanManager.Maui.Shared.Services;

public class AuthHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuthHandler error: {ex.Message}");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

