// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for per-module NFPA coverage exposure at a single site -- backs the
//   "site-specific exposure" dashboard tile. Also records a snapshot per module as a side effect
//   (see NFPAExposureSnapshot.sql).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

/// <summary>Returns per-module NFPA coverage exposure for a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to compute exposure for.</param>
public sealed record GetNfpaSiteExposureQuery(
    Guid TenantId,
    Guid SiteId) : IRequest<Result<IReadOnlyList<NfpaGapSummaryDto>>>;
