// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve every recommended control derived for a given hazard.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Retrieves every Compliance.NFPA_ControlMatrix row for a given hazard.</summary>
/// <param name="HazardId">Identity of the hazard profile.</param>
public sealed record GetNfpaControlsForHazardQuery(
    Guid HazardId) : IRequest<Result<IReadOnlyList<NfpaControlMatrixDto>>>;
