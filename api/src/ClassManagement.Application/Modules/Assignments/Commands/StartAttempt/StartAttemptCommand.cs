// Student starts an attempt for an assignment. If an InProgress attempt already exists it is returned
// instead of creating a new one (edge case: redirect to the in-progress attempt). IpAddress/UserAgent
// are captured by the controller for audit. Returns the attempt's PublicId.
public record StartAttemptCommand(Guid AssignmentId) : IRequest<Guid>
{
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}
