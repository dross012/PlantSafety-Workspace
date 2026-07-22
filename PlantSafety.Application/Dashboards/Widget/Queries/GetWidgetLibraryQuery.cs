// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve the full reusable widget library, per "5. Widget Library
//   (Reusable Components)".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Widget.Models;

namespace PlantSafety.Application.Dashboards.Widget.Queries;

/// <summary>Retrieves the full reusable widget library (all Active widgets) for a tenant.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
public sealed record GetWidgetLibraryQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<WidgetDto>>>;
