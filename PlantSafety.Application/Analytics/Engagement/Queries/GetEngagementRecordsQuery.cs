// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           GetEngagementRecordsQuery.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve Engagement read-model data for a tenant/site within a date
//                  range. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsQuery. Distinct
//                  from the pre-existing, unrelated
//                  PlantSafety.Application.Analytics.Queries.GetEngagementQuery (generic
//                  engagement-metric time series backing EngagementV1Controller's existing GET
//                  action).
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Engagement.Queries;

/// <summary>
/// Retrieves Engagement data for the given tenant, optionally scoped to a single site,
/// within the inclusive UTC date range [<paramref name="From"/>, <paramref name="To"/>].
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record GetEngagementRecordsQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime From,
    DateTime To) : IRequest<Result<EngagementRecordsQueryResult>>;
