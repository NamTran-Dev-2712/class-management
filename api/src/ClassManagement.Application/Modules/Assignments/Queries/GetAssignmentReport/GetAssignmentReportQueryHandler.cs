using ClassManagement.Application.Exceptions;
using ClassManagement.Application.Modules.Assignments.DTOs;
using ClassManagement.Application.Modules.Assignments.Interfaces;
using ClassManagement.Domain.Modules.Assignments.Enums;
using ClassManagement.Domain.Modules.Classroom.Entities;

public class GetAssignmentReportQueryHandler
    : IRequestHandler<GetAssignmentReportQuery, AssignmentReportDto>
{
    private const string ApprovedStatus = "Approved";
    private static readonly string GradedStatus = nameof(AttemptStatus.Graded);
    private static readonly string AutoGradedStatus = nameof(AttemptStatus.AutoGraded);
    private static readonly string NeedManualStatus = nameof(AttemptStatus.NeedManualGrading);
    private static readonly string InProgressStatus = nameof(AttemptStatus.InProgress);
    private const string NotSubmittedStatus = "NotSubmitted";

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAssignmentPolicy _policy;

    public GetAssignmentReportQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IAssignmentPolicy policy
    )
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _policy = policy;
    }

    public async Task<AssignmentReportDto> Handle(
        GetAssignmentReportQuery request,
        CancellationToken ct
    )
    {
        var view =
            await AssignmentDetailLoader.LoadViewAsync(_unitOfWork, request.PublicId, ct)
            ?? throw new NotFoundException("Assignment.NotFound");

        if (request.OwnerScoped && view.TeacherId != _currentUser.UserId)
            throw new ForbiddenException("Assignment.NotOwner");

        // Roster: approved students of the assignment's class.
        var cmRepo = _unitOfWork.Repository<ClassMemberView>();
        var roster = await cmRepo.ToListAsync(
            cmRepo
                .Query()
                .Where(m => m.ClassId == view.ClassId && m.Status == ApprovedStatus)
                .Select(m => new RosterRow(m.StudentId, m.StudentPublicId, m.StudentName)),
            ct
        );

        // All attempts of this assignment (bounded by class size × max attempts).
        var avRepo = _unitOfWork.Repository<AttemptView>();
        var attempts = await avRepo.ToListAsync(
            avRepo
                .Query()
                .Where(a => a.AssignmentId == view.Id)
                .Select(a => new AttemptRow(a.StudentId, a.AttemptNumber, a.Status, a.TotalScore)),
            ct
        );
        var attemptsByStudent = attempts
            .GroupBy(a => a.StudentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var isHighest = !string.Equals(
            view.ScorePolicy,
            nameof(ScorePolicy.Latest),
            StringComparison.Ordinal
        );

        var rows = new List<ReportStudentRowDto>(roster.Count);
        var effectiveScores = new List<decimal>();
        var submittedCount = 0;
        var pendingCount = 0;

        foreach (var student in roster)
        {
            attemptsByStudent.TryGetValue(student.StudentId, out var studentAttempts);
            studentAttempts ??= [];

            var submitted = studentAttempts.Any(a => a.Status != InProgressStatus);
            if (submitted)
                submittedCount++;

            var hasPending = studentAttempts.Any(a => a.Status == NeedManualStatus);
            if (hasPending)
                pendingCount++;

            // Finalized (graded/auto-graded) attempts back the effective score.
            var finalized = studentAttempts
                .Where(a =>
                    (a.Status == GradedStatus || a.Status == AutoGradedStatus)
                    && a.TotalScore.HasValue
                )
                .ToList();

            decimal? effectiveScore = null;
            string status;
            if (finalized.Count > 0)
            {
                var chosen = isHighest
                    ? finalized.OrderByDescending(a => a.TotalScore).First()
                    : finalized.OrderByDescending(a => a.AttemptNumber).First();
                effectiveScore = chosen.TotalScore;
                status = chosen.Status;
                effectiveScores.Add(chosen.TotalScore!.Value);
            }
            else if (hasPending)
            {
                status = NeedManualStatus;
            }
            else if (submitted)
            {
                status = nameof(AttemptStatus.Submitted);
            }
            else
            {
                status = NotSubmittedStatus;
            }

            rows.Add(
                new ReportStudentRowDto
                {
                    StudentPublicId = student.StudentPublicId,
                    StudentName = student.StudentName,
                    Submitted = submitted,
                    Status = status,
                    EffectiveScore = effectiveScore,
                    AttemptCount = studentAttempts.Count,
                }
            );
        }

        var buckets = BuildBuckets(
            effectiveScores,
            view.TotalPoint,
            _policy.ReportHistogramBuckets
        );

        return new AssignmentReportDto
        {
            PublicId = view.PublicId,
            Title = view.Title,
            ScorePolicy = view.ScorePolicy,
            GradePublishPolicy = view.GradePublishPolicy,
            GradesReleasedAt = view.GradesReleasedAt,
            TotalPoint = view.TotalPoint,
            TotalStudents = roster.Count,
            SubmittedCount = submittedCount,
            NotSubmittedCount = roster.Count - submittedCount,
            GradedCount = effectiveScores.Count,
            PendingGradingCount = pendingCount,
            AverageScore =
                effectiveScores.Count > 0 ? Math.Round(effectiveScores.Average(), 2) : null,
            MinScore = effectiveScores.Count > 0 ? effectiveScores.Min() : null,
            MaxScore = effectiveScores.Count > 0 ? effectiveScores.Max() : null,
            Buckets = buckets,
            Students = rows.OrderBy(r => r.StudentName, StringComparer.OrdinalIgnoreCase).ToList(),
        };
    }

    // Equal-width score buckets over [0, totalPoint]; the top boundary is inclusive (a perfect score
    // lands in the last bucket). Empty when the assignment has no snapshot total (unpublished).
    private static List<ReportBucketDto> BuildBuckets(
        IReadOnlyList<decimal> scores,
        decimal? totalPoint,
        int bucketCount
    )
    {
        if (totalPoint is not decimal total || total <= 0m || bucketCount <= 0)
            return [];

        var width = total / bucketCount;
        var counts = new int[bucketCount];
        foreach (var score in scores)
        {
            var index = (int)Math.Floor(score / width);
            if (index >= bucketCount)
                index = bucketCount - 1;
            if (index < 0)
                index = 0;
            counts[index]++;
        }

        var buckets = new List<ReportBucketDto>(bucketCount);
        for (var i = 0; i < bucketCount; i++)
        {
            var start = Math.Round(width * i, 2);
            var end = i == bucketCount - 1 ? total : Math.Round(width * (i + 1), 2);
            buckets.Add(new ReportBucketDto(start, end, counts[i], $"{start:0.##}-{end:0.##}"));
        }

        return buckets;
    }

    private readonly record struct RosterRow(
        long StudentId,
        Guid StudentPublicId,
        string StudentName
    );

    private readonly record struct AttemptRow(
        long StudentId,
        int AttemptNumber,
        string Status,
        decimal? TotalScore
    );
}
