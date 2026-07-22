// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to issue units of PPE stock out of a PPEInventory item's on-hand
// balance, triggering low-stock reorder alerts when applicable.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Issues units of PPE out of a PPEInventory item's on-hand stock.</summary>
/// <param name="ReorderThreshold">Available balance at or below which the item is flagged LowStock ("Automated reorder alerts").</param>
public sealed record IssuePPEUnitsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PPEInventoryId,
    int Quantity,
    int ReorderThreshold,
    Guid PerformedByUserId) : ICommand<Result<PPEInventoryActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PPEInventoryActionResult>>.CausationId => CausationId;
}
