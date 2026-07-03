using ClassManagement.Infrastructure.Configuration;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Media.Jobs;

// Thin Hangfire trigger for the media cleanup sweep (MVP-9). All logic lives in the Application
// RunMediaCleanupCommand handler; this only reads the appsettings tunables and forwards them so the
// Application layer stays Options-free. Idempotent — a missed tick is caught next run.
public sealed class MediaCleanupJob
{
    private readonly ISender _mediator;
    private readonly MediaOptions _options;

    public MediaCleanupJob(ISender mediator, IOptions<MediaOptions> options)
    {
        _mediator = mediator;
        _options = options.Value;
    }

    [AutomaticRetry(Attempts = 0)]
    public Task ExecuteAsync() =>
        _mediator.Send(
            new RunMediaCleanupCommand(_options.PendingConfirmTtlMinutes, _options.CleanupBatchSize)
        );
}
