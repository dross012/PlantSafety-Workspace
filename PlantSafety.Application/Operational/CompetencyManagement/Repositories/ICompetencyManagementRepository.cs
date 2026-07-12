// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the CompetencyManagement read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.CompetencyManagement.Models;

namespace PlantSafety.Application.Operational.CompetencyManagement.Repositories;

/// <summary>Read-only repository for CompetencyManagement data. Implemented in the Infrastructure layer.</summary>
public interface ICompetencyManagementRepository
{
    /// <summary>Returns CompetencyManagement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CompetencyManagementItemDto>> GetCompetencyManagementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
