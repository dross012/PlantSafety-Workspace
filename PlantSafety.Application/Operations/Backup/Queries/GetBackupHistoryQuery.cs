// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Query to retrieve recent backup history from msdb.dbo.backupset.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Application.Operations.Backup.Queries;

/// <summary>Retrieves the most recent backups, newest first, optionally filtered to a single database.</summary>
/// <param name="DatabaseName">Optional exact database name filter. Null returns all PlantSafety.* databases.</param>
/// <param name="Top">Maximum number of rows to return. Defaults to 50.</param>
public sealed record GetBackupHistoryQuery(string? DatabaseName, int Top = 50) : IRequest<Result<IReadOnlyList<BackupHistoryItemDto>>>;
