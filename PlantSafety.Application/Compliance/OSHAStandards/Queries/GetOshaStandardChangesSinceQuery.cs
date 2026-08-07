// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve OSHA standard changes detected at or after a given UTC
//   instant. Backs "show me all sections updated since last month"-style consumers.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.OSHAStandards.Models;

namespace PlantSafety.Application.Compliance.OSHAStandards.Queries;

/// <summary>Retrieves Compliance.OSHAStandardChangeLog entries detected at or after <paramref name="SinceUtc"/>, most recent first.</summary>
/// <param name="SinceUtc">Inclusive lower bound on DetectedAtUtc.</param>
public sealed record GetOshaStandardChangesSinceQuery(
    DateTime SinceUtc) : IRequest<Result<IReadOnlyList<OSHAStandardChangeDto>>>;
