using ClassManagement.Application.Modules.Questions.DTOs;
using ClassManagement.Domain.Modules.Questions.Enums;

// Builds the child Option/Tag entities for a question from client input, applying the per-type
// rules (BR-3-02..05): choice types keep the supplied options in order; TrueFalse forces the two
// canonical "True"/"False" labels (server-seeded) while honouring the teacher's correct choice;
// writing types have no options. Validation of counts/correctness lives in the validators.
internal static class QuestionAssembler
{
    public const string TrueLabel = "True";
    public const string FalseLabel = "False";

    public static List<QuestionOption> BuildOptions(
        QuestionType type,
        IReadOnlyList<QuestionOptionInput>? inputs
    )
    {
        if (type is QuestionType.ShortWriting or QuestionType.LongWriting || inputs is null)
            return [];

        if (type == QuestionType.TrueFalse)
        {
            // Seed the fixed labels by position; the teacher only chooses which side is correct.
            return
            [
                new QuestionOption
                {
                    Content = TrueLabel,
                    IsCorrect = inputs.Count > 0 && inputs[0].IsCorrect,
                    DisplayOrder = 0,
                },
                new QuestionOption
                {
                    Content = FalseLabel,
                    IsCorrect = inputs.Count > 1 && inputs[1].IsCorrect,
                    DisplayOrder = 1,
                },
            ];
        }

        return inputs
            .Select(
                (o, i) =>
                    new QuestionOption
                    {
                        Content = o.Content.Trim(),
                        IsCorrect = o.IsCorrect,
                        DisplayOrder = i,
                    }
            )
            .ToList();
    }

    public static List<QuestionTag> BuildTags(IEnumerable<string>? rawTags) =>
        QuestionTagNormalizer.Normalize(rawTags).Select(t => new QuestionTag { Tag = t }).ToList();
}
