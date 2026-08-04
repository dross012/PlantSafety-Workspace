// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           GetJointResponsePlanByIncidentQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query backing the Active Joint Response Status dashboard.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;

/// <summary>Retrieves joint response plans coordinated for the given incident.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="IncidentId">Identity of the SafetyEvents incident.</param>
public sealed record GetJointResponsePlanByIncidentQuery(
    Guid TenantId,
    Guid IncidentId) : IRequest<Result<GetJointResponsePlanByIncidentResult>>;

/// <summary>Result of <see cref="GetJointResponsePlanByIncidentQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="IncidentId">Incident the plans relate to.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching joint response plans.</param>
public sealed record GetJointResponsePlanByIncidentResult(
    Guid TenantId,
    Guid IncidentId,
    DateTime GeneratedAt,
    IReadOnlyList<JointResponsePlanItemDto> Items);
