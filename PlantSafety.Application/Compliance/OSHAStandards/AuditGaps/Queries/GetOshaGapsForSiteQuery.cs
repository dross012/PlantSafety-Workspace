// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to list OSHA requirements for a module with no covering AuditFinding
//   at a site -- "show me OSHA gaps for this site/operation".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.AuditGaps.Queries;

/// <summary>Lists OSHA requirements mapped to <paramref name="Module"/> with no covering AuditFinding at the site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to check coverage for.</param>
/// <param name="Module">PlantSafety module to check (an AppliesToModule value, e.g. "MachineSafety").</param>
public sealed record GetOshaGapsForSiteQuery(
    Guid TenantId,
    Guid SiteId,
    string Module) : IRequest<Result<IReadOnlyList<OshaGapDto>>>;
