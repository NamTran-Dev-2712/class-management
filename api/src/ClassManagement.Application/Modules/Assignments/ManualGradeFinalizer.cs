using ClassManagement.Domain.Modules.Assignments.Enums;

// Shared recompute routine for manual grading (MVP-6), mirroring AttemptGrading for the auto path.
// Given a tracked attempt plus the ids of its writing snapshot questions and the attempt's current
// manual grades, it recomputes the attempt's manual + final totals and next status:
//   TotalManualScore = SUM(manual grade scores)
//   when EVERY writing question has a grade → Status = Graded, TotalScore = (auto ?? 0) + manual
//   otherwise                               → Status stays NeedManualGrading, TotalScore = null
// Idempotent and also correct when a teacher edits an already-Graded attempt (all writing questions
// still graded → stays Graded, totals recomputed). Does NOT commit — the caller owns SaveChanges.
internal static class ManualGradeFinalizer
{
    public static void Recompute(
        IUnitOfWork unitOfWork,
        Attempt attempt,
        IReadOnlyCollection<long> writingQuestionIds,
        IReadOnlyCollection<ManualGrade> manualGrades
    )
    {
        var totalManual = manualGrades.Sum(g => g.Score);
        var gradedQuestionIds = manualGrades.Select(g => g.SnapshotQuestionId).ToHashSet();
        var allGraded = writingQuestionIds.All(gradedQuestionIds.Contains);

        attempt.TotalManualScore = totalManual;

        if (allGraded)
        {
            attempt.Status = AttemptStatus.Graded;
            attempt.TotalScore = (attempt.TotalAutoScore ?? 0m) + totalManual;
        }
        else
        {
            attempt.Status = AttemptStatus.NeedManualGrading;
            attempt.TotalScore = null;
        }

        unitOfWork.Attempts.Update(attempt);
    }
}
