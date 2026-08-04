// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query listing active SabotageIndicators for a tenant. Backs the Active
//   Sabotage Indicator Board dashboard.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Queries;

/// <summary>Lists active SabotageIndicators for a tenant.</summary>
public sealed record ListActiveSabotageIndicatorsQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<SabotageIndicatorDto>>>;
