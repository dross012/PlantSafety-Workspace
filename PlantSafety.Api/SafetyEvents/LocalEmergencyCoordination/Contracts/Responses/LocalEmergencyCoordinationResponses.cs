// ============================================================================
// File: LocalEmergencyCoordinationResponses.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.LocalEmergencyCoordination
// Created: 2026-08-04
// Description: External-facing response DTOs returned inside ApiResponse<T> for
//              LocalEmergencyCoordination queries. Mirrors EmergencyManagementResponse.
// ============================================================================

using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

namespace PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Contracts.Responses;

/// <summary>Response payload for ListMutualAidAgreements -- backs the Mutual Aid Agreement Registry dashboard.</summary>
public sealed record ListMutualAidAgreementsResponse(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<MutualAidAgreementItemDto> Items);

/// <summary>Response payload for GetJointResponsePlanByIncident -- backs the Active Joint Response Status dashboard.</summary>
public sealed record GetJointResponsePlanByIncidentResponse(
    Guid TenantId,
    Guid IncidentId,
    DateTime GeneratedAt,
    IReadOnlyList<JointResponsePlanItemDto> Items);

/// <summary>Response payload for GetLiaisonLogByIncident.</summary>
public sealed record GetLiaisonLogByIncidentResponse(
    Guid TenantId,
    Guid IncidentId,
    DateTime GeneratedAt,
    IReadOnlyList<LiaisonLogEntryItemDto> Items);

/// <summary>Response payload for ListJointDebriefsByPeriod -- backs the Post-Incident Debrief Summary dashboard.</summary>
public sealed record ListJointDebriefsByPeriodResponse(
    Guid TenantId,
    DateTime GeneratedAt,
    IReadOnlyList<JointDebriefItemDto> Items);
