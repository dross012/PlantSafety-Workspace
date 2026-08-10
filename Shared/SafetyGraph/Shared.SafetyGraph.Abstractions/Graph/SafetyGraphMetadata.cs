// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Graph
//  File:           SafetyGraphMetadata.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Safety Graph's module-scoped unified
//                  schema metadata envelope (Phase 12 precedent: no
//                  ContractBase exists in this program -- see Shared.
//                  Scheduling.SchedulingContractMetadata and Shared.
//                  PredictiveSafety.PredictiveSafetyContractMetadata, which
//                  established this exact deterministic-field-ordering,
//                  versioned-envelope pattern for Phases 14 and 15).
// ============================================================================
namespace Shared.SafetyGraph.Abstractions.Graph;

/// <summary>Safety Graph's module-scoped contract metadata: deterministic field ordering and versioning, tenant and correlation propagation.</summary>
public sealed record SafetyGraphMetadata(
    int SchemaVersion,
    string TenantId,
    string CorrelationId,
    DateTime IssuedAtUtc)
{
    /// <summary>The current SafetyGraphMetadata schema version. Increment only on a breaking field-shape change.</summary>
    public const int CurrentSchemaVersion = 1;
}
