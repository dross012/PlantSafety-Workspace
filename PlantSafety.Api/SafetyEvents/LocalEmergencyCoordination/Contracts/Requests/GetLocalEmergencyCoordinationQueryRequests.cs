// ============================================================================
// File: GetLocalEmergencyCoordinationQueryRequests.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.LocalEmergencyCoordination
// Created: 2026-08-04
// Description: External-facing query-string DTOs for the LocalEmergencyCoordination V1
//              controller's GET endpoints. Mirrors GetEmergencyManagementRequest.
// ============================================================================

namespace PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Contracts.Requests;

/// <summary>Query parameters for listing mutual aid agreements (Mutual Aid Agreement Registry dashboard).</summary>
/// <param name="SiteId">Optional site filter. Null returns agreements for all accessible sites.</param>
public sealed record ListMutualAidAgreementsRequest(Guid? SiteId);

/// <summary>Query parameters for listing joint post-incident debriefs (Post-Incident Debrief Summary dashboard).</summary>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC). Defaults to UtcNow when not supplied.</param>
public sealed record ListJointDebriefsByPeriodRequest(DateTime From, DateTime? To);
