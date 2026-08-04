// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list CraneHoistSystems assets whose operator qualification expires within a window.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;

/// <summary>Lists CraneHoistSystems assets whose current operator qualification expires within <paramref name="WithinDays"/> days.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="WithinDays">Number of days from now within which the qualification expiry must fall.</param>
public sealed record ListExpiringOperatorQualificationsQuery(
    Guid TenantId,
    Guid? SiteId,
    int WithinDays) : IRequest<Result<IReadOnlyList<CraneHoistSystemsItemDto>>>;
