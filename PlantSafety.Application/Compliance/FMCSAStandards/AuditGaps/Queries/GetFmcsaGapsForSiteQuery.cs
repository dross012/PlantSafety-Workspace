// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to list FMCSA requirements for a module (or every mapped module) with
//   no covering AuditFinding at a site -- "show me FMCSA gaps for this site/operation".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.AuditGaps.Queries;

/// <summary>Lists FMCSA requirements mapped to <paramref name="Module"/> (or every mapped module when null) with no covering AuditFinding at the site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to check coverage for; null means any site in the tenant.</param>
/// <param name="Module">PlantSafety module to check (an AppliesToModule value, e.g. "Routing"); null means every mapped module.</param>
public sealed record GetFmcsaGapsForSiteQuery(
    Guid TenantId,
    Guid? SiteId,
    string? Module) : IRequest<Result<IReadOnlyList<FmcsaGapDto>>>;
