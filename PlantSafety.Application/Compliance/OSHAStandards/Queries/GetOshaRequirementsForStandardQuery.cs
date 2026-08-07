// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every requirement derived from a given OSHA standard.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Retrieves every requirement derived from a given OSHA standard.</summary>
/// <param name="OSHAStandardId">Identity of the standard.</param>
public sealed record GetOshaRequirementsForStandardQuery(
    Guid OSHAStandardId) : IRequest<Result<IReadOnlyList<OSHARequirementItemDto>>>;
