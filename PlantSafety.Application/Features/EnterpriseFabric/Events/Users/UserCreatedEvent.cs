// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Integration event published after a new User account is created.
// EventType: EnterpriseFabric.UserCreated.v1
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;

namespace PlantSafety.Application.Features.EnterpriseFabric.Events.Users;

/// <summary>
/// Raised after a new user account is created within a tenant.
/// Consumers: Alerts (welcome notification), Operational (worker profile bootstrap).
/// </summary>
public sealed record UserCreatedEvent : IIntegrationEvent
{
    /// <inheritdoc/>
    public required Guid EventId { get; init; }

    /// <inheritdoc/>
    public string EventType => "EnterpriseFabric.UserCreated.v1";

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

    /// <summary>Email address of the new user.</summary>
    public required string Email { get; init; }

    /// <summary>Initial roles assigned.</summary>
    public required IReadOnlyList<string> Roles { get; init; }
}
