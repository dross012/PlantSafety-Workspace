// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Events
//  Layer:          SharedKernel
//  File:           IntegrationEvent.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Abstract base record for all integration events.
//                  Concrete integration events MUST be sealed records.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Events;

/// <summary>
/// Abstract base for all integration events.
/// Concrete events MUST be <c>sealed record</c> types.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public abstract string EventName { get; }

    /// <inheritdoc/>
    public int EventVersion { get; init; } = 1;

    /// <inheritdoc/>
    public string SchemaVersion { get; init; } = "2026.1";

    /// <inheritdoc/>
    public Guid EventId { get; init; }

    /// <inheritdoc/>
    public Guid TenantId { get; init; }

    /// <inheritdoc/>
    public Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public Guid CausationId { get; init; }

    /// <inheritdoc/>
    public DateTime OccurredAtUtc { get; init; }

    protected IntegrationEvent(
        Guid eventId,
        Guid tenantId,
        Guid correlationId,
        Guid causationId,
        DateTime occurredAtUtc)
    {
        EventId       = eventId == Guid.Empty ? throw new ArgumentException("EventId must not be empty.", nameof(eventId)) : eventId;
        TenantId      = tenantId;
        CorrelationId = correlationId;
        CausationId   = causationId;
        OccurredAtUtc = occurredAtUtc.Kind == DateTimeKind.Utc ? occurredAtUtc : occurredAtUtc.ToUniversalTime();
    }
}
