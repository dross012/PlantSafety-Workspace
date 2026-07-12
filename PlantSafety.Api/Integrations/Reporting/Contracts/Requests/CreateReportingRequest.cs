// ============================================================================
// File: CreateReportingRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new Integrations.Reporting record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateReportingCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add file attachment metadata here when Integrations.Reporting records support document uploads.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Reporting.Contracts.Requests;

/// <summary>Request body for creating a new Integrations.Reporting record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="ReportName">Name of the report being delivered.</param>
/// <param name="Description">Detailed description or notes.</param>
/// <param name="ScheduledAtUtc">Date/time the Reporting activity is scheduled (UTC).</param>
/// <param name="AssignedToUserId">Optional assigned user.</param>
public sealed record CreateReportingRequest(
    Guid     TenantId,
    Guid     SiteId,
    string   ReportName,
    string?  Description,
    DateTime ScheduledAtUtc,
    Guid?    AssignedToUserId);
