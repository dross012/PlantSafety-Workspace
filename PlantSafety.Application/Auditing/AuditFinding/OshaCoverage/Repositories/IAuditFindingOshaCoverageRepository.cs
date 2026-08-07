// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Application-layer repository abstraction for AuditFinding<->OSHARequirement
//   coverage links. Implemented in Infrastructure.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.AuditFinding.OshaCoverage.Repositories;

/// <summary>Repository for AuditFinding-to-OSHARequirement coverage links.</summary>
public interface IAuditFindingOshaCoverageRepository
{
    /// <summary>
    /// Links <paramref name="auditFindingId"/> to each of <paramref name="oshaRequirementIds"/>,
    /// scoped to <paramref name="tenantId"/> (the finding must belong to this tenant or nothing
    /// is linked). Already-linked pairs are skipped, not duplicated. Returns the number of new
    /// links created.
    /// </summary>
    Task<int> LinkAsync(Guid tenantId, Guid auditFindingId, IReadOnlyCollection<Guid> oshaRequirementIds, Guid actorId, CancellationToken cancellationToken);
}
