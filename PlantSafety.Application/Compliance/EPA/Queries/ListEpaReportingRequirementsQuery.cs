// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to list EPA reporting requirements, optionally filtered by program.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Lists Compliance.EpaReportingRequirement rows, optionally filtered by program.</summary>
/// <param name="ProgramCode">Optional exact EPA program code filter.</param>
public sealed record ListEpaReportingRequirementsQuery(
    string? ProgramCode) : IRequest<Result<IReadOnlyList<EpaReportingRequirementDto>>>;
