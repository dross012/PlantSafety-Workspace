// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Command to receive new stock into a PPEInventory item's on-hand balance
// ("PPE Inventory & Lifecycle Management" - inventory tracking).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Receives new stock into a PPEInventory item.</summary>
public sealed record ReceivePPEStockCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid PPEInventoryId,
    int Quantity,
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
