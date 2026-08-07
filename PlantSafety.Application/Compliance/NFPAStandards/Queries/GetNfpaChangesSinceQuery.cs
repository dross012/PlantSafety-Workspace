// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve NFPA hazard profile changes detected at or after a given
//   UTC instant. Backs "show me all hazard profiles updated since last month"-style consumers.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Retrieves Compliance.NFPA_HazardChangeLog entries detected at or after <paramref name="SinceUtc"/>, most recent first.</summary>
/// <param name="SinceUtc">Inclusive lower bound on DetectedAtUtc.</param>
public sealed record GetNfpaChangesSinceQuery(
    DateTime SinceUtc) : IRequest<Result<IReadOnlyList<NfpaHazardChangeDto>>>;
