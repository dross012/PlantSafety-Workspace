// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every consensus-standards requirement for a given family, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Retrieves every consensus-standards requirement for a given family, paged.</summary>
/// <param name="FamilyCode">The standards family code to filter by (e.g. "ISO").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record GetStandardsByFamilyQuery(
    string FamilyCode,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<StandardsRequirementDto>>>;
