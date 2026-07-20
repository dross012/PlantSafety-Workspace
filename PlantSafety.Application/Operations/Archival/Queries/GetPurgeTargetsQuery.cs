// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve the static metadata for the 5 live purge targets.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Archival.Models;

namespace PlantSafety.Application.Operations.Archival.Queries;

/// <summary>Retrieves static metadata for the 5 live purge targets.</summary>
public sealed record GetPurgeTargetsQuery() : IRequest<Result<IReadOnlyList<PurgeTargetDto>>>;
