// ============================================================================
// File: RootCauseAnalysisResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned inside ApiResponse<T> for
//              Root Cause Analysis read endpoints.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.RootCauseAnalysis.Contracts.Responses;

/// <summary>Response payload for a single Root Cause Analysis.</summary>
/// <param name="RcaId">Unique identifier (UUIDv7).</param>
/// <param name="TenantId">Tenant owning this record.</param>
/// <param name="LinkedEventId">The Incident or NearMiss this RCA is linked to.</param>
/// <param name="LinkedEventType">Type of linked event: Incident, NearMiss.</param>
/// <param name="Methodology">RCA methodology used (e.g., 5Why, FishboneDiagram, FaultTree).</param>
/// <param name="RootCauses">Identified root causes (structured as JSON or delimited text).</param>
/// <param name="CorrectiveActions">Corrective actions recommended as output of this RCA.</param>
/// <param name="Status">Workflow status (e.g., Draft, UnderReview, Approved, Closed).</param>
/// <param name="LeadInvestigatorId">Worker who led this RCA.</param>
/// <param name="CompletedAtUtc">UTC timestamp when the RCA was completed, if applicable.</param>
/// <param name="LastModifiedAtUtc">UTC timestamp of last modification.</param>
/// <param name="ETag">Row version for concurrency.</param>
public sealed record RootCauseAnalysisResponse(
    Guid            RcaId,
    Guid            TenantId,
    Guid            LinkedEventId,
    string          LinkedEventType,
    string          Methodology,
    string          RootCauses,
    string          CorrectiveActions,
    string          Status,
    Guid            LeadInvestigatorId,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset  LastModifiedAtUtc,
    string          ETag);
