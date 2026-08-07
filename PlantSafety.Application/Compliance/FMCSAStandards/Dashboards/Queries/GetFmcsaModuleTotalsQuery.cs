// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for total FMCSA requirement counts per module -- global reference data,
//   not tenant/site scoped. Backs the "total FMCSA requirements per module" tile.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Dashboards.Queries;

/// <summary>Returns total/High/Critical requirement counts for every module.</summary>
public sealed record GetFmcsaModuleTotalsQuery : IRequest<Result<IReadOnlyList<FmcsaModuleTotalDto>>>;
