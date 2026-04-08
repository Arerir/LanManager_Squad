using LanManager.Api.Models;
using LanManager.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/report")]
[Authorize(Roles = "Admin,Organizer")]
public class ReportController(
    EventReportService reportService,
    EventReportPdfGenerator pdfGenerator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> DownloadReport(
        Guid eventId,
        [FromQuery] string sections = "All",
        CancellationToken ct = default)
    {
        // Parse comma-separated sections tokens into flags enum
        var parsedSections = ReportSections.Summary; // neutral starting point
        bool first = true;

        foreach (var token in sections.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Enum.TryParse<ReportSections>(token, ignoreCase: true, out var section))
                return BadRequest(new { error = $"Unknown report section: '{token}'." });

            parsedSections = first ? section : (parsedSections | section);
            first = false;
        }

        var data = await reportService.GetReportDataAsync(eventId, parsedSections, ct);
        if (data is null)
            return NotFound();

        if (data.Status != LanManager.Data.Models.EventStatus.Closed)
            return UnprocessableEntity(new { error = "Report is only available for closed events." });

        var pdfBytes = pdfGenerator.GeneratePdf(data);
        var fileName = $"{data.Name.Replace(" ", "-")}-report.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
}
