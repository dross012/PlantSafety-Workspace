// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every NFPA hazard mapped to a given PlantSafety module.
//   Answers "what NFPA hazard guidance applies to this operation?" -- equivalent of OSHA's
//   GetOshaRequirementsByModuleQuery, adapted to NFPA's hazard-profile-plus-mapping shape.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Retrieves every NFPA hazard whose module mapping matches <paramref name="ModuleName"/> exactly.</summary>
/// <param name="ModuleName">The PlantSafety module name to match (e.g. "MachineSafety", "HazardousWasteManagement").</param>
public sealed record GetNfpaHazardModulesQuery(
    string ModuleName) : IRequest<Result<IReadOnlyList<NfpaHazardModuleDto>>>;
