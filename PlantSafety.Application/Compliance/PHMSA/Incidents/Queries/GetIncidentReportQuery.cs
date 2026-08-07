// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           GetIncidentReportQuery.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Query to retrieve a single IncidentReport read-model record by identity,
//                  including its current ETag, so a caller can read-before-write into one of the
//                  lifecycle commands. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Queries.GetPermitsByIdQuery.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Models;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Queries;

/// <summary>Retrieves a single IncidentReport read-model record by identity.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="IncidentReportId">Identity of the IncidentReport record to retrieve.</param>
public sealed record GetIncidentReportQuery(
    Guid TenantId,
    Guid IncidentReportId) : IRequest<Result<IncidentReportDto>>;
