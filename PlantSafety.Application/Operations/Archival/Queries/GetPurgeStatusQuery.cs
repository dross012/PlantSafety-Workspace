// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve recent purge run history from Archival.ArchivalJob.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Models;

namespace PlantSafety.Application.Operations.Archival.Queries;

/// <summary>Retrieves recent purge run history, newest first, optionally filtered to a single Zone.</summary>
/// <param name="Zone">Optional exact Archival.TierPolicy.Zone filter. Null returns all zones.</param>
/// <param name="Top">Maximum number of rows to return. Defaults to 50.</param>
public sealed record GetPurgeStatusQuery(string? Zone, int Top = 50) : IRequest<Result<IReadOnlyList<ArchivalJobDto>>>;
