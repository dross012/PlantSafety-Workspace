// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           ListIncidentReportsQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to list IncidentReport read-model data for a tenant, optionally scoped to
//                  a site, an incident-date range, and PHMSA reportability. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Queries;

/// <summary>
/// Lists IncidentReport data for the given tenant, optionally scoped to a single site, an
/// inclusive UTC incident-date range [<paramref name="FromUtc"/>, <paramref name="ToUtc"/>], and
/// PHMSA reportability.
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="FromUtc">Optional inclusive start of the incident-date range (UTC).</param>
/// <param name="ToUtc">Optional inclusive end of the incident-date range (UTC).</param>
/// <param name="IsReportableToPhmsa">Optional filter restricting results to (non-)reportable incidents.</param>
public sealed record ListIncidentReportsQuery(
    Guid TenantId,
    Guid? SiteId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    bool? IsReportableToPhmsa) : IRequest<Result<ListIncidentReportsResult>>;
