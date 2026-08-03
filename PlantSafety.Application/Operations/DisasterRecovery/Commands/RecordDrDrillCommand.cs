// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command recording that a disaster-recovery drill was completed (Phase 7
//   §4.4 Disaster Recovery). No real multi-region failover infrastructure exists in this
//   sandbox to actually run a drill against -- this command is the platform's durable record
//   that a real drill happened (elsewhere, against real infrastructure), carrying its date,
//   scope, outcome, and any issues found. Modeled on RecordSecretRotationCommand (Phase 6 §6.3).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operations.DisasterRecovery.Commands;

/// <summary>
/// Records that a disaster-recovery drill was completed.
/// </summary>
/// <param name="Scope">
/// Human-readable description of what the drill exercised (e.g. "SQL Server Always On manual
/// failover, US East -> US West").
/// </param>
/// <param name="DrillDateUtc">UTC date/time the drill was performed.</param>
/// <param name="Outcome">The drill's outcome (e.g. "Passed", "Failed", "PartialSuccess").</param>
/// <param name="IssuesFound">Any issues identified during the drill (empty if none).</param>
public sealed record RecordDrDrillCommand(
    string Scope,
    DateTime DrillDateUtc,
    string Outcome,
    IReadOnlyList<string> IssuesFound) : ICommand<Result<Guid>>
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
    /// Placeholder: DR drills are platform-level, not tenant-scoped; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;

    /// <summary>Gets the identifier of the actor who logged the drill (the operator who ran or supervised it).</summary>
    public Guid ActorId { get; init; } = Guid.Empty;
}
