// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Command to record that an AuditFinding's evidence covers one or more NFPA
//   hazard profiles. NFPA hazard guidance is voluntary reference data, not a regulatory
//   obligation -- this link exists so gap-detection dashboards can show which recommended
//   controls a site's audit evidence already addresses, mirroring
//   LinkAuditFindingToOshaRequirementsCommand's role for OSHA. Idempotent: re-linking an
//   already-linked pair is a no-op, not an error.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditFinding.NfpaCoverage.Commands;

/// <summary>Links an AuditFinding to the specific NFPA hazard profiles its evidence covers.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims; the finding must belong to this tenant.</param>
/// <param name="AuditFindingId">Identity of the finding.</param>
/// <param name="HazardIds">Identities of the NFPA hazard profiles this finding covers.</param>
/// <param name="CreatedBy">Display identity of the user performing the link.</param>
public sealed record LinkAuditFindingToNfpaHazardsCommand(
    Guid TenantId,
    Guid AuditFindingId,
    IReadOnlyCollection<Guid> HazardIds,
    string? CreatedBy) : IRequest<Result<int>>;
