// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Command to record that an AuditFinding's evidence covers one or more
//   specific Compliance.FMCSARequirement rows -- the one real link between Auditing and FMCSA
//   data, mirroring LinkAuditFindingToOshaRequirementsCommand's role for OSHA. Idempotent:
//   re-linking an already-linked pair is a no-op, not an error.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditFinding.FmcsaCoverage.Commands;

/// <summary>Links an AuditFinding to the specific FMCSA requirements its evidence covers.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims; the finding must belong to this tenant.</param>
/// <param name="AuditFindingId">Identity of the finding.</param>
/// <param name="RequirementIds">Identities of the FMCSA requirements this finding covers.</param>
/// <param name="ActorId">Identity of the user performing the link.</param>
public sealed record LinkAuditFindingToFmcsaRequirementsCommand(
    Guid TenantId,
    Guid AuditFindingId,
    IReadOnlyCollection<Guid> RequirementIds,
    Guid ActorId) : IRequest<Result<int>>;
