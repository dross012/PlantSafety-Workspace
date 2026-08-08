// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to list EPA numeric threshold rules, optionally filtered by program
//   and/or parameter name.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Lists Compliance.EpaThresholdRule rows, optionally filtered by program and/or parameter name.</summary>
/// <param name="ProgramCode">Optional exact EPA program code filter.</param>
/// <param name="ParameterName">Optional exact parameter name filter.</param>
public sealed record ListEpaThresholdRulesQuery(
    string? ProgramCode,
    string? ParameterName) : IRequest<Result<IReadOnlyList<EpaThresholdRuleDto>>>;
