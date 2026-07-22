// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Result payload for BulkUpdatePPECommand.
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Aggregate result of a <see cref="BulkUpdatePPECommand"/> execution.</summary>
/// <param name="Succeeded">Number of resources successfully updated.</param>
/// <param name="Failed">Number of resources that failed to update.</param>
/// <param name="Results">Per-item results, one entry per requested resource identifier.</param>
public sealed record BulkUpdatePPEResult(
    int Succeeded,
    int Failed,
    IReadOnlyList<BulkPPEItemResult> Results);

/// <summary>Per-item result within a <see cref="BulkUpdatePPEResult"/>.</summary>
/// <param name="Id">The resource identifier this result corresponds to.</param>
/// <param name="Success">Whether the operation succeeded for this item.</param>
/// <param name="ErrorMessage">Human-readable failure reason. Null when <see cref="Success"/> is true.</param>
/// <param name="ErrorCode">Optional machine-readable failure code (e.g. "NOT_FOUND", "INVALID_TRANSITION").</param>
public sealed record BulkPPEItemResult(
    Guid Id,
    bool Success,
    string? ErrorMessage = null,
    string? ErrorCode = null);