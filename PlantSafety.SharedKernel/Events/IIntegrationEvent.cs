// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Events
//  Layer:          SharedKernel
//  File:           IIntegrationEvent.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Contract for integration events published via the Outbox
//                  pattern to Azure Service Bus for cross-module communication.
//                  Naming convention: <Module>.<EventName> (e.g., Alerts.AlertCreated).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Events;

/// <summary>
/// Contract for integration events published to Azure Service Bus.
/// Integration events MUST follow the naming convention: <c>Module.EventName</c>.
/// They MUST be JSON-serializable sealed records.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>Canonical event name (e.g., Alerts.AlertCreated).</summary>
    string EventName { get; }

    /// <summary>Monotonically increasing schema version.</summary>
    int EventVersion { get; }

    /// <summary>Schema version tag for consumer compatibility (always "2026.1").</summary>
    string SchemaVersion { get; }

    /// <summary>Unique event identifier for idempotency deduplication.</summary>
    Guid EventId { get; }

    /// <summary>Tenant scope of this event.</summary>
    Guid TenantId { get; }

    /// <summary>Correlation identifier for distributed tracing.</summary>
    Guid CorrelationId { get; }

    /// <summary>Causation identifier (Id of the command that caused this event).</summary>
    Guid CausationId { get; }

    /// <summary>UTC instant at which the event was created.</summary>
    DateTime OccurredAtUtc { get; }
}
