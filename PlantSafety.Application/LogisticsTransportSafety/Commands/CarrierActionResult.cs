// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for Carrier lifecycle commands ("LogisticsTransportSafety.Carriers").
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Result of a successful Carrier lifecycle command execution.</summary>
/// <param name="CarrierId">Identity of the affected Carrier aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record CarrierActionResult(
    Guid CarrierId,
    string Status);
