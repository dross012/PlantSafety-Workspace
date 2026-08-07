// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every FMCSA requirement mapped to a given PlantSafety
//   module. Answers "what FMCSA requirements apply to this operation?" -- queries the denormalized
//   Compliance.FMCSARequirement.AppliesToModule column directly, not the mapping table.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Retrieves every FMCSA requirement whose AppliesToModule matches <paramref name="AppliesToModule"/> exactly.</summary>
/// <param name="AppliesToModule">The PlantSafety module name to match (e.g. "Scheduling", "Routing").</param>
public sealed record GetFmcsaRequirementsForModuleQuery(
    string AppliesToModule) : IRequest<Result<IReadOnlyList<FmcsaRequirementDto>>>;
