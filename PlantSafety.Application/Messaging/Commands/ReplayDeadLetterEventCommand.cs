// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Command to replay a single dead-lettered event: re-publishes its original
//   payload back onto the message bus and marks the dead-letter row as replayed. See
//   IDeadLetterEventRepository.ReplayAsync for why the bus re-publish itself is implemented in
//   Infrastructure.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Messaging.Commands;

/// <summary>Replays a single dead-lettered event back onto the message bus.</summary>
/// <param name="DeadLetterId">The dead-letter row to replay.</param>
public sealed record ReplayDeadLetterEventCommand(Guid DeadLetterId)
    : ICommand<Result<bool>>
{
    /// <summary>Gets the unique identifier of this command instance (UUIDv7).</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <summary>Gets the correlation identifier that groups all operations in a single business transaction.</summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>Gets the causation identifier that identifies the immediate parent command or event.</summary>
    public Guid? CausationId { get; init; }

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the tenant identifier for multi-tenancy isolation.
    /// Placeholder: this operations-tooling command is not itself tenant-scoped (the dead-letter row's
    /// own TenantId, if any, is preserved unchanged on replay); defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;

    /// <summary>Gets the identifier of the actor (operator) who triggered the replay.</summary>
    public required Guid ActorId { get; init; }
}
