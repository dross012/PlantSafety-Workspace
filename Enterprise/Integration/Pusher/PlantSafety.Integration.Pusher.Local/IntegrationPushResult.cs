namespace PlantSafety.Integration.Pusher.Local;

using PlantSafety.Integration.Pusher.Abstractions;

/// <summary>Default <see cref="IIntegrationPushResult"/> implementation.</summary>
public sealed class IntegrationPushResult : IIntegrationPushResult
{
    public required IntegrationPushStatus Status { get; init; }
    public required int SucceededCount { get; init; }
    public required int FailedCount { get; init; }
    public required IReadOnlyDictionary<string, string> FeedDetails { get; init; }
    public required DateTime StartedUtc { get; init; }
    public required DateTime CompletedUtc { get; init; }
}
