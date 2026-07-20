// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve the most recent deadlock events parsed from the
//   built-in "system_health" Extended Events session's ring_buffer target.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Monitoring.Models;

namespace PlantSafety.Application.Operations.Monitoring.Queries;

/// <summary>Retrieves the most recent deadlock events, newest first.</summary>
/// <param name="Top">Maximum number of events to return. Defaults to 20.</param>
public sealed record GetDeadlockEventsQuery(int Top = 20) : IRequest<Result<IReadOnlyList<DeadlockEventDto>>>;
