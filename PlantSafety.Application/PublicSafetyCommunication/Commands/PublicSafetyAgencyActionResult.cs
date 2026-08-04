// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for PublicSafetyAgency directory commands (registration,
// verification, deactivation).
// SchemaVersion: 1

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Result of a successful PublicSafetyAgency command execution.</summary>
/// <param name="PublicSafetyAgencyId">Identity of the affected PublicSafetyAgency aggregate.</param>
/// <param name="Status">Operational status of the record after the command was applied.</param>
public sealed record PublicSafetyAgencyActionResult(
    Guid PublicSafetyAgencyId,
    string Status);
