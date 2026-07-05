using ClassManagement.Domain.Modules.Assignments.Constants;

// Shape checks for a batch of proctoring events (MVP-10). The server owns the counting + threshold; this
// only bounds the batch and validates event-type names. Message keys are dotted i18n keys.
public class RecordAttemptEventsValidator : AbstractValidator<RecordAttemptEventsCommand>
{
    // Upper bound on a single batch to blunt client spam (rate limiting is the primary guard).
    private const int MaxBatchSize = 50;

    public RecordAttemptEventsValidator()
    {
        RuleFor(x => x.Events)
            .NotNull()
            .WithMessage("Validation.Attempt.EventsRequired")
            .Must(e => e.Count <= MaxBatchSize)
            .WithMessage("Validation.Attempt.EventsBatchTooLarge");

        RuleForEach(x => x.Events)
            .ChildRules(e =>
                e.RuleFor(i => i.EventType)
                    .Must(AttemptEventTypes.All.Contains)
                    .WithMessage("Validation.Attempt.EventTypeInvalid")
            );
    }
}
