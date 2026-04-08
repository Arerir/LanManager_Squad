using LanManager.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LanManager.Api.Services;

public class EventReportPdfGenerator
{
    public byte[] GeneratePdf(EventReportData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader(data));
                page.Content().Element(content => ComposeContent(content, data));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private Action<IContainer> ComposeHeader(EventReportData data) => container =>
    {
        container.Column(col =>
        {
            col.Item().Text(data.Name).FontSize(22).Bold();
            if (!string.IsNullOrEmpty(data.Location))
                col.Item().Text(data.Location).FontSize(12).FontColor(Colors.Grey.Darken2);
            col.Item().Text($"{data.StartDate:yyyy-MM-dd} — {data.EndDate:yyyy-MM-dd}").FontSize(10);

            int attended = data.CheckIns?.Length ?? 0;
            col.Item().Text($"Capacity: {data.Capacity} / {attended} attended")
                .FontSize(10).FontColor(Colors.Grey.Darken1);

            col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    };

    private void ComposeContent(IContainer container, EventReportData data)
    {
        container.Column(col =>
        {
            if (data.Registrations != null)
            {
                col.Item().PaddingTop(12).Element(c => ComposeRegistrations(c, data.Registrations));
            }

            if (data.CheckIns != null)
            {
                col.Item().PaddingTop(12).Element(c => ComposeCheckIns(c, data.CheckIns));
            }

            if (data.Equipment != null && data.Equipment.Length > 0)
            {
                col.Item().PaddingTop(12).Element(ComposeEquipment);
            }

            if (data.Tournaments != null && data.Tournaments.Length > 0)
            {
                col.Item().PaddingTop(12).Element(ComposeTournaments);
            }
        });
    }

    private void ComposeRegistrations(IContainer container, RegistrationSummary[] registrations)
    {
        container.Column(col =>
        {
            col.Item().Text("Registrations").FontSize(14).Bold();
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Darken2).Padding(4)
                        .Text("Name").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Grey.Darken2).Padding(4)
                        .Text("Status").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Grey.Darken2).Padding(4)
                        .Text("Registered At").FontColor(Colors.White).Bold();
                });

                for (int i = 0; i < registrations.Length; i++)
                {
                    var reg = registrations[i];
                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten3;

                    table.Cell().Background(bg).Padding(4).Text(reg.UserName);
                    table.Cell().Background(bg).Padding(4).Text(reg.Status.ToString());
                    table.Cell().Background(bg).Padding(4).Text(reg.RegisteredAt.ToString("yyyy-MM-dd HH:mm"));
                }
            });
        });
    }

    private void ComposeCheckIns(IContainer container, CheckInSummary[] checkIns)
    {
        container.Column(col =>
        {
            col.Item().Text("Check-Ins").FontSize(14).Bold();
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Darken2).Padding(4)
                        .Text("Name").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Grey.Darken2).Padding(4)
                        .Text("Checked In").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Grey.Darken2).Padding(4)
                        .Text("Checked Out").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Grey.Darken2).Padding(4)
                        .Text("Duration").FontColor(Colors.White).Bold();
                });

                for (int i = 0; i < checkIns.Length; i++)
                {
                    var ci = checkIns[i];
                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten3;
                    string duration = ci.Duration.HasValue
                        ? $"{(int)ci.Duration.Value.TotalHours}:{ci.Duration.Value.Minutes:D2}"
                        : "Still inside";
                    string checkedOut = ci.CheckedOutAt.HasValue
                        ? ci.CheckedOutAt.Value.ToString("yyyy-MM-dd HH:mm")
                        : "—";

                    table.Cell().Background(bg).Padding(4).Text(ci.UserName);
                    table.Cell().Background(bg).Padding(4).Text(ci.CheckedInAt.ToString("yyyy-MM-dd HH:mm"));
                    table.Cell().Background(bg).Padding(4).Text(checkedOut);
                    table.Cell().Background(bg).Padding(4).Text(duration);
                }
            });
        });
    }

    private void ComposeEquipment(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Equipment").FontSize(14).Bold();
            col.Item().PaddingTop(4).Background(Colors.Grey.Lighten3).Padding(8)
                .Text("Equipment data coming soon").FontColor(Colors.Grey.Darken2).Italic();
        });
    }

    private void ComposeTournaments(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Text("Tournaments").FontSize(14).Bold();
            col.Item().PaddingTop(4).Background(Colors.Grey.Lighten3).Padding(8)
                .Text("Tournament data coming soon").FontColor(Colors.Grey.Darken2).Italic();
        });
    }
}
