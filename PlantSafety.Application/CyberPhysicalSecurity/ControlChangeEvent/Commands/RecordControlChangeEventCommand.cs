// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record an OT control-system change observation, typically produced
//   by Infrastructure.Integrations SCADA/HMI/ICS telemetry ingestion.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Commands;

/// <summary>Records a new OT control-system change observation.</summary>
public sealed record RecordControlChangeEventCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid SiteId,
    Guid OtAssetId,
    string ChangeDescription,
    string ChangedParameter,
    string OldValue,
    string NewValue,
    DateTime DetectedAtUtc,
    Guid? ChangeSourceUserId,
    bool HasApprovedChangeRecord,
    Guid ActorId) : IRequest<Result<RecordControlChangeEventResult>>;
