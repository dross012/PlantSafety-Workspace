// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         Contracts
//  File:           SchedulingDispatcherContracts.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Pure,
//                  versioned, serializable dispatcher contract shapes --
//                  deliberately data-only. Phase 14's own scope explicitly
//                  excludes implementing a dispatcher (that is Phase 16 or
//                  17's responsibility); this file defines only the shape a
//                  future dispatcher will consume, mirroring how Phase 13's
//                  EventRoutingResult already carries NotificationRouting-
//                  shaped data without this program owning a notification
//                  transport. No dispatch semantics (retry policy, delivery
//                  guarantee, transport) are invented here.
// ============================================================================
namespace Shared.Scheduling.Abstractions.Contracts;

/// <summary>Versioning and correlation metadata for a scheduling dispatch contract.</summary>
public sealed record SchedulingDispatchMetadata(
    int ContractVersion,
    string CorrelationId,
    DateTime IssuedAtUtc)
{
    /// <summary>The current SchedulingDispatchMetadata contract version. Increment only on a breaking field-shape change.</summary>
    public const int CurrentContractVersion = 1;
}

/// <summary>The deterministic, pure-data shape of a single scheduling dispatch request. Carries no dispatch behavior.</summary>
public sealed record SchedulingDispatchRequest(
    string ActionCode,
    string Description,
    DateTime DueAtUtc,
    bool IsOverdue,
    string TenantId,
    Guid? FacilityId);

/// <summary>The pure-data contract a future dispatcher (Phase 16/17) will consume. Defines shape only -- performs no dispatching.</summary>
public interface ISchedulingDispatcherContract
{
    /// <summary>The dispatch request payload.</summary>
    SchedulingDispatchRequest Request { get; }

    /// <summary>The dispatch contract's versioning and correlation metadata.</summary>
    SchedulingDispatchMetadata Metadata { get; }
}

/// <summary>The concrete, immutable ISchedulingDispatcherContract implementation.</summary>
public sealed record SchedulingDispatchEnvelope(
    SchedulingDispatchRequest Request,
    SchedulingDispatchMetadata Metadata) : ISchedulingDispatcherContract;
