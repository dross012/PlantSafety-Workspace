// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Control Measures (Hierarchy of Controls) read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Models;

namespace PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Repositories;

/// <summary>Read-only repository for Control Measures (Hierarchy of Controls) data. Implemented in the Infrastructure layer.</summary>
public interface IControlMeasuresHierarchyRepository
{
    /// <summary>Returns Control Measures (Hierarchy of Controls) items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ControlMeasuresHierarchyItemDto>> GetControlMeasuresHierarchyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}