// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model DTOs for the ControlChangeEvent subdomain.
// SchemaVersion: 1

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;

/// <summary>Read-model DTO for a single ControlChangeEvent record.</summary>
public sealed record ControlChangeEventDto(
    Guid ControlChangeEventId,
    Guid TenantId,
    Guid SiteId,
    Guid OtAssetId,
    string ChangeDescription,
    string ChangedParameter,
    string OldValue,
    string NewValue,
    Guid? ChangeSourceUserId,
    string AuthorizationStatus,
    DateTime DetectedAtUtc,
    string RecordHash);

/// <summary>Result of a successful RecordControlChangeEventCommand.</summary>
public sealed record RecordControlChangeEventResult(Guid ControlChangeEventId, string AuthorizationStatus);

/// <summary>Result of a successful FlagUnauthorizedControlChangeCommand.</summary>
public sealed record FlagUnauthorizedControlChangeResult(Guid ControlChangeEventId, string AuthorizationStatus);
