// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to retrieve the full detail for a single SafetyProgram, including a
//   live composed rollup of related SafetyPolicies/SafetyProcedures/SafetyCulture activity.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPrograms.Models;

namespace PlantSafety.Application.SafetyPrograms.Queries;

/// <summary>Retrieves the full detail for a single SafetyProgram.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SafetyProgramId">Identity of the program to retrieve.</param>
public sealed record GetSafetyProgramDetailQuery(
    Guid TenantId,
    Guid SafetyProgramId) : IRequest<Result<SafetyProgramDetailDto>>;
