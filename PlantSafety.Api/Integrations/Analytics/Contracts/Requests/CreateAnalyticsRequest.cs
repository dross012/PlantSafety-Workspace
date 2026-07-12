// ============================================================================
// File: CreateAnalyticsRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new ingested analytics record.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateAnalyticsCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add raw payload metadata here when Analytics ingestion supports arbitrary source schemas.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analytics.Contracts.Requests;

/// <summary>Request body for creating a new ingested analytics record.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="SourceSystem">Name of the external analytics system the record was ingested from.</param>
/// <param name="MetricName">Name of the ingested analytics metric.</param>
/// <param name="MetricValue">Value of the ingested analytics metric.</param>
public sealed record CreateAnalyticsRequest(
    Guid    TenantId,
    Guid    SiteId,
    string  SourceSystem,
    string  MetricName,
    decimal MetricValue);
