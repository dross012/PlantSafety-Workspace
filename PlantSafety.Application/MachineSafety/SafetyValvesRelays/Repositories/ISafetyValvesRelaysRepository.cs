// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the SafetyValvesRelays read-model.
// SchemaVersion: 1

using PlantSafety.Application.MachineSafety.SafetyValvesRelays.Models;

namespace PlantSafety.Application.MachineSafety.SafetyValvesRelays.Repositories;

/// <summary>Read-only repository for SafetyValvesRelays data. Implemented in the Infrastructure layer.</summary>
public interface ISafetyValvesRelaysRepository
{
    /// <summary>Returns SafetyValvesRelays items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SafetyValvesRelaysItemDto>> GetSafetyValvesRelaysItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}