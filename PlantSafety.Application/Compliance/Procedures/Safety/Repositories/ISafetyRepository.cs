// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Repository abstraction for the Safety read-model.
// SchemaVersion: 1

using PlantSafety.Application.Compliance.Procedures.Safety.Models;

namespace PlantSafety.Application.Compliance.Procedures.Safety.Repositories;

/// <summary>Read-only repository for Safety data. Implemented in the Infrastructure layer.</summary>
public interface ISafetyRepository
{
    /// <summary>Returns Safety items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SafetyItemDto>> GetSafetyProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
