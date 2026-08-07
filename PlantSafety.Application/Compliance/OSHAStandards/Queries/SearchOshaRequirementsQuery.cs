// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to free-text search OSHA requirement notes.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Free-text searches Compliance.OSHARequirement.Notes (case-insensitive contains), paged.</summary>
/// <param name="Keyword">Text to search for (e.g. "fall protection").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record SearchOshaRequirementsQuery(
    string Keyword,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<OSHARequirementItemDto>>>;
