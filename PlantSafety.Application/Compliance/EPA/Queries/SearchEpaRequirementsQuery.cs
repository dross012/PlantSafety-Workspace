// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to search EPA regulatory requirements by keyword and/or program, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Searches Compliance.EpaRegulatoryRequirement by optional keyword/program filters, paged.</summary>
/// <param name="Keyword">Optional case-insensitive contains match against Title/Description/Citation.</param>
/// <param name="ProgramCode">Optional exact EPA program code filter.</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record SearchEpaRequirementsQuery(
    string? Keyword,
    string? ProgramCode,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<EpaRegulatoryRequirementDto>>>;
