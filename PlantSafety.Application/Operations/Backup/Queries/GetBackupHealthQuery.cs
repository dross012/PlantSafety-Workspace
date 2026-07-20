// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Query to derive backup-health status (Green/Yellow/Red) for every
//   live PlantSafety.* database from the recency of its most recent successful FULL backup.
// SchemaVersion: 1

using System.Collections.Generic;

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Application.Operations.Backup.Queries;

/// <summary>Retrieves the derived backup-health status for every live PlantSafety.* database.</summary>
public sealed record GetBackupHealthQuery : IRequest<Result<IReadOnlyList<BackupHealthDto>>>;
