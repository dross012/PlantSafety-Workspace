// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for PPEInventory lifecycle commands (receive stock,
// issue units, flag for replacement, retire).
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Result of a successful PPEInventory lifecycle command execution.</summary>
/// <param name="PPEInventoryId">Identity of the affected PPEInventory aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
/// <param name="AvailableQuantity">Available (StockQuantity - IssuedQuantity) balance after the command was applied.</param>
public sealed record PPEInventoryActionResult(
    Guid PPEInventoryId,
    string Status,
    int AvailableQuantity);
