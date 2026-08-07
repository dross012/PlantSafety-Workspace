// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every OSHA requirement mapped to a given PlantSafety
//   module. Answers "what OSHA requirements apply to this operation?" -- but only once a
//   separate mapping pass has populated Compliance.OSHARequirement.AppliesToModule; import
//   alone always leaves it NULL, so this returns an empty list until that mapping exists.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Retrieves every OSHA requirement whose AppliesToModule matches <paramref name="AppliesToModule"/> exactly.</summary>
/// <param name="AppliesToModule">The PlantSafety module name to match (e.g. "PermitToWork", "PPE").</param>
public sealed record GetOshaRequirementsByModuleQuery(
    string AppliesToModule) : IRequest<Result<IReadOnlyList<OSHARequirementItemDto>>>;
