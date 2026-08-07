// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve a single OSHA standard by (PartNumber, Section).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Retrieves a single OSHA standard by (PartNumber, Section).</summary>
/// <param name="PartNumber">CFR part number (e.g. 1910).</param>
/// <param name="Section">Section citation (e.g. "&#167; 1910.1200").</param>
public sealed record GetOshaStandardBySectionQuery(
    int PartNumber,
    string Section) : IRequest<Result<OSHAStandardItemDto>>;
