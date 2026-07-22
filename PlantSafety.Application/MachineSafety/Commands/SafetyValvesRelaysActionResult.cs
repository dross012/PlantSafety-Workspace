// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for SafetyValvesRelays lifecycle commands ("MachineSafety.SafetyValvesRelays").
// SchemaVersion: 1

namespace PlantSafety.Application.MachineSafety.Commands;

/// <summary>Result of a successful SafetyValvesRelays lifecycle command execution.</summary>
/// <param name="SafetyValvesRelaysId">Identity of the affected SafetyValvesRelays aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record SafetyValvesRelaysActionResult(
    Guid SafetyValvesRelaysId,
    string Status);