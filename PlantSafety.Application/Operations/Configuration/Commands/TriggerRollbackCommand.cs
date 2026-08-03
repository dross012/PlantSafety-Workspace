// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to execute a real, code-level feature-flag rollback (Phase 7
//   §7.2) -- reverts a feature's canary rollout percentage to 0 across the global flag and
//   every tenant/region override, and optionally hard-disables the global flag outright.
//   A full container/infra rollback needs real deploy infrastructure not available here;
//   this is the feature-flag-level rollback that IS real and executable in this codebase.
// SchemaVersion: 1

using System;

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Commands;

/// <summary>Executes a real feature-flag rollback for a bad release.</summary>
/// <param name="DatabaseKey">The logical database key the global flag lives in.</param>
/// <param name="FeatureName">The feature flag to roll back.</param>
/// <param name="Reason">The reason for the rollback (e.g. "Error rate 8.4% exceeded 2% threshold over 500 requests").</param>
/// <param name="HardDisable">
/// When <see langword="true"/>, also forces the global flag's IsEnabled to false (a full
/// kill), in addition to resetting every rollout percentage to 0. Defaults to
/// <see langword="false"/> (percentage-only revert, matching a typical canary rollback that
/// keeps the flag available to whichever cohort was already fully enabled before the canary
/// began).
/// </param>
public sealed record TriggerRollbackCommand(
    string DatabaseKey,
    string FeatureName,
    string Reason,
    bool HardDisable = false) : ICommand<Result<RollbackResultDto>>
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
    /// Placeholder: this ops-tooling command is not tenant-scoped; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;

    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: no dedicated actor field exists on this command; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;
}
