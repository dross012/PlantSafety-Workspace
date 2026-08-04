// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list CraneHoistSystems assets due (or overdue) for inspection as of a given UTC instant.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;

/// <summary>Lists CraneHoistSystems assets whose inspection is due (or overdue) as of <paramref name="AsOfUtc"/>.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="AsOfUtc">UTC instant to evaluate inspection due dates against.</param>
public sealed record ListAssetsDueForInspectionQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime AsOfUtc) : IRequest<Result<IReadOnlyList<CraneHoistSystemsItemDto>>>;
