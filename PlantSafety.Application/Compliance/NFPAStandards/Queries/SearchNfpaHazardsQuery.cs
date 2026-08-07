// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to search NFPA hazard profiles by keyword/code number, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Searches Compliance.NFPA_HazardProfile by optional keyword/code filters, paged.</summary>
/// <param name="Keyword">Optional case-insensitive contains match against HazardName/HazardDescription.</param>
/// <param name="CodeNumber">Optional exact NFPA code filter (e.g. "70").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record SearchNfpaHazardsQuery(
    string? Keyword,
    string? CodeNumber,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<NfpaHazardProfileDto>>>;
