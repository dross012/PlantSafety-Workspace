// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to reclassify an existing ControlChangeEvent as unauthorized, typically
//   issued after a manual change-management review.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Commands;

/// <summary>Flags an existing ControlChangeEvent as unauthorized.</summary>
public sealed record FlagUnauthorizedControlChangeCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    Guid ControlChangeEventId,
    Guid ActorId) : IRequest<Result<FlagUnauthorizedControlChangeResult>>;
