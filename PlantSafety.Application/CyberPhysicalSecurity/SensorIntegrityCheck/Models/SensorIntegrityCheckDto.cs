// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTOs for the SensorIntegrityCheck subdomain.
// SchemaVersion: 1

namespace PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Models;

/// <summary>Read-model DTO for a single SensorIntegrityCheck record.</summary>
public sealed record SensorIntegrityCheckDto(
    Guid SensorIntegrityCheckId,
    Guid TenantId,
    Guid SiteId,
    Guid OtAssetId,
    string CheckType,
    string Result,
    string Details,
    DateTime CheckedAtUtc,
    DateTime? NextCheckDueUtc);

/// <summary>Result of a successful RunSensorIntegrityCheckCommand.</summary>
public sealed record RunSensorIntegrityCheckResult(Guid SensorIntegrityCheckId, string Result);
