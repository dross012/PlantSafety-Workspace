// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to search OSHA standards by keyword/part/status, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Searches Compliance.OSHAStandard by optional keyword/part/status filters, paged.</summary>
/// <param name="Keyword">Optional case-insensitive contains match against SectionTitle/SectionText.</param>
/// <param name="PartNumber">Optional exact CFR part filter (e.g. 1910).</param>
/// <param name="Status">Optional exact status filter ("Active"/"Reserved"/"Rescinded").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record SearchOshaStandardsQuery(
    string? Keyword,
    int? PartNumber,
    string? Status,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<OSHAStandardItemDto>>>;
