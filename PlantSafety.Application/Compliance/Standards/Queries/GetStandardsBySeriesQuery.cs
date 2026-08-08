// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every consensus-standards requirement for a given
//   (family, series), paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Retrieves every consensus-standards requirement for a given (family, series), paged.</summary>
/// <param name="FamilyCode">The standards family code (e.g. "ISO").</param>
/// <param name="SeriesCode">The standards series code (e.g. "9001").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record GetStandardsBySeriesQuery(
    string FamilyCode,
    string SeriesCode,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<StandardsRequirementDto>>>;
