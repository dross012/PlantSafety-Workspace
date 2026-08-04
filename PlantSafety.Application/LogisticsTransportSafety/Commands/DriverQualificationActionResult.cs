// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for DriverQualification lifecycle commands ("LogisticsTransportSafety.DriverQualifications").
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Result of a successful DriverQualification lifecycle command execution.</summary>
/// <param name="DriverQualificationId">Identity of the affected DriverQualification aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record DriverQualificationActionResult(
    Guid DriverQualificationId,
    string Status);
