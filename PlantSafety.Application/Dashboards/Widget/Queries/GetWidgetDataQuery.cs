// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to resolve a single Widget's normalized data contract, per "Dashboard
//   API Queries: Widgets call GET /dashboard/{id}/widgets or GET /widget/{id}/data.".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Models;

namespace PlantSafety.Application.Dashboards.Widget.Queries;

/// <summary>Resolves the normalized data contract (Series/Categories/Metrics) for a single widget instance.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WidgetId">Identifier of the widget to resolve data for.</param>
/// <param name="FilterOverrides">Optional filter overrides (time range, plant, line, department, shift) applied for this resolution.</param>
public sealed record GetWidgetDataQuery(
    Guid TenantId,
    Guid WidgetId,
    IReadOnlyDictionary<string, string>? FilterOverrides) : IRequest<Result<WidgetDataContractDto>>;
