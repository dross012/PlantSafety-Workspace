// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Layer:          Cross-module contracts
//  File:           IIntegrationEventContract.cs
//  Created:        2026-07-27
//  Description:    Marker contract implemented by every versioned, plain-DTO cross-module
//                  integration event payload published through Shared.EventBus. Payload
//                  classes here are the STABLE WIRE CONTRACT consumed by other modules
//                  (Reporting, etc.) -- they intentionally do NOT reference PlantSafety.Domain
//                  value objects (StrongId/UtcTimestamp/PerformedByActor/etc.), so that
//                  consuming modules never take a compile-time dependency on PlantSafety's
//                  internal domain types. No EF, no ORM coupling -- plain immutable records.
//
//                  Naming convention (per project_plantsafety_patterns.md §4/§14):
//                  EventType = "<Module>.<EventName>" (e.g. "Incidents.IncidentReported").
//                  Version is NOT encoded in the type name; it lives in SchemaVersion below
//                  and is carried at runtime in EventEnvelope.SchemaVersion.
// ============================================================================
namespace Shared.DomainEvents.Abstractions;

/// <summary>
/// Implemented by every cross-module integration event DTO. Provides the canonical
/// <c>&lt;Module&gt;.&lt;EventName&gt;</c> event type string and the DTO's current schema version.
/// </summary>
public interface IIntegrationEventContract
{
    /// <summary>Canonical event type in the form <c>&lt;Module&gt;.&lt;EventName&gt;</c>.</summary>
    static abstract string EventType { get; }

    /// <summary>Schema version of this DTO shape (1-based; increment on breaking change).</summary>
    static abstract int SchemaVersion { get; }
}
