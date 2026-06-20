using System.Globalization;
using System.Text;
using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Assignments;

// Builds an RFC-4180 CSV of an assignment grade report. Writes a UTF-8 BOM so Excel detects the encoding,
// escapes fields containing comma/quote/newline, and soft-caps the row count (MaxExportRows). A
// not-submitted student shows the NotSubmitted placeholder; a submitted-but-ungraded student shows
// NotGraded — so the export honours grade visibility without leaking partial scores.
public sealed class CsvGradeExportService : IGradeExportService
{
    private readonly AssignmentOptions _options;

    public CsvGradeExportService(IOptions<AssignmentOptions> options)
    {
        _options = options.Value;
    }

    public byte[] BuildCsv(AssignmentReportDto report, GradeExportLabels labels)
    {
        var sb = new StringBuilder();
        sb.Append('﻿'); // UTF-8 BOM for Excel

        AppendRow(sb, labels.Student, labels.Score, labels.Total, labels.Attempts);

        var total =
            report.TotalPoint?.ToString("0.##", CultureInfo.InvariantCulture) ?? string.Empty;

        foreach (var row in report.Students.Take(_options.MaxExportRows))
        {
            string score;
            if (!row.Submitted)
                score = labels.NotSubmitted;
            else if (row.EffectiveScore is decimal value)
                score = value.ToString("0.##", CultureInfo.InvariantCulture);
            else
                score = labels.NotGraded;

            AppendRow(
                sb,
                row.StudentName,
                score,
                total,
                row.AttemptCount.ToString(CultureInfo.InvariantCulture)
            );
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static void AppendRow(StringBuilder sb, params string[] fields)
    {
        for (var i = 0; i < fields.Length; i++)
        {
            if (i > 0)
                sb.Append(',');
            sb.Append(Escape(fields[i]));
        }
        sb.Append("\r\n");
    }

    private static string Escape(string field)
    {
        if (
            field.Contains(',', StringComparison.Ordinal)
            || field.Contains('"', StringComparison.Ordinal)
            || field.Contains('\n', StringComparison.Ordinal)
            || field.Contains('\r', StringComparison.Ordinal)
        )
        {
            return $"\"{field.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return field;
    }
}
