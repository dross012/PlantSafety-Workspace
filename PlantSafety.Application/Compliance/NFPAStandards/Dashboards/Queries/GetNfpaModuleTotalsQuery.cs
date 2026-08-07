// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query for total NFPA hazard-mapping counts per module -- global reference
//   data, not tenant/site scoped. Backs the "total NFPA hazards per module" tile.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Dashboards.Queries;

/// <summary>Returns total/High/Critical hazard-mapping counts for every module.</summary>
public sealed record GetNfpaModuleTotalsQuery : IRequest<Result<IReadOnlyList<NfpaModuleTotalDto>>>;
