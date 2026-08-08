// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every EPA regulatory requirement for a given program, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Retrieves every EPA regulatory requirement for a given program, paged.</summary>
/// <param name="ProgramCode">The EPA program code to filter by (e.g. "RCRA").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record GetEpaRequirementsByProgramQuery(
    string ProgramCode,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<EpaRegulatoryRequirementDto>>>;
