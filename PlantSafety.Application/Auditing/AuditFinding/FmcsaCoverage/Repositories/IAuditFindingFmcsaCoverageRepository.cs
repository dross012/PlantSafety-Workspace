// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Application-layer repository abstraction for AuditFinding<->FMCSARequirement
//   coverage links. Implemented in Infrastructure. usp_AuditFindingFmcsaCoverage_Link is
//   single-requirement-per-call (like NFPA's equivalent), so the caller
//   (LinkAuditFindingToFmcsaRequirementsCommandHandler) loops once per requirement id.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.AuditFinding.FmcsaCoverage.Repositories;

/// <summary>Repository for AuditFinding-to-FMCSARequirement coverage links.</summary>
public interface IAuditFindingFmcsaCoverageRepository
{
    /// <summary>
    /// Links <paramref name="auditFindingId"/> to <paramref name="requirementId"/>, scoped to
    /// <paramref name="tenantId"/> (the finding must belong to this tenant or nothing is linked).
    /// Already-linked pairs are skipped, not duplicated. Returns true when a new link was created.
    /// </summary>
    Task<bool> LinkAsync(Guid tenantId, Guid auditFindingId, Guid requirementId, Guid createdBy, CancellationToken cancellationToken);
}
