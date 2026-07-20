// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-19
// FILE DESCRIPTION: Command to run the SAFE, non-destructive restore-verify
//   (msdb.dbo.usp_PlantSafety_RestoreTest) against a database's latest FULL backup.
//   This is NOT a production restore — see DR-manual-restore-runbook.md for that
//   deliberately manual, human-run procedure.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Backup.Models;

namespace PlantSafety.Application.Operations.Backup.Commands;

/// <summary>Runs a safe, isolated restore-verify against <paramref name="DatabaseName"/>'s latest FULL backup.</summary>
/// <param name="DatabaseName">The PlantSafety.* database to restore-verify.</param>
public sealed record RunRestoreVerifyCommand(string DatabaseName) : IRequest<Result<RestoreVerifyResultDto>>;
