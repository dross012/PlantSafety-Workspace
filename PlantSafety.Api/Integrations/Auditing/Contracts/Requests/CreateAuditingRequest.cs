// ============================================================================
// File: CreateAuditingRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new integration audit log entry.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateAuditingCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add request/response payload capture here when Auditing entries support full call body archival.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Auditing.Contracts.Requests;

/// <summary>Request body for creating a new integration audit log entry.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="ExternalSystem">Name of the external system involved in the call.</param>
/// <param name="Direction">Direction of the call (Inbound/Outbound).</param>
/// <param name="Endpoint">Endpoint or operation invoked.</param>
public sealed record CreateAuditingRequest(
    Guid   TenantId,
    Guid   SiteId,
    string ExternalSystem,
    string Direction,
    string Endpoint);
