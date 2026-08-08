// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every consensus-standards requirement mapped to a given
//   PlantSafety module, paged.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Models;

namespace PlantSafety.Application.Compliance.Standards.Queries;

/// <summary>Retrieves every consensus-standards requirement whose ImpactedModules field contains a given PlantSafety module, paged.</summary>
/// <param name="ModuleName">PlantSafety module name to match (e.g. "EquipmentSafety").</param>
/// <param name="Skip">Number of rows to skip.</param>
/// <param name="Take">Maximum number of rows to return (capped server-side).</param>
public sealed record GetStandardsByModuleQuery(
    string ModuleName,
    int Skip,
    int Take) : IRequest<Result<IReadOnlyList<StandardsRequirementDto>>>;
