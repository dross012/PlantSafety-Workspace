// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query retrieving the sensor-integrity check history for a given OT asset.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Queries;

/// <summary>Retrieves the sensor-integrity check history for a given OT asset.</summary>
public sealed record GetSensorIntegrityHistoryQuery(Guid TenantId, Guid OtAssetId) : IRequest<Result<IReadOnlyList<SensorIntegrityCheckDto>>>;
