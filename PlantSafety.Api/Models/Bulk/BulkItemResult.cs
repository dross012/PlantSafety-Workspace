// ============================================================================
// File: BulkItemResult.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Per-item result record within a BulkOperationResponse. Reports
//              whether the operation succeeded for a specific resource ID and
//              provides an error message if it failed.
// ============================================================================
//
// Onboarding Notes:
// - One BulkItemResult is returned per resource ID in the original
//   BulkOperationRequest.ResourceIds list, in the same order.
// - ErrorMessage is null when Success is true. When Success is false, it contains
//   the human-readable reason (e.g. "Resource not found", "Transition not permitted
//   from current status: Closed → Approved").
// - ErrorCode is an optional machine-readable code for clients that need to
//   categorise failures without parsing ErrorMessage strings
//   (e.g. "NOT_FOUND", "INVALID_TRANSITION", "PERMISSION_DENIED").
//
// Rationale:
// - Keeping per-item results as a simple flat record (not nested) makes
//   client-side processing straightforward: iterate Results, filter Success=false.
//
// ============================================================================

namespace PlantSafety.Api.Models.Bulk;

/// <summary>
/// Per-item result within a <see cref="BulkOperationResponse"/>.
/// One entry is returned for each resource ID in the original
/// <see cref="BulkOperationRequest.ResourceIds"/> list.
/// </summary>
/// <param name="ResourceId">The resource identifier this result corresponds to.</param>
/// <param name="Success">Indicates whether the operation succeeded for this item.</param>
/// <param name="ErrorMessage">Human-readable failure reason. <c>null</c> when <see cref="Success"/> is <c>true</c>.</param>
/// <param name="ErrorCode">Optional machine-readable failure code (e.g. <c>"NOT_FOUND"</c>, <c>"INVALID_TRANSITION"</c>).</param>
public sealed record BulkItemResult(
    Guid    ResourceId,
    bool    Success,
    string? ErrorMessage = null,
    string? ErrorCode    = null);
