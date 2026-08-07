// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every NIOSH respirator recommendation for a hazard profile.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Retrieves every respirator recommendation for a given NIOSH hazard profile.</summary>
/// <param name="HazardId">Identity of the hazard profile.</param>
public sealed record GetRespiratorRecommendationsQuery(
    Guid HazardId) : IRequest<Result<IReadOnlyList<NioshRespiratorMatrixDto>>>;
