// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           GetLiaisonLogByIncidentQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query retrieving the liaison communication log for a live incident.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;

/// <summary>Retrieves the liaison communication log for the given incident.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="IncidentId">Identity of the SafetyEvents incident.</param>
public sealed record GetLiaisonLogByIncidentQuery(
    Guid TenantId,
    Guid IncidentId) : IRequest<Result<GetLiaisonLogByIncidentResult>>;

/// <summary>Result of <see cref="GetLiaisonLogByIncidentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="IncidentId">Incident the log relates to.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching liaison log entries.</param>
public sealed record GetLiaisonLogByIncidentResult(
    Guid TenantId,
    Guid IncidentId,
    DateTime GeneratedAt,
    IReadOnlyList<LiaisonLogEntryItemDto> Items);
