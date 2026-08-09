// ============================================================================
//  Project:        AI.EnterpriseOrchestration
//  Module:         Abstractions.Orchestration
//  File:           EnterpriseOrchestrationInput.cs
//  Created:        2026-08-08
//  Description:    Phase E.9: the input EnterpriseOrchestrationEngine.Orchestrate
//                  reasons over -- every facility's orchestration context
//                  participating in this enterprise-wide session.
// ============================================================================
namespace AI.EnterpriseOrchestration.Abstractions.Orchestration;

/// <summary>The input an enterprise-orchestration session reasons over.</summary>
public sealed record EnterpriseOrchestrationInput(
    IReadOnlyList<FacilityOrchestrationContext> Facilities);
