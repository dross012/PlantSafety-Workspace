// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         Contracts
//  File:           SchedulingContracts.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Unified
//                  schema (Phase 12) request/result contracts for the
//                  Scheduling evaluation pipeline. Phase 12 established --
//                  and its own commit record documents -- that no
//                  ContractBase/ContractMetadata type exists in this
//                  program's Shared.Telemetry/Shared.Deterministic layers;
//                  the "unified schema" is the existing plain-record
//                  convention (LogEvent, WorkflowContext/WorkflowDecision)
//                  plus the real Shared.Authorization.Abstractions.Contracts.
//                  ContractMetadata type every downstream module (Phase
//                  9-13) already references. SchedulingContractMetadata
//                  below is Scheduling's own module-scoped metadata
//                  envelope, deliberately shaped the same way (deterministic
//                  field ordering: version, tenant, correlation, then
//                  timestamp) so it composes cleanly alongside the real
//                  ContractMetadata inside SchedulingOrchestrator.
// ============================================================================
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.WorkflowEngine.Abstractions.Actions;

namespace Shared.Scheduling.Abstractions.Contracts;

/// <summary>Scheduling's module-scoped contract metadata: deterministic field ordering and versioning, tenant and correlation propagation.</summary>
public sealed record SchedulingContractMetadata(
    int SchemaVersion,
    string TenantId,
    string CorrelationId,
    DateTime IssuedAtUtc)
{
    /// <summary>The current SchedulingContractMetadata schema version. Increment only on a breaking field-shape change.</summary>
    public const int CurrentSchemaVersion = 1;
}

/// <summary>The deterministic input contract to a scheduling evaluation.</summary>
public sealed record SchedulingEvaluationRequestContract(
    SchedulingContractMetadata Metadata,
    IReadOnlyList<RequiredAction> RequiredActions,
    int OverdueActionCount,
    int ActiveMachineGroupCount,
    DateTime AsOfUtc);

/// <summary>The deterministic output contract of a scheduling evaluation.</summary>
public sealed record SchedulingEvaluationResultContract(
    SchedulingContractMetadata Metadata,
    SchedulingSchedule Schedule,
    SchedulingDueTimeStatus Status);
