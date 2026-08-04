// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Result payload for ListHumanRobotInteractionEventsQuery.
// SchemaVersion: 1

using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Models;

namespace PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Queries;

/// <summary>Result of <see cref="ListHumanRobotInteractionEventsQuery"/>.</summary>
/// <param name="TenantId">Tenant the data belongs to.</param>
/// <param name="SiteId">Site the data is scoped to, or null for tenant-wide results.</param>
/// <param name="GeneratedAt">UTC timestamp when this read-model snapshot was generated.</param>
/// <param name="Items">Matching HumanRobotInteractionEvent items.</param>
public sealed record HumanRobotInteractionEventQueryResult(
    Guid TenantId,
    Guid? SiteId,
    DateTime GeneratedAt,
    IReadOnlyList<HumanRobotInteractionEventItemDto> Items);
