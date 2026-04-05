using System.Net.Http.Json;
using System.Text.Json;

namespace LanManager.Maui.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(Config.ApiBaseUrl);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<EventDto>> GetEventsAsync(string? status = null)
    {
        try
        {
            var url = status != null ? $"/api/events?status={status}" : "/api/events";
            return await _httpClient.GetFromJsonAsync<List<EventDto>>(url, _jsonOptions) ?? new List<EventDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetEventsAsync error: {ex.Message}");
            return new List<EventDto>();
        }
    }

    public async Task<List<UserDto>> GetUsersAsync(int page = 1, int pageSize = 100)
    {
        try
        {
            var url = $"/api/users?page={page}&pageSize={pageSize}";
            var result = await _httpClient.GetFromJsonAsync<PagedResult<UserDto>>(url, _jsonOptions);
            return result?.Items.ToList() ?? new List<UserDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetUsersAsync error: {ex.Message}");
            return new List<UserDto>();
        }
    }

    public async Task<CheckInDto?> CheckInAsync(Guid eventId, Guid userId)
    {
        try
        {
            var request = new CheckInRequest(userId);
            var response = await _httpClient.PostAsJsonAsync($"/api/events/{eventId}/checkin", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CheckInDto>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Check-in failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckInAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<CheckInDto?> CheckOutAsync(Guid eventId, Guid userId)
    {
        try
        {
            var request = new CheckInRequest(userId);
            var response = await _httpClient.PostAsJsonAsync($"/api/events/{eventId}/checkout", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CheckInDto>(_jsonOptions);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Check-out failed: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckOutAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<AttendanceDto>> GetAttendanceAsync(Guid eventId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<AttendanceDto>>($"/api/events/{eventId}/attendance", _jsonOptions) 
                   ?? new List<AttendanceDto>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetAttendanceAsync error: {ex.Message}");
            return new List<AttendanceDto>();
        }
    }

    public async Task<DoorPassDto?> DoorScanAsync(Guid eventId, Guid userId, string direction)
    {
        var request = new DoorScanRequest(userId, direction);
        var response = await _httpClient.PostAsJsonAsync($"/api/events/{eventId}/door-scan", request);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<DoorPassDto>(_jsonOptions);
        var error = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Door scan failed ({response.StatusCode}): {error}");
    }

    public async Task<int> GetOutsideCountAsync(Guid eventId)
    {
        try {
            var outside = await _httpClient.GetFromJsonAsync<List<OutsideUserDto>>($"/api/events/{eventId}/outside", _jsonOptions);
            return outside?.Count ?? 0;
        } catch { return 0; }
    }
}

public record CheckInRequest(Guid UserId);
public record CheckInDto(Guid Id, Guid EventId, Guid UserId, string UserName, DateTime CheckedInAt, DateTime? CheckedOutAt);
public record AttendanceDto(Guid UserId, string UserName, DateTime CheckedInAt);
public record UserDto(Guid Id, string Name, string UserName, string Email);
public record EventDto(Guid Id, string Name, string? Description, string? Location, DateTime StartDate, DateTime EndDate, int Capacity, string Status, DateTime CreatedAt);
public record PagedResult<T>(IEnumerable<T> Items, int Page, int PageSize, int TotalCount);
public record DoorScanRequest(Guid UserId, string Direction);
public record DoorPassDto(Guid Id, Guid EventId, Guid UserId, string UserName, string Direction, DateTime ScannedAt);
public record OutsideUserDto(Guid UserId, string UserName, DateTime ExitedAt);
