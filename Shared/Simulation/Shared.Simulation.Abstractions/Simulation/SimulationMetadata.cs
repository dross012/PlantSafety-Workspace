// ============================================================================
//  Project:        Shared.Simulation.Abstractions
//  Module:         Simulation
//  File:           SimulationMetadata.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Simulation's module-scoped unified
//                  schema metadata envelope (Phase 12 precedent: no
//                  ContractBase exists in this program -- see Shared.
//                  Scheduling.SchedulingContractMetadata, Shared.
//                  PredictiveSafety.PredictiveSafetyContractMetadata, and
//                  Shared.SafetyGraph.SafetyGraphMetadata, which established
//                  this exact deterministic-field-ordering, versioned-
//                  envelope pattern for Phases 14, 15, and 16).
// ============================================================================
namespace Shared.Simulation.Abstractions.Simulation;

/// <summary>Simulation's module-scoped contract metadata: deterministic field ordering and versioning, tenant and correlation propagation.</summary>
public sealed record SimulationMetadata(
    int SchemaVersion,
    string TenantId,
    string CorrelationId,
    DateTime IssuedAtUtc)
{
    /// <summary>The current SimulationMetadata schema version. Increment only on a breaking field-shape change.</summary>
    public const int CurrentSchemaVersion = 1;
}
