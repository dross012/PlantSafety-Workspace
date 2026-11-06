namespace PlantSafety.Integration.Pusher.Abstractions;

/// <summary>Result of one <see cref="IIntegrationPusher"/> run.</summary>
public interface IIntegrationPushResult
{
    /// <summary>Overall outcome classification.</summary>
    IntegrationPushStatus Status { get; }

    /// <summary>Number of rows successfully pushed, across all feeds.</summary>
    int SucceededCount { get; }

    /// <summary>Number of rows that failed to push, across all feeds.</summary>
    int FailedCount { get; }

    /// <summary>Per-feed human-readable detail lines (feed name -> outcome summary).</summary>
    IReadOnlyDictionary<string, string> FeedDetails { get; }

    /// <summary>UTC timestamp the run started.</summary>
    DateTime StartedUtc { get; }

    /// <summary>UTC timestamp the run completed.</summary>
    DateTime CompletedUtc { get; }
}
