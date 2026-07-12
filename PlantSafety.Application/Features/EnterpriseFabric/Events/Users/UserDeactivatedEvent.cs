// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published when a user account is deactivated.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.EnterpriseFabric.Events.Users;

/// <summary>Published via outbox when a user account is deactivated.</summary>
public sealed record UserDeactivatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "EnterpriseFabric.UserDeactivated.v1";

    /// <inheritdoc/>
    public int EventVersion => 1;

    /// <inheritdoc/>
    public required Guid AggregateId { get; init; }

    /// <inheritdoc/>
    public string AggregateName => "UserProfile";

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public required Guid CausationId { get; init; }

    /// <inheritdoc/>
    public required DateTime TimestampUtc { get; init; }

    /// <inheritdoc/>
    public required Guid ActorId { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <summary>Deactivated user identifier.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Reason for deactivation.</summary>
    public required string? Reason { get; init; }

    /// <summary>Actor who deactivated the account.</summary>
    public required Guid DeactivatedByActorId { get; init; }

    /// <summary>UTC deactivation timestamp.</summary>
    public required DateTimeOffset DeactivatedAtUtc { get; init; }
}
