// ============================================================================
// File: AnalyzersCommandRequest.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Internal Presentation-layer request models for Integrations.Analyzers queries and commands.
// ============================================================================
//
// Onboarding Notes:
// - Created by the controller from validated Contracts/Requests DTOs. Passed to Application-layer query/command constructors.
//
// Rationale:
// - Decouples the external HTTP contract from Application-layer types and allows Presentation-layer enrichment.
//
// Future Extension Seams:
// - Add CallerUserId or resolved permission context here when commands require Presentation-layer enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analyzers.Models.Requests;

/// <summary>Internal Presentation-layer command request for Integrations.Analyzers create/update operations.</summary>
/// <param name="TenantId">Resolved tenant identifier.</param>
/// <param name="SiteId">Target site.</param>
/// <param name="AnalyzerName">Name of the analyzer that executed.</param>
/// <param name="TargetSystem">External system the analyzer evaluated.</param>
/// <param name="FindingsCount">Number of findings produced by the analyzer run.</param>
public sealed record AnalyzersCommandRequest(
    Guid   TenantId,
    Guid   SiteId,
    string AnalyzerName,
    string TargetSystem,
    int    FindingsCount);
