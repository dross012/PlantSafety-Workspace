// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for the highest-severity uncovered hazard mappings across every module
//   at a site -- backs the "top 10 uncovered High-severity hazards" tile.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

/// <summary>Returns the <paramref name="Take"/> highest-severity uncovered NFPA hazard mappings across every module at a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to check coverage for; null means any site.</param>
/// <param name="Take">Maximum number of rows to return.</param>
public sealed record GetTopUncoveredNfpaHazardsQuery(
    Guid TenantId,
    Guid? SiteId,
    int Take) : IRequest<Result<IReadOnlyList<NfpaGapDto>>>;
