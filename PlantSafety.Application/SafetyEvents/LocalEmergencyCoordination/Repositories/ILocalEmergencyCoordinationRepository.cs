// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           ILocalEmergencyCoordinationRepository.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-only repository abstraction for the LocalEmergencyCoordination
//                  read-model. Implemented in the Infrastructure layer. Distinct from the
//                  Domain-layer PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.
//                  Interfaces.ILocalEmergencyCoordinationRepository (aggregate write-side
//                  repository) -- mirrors the EmergencyManagement precedent of two same-named
//                  interfaces in different namespaces.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Models;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

/// <summary>Read-only repository for LocalEmergencyCoordination data. Implemented in the Infrastructure layer.</summary>
public interface ILocalEmergencyCoordinationRepository
{
    /// <summary>
    /// Returns the LocalEmergencyCoordination case read-model record for the given identity,
    /// scoped to the given tenant, or <see langword="null"/> if not found.
    /// </summary>
    Task<LocalEmergencyCoordinationItemDto?> GetByIdAsync(Guid localEmergencyCoordinationId, Guid tenantId, CancellationToken cancellationToken);

    /// <summary>Returns mutual aid agreements for the tenant, optionally scoped to a single site -- backs the Mutual Aid Agreement Registry dashboard.</summary>
    Task<IReadOnlyList<MutualAidAgreementItemDto>> ListMutualAidAgreementsAsync(Guid tenantId, Guid? siteId, CancellationToken cancellationToken);

    /// <summary>Returns joint response plans coordinated for the given incident -- backs the Active Joint Response Status dashboard.</summary>
    Task<IReadOnlyList<JointResponsePlanItemDto>> GetJointResponsePlansByIncidentAsync(Guid tenantId, Guid incidentId, CancellationToken cancellationToken);

    /// <summary>Returns liaison communication log entries for the given incident.</summary>
    Task<IReadOnlyList<LiaisonLogEntryItemDto>> GetLiaisonLogByIncidentAsync(Guid tenantId, Guid incidentId, CancellationToken cancellationToken);

    /// <summary>Returns joint post-incident debrief records recorded within the given UTC date range -- backs the Post-Incident Debrief Summary dashboard.</summary>
    Task<IReadOnlyList<JointDebriefItemDto>> ListJointDebriefsByPeriodAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken cancellationToken);
}
