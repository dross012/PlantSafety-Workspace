// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for aggregate NFPA coverage counts per module at a site -- the compact
//   form of GetNfpaGapsForSiteQuery, suited to a dashboard tile.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Queries;

/// <summary>Returns aggregate NFPA coverage counts per module at a site, optionally filtered to <paramref name="Module"/>.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to check coverage for; null means any site.</param>
/// <param name="Module">Optional PlantSafety module to filter to (e.g. "MachineSafety").</param>
public sealed record GetNfpaGapsSummaryForSiteQuery(
    Guid TenantId,
    Guid? SiteId,
    string? Module) : IRequest<Result<IReadOnlyList<NfpaGapSummaryDto>>>;
