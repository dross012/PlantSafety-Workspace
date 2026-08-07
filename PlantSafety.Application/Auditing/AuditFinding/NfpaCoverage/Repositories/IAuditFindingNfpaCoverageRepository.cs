// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Application-layer repository abstraction for AuditFinding<->NFPA_HazardProfile
//   coverage links. Implemented in Infrastructure.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.AuditFinding.NfpaCoverage.Repositories;

/// <summary>Repository for AuditFinding-to-NFPA-hazard coverage links.</summary>
public interface IAuditFindingNfpaCoverageRepository
{
    /// <summary>
    /// Links <paramref name="auditFindingId"/> to <paramref name="hazardId"/>, scoped to
    /// <paramref name="tenantId"/> (the finding must belong to this tenant or nothing is
    /// linked). Already-linked pairs are a no-op, not a duplicate. Returns <see langword="true"/>
    /// when a new link row was created.
    /// </summary>
    Task<bool> LinkAsync(Guid tenantId, Guid auditFindingId, Guid hazardId, string? createdBy, CancellationToken cancellationToken);
}
