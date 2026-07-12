// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Result payload for BulkUpdateOperationalCommand.
// SchemaVersion: 1

namespace PlantSafety.Application.Operational.Commands;

/// <summary>Aggregate result of a <see cref="BulkUpdateOperationalCommand"/> execution.</summary>
/// <param name="Succeeded">Number of resources successfully updated.</param>
/// <param name="Failed">Number of resources that failed to update.</param>
/// <param name="Results">Per-item results, one entry per requested resource identifier.</param>
public sealed record BulkUpdateOperationalResult(
    int Succeeded,
    int Failed,
    IReadOnlyList<BulkOperationalItemResult> Results);

/// <summary>Per-item result within a <see cref="BulkUpdateOperationalResult"/>.</summary>
/// <param name="Id">The resource identifier this result corresponds to.</param>
/// <param name="Success">Whether the operation succeeded for this item.</param>
/// <param name="ErrorMessage">Human-readable failure reason. Null when <see cref="Success"/> is true.</param>
/// <param name="ErrorCode">Optional machine-readable failure code (e.g. "NOT_FOUND", "INVALID_TRANSITION").</param>
public sealed record BulkOperationalItemResult(
    Guid Id,
    bool Success,
    string? ErrorMessage = null,
    string? ErrorCode = null);
