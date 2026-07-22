// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to trigger an on-demand retention purge for a single target,
//   or all 5 targets when Target is null. Runs the SAME already-scheduled, already-live
//   stored procedures early (see IArchivalRepository for the safety rationale).
// SchemaVersion: 1

using System.Collections.Generic;

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Models;

namespace PlantSafety.Application.Operations.Archival.Commands;

/// <summary>Triggers an on-demand retention purge.</summary>
/// <param name="Target">The single target to purge, or null to purge all 5 targets.</param>
public sealed record TriggerPurgeCommand(PurgeTarget? Target)
    : ICommand<Result<IReadOnlyList<PurgeTriggerResultDto>>>
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
    /// Placeholder: this operations-tooling command is not tenant-scoped; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;

    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: no dedicated actor field exists on this command; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;
}
