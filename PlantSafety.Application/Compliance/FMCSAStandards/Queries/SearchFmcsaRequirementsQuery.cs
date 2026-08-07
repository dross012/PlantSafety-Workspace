// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to free-text search FMCSA requirement text.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Free-text searches Compliance.FMCSARequirement.RequirementText (case-insensitive contains), paged.</summary>
/// <param name="Keyword">Text to search for (e.g. "hours of service").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record SearchFmcsaRequirementsQuery(
    string Keyword,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<FmcsaRequirementDto>>>;
