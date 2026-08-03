// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to set (or clear) a single feature flag's canary-rollout
//   percentage (Phase 6.6, §6.2 Blue/Green + Canary + Tenant Rollout). A normal, safe,
//   reversible UPDATE, sibling to SetFeatureFlagEnabledCommand.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Commands;

/// <summary>Sets (or clears) a single feature flag's canary-rollout percentage in the given database.</summary>
/// <param name="DatabaseKey">The logical database key the flag lives in.</param>
/// <param name="FlagName">The flag to update.</param>
/// <param name="RolloutPercentage">The new rollout percentage (0-100), or null to clear it.</param>
public sealed record SetFeatureFlagRolloutPercentageCommand(string DatabaseKey, string FlagName, int? RolloutPercentage)
    : ICommand<Result<FeatureFlagDto>>
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
