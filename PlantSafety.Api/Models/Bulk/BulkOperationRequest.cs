// ============================================================================
// File: BulkOperationRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Request body model for all PlantSafety.Api bulk operation endpoints.
//              Carries a list of resource IDs and the operation to apply uniformly
//              across all of them.
// ============================================================================
//
// Onboarding Notes:
// - Bound via [FromBody] on PUT/POST bulk action methods.
// - Operation is a free-text discriminator that each module's Application layer
//   command validates against its own allowed operation set. For example, Compliance
//   bulk accepts "Close", "Approve", "Reassign"; SafetyEvents bulk accepts
//   "Acknowledge", "Escalate", "Close".
// - Parameters is an optional dictionary for operation-specific data (e.g. the
//   AssignedToUserId for a Reassign operation, or the ClosureReason for a Close).
//   The Application layer deserialises only the keys it expects; unknown keys are ignored.
// - ResourceIds is capped at 500 per request. Exceeding this returns HTTP 422 from
//   model validation. Clients should page bulk operations into batches.
//
// Rationale:
// - A single shared bulk request type prevents each module from defining its own
//   near-identical BulkStatusUpdateRequest / BulkApproveRequest classes.
//
// Future Extension Seams:
// - Add DryRun (bool) to allow clients to validate a bulk operation without
//   committing changes, returning what WOULD be updated.
//
// ============================================================================

using System.ComponentModel.DataAnnotations;

namespace PlantSafety.Api.Models.Bulk;

/// <summary>
/// Request body model for PlantSafety.Api bulk operation endpoints.
/// Bound via <c>[FromBody]</c> on <c>PUT .../bulk</c> and <c>POST .../bulk</c> actions.
/// </summary>
/// <param name="TenantId">The tenant context for this bulk operation.</param>
/// <param name="SiteId">Optional site scope. When <c>null</c>, applies tenant-wide.</param>
/// <param name="ResourceIds">
/// The identifiers of the resources to operate on. Maximum 500 per request.
/// </param>
/// <param name="Operation">
/// The operation to apply (e.g. <c>"Close"</c>, <c>"Approve"</c>, <c>"Reassign"</c>).
/// Valid values are module-specific and validated by the Application layer command.
/// </param>
/// <param name="Parameters">
/// Optional key-value pairs for operation-specific data
/// (e.g. <c>{ "AssignedToUserId": "..." }</c> for a Reassign operation).
/// </param>
public sealed record BulkOperationRequest(
    Guid TenantId,
    Guid? SiteId,
    [property: MaxLength(500)] IReadOnlyList<Guid> ResourceIds,
    [property: MinLength(1), MaxLength(64)] string Operation,
    Dictionary<string, string>? Parameters = null);
