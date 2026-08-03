// ============================================================================
// File: GetAuditTimelineRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: External-facing DTO for GET /auditing/audit-timeline query parameters, bound from
//              query string by AuditTimelineV1Controller. Matches
//              PlantSafety.UI.Modules.Auditing.ApiClients.AuditingApiClient.GetAuditTimelineAsync's
//              query contract exactly.
// ============================================================================

namespace PlantSafety.Api.Auditing.AuditTimeline.Contracts.Requests;

/// <summary>Query parameters for reconstructing a single entity/aggregate's AuditTimeline.</summary>
/// <remarks>
/// TenantId is intentionally NOT a member of this DTO -- the trusted, server-resolved tenant is
/// obtained by the controller from <see cref="PlantSafety.Api.Context.RequestEnvelopeFactory"/>
/// (backed by <c>IOperationContext</c>, populated by TenantResolutionMiddleware from the
/// authenticated JWT's <c>tenantId</c> claim), never from client-supplied query input. See
/// Infrastructure/Security manual, Zero-Trust tenant-isolation requirement.
/// </remarks>
/// <param name="EntityType">Type discriminator of the entity (e.g. "Risk", "Permit", "AuditFinding").</param>
/// <param name="EntityId">Identity of the entity.</param>
public sealed record GetAuditTimelineRequest(
    string EntityType,
    Guid   EntityId);
