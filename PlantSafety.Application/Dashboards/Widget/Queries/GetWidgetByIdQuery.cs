// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve a single Widget definition by identifier.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Models;

namespace PlantSafety.Application.Dashboards.Widget.Queries;

/// <summary>Retrieves a single Widget definition by identifier.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WidgetId">Identifier of the widget to retrieve.</param>
public sealed record GetWidgetByIdQuery(Guid TenantId, Guid WidgetId) : IRequest<Result<WidgetDto>>;
