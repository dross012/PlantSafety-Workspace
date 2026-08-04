// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to list all currently open route obstructions for a tenant/site.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Models;

namespace PlantSafety.Application.DigitalTwin.EvacuationModeling.EvacuationRoutes.Queries;

/// <summary>Lists all currently open (uncleared) route obstructions for the tenant, optionally scoped to a site.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
public sealed record ListActiveObstructionsQuery(Guid TenantId, Guid? SiteId) : IRequest<Result<IReadOnlyList<RouteObstructionItemDto>>>;
