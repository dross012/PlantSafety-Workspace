// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve FMCSA standard changes detected at or after a given UTC
//   instant. Backs "show me all sections updated since last month"-style consumers.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Retrieves Compliance.FMCSAStandardChangeLog entries detected at or after <paramref name="SinceUtc"/>, most recent first.</summary>
/// <param name="SinceUtc">Inclusive lower bound on DetectedAtUtc.</param>
public sealed record GetFmcsaChangesSinceQuery(
    DateTime SinceUtc) : IRequest<Result<IReadOnlyList<FmcsaStandardChangeDto>>>;
