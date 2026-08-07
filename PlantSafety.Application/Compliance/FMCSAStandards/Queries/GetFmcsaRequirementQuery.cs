// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every requirement derived from a given FMCSA standard.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Retrieves every requirement derived from a given FMCSA standard.</summary>
/// <param name="StandardId">Identity of the standard.</param>
public sealed record GetFmcsaRequirementQuery(
    Guid StandardId) : IRequest<Result<IReadOnlyList<FmcsaRequirementDto>>>;
