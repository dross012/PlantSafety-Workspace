// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           GetPermitsQuery.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve Permits read-model data for a tenant/site within a date
//                  range. Mirrors PlantSafety.Application.PermitToWork.PermitWorkflow.Queries.
//                  GetPermitWorkflowQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Permits.Queries;

/// <summary>
/// Retrieves Permits data for the given tenant, optionally scoped to a single site,
/// within the inclusive UTC date range [<paramref name="From"/>, <paramref name="To"/>].
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record GetPermitsQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime From,
    DateTime To) : IRequest<Result<PermitsQueryResult>>;
