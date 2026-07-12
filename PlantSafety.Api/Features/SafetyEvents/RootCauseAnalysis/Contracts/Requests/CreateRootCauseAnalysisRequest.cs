// ============================================================================
// File: CreateRootCauseAnalysisRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing DTO for POST /features/safety-events/root-cause-analysis,
//              bound from the request body by the V1 controller.
// ============================================================================

namespace PlantSafety.Api.Features.SafetyEvents.RootCauseAnalysis.Contracts.Requests;

/// <summary>Request body for initiating a Root Cause Analysis.</summary>
/// <param name="LinkedEventId">The Incident or NearMiss this RCA is linked to.</param>
/// <param name="LinkedEventType">Type of linked event: Incident, NearMiss.</param>
/// <param name="Methodology">RCA methodology (e.g., 5Why, FishboneDiagram, FaultTree).</param>
/// <param name="LeadInvestigatorId">Worker leading this RCA.</param>
/// <param name="RootCauses">Root causes identified (serialised). Max 4000 characters.</param>
/// <param name="CorrectiveActions">Corrective actions recommended (serialised). Max 4000 characters.</param>
public sealed record CreateRootCauseAnalysisRequest(
    Guid   LinkedEventId,
    string LinkedEventType,
    string Methodology,
    Guid   LeadInvestigatorId,
    string RootCauses,
    string CorrectiveActions);
