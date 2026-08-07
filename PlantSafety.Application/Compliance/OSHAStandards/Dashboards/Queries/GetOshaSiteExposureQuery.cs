// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for per-module coverage exposure at a single site -- backs the
//   "site-specific exposure" tile. Also records a snapshot per module (see
//   OSHAExposureSnapshot.sql).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Dashboards.Queries;

/// <summary>Returns per-module coverage exposure for a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to compute exposure for.</param>
public sealed record GetOshaSiteExposureQuery(
    Guid TenantId,
    Guid SiteId) : IRequest<Result<IReadOnlyList<OshaGapSummaryDto>>>;
