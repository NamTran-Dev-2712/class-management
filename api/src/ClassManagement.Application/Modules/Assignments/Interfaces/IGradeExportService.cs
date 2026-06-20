using ClassManagement.Application.Modules.Assignments.DTOs;

namespace ClassManagement.Application.Modules.Assignments.Interfaces;

/// <summary>
/// Serializes an assignment grade report to a downloadable CSV (MVP-6, T6-09). Implemented in
/// Infrastructure. The localized header + placeholder labels are passed in (resolved against the request
/// culture by the controller) so the service stays free of any localization dependency. The row count is
/// soft-capped by <see cref="IAssignmentPolicy.MaxExportRows"/>.
/// </summary>
public interface IGradeExportService
{
    byte[] BuildCsv(AssignmentReportDto report, GradeExportLabels labels);
}

/// <summary>Localized CSV header + placeholder strings for the grade export.</summary>
public sealed record GradeExportLabels(
    string Student,
    string Score,
    string Total,
    string Attempts,
    string NotSubmitted,
    string NotGraded
);
