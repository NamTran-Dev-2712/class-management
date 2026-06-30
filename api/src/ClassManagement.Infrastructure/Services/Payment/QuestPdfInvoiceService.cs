using System.Globalization;
using ClassManagement.Application.Modules.Payment.DTOs;
using ClassManagement.Application.Modules.Payment.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClassManagement.Infrastructure.Services.Payment;

// Renders an invoice to PDF with QuestPDF (MVP-8). Localization-free: the controller passes culture-
// resolved labels. The Community license is set once (free for our scale). Amounts are VND (no decimals).
public sealed class QuestPdfInvoiceService : IInvoicePdfService
{
    static QuestPdfInvoiceService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Render(InvoiceDocumentDto invoice, InvoicePdfLabels labels)
    {
        var amount = FormatVnd(invoice.AmountVnd);

        return Document
            .Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(t => t.FontSize(11));

                    page.Header()
                        .Column(col =>
                        {
                            col.Item().Text(labels.Title).FontSize(22).Bold();
                            col.Item().Text(labels.Issuer).FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(col =>
                        {
                            col.Spacing(6);
                            Row(col, labels.InvoiceNumber, invoice.InvoiceNumber);
                            Row(
                                col,
                                labels.IssuedAt,
                                invoice.IssuedAt.ToString(
                                    "yyyy-MM-dd HH:mm 'UTC'",
                                    CultureInfo.InvariantCulture
                                )
                            );
                            Row(col, labels.BilledTo, invoice.BilledToEmail);
                            Row(col, labels.Plan, invoice.PlanName);
                            Row(col, labels.BillingCycle, invoice.BillingCycle);

                            col.Item()
                                .PaddingTop(12)
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten1);
                            col.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem().Text(labels.Total).Bold().FontSize(14);
                                    row.ConstantItem(160)
                                        .AlignRight()
                                        .Text(amount)
                                        .Bold()
                                        .FontSize(14);
                                });
                            col.Item()
                                .PaddingTop(8)
                                .Text(labels.PaidNote)
                                .FontColor(Colors.Green.Darken1);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(t =>
                            t.Span($"{labels.Amount}: {amount}").FontColor(Colors.Grey.Medium)
                        );
                });
            })
            .GeneratePdf();
    }

    private static void Row(ColumnDescriptor col, string label, string value) =>
        col.Item()
            .Row(row =>
            {
                row.ConstantItem(160).Text(label).FontColor(Colors.Grey.Darken1);
                row.RelativeItem().Text(value);
            });

    private static string FormatVnd(long amount) =>
        string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} ₫", amount);
}
