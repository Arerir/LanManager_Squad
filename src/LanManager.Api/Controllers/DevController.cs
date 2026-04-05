using LanManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/dev")]
public class DevController : ControllerBase
{
    private readonly DataSeeder _seeder;
    private readonly IWebHostEnvironment _env;

    public DevController(DataSeeder seeder, IWebHostEnvironment env)
    {
        _seeder = seeder;
        _env = env;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        if (!_env.IsDevelopment())
            return NotFound();
        await _seeder.SeedAsync();
        return Ok(new { message = "Database seeded successfully." });
    }
}
