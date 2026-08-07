// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for aggregate OSHA coverage counts for a module at a site -- the
//   compact form of GetOshaGapsForSiteQuery, suited to a dashboard tile.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Queries;

/// <summary>Returns aggregate OSHA coverage counts for <paramref name="Module"/> at a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to check coverage for.</param>
/// <param name="Module">PlantSafety module to check (an AppliesToModule value, e.g. "MachineSafety").</param>
public sealed record GetOshaGapsSummaryForSiteQuery(
    Guid TenantId,
    Guid SiteId,
    string Module) : IRequest<Result<OshaGapSummaryDto>>;
