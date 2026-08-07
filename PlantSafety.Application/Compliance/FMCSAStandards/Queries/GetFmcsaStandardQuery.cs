// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve a single FMCSA standard by (PartNumber, SectionNumber).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Retrieves a single FMCSA standard by (PartNumber, SectionNumber).</summary>
/// <param name="PartNumber">CFR part number (e.g. "395").</param>
/// <param name="SectionNumber">Section citation (e.g. "395.8").</param>
public sealed record GetFmcsaStandardQuery(
    string PartNumber,
    string SectionNumber) : IRequest<Result<FmcsaStandardDto>>;
