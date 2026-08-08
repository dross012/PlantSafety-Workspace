// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to search consensus-standards requirements by keyword and/or family, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Searches Compliance.StandardsRequirement by optional keyword/family filters, paged.</summary>
/// <param name="Keyword">Optional case-insensitive contains match against Title/Description.</param>
/// <param name="FamilyCode">Optional exact standards family code filter.</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record SearchStandardsRequirementsQuery(
    string? Keyword,
    string? FamilyCode,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<StandardsRequirementDto>>>;
