// ============================================================================
// File: JobSafetyAnalysisResponse.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: External-facing response DTO returned inside ApiResponse<JobSafetyAnalysisResponse> for JobSafetyAnalysis queries.
// ============================================================================
//
// Onboarding Notes:
// - Serialised by the controller and wrapped in ApiResponse<T> before being sent to callers.
//
// Rationale:
// - Keeps the external contract stable even when Application-layer result shapes change.
//
// Future Extension Seams:
// - Add hypermedia links or pagination metadata here when HAL/HATEOAS support is added.
//
// ============================================================================

using PlantSafety.Application.Operational.JobSafetyAnalysis.Models;

namespace PlantSafety.Api.Operational.JobSafetyAnalysis.Contracts.Responses;

/// <summary>Response payload for JobSafetyAnalysis queries.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Result items from the JobSafetyAnalysis read-model.</param>
public sealed record JobSafetyAnalysisResponse(
    Guid                  TenantId,
    Guid?                 SiteId,
    DateTime              GeneratedAt,
    IReadOnlyList<JobSafetyAnalysisItemDto> Items);
