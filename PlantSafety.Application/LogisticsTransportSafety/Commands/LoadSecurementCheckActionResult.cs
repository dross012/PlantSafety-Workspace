// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payload for LoadSecurementCheck lifecycle commands ("LogisticsTransportSafety.LoadSecurementChecks").
// SchemaVersion: 1

namespace PlantSafety.Application.LogisticsTransportSafety.Commands;

/// <summary>Result of a successful LoadSecurementCheck lifecycle command execution.</summary>
/// <param name="LoadSecurementCheckId">Identity of the affected LoadSecurementCheck aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record LoadSecurementCheckActionResult(
    Guid LoadSecurementCheckId,
    string Status);
