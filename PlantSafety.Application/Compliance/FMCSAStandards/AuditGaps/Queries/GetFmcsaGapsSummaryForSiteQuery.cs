// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for aggregate FMCSA coverage counts per module (or a single module) at
//   a site -- the compact form of GetFmcsaGapsForSiteQuery, suited to a dashboard tile.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Queries;

/// <summary>Returns aggregate FMCSA coverage counts per module (or the single <paramref name="Module"/> when given) at a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to check coverage for; null means any site in the tenant.</param>
/// <param name="Module">PlantSafety module to check; null means every mapped module.</param>
public sealed record GetFmcsaGapsSummaryForSiteQuery(
    Guid TenantId,
    Guid? SiteId,
    string? Module) : IRequest<Result<IReadOnlyList<FmcsaGapSummaryDto>>>;
