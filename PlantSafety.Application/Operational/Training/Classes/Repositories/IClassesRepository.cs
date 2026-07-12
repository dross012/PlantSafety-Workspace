// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Classes read-model.
// SchemaVersion: 1

using PlantSafety.Application.Operational.Training.Classes.Models;

namespace PlantSafety.Application.Operational.Training.Classes.Repositories;

/// <summary>Read-only repository for Classes data. Implemented in the Infrastructure layer.</summary>
public interface IClassesRepository
{
    /// <summary>Returns Classes items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ClassItemDto>> GetClassesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
