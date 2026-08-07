// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Command to record that an AuditFinding's evidence covers one or more
//   specific Compliance.OSHARequirement rows. This is the one real link between Auditing and
//   OSHA data -- without it, no accurate per-requirement gap detection is possible (see
//   AuditFindingOshaCoverage.sql's header for why a coarse module tag alone isn't enough).
//   Idempotent: re-linking an already-linked pair is a no-op, not an error.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.AuditFinding.OshaCoverage.Commands;

/// <summary>Links an AuditFinding to the specific OSHA requirements its evidence covers.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims; the finding must belong to this tenant.</param>
/// <param name="AuditFindingId">Identity of the finding.</param>
/// <param name="OSHARequirementIds">Identities of the OSHA requirements this finding covers.</param>
/// <param name="ActorId">Identity of the user performing the link.</param>
public sealed record LinkAuditFindingToOshaRequirementsCommand(
    Guid TenantId,
    Guid AuditFindingId,
    IReadOnlyCollection<Guid> OSHARequirementIds,
    Guid ActorId) : IRequest<Result<int>>;
