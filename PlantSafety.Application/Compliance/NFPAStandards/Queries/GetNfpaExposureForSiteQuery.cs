// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for per-module NFPA coverage exposure at a single site. Delegates to
//   INfpaAuditGapRepository.GetSiteExposureAsync, the same underlying read as
//   Dashboards.Queries.GetNfpaSiteExposureQuery -- kept as a separate, top-level query per this
//   module's own vocabulary (fact providers/consumers outside the Dashboards sub-feature reach
//   for "exposure for a site" without needing to know it also lives under Dashboards). Also
//   records a snapshot per module, mirroring GetOshaSiteExposureQuery's side effect.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Returns per-module NFPA coverage exposure for a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to compute exposure for.</param>
public sealed record GetNfpaExposureForSiteQuery(
    Guid TenantId,
    Guid SiteId) : IRequest<Result<IReadOnlyList<NfpaGapSummaryDto>>>;
