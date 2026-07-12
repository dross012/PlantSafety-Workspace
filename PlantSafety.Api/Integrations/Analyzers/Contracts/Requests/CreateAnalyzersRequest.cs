// ============================================================================
// File: CreateAnalyzersRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: External POST request DTO for creating a new analyzer run result.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body. Validated by FluentValidation via ApiValidationFilter. Controller maps this to a CreateAnalyzersCommand.
//
// Rationale:
// - Decouples external HTTP contract from internal Application command type.
//
// Future Extension Seams:
// - Add raw findings payload metadata here when Analyzers results support attached diagnostic artifacts.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analyzers.Contracts.Requests;

/// <summary>Request body for creating a new analyzer run result.</summary>
/// <param name="TenantId">Tenant the record belongs to.</param>
/// <param name="SiteId">Site the record is scoped to.</param>
/// <param name="AnalyzerName">Name of the analyzer that executed.</param>
/// <param name="TargetSystem">External system the analyzer evaluated.</param>
/// <param name="FindingsCount">Number of findings produced by the analyzer run.</param>
public sealed record CreateAnalyzersRequest(
    Guid   TenantId,
    Guid   SiteId,
    string AnalyzerName,
    string TargetSystem,
    int    FindingsCount);
