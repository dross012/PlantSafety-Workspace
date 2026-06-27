// ============================================================================
// File: BulkOperationResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Response payload for all PlantSafety.Api bulk operation endpoints.
//              Reports aggregate success/failure counts and per-item results.
// ============================================================================
//
// Onboarding Notes:
// - Returned as the Data payload inside ApiResponse<BulkOperationResponse>.
// - Bulk operations use partial-success semantics: a bulk request that partially
//   fails returns HTTP 200 with Succeeded < TotalRequested (not 400 or 207).
//   Clients inspect Results to identify which IDs failed and why.
// - This matches the pattern used by Azure Blob Batch API and Stripe batch endpoints,
//   which is familiar to integration developers.
// - HasPartialFailure is a convenience flag: true when Succeeded > 0 && Failed > 0.
//   Clients can use this to show a "partial success" warning in UI.
//
// Rationale:
// - Returning per-item results in the same response as the aggregate counts avoids
//   a second "get bulk job status" endpoint for small-to-medium batch sizes (< 500).
//
// Future Extension Seams:
// - Add AsyncJobId (Guid?) for large batches (> 500 items) that are processed
//   asynchronously. When present, Results will be empty and the client should poll
//   the job status endpoint.
//
// ============================================================================

namespace PlantSafety.Api.Models.Bulk;

/// <summary>
/// Response payload for PlantSafety.Api bulk operation endpoints.
/// Reports aggregate counts and per-item results for the operation.
/// </summary>
/// <param name="TotalRequested">Total number of resource IDs submitted in the request.</param>
/// <param name="Succeeded">Number of items successfully processed.</param>
/// <param name="Failed">Number of items that failed processing.</param>
/// <param name="Results">Per-item operation results. One entry per input resource ID.</param>
public sealed record BulkOperationResponse(
    int                         TotalRequested,
    int                         Succeeded,
    int                         Failed,
    IReadOnlyList<BulkItemResult> Results)
{
    /// <summary>
    /// Gets a value indicating whether the operation partially succeeded.
    /// <c>true</c> when at least one item succeeded and at least one failed.
    /// </summary>
    public bool HasPartialFailure => Succeeded > 0 && Failed > 0;

    /// <summary>Gets a value indicating whether all items succeeded.</summary>
    public bool IsFullSuccess => Failed == 0 && Succeeded == TotalRequested;
}
