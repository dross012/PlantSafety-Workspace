// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for BulkUpdateObservabilityCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Aggregate result of a <see cref="BulkUpdateObservabilityCommand"/> execution.</summary>
/// <param name="Succeeded">Number of resources successfully updated.</param>
/// <param name="Failed">Number of resources that failed to update.</param>
/// <param name="Results">Per-item results, one entry per requested resource identifier.</param>
public sealed record BulkUpdateObservabilityResult(
    int Succeeded,
    int Failed,
    IReadOnlyList<BulkObservabilityItemResult> Results);

/// <summary>Per-item result within a <see cref="BulkUpdateObservabilityResult"/>.</summary>
/// <param name="Id">The resource identifier this result corresponds to.</param>
/// <param name="Success">Whether the operation succeeded for this item.</param>
/// <param name="ErrorMessage">Human-readable failure reason. Null when <see cref="Success"/> is true.</param>
/// <param name="ErrorCode">Optional machine-readable failure code (e.g. "NOT_FOUND", "INVALID_TRANSITION").</param>
public sealed record BulkObservabilityItemResult(
    Guid Id,
    bool Success,
    string? ErrorMessage = null,
    string? ErrorCode = null);