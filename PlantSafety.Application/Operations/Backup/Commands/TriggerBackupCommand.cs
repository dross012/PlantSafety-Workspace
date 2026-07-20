// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Command to trigger an on-demand SQL Server backup (Full/Diff/Log)
//   via Invoke-PlantSafetyBackup.ps1.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Application.Operations.Backup.Commands;

/// <summary>Triggers an on-demand backup of the given type across all live PlantSafety.* databases.</summary>
/// <param name="BackupType">Full | Diff | Log.</param>
public sealed record TriggerBackupCommand(BackupType BackupType) : IRequest<Result<BackupTriggerResultDto>>;
