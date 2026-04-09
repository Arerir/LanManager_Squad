using LanManager.Api.Clients;
using LanManager.Api.DTOs;
using LanManager.Api.Services;
using Moq;

namespace LanManager.Api.Tests;

public class TestServiceTests
{
    private static readonly TestEnum DefaultTest = TestEnum.Value1;
    private static readonly Guid DefaultGuid = Guid.NewGuid();

    [Fact]
    public async Task GetCheckInAsync_ReturnsClientResult()
    {
        var expected = new CheckInDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "user", DateTime.UtcNow, null);
        var mockClient = new Mock<ICustomHttpClient>();
        mockClient.Setup(c => c.GetCheckInAsync(DefaultTest, DefaultGuid)).ReturnsAsync(expected);

        var service = new TestService(mockClient.Object);
        var result = await service.GetCheckInAsync(DefaultTest, DefaultGuid);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetCheckInAsync_ForwardsCorrectArguments()
    {
        var test = TestEnum.Value2;
        var guid = Guid.NewGuid();
        var mockClient = new Mock<ICustomHttpClient>();
        mockClient.Setup(c => c.GetCheckInAsync(test, guid))
            .ReturnsAsync(new CheckInDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "u", DateTime.UtcNow, null));

        var service = new TestService(mockClient.Object);
        await service.GetCheckInAsync(test, guid);

        mockClient.Verify(c => c.GetCheckInAsync(test, guid), Times.Once);
    }

    [Fact]
    public async Task GetDoorPassAsync_ReturnsClientResult()
    {
        var expected = new List<DoorPassDto>
        {
            new DoorPassDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "user", "Entry", DateTime.UtcNow)
        };
        var mockClient = new Mock<ICustomHttpClient>();
        mockClient.Setup(c => c.GetDoorPassAsync(DefaultTest, DefaultGuid)).ReturnsAsync(expected);

        var service = new TestService(mockClient.Object);
        var result = await service.GetDoorPassAsync(DefaultTest, DefaultGuid);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(expected[0], result.First());
    }

    [Fact]
    public async Task GetDoorPassAsync_ForwardsCorrectArguments()
    {
        var test = TestEnum.Value3;
        var guid = Guid.NewGuid();
        var mockClient = new Mock<ICustomHttpClient>();
        mockClient.Setup(c => c.GetDoorPassAsync(test, guid))
            .ReturnsAsync(new List<DoorPassDto> { new DoorPassDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "u", "Exit", DateTime.UtcNow) });

        var service = new TestService(mockClient.Object);
        await service.GetDoorPassAsync(test, guid);

        mockClient.Verify(c => c.GetDoorPassAsync(test, guid), Times.Once);
    }

    [Fact]
    public async Task GetQrCodeAsync_ReturnsQrCodeAttWithClientBytes()
    {
        var qrBytes = new byte[] { 1, 2, 3 };
        var dto = new UserDto(Guid.NewGuid(), "Alice Smith", "alice", "alice@test.com");
        var mockClient = new Mock<ICustomHttpClient>();
        mockClient.Setup(c => c.GetQrCodeAsync(DefaultTest, DefaultGuid, dto.Id)).ReturnsAsync(qrBytes);

        var service = new TestService(mockClient.Object);
        var result = await service.GetQrCodeAsync(DefaultTest, DefaultGuid, dto);

        Assert.Equal(qrBytes, result.QrCode);
    }

    [Fact]
    public async Task GetQrCodeAsync_ForwardsDtoIdToClient()
    {
        var dto = new UserDto(Guid.NewGuid(), "Bob Jones", "bob", "bob@test.com");
        var mockClient = new Mock<ICustomHttpClient>();
        mockClient.Setup(c => c.GetQrCodeAsync(It.IsAny<TestEnum>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(Array.Empty<byte>());

        var service = new TestService(mockClient.Object);
        await service.GetQrCodeAsync(DefaultTest, DefaultGuid, dto);

        mockClient.Verify(c => c.GetQrCodeAsync(DefaultTest, DefaultGuid, dto.Id), Times.Once);
    }

    [Fact]
    public async Task GetQrCodeAsync_WrapsNameAndUserNameFromDto()
    {
        var dto = new UserDto(Guid.NewGuid(), "Carol White", "carol", "carol@test.com");
        var mockClient = new Mock<ICustomHttpClient>();
        mockClient.Setup(c => c.GetQrCodeAsync(It.IsAny<TestEnum>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new byte[] { 9, 8, 7 });

        var service = new TestService(mockClient.Object);
        var result = await service.GetQrCodeAsync(DefaultTest, DefaultGuid, dto);

        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.UserName, result.UserName);
    }
}
