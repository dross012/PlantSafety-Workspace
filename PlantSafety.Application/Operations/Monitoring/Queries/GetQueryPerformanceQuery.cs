// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Query to retrieve the top N queries by average CPU time for a given
//   database from its SQL Server Query Store (Phase 11).
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Monitoring.Models;

namespace PlantSafety.Application.Operations.Monitoring.Queries;

/// <summary>Retrieves the top queries by average CPU time for a single database's Query Store.</summary>
/// <param name="DatabaseKey">Logical database key (see MonitoringOptions.DatabaseConnectionStrings), e.g. "CoreDB".</param>
/// <param name="Top">Maximum number of rows to return. Defaults to 20.</param>
/// <param name="LookbackHours">How far back to look, in hours. Defaults to 24.</param>
public sealed record GetQueryPerformanceQuery(string DatabaseKey, int Top = 20, int LookbackHours = 24)
    : IRequest<Result<IReadOnlyList<QueryPerformanceItemDto>>>;
