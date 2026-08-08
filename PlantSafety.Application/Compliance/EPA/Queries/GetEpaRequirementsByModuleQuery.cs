// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every EPA regulatory requirement mapped to a given
//   PlantSafety module, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Models;

namespace PlantSafety.Application.Compliance.EPA.Queries;

/// <summary>Retrieves every EPA regulatory requirement whose ImpactedModules contains <paramref name="ModuleName"/>, paged.</summary>
/// <param name="ModuleName">The PlantSafety module name to match (e.g. "HazardousWasteManagement").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record GetEpaRequirementsByModuleQuery(
    string ModuleName,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<EpaRegulatoryRequirementDto>>>;
