using LanManager.Api.Clients;
using LanManager.Api.DTOs;

namespace LanManager.Api.Services;

public interface ITestService
{
    Task<CheckInDto> GetCheckInAsync(TestEnum test, Guid guid);
    Task<IEnumerable<DoorPassDto>> GetDoorPassAsync(TestEnum test, Guid guid);
    Task<QrCodeAtt> GetQrCodeAsync(TestEnum test, Guid guid, UserDto dto);
}

public class TestService(ICustomHttpClient customHttpClient) : ITestService
{
    private readonly ICustomHttpClient _customHttpClient = customHttpClient;

    public async Task<CheckInDto> GetCheckInAsync(TestEnum test, Guid guid)
    {
        return await _customHttpClient.GetCheckInAsync(test, guid);
    }

    public async Task<IEnumerable<DoorPassDto>> GetDoorPassAsync(TestEnum test, Guid guid)
    {
        return await _customHttpClient.GetDoorPassAsync(test, guid);
    }

    public async Task<QrCodeAtt> GetQrCodeAsync(TestEnum test, Guid guid, UserDto dto)
    {
        var qrCode = await _customHttpClient.GetQrCodeAsync(test, guid, dto.Id);
        return new QrCodeAtt(dto.Name, dto.UserName, qrCode);
    }
}

public record QrCodeAtt(string Name, string UserName, byte[] QrCode);