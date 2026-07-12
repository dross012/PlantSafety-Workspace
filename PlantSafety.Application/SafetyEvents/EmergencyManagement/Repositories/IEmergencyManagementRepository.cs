// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the EmergencyManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Repositories;

/// <summary>Read-only repository for EmergencyManagement data. Implemented in the Infrastructure layer.</summary>
public interface IEmergencyManagementRepository
{
    /// <summary>Returns EmergencyManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EmergencyManagementItemDto>> GetEmergenciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
