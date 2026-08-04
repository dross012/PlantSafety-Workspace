// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query listing ControlChangeEvent records flagged unauthorized for a tenant.
//   Backs the Control-Change Audit Feed dashboard.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Queries;

/// <summary>Lists unauthorized ControlChangeEvent records for a tenant.</summary>
public sealed record ListUnauthorizedControlChangesQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<ControlChangeEventDto>>>;
