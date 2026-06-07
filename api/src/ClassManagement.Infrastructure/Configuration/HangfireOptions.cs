namespace ClassManagement.Infrastructure.Configuration;

public sealed class HangfireOptions
{
    public const string SectionName = "Hangfire";

    public bool Enabled { get; init; } = true;
    public bool EnableServer { get; init; } = true;
    public string DashboardPath { get; init; } = "/hangfire";

    // 0 = Hangfire default (Environment.ProcessorCount * 5)
    public int WorkerCount { get; init; }
}
