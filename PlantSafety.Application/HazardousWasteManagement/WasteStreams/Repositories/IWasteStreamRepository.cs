// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the WasteStream read-model.
// SchemaVersion: 1

using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Models;

namespace PlantSafety.Application.HazardousWasteManagement.WasteStreams.Repositories;

/// <summary>Read-only repository for WasteStream data. Implemented in the Infrastructure layer.</summary>
public interface IWasteStreamRepository
{
    /// <summary>Returns a single WasteStream item by identifier, or null if not found.</summary>
    Task<WasteStreamItemDto?> GetWasteStreamByIdAsync(Guid tenantId, Guid wasteStreamId, CancellationToken cancellationToken);
}
