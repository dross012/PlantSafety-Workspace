// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to search NIOSH hazard profiles by keyword, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Searches Compliance.NIOSHHazardProfile by an optional keyword filter, paged.</summary>
/// <param name="Keyword">Optional case-insensitive contains match against ChemicalName/CASNumber.</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record SearchNioshHazardsQuery(
    string? Keyword,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<NioshHazardProfileDto>>>;
