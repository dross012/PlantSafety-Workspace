// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for per-module FMCSA coverage exposure at a single site. Delegates to
//   IFmcsaAuditGapRepository.GetSiteExposureAsync -- the same underlying read as
//   Dashboards.GetFmcsaSiteExposureQuery, exposed here under Compliance.FMCSAStandards per the
//   canonical query surface for this submodule. Also records a snapshot per module as a side
//   effect (inherited from the repository method).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Returns per-module FMCSA coverage exposure for a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to compute exposure for.</param>
public sealed record GetFmcsaExposureForSiteQuery(
    Guid TenantId,
    Guid SiteId) : IRequest<Result<IReadOnlyList<FmcsaGapSummaryDto>>>;
