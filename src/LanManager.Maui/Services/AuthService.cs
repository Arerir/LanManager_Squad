using System.Text;
using System.Text.Json;

namespace LanManager.Maui.Services;

public record AuthUser(string Id, string Name, List<string> Roles);

public class AuthService
{
    private const string TokenKey = "auth_token";

    public AuthUser? CurrentUser { get; private set; }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            using var http = new HttpClient();
            http.BaseAddress = new Uri(Config.ApiBaseUrl);

            var payload = JsonSerializer.Serialize(new { email, password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await http.PostAsync("/api/auth/login", content);
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("token", out var tokenProp))
                return false;

            var token = tokenProp.GetString();
            if (string.IsNullOrEmpty(token))
                return false;

            await SecureStorage.SetAsync(TokenKey, token);
            CurrentUser = ParseJwtClaims(token);
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoginAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Remove(TokenKey);
        CurrentUser = null;
        await Task.CompletedTask;
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsLoggedInAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            var claims = ParseClaimsFromJwt(token);
            if (claims.TryGetValue("exp", out var expValue) &&
                long.TryParse(expValue, out var exp))
            {
                var expiry = DateTimeOffset.FromUnixTimeSeconds(exp);
                if (expiry <= DateTimeOffset.UtcNow)
                    return false;
            }

            if (CurrentUser == null)
                CurrentUser = ParseJwtClaims(token);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static AuthUser ParseJwtClaims(string token)
    {
        var claims = ParseClaimsFromJwt(token);

        var id = claims.TryGetValue("sub", out var sub) ? sub
               : claims.TryGetValue("nameid", out var nameid) ? nameid
               : string.Empty;

        var name = claims.TryGetValue("name", out var n) ? n : string.Empty;

        var roles = new List<string>();
        if (claims.TryGetValue("role", out var roleValue))
        {
            // roleValue may be a JSON array or a plain string stored during parsing
            if (roleValue.StartsWith("["))
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<List<string>>(roleValue);
                    if (arr != null) roles.AddRange(arr);
                }
                catch { roles.Add(roleValue); }
            }
            else
            {
                roles.Add(roleValue);
            }
        }

        return new AuthUser(id, name, roles);
    }

    private static Dictionary<string, string> ParseClaimsFromJwt(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2)
            return new Dictionary<string, string>();

        var payload = parts[1];
        // Base64url → Base64
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var bytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(bytes);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var doc = JsonDocument.Parse(json);

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            var key = prop.Name;
            var val = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString() ?? string.Empty,
                JsonValueKind.Number => prop.Value.GetRawText(),
                JsonValueKind.Array  => prop.Value.GetRawText(),
                JsonValueKind.True   => "true",
                JsonValueKind.False  => "false",
                _                    => prop.Value.GetRawText()
            };
            result[key] = val;
        }

        return result;
    }
}
