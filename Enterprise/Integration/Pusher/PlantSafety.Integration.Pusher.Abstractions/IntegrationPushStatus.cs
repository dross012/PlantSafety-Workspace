namespace PlantSafety.Integration.Pusher.Abstractions;

/// <summary>Outcome classification for a single pusher run.</summary>
public enum IntegrationPushStatus
{
    /// <summary>Every feed pushed with no errors.</summary>
    Success = 0,

    /// <summary>At least one feed pushed successfully, at least one failed.</summary>
    PartialFailure = 1,

    /// <summary>No feed pushed successfully.</summary>
    Failed = 2,
}
