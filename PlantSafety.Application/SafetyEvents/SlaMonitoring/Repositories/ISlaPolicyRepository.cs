// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Read-side repository interface for SLA policy definitions. Implemented in
//   PlantSafety.Infrastructure (stored procedures, Dapper) against the new
//   [SafetyEvents].[IncidentSlaPolicy] table, seeded with a sensible default set (no admin
//   authoring UI exists yet for this genuinely new domain -- UI Gaps Scoping, IncidentManagement
//   Part 2).
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.SlaMonitoring.Repositories;

/// <summary>Read-side projection of a single persisted SLA policy row.</summary>
public sealed record SlaPolicyRow
{
    /// <summary>Severity tier this policy applies to.</summary>
    public required string Severity { get; init; }

    /// <summary>Target time (minutes) to first assignment/acknowledgement.</summary>
    public required int AcknowledgeWithinMinutes { get; init; }

    /// <summary>Target time (minutes) to resolution.</summary>
    public required int ResolveWithinMinutes { get; init; }
}

/// <summary>
/// Read-side repository for SLA policy definitions, keyed by severity. Tenant-scoped: a tenant
/// with no persisted rows falls back to <see cref="Domain.SafetyEvents.SlaMonitoring.ValueObjects.SlaPolicy.Default"/>.
/// </summary>
public interface ISlaPolicyRepository
{
    /// <summary>Returns every persisted SLA policy row for the tenant.</summary>
    Task<IReadOnlyList<SlaPolicyRow>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken);
}
