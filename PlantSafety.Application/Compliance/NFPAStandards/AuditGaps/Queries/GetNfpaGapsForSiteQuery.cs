// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to list NFPA hazard mappings with no covering AuditFinding at a site --
//   "show me NFPA gaps for this site/operation".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.AuditGaps.Queries;

/// <summary>Lists NFPA hazard mappings with no covering AuditFinding at the site, optionally filtered to <paramref name="Module"/>.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to check coverage for; null means any site.</param>
/// <param name="Module">Optional PlantSafety module to filter to (e.g. "MachineSafety").</param>
public sealed record GetNfpaGapsForSiteQuery(
    Guid TenantId,
    Guid? SiteId,
    string? Module) : IRequest<Result<IReadOnlyList<NfpaGapDto>>>;
