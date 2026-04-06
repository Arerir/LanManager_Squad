using LanManager.Data;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Api.Tests.Helpers;

public static class TestDbContextFactory
{
    public static LanManagerDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<LanManagerDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var ctx = new LanManagerDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
