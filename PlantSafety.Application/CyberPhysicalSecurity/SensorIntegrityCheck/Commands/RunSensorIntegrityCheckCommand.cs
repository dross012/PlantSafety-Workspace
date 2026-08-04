// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record a sensor-integrity check result, either from a manual
//   analyst-initiated check or the recurring sensor integrity check scheduling hook.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Commands;

/// <summary>Records a completed sensor-integrity check.</summary>
public sealed record RunSensorIntegrityCheckCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SiteId,
    Guid OtAssetId,
    string CheckType,
    string Result,
    string Details,
    Guid ActorId) : IRequest<Result<RunSensorIntegrityCheckResult>>;
