// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve the most recently recorded lift event for a CraneHoistSystems asset.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.CraneHoistSystems.Models;

namespace PlantSafety.Application.MachineSafety.CraneHoistSystems.Queries;

/// <summary>
/// Retrieves the lift-event history for a single asset. The current read model persists only the
/// most recently observed lift (see <see cref="CraneHoistSystemsItemDto.LastObservedLoadKg"/>);
/// this query returns that single most-recent entry as a list to preserve a stable multi-item
/// contract for future extension to full per-lift event history.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="CraneHoistSystemsId">Identity of the asset whose lift events are requested.</param>
public sealed record ListLiftEventsByAssetQuery(
    Guid TenantId,
    Guid CraneHoistSystemsId) : IRequest<Result<IReadOnlyList<CraneHoistSystemsItemDto>>>;
