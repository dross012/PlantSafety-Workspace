// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve a single NIOSH hazard profile by CAS Registry Number.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NIOSHStandards.Models;

namespace PlantSafety.Application.Compliance.NIOSHStandards.Queries;

/// <summary>Retrieves a single NIOSH hazard profile by CAS Registry Number.</summary>
/// <param name="CasNumber">CAS Registry Number (e.g. "7647-01-0").</param>
public sealed record GetNioshHazardProfileQuery(
    string CasNumber) : IRequest<Result<NioshHazardProfileDto>>;
