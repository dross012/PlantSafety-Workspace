// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Thin passthrough query so the Operations.Monitoring dashboard can pull
//   DB Size data alongside DB Health/Performance/Errors/Deadlocks in a single module without
//   re-implementing PlantSafety.Application.Operations.Backup.Repositories.IBackupRepository
//   .GetDatabaseSizeHistoryAsync (Phase 18, msdb.dbo.PlantSafety_DatabaseSizeLog). The
//   handler injects IBackupRepository directly rather than duplicating its SQL.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Application.Operations.Monitoring.Queries;

/// <summary>Retrieves recent database size snapshots via the existing Operations.Backup repository.</summary>
/// <param name="DatabaseName">Optional exact database name filter. Null returns all PlantSafety.* databases.</param>
/// <param name="Top">Maximum number of rows to return. Defaults to 100.</param>
public sealed record GetMonitoringDatabaseSizeQuery(string? DatabaseName, int Top = 100)
    : IRequest<Result<IReadOnlyList<DatabaseSizeSnapshotDto>>>;
