// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared result payload for Ppe lifecycle commands ("EmployeeSafety.Ppe").
// SchemaVersion: 1

namespace PlantSafety.Application.EmployeeSafety.Commands;

/// <summary>Result of a successful Ppe lifecycle command execution.</summary>
/// <param name="PpeId">Identity of the affected Ppe aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record PpeActionResult(
    Guid PpeId,
    string Status);