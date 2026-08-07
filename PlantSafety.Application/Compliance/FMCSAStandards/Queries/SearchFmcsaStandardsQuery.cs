// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to search FMCSA standards by keyword/part, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Searches Compliance.FMCSAStandard by optional keyword/part filters, paged.</summary>
/// <param name="Keyword">Optional case-insensitive contains match against SectionTitle/SectionText.</param>
/// <param name="PartNumber">Optional exact CFR part filter (e.g. "395").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record SearchFmcsaStandardsQuery(
    string? Keyword,
    string? PartNumber,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<FmcsaStandardDto>>>;
