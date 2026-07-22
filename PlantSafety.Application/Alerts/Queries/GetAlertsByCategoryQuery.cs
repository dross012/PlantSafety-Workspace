// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve Alerts within a given catalog category.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Queries;

/// <summary>Retrieves Alerts within a given catalog category, optionally scoped to a single site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="CategoryCode">Catalog category code (e.g. "LifeSafetyEmergency").</param>
public sealed record GetAlertsByCategoryQuery(Guid TenantId, Guid? SiteId, string CategoryCode) : IRequest<Result<GetAlertsByCategoryResult>>;

/// <summary>Result of <see cref="GetAlertsByCategoryQuery"/>.</summary>
public sealed record GetAlertsByCategoryResult(Guid TenantId, string CategoryCode, DateTime GeneratedAtUtc, IReadOnlyList<AlertDto> Items);
