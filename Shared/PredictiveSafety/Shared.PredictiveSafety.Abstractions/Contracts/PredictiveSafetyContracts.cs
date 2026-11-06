// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Contracts
//  File:           PredictiveSafetyContracts.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Unified schema (Phase 12 precedent:
//                  no ContractBase/UnifiedEventBase exists in this program --
//                  see Phase 12's own commit record and Shared.Scheduling's
//                  SchedulingContracts.cs, which established this same
//                  module-scoped-metadata-envelope pattern for Phase 14)
//                  request/result contracts for the Predictive Safety
//                  evaluation pipeline. The request contract carries raw
//                  cross-module inputs -- the real EventRoutingResult (Phase
//                  13), MachineMaintenanceProfile (Phase C.2), and
//                  SchedulingRuleDecision (Phase 14) -- rather than
//                  pre-computed domain-to-domain flags, because those flags
//                  (HazardPredictionRequiresEscalation, etc.) can only be
//                  derived DURING evaluation, in PredictiveSafetyEvaluationPipeline's
//                  fixed order; the caller cannot supply them upfront.
// ============================================================================
using Shared.MachineDomain.Abstractions.Maintenance;
using Shared.PredictiveSafety.Abstractions.Audit;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Hazard;
using Shared.PredictiveSafety.Abstractions.Maintenance;
using Shared.PredictiveSafety.Abstractions.Routing;
using Shared.PredictiveSafety.Abstractions.Scheduling;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.Scheduling.Abstractions.Rules;

namespace Shared.PredictiveSafety.Abstractions.Contracts;

/// <summary>Predictive Safety's module-scoped contract metadata: deterministic field ordering and versioning, tenant and correlation propagation.</summary>
public sealed record PredictiveSafetyContractMetadata(
    int SchemaVersion,
    string TenantId,
    string CorrelationId,
    DateTime IssuedAtUtc)
{
    /// <summary>The current PredictiveSafetyContractMetadata schema version. Increment only on a breaking field-shape change.</summary>
    public const int CurrentSchemaVersion = 1;
}

/// <summary>The deterministic, raw cross-module input contract to a Predictive Safety evaluation.</summary>
public sealed record PredictiveSafetyEvaluationRequestContract(
    PredictiveSafetyContractMetadata Metadata,
    string EventType,
    string? EventDescription,
    int EmployeeExposureCount,
    int MachineFailureCount,
    int EventChainLength,
    EventRoutingResult? RoutingResult,
    Guid MachineId,
    MachineMaintenanceProfile? MaintenanceProfile,
    int RecentMachineFailureCount,
    SchedulingRuleDecision? SchedulingDecision,
    int RecentReliabilityFailureCount,
    int TotalLogEventCount,
    int WarningOrAboveLogEventCount,
    int DistinctEventIdCount,
    int GovernanceViolationCount,
    DateTime AsOfUtc);

/// <summary>The deterministic, composed output contract of a Predictive Safety evaluation.</summary>
public sealed record PredictiveSafetyEvaluationResultContract(
    PredictiveSafetyContractMetadata Metadata,
    PredictiveHazardDecision Hazard,
    PredictiveMaintenanceDecision Maintenance,
    PredictiveRoutingDecision Routing,
    PredictiveSchedulingDecision Scheduling,
    PredictiveAuditAnomaly Audit,
    PredictivePriority OverallPriority);
