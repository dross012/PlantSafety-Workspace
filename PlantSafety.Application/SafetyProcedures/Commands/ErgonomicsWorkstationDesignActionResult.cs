// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for ErgonomicsWorkstationDesign lifecycle commands ("SafetyProcedures.ErgonomicsWorkstationDesign").
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Result of a successful ErgonomicsWorkstationDesign lifecycle command execution.</summary>
/// <param name="ErgonomicsWorkstationDesignId">Identity of the affected ErgonomicsWorkstationDesign aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record ErgonomicsWorkstationDesignActionResult(
    Guid ErgonomicsWorkstationDesignId,
    string Status);