// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Shared result payloads for PhysicalAccessControl lifecycle commands, one
//   per entity ("PhysicalAccessControl.<Entity>").
// SchemaVersion: 1

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Result of a successful AccessPoint lifecycle command execution.</summary>
/// <param name="AccessPointId">Identity of the affected AccessPoint aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record AccessPointActionResult(Guid AccessPointId, string Status);

/// <summary>Result of a successful Credential lifecycle command execution.</summary>
/// <param name="CredentialId">Identity of the affected Credential aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record CredentialActionResult(Guid CredentialId, string Status);

/// <summary>Result of a successful AccessEvent lifecycle command execution.</summary>
/// <param name="AccessEventId">Identity of the affected AccessEvent aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record AccessEventActionResult(Guid AccessEventId, string Status);

/// <summary>Result of a successful SafetyZone lifecycle command execution.</summary>
/// <param name="SafetyZoneId">Identity of the affected SafetyZone aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record SafetyZoneActionResult(Guid SafetyZoneId, string Status);

/// <summary>Result of a successful AccessAnomaly lifecycle command execution.</summary>
/// <param name="AccessAnomalyId">Identity of the affected AccessAnomaly aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record AccessAnomalyActionResult(Guid AccessAnomalyId, string Status);

/// <summary>Result of a successful RestrictedZonePolicy lifecycle command execution.</summary>
/// <param name="RestrictedZonePolicyId">Identity of the affected RestrictedZonePolicy aggregate.</param>
/// <param name="Status">Lifecycle status of the record after the command was applied.</param>
public sealed record RestrictedZonePolicyActionResult(Guid RestrictedZonePolicyId, string Status);
