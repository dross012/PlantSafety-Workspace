// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every NIOSH hazard profile mapped to a given PlantSafety
//   module. Answers "what chemical hazards are relevant to this operation?".
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Retrieves every NIOSH hazard profile whose Compliance.NIOSHHazardModuleMapping.Module matches <paramref name="Module"/> exactly.</summary>
/// <param name="Module">The PlantSafety module name to match (e.g. "PPE", "MachineSafety").</param>
public sealed record GetNioshHazardsByModuleQuery(
    string Module) : IRequest<Result<IReadOnlyList<NioshHazardProfileDto>>>;
