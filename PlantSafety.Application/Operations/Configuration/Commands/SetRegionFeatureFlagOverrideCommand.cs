// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to create/update a region-level feature flag override
//   (Phase 7 §7.1) -- e.g. a feature only live in EU first. A normal, reversible upsert.
// SchemaVersion: 1

using System;

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Models;

namespace PlantSafety.Application.Operations.Configuration.Commands;

/// <summary>Creates or updates a region-level feature flag override.</summary>
/// <param name="Region">The region the override applies to (e.g. "EU", "US", "APAC").</param>
/// <param name="FeatureName">The feature flag name.</param>
/// <param name="Enabled">Whether the feature is enabled in this region.</param>
/// <param name="RolloutPercentage">Optional region-scoped rollout percentage (0-100), or null to clear it.</param>
public sealed record SetRegionFeatureFlagOverrideCommand(
    string Region,
    string FeatureName,
    bool Enabled,
    int? RolloutPercentage) : ICommand<Result<RegionFeatureFlagOverrideDto>>
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
