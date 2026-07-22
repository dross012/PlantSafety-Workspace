// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Command to run the SAFE, non-destructive restore-verify
//   (msdb.dbo.usp_PlantSafety_RestoreTest) against a database's latest FULL backup.
//   This is NOT a production restore — see DR-manual-restore-runbook.md for that
//   deliberately manual, human-run procedure.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Application.Operations.Backup.Commands;

/// <summary>Runs a safe, isolated restore-verify against <paramref name="DatabaseName"/>'s latest FULL backup.</summary>
/// <param name="DatabaseName">The PlantSafety.* database to restore-verify.</param>
public sealed record RunRestoreVerifyCommand(string DatabaseName) : ICommand<Result<RestoreVerifyResultDto>>
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
