namespace ClassManagement.Application.Common.Behaviors;

// Coarse audit for commands marked IAuditableRequest (MVP-7): after the handler succeeds, record the
// action + actor in its own commit. Runs inner to ValidationBehavior so only valid, executed commands
// are logged. Handlers needing rich old/new metadata call IAuditLogger directly instead of marking.
public sealed class AuditLoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditLogger _auditLogger;

    public AuditLoggingBehavior(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var response = await next(cancellationToken);

        if (request is IAuditableRequest auditable)
        {
            var targetPublicId =
                auditable.AuditTargetPublicId ?? (response as Guid?) ?? ExtractGuid(response);

            await _auditLogger.LogAndSaveAsync(
                new AuditEntry
                {
                    Action = auditable.AuditAction,
                    TargetType = auditable.AuditTargetType,
                    TargetPublicId = targetPublicId,
                    Metadata = auditable.GetAuditMetadata(),
                },
                cancellationToken
            );
        }

        return response;
    }

    // Create commands often return a small record carrying the new PublicId — pick it up if present.
    private static Guid? ExtractGuid(TResponse response)
    {
        if (response is null)
            return null;
        var prop = response.GetType().GetProperty("PublicId");
        return prop?.GetValue(response) as Guid?;
    }
}
