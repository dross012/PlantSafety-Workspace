// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for recorded exposure snapshots over time -- backs the "trend over
//   time" tile. Only returns data from snapshots actually recorded by prior
//   GetFmcsaSiteExposureQuery/GetFmcsaTenantExposureQuery calls; there is no backfilled history.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Queries;

/// <summary>Returns recorded exposure snapshots for <paramref name="Module"/> between <paramref name="FromUtc"/> and <paramref name="ToUtc"/>.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Site to return the trend for; null returns the tenant-wide trend.</param>
/// <param name="Module">PlantSafety module to check (e.g. "Scheduling").</param>
/// <param name="FromUtc">Inclusive lower bound on SnapshotAtUtc.</param>
/// <param name="ToUtc">Inclusive upper bound on SnapshotAtUtc.</param>
public sealed record GetFmcsaExposureTrendQuery(
    Guid TenantId,
    Guid? SiteId,
    string Module,
    DateTime FromUtc,
    DateTime ToUtc) : IRequest<Result<IReadOnlyList<FmcsaExposureSnapshotDto>>>;
