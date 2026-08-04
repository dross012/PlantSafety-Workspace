// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           ListJointDebriefsByPeriodQuery.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Query backing the Post-Incident Debrief Summary dashboard and the "joint
//                  debrief completion rate" KPI.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;

/// <summary>Retrieves joint post-incident debrief records recorded within the inclusive UTC date range [<paramref name="From"/>, <paramref name="To"/>].</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record ListJointDebriefsByPeriodQuery(
    Guid TenantId,
    DateTime From,
    DateTime To) : IRequest<Result<ListJointDebriefsByPeriodResult>>;

/// <summary>Result of <see cref="ListJointDebriefsByPeriodQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching joint debrief records.</param>
public sealed record ListJointDebriefsByPeriodResult(
    Guid TenantId,
    DateTime GeneratedAt,
    IReadOnlyList<JointDebriefItemDto> Items);
