// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the HumanRobotInteractionEvent read-model.
// SchemaVersion: 1

using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Models;

namespace PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Repositories;

/// <summary>Read-only repository for HumanRobotInteractionEvent data. Implemented in the Infrastructure layer.</summary>
public interface IHumanRobotInteractionEventRepository
{
    /// <summary>Returns HumanRobotInteractionEvent items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HumanRobotInteractionEventItemDto>> GetHumanRobotInteractionEventItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
