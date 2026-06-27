// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validation constants specific to the SafetyEvents module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Validation;

/// <summary>Validation constraint constants for SafetyEvents features.</summary>
public static class SafetyEventsValidationConstants
{
    /// <summary>Maximum length for incident/near-miss title fields.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum length for description fields.</summary>
    public const int MaxDescriptionLength = 4_000;

    /// <summary>Maximum length for location fields.</summary>
    public const int MaxLocationLength = 500;

    /// <summary>Maximum length for deletion reason fields.</summary>
    public const int MaxDeletionReasonLength = 1_000;

    /// <summary>Minimum length for full-text search terms.</summary>
    public const int MinSearchTermLength = 2;

    /// <summary>Maximum length for full-text search terms.</summary>
    public const int MaxSearchTermLength = 200;

    /// <summary>Maximum number of records returned by a synchronous export query.</summary>
    public const int MaxSyncExportRecords = 5_000;

    /// <summary>Hard cap on export job record count.</summary>
    public const int MaxExportJobRecords = 10_000;

    /// <summary>Tolerance window in seconds for "not in future" UTC timestamp checks.</summary>
    public const int FutureToleranceSeconds = 30;
}
