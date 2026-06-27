// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical marker interfaces for Commands in the CQRS pipeline.
//   Commands mutate state; they must never be used for queries.
//   Every command carries the full correlation lineage required by the Canonical Lineage Rule (§2.17).
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no fields may be removed or renamed.

using MediatR;

namespace PlantSafety.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for commands that return a typed result.
/// Commands mutate state and flow through the full MediatR pipeline:
/// Logging → Performance → Validation → Transaction → Handler.
/// </summary>
/// <typeparam name="TResponse">The strongly typed result produced by the command handler.</typeparam>
public interface ICommand<TResponse> : IRequest<TResponse>
{
    /// <summary>Gets the unique identifier of this command instance (UUIDv7).</summary>
    Guid CommandId { get; }

    /// <summary>Gets the correlation identifier that groups all operations in a single business transaction.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the causation identifier that identifies the immediate parent command or event.</summary>
    Guid? CausationId { get; }

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    DateTime RequestedAtUtc { get; }

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    string ApiVersionToken { get; }

    /// <summary>Gets the tenant identifier for multi-tenancy isolation.</summary>
    Guid TenantId { get; }

    /// <summary>Gets the identifier of the actor who initiated the command (User, System, Workflow, Sensor, or External).</summary>
    Guid ActorId { get; }
}

/// <summary>
/// Marker interface for commands that return no typed result (fire-and-forget pattern).
/// Always prefer <see cref="ICommand{TResponse}"/> with a <see cref="Result"/> when possible.
/// </summary>
public interface ICommand : IRequest
{
    /// <summary>Gets the unique identifier of this command instance (UUIDv7).</summary>
    Guid CommandId { get; }

    /// <summary>Gets the correlation identifier that groups all operations in a single business transaction.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the causation identifier that identifies the immediate parent command or event.</summary>
    Guid? CausationId { get; }

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    DateTime RequestedAtUtc { get; }

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    string ApiVersionToken { get; }

    /// <summary>Gets the tenant identifier for multi-tenancy isolation.</summary>
    Guid TenantId { get; }

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    Guid ActorId { get; }
}
