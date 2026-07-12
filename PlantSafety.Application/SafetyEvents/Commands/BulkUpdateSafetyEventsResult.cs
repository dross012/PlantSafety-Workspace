// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Result payload for BulkUpdateSafetyEventsCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.Commands;

/// <summary>Aggregate result of a <see cref="BulkUpdateSafetyEventsCommand"/> execution.</summary>
/// <param name="Succeeded">Number of resources successfully updated.</param>
/// <param name="Failed">Number of resources that failed to update.</param>
/// <param name="Results">Per-item results, one entry per requested resource identifier.</param>
public sealed record BulkUpdateSafetyEventsResult(
    int Succeeded,
    int Failed,
    IReadOnlyList<BulkSafetyEventsItemResult> Results);

/// <summary>Per-item result within a <see cref="BulkUpdateSafetyEventsResult"/>.</summary>
/// <param name="Id">The resource identifier this result corresponds to.</param>
/// <param name="Success">Whether the operation succeeded for this item.</param>
/// <param name="ErrorMessage">Human-readable failure reason. Null when <see cref="Success"/> is true.</param>
/// <param name="ErrorCode">Optional machine-readable failure code (e.g. "NOT_FOUND", "INVALID_TRANSITION").</param>
public sealed record BulkSafetyEventsItemResult(
    Guid Id,
    bool Success,
    string? ErrorMessage = null,
    string? ErrorCode = null);
