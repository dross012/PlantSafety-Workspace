// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Query to retrieve database size snapshots from
//   msdb.dbo.PlantSafety_DatabaseSizeLog (Phase 18).
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Application.Operations.Backup.Queries;

/// <summary>Retrieves the most recent database size snapshots, newest first, optionally filtered to a single database.</summary>
/// <param name="DatabaseName">Optional exact database name filter. Null returns all PlantSafety.* databases.</param>
/// <param name="Top">Maximum number of rows to return. Defaults to 100.</param>
public sealed record GetDatabaseSizeHistoryQuery(string? DatabaseName, int Top = 100) : IRequest<Result<IReadOnlyList<DatabaseSizeSnapshotDto>>>;
