// ============================================================================
// File: OperationsBackupV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-19
// Description: V1 controller surfacing the real SQL Server backup/DR automation built
//   in Phase 12 (msdb.dbo.usp_PlantSafety_Backup*, usp_PlantSafety_RestoreTest) and
//   Phase 18 (Invoke-PlantSafetyBackup.ps1, PlantSafety_DatabaseSizeLog). Routes all
//   requests through MediatR ISender to the Application layer. No domain logic or data
//   access here.
//
//   NOT to be confused with the PlantSafety.Api.CrossDomain.Backup.* controllers, which
//   are an unrelated generic CRUD stub for a domain entity literally named "Backup".
//
//   SAFETY NOTE: There is deliberately no "restore production database" endpoint on
//   this controller. RestoreVerifyAsync below only ever triggers the safe, isolated,
//   self-cleaning msdb.dbo.usp_PlantSafety_RestoreTest. Actual production restore is a
//   manual, human-run procedure -- see DR-manual-restore-runbook.md.
// ============================================================================
using PlantSafety.Api.Operations.Backup.Contracts.Requests;
using PlantSafety.Application.Operations.Backup.Commands;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Queries;

namespace PlantSafety.Api.Operations.Backup.Controllers.V1;

/// <summary>V1 controller for the Operations.Backup module.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/backup")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operations.Backup")]
public sealed class OperationsBackupV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationsBackupV1Controller"/>.</summary>
    public OperationsBackupV1Controller(ISender sender) => _sender = sender;

    private string CorrelationId =>
        HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>Triggers an on-demand backup (Full/Diff/Log). Non-blocking -- returns an immediate "Started" acknowledgement.</summary>
    [HttpPost("trigger")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<BackupTriggerResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BackupTriggerResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TriggerAsync([FromBody] TriggerBackupRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new TriggerBackupCommand(request.BackupType), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<BackupTriggerResultDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<BackupTriggerResultDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>Returns recent backup history from msdb.dbo.backupset, newest first.</summary>
    [HttpGet("history")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BackupHistoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistoryAsync(
        [FromQuery] string? databaseName,
        [FromQuery] int top = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetBackupHistoryQuery(databaseName, top), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<BackupHistoryItemDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<BackupHistoryItemDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>Returns recent database size snapshots from msdb.dbo.PlantSafety_DatabaseSizeLog.</summary>
    [HttpGet("database-size")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DatabaseSizeSnapshotDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDatabaseSizeAsync(
        [FromQuery] string? databaseName,
        [FromQuery] int top = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetDatabaseSizeHistoryQuery(databaseName, top), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<DatabaseSizeSnapshotDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<DatabaseSizeSnapshotDto>>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Runs a SAFE, isolated restore-verify (msdb.dbo.usp_PlantSafety_RestoreTest) against the
    /// given database's latest FULL backup. Restores into an isolated *_DRTest database, runs
    /// DBCC CHECKDB, then always drops the test database. Never touches the live database.
    /// This is NOT a production restore endpoint.
    /// </summary>
    [HttpPost("restore-verify")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<RestoreVerifyResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RestoreVerifyResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreVerifyAsync([FromBody] RestoreVerifyRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RunRestoreVerifyCommand(request.DatabaseName), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<RestoreVerifyResultDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<RestoreVerifyResultDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>Returns derived backup-health status (Green/Yellow/Red) for every live PlantSafety.* database.</summary>
    [HttpGet("health")]
    [Authorize(Policy = SafetyTierPolicies.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BackupHealthDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthAsync(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBackupHealthQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<IReadOnlyList<BackupHealthDto>>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<IReadOnlyList<BackupHealthDto>>.Ok(result.Value, CorrelationId));
    }
}
