<#
.SYNOPSIS
    Express-safe replacement for the Phase 12 SQL Server Agent backup jobs.

.DESCRIPTION
    SQL Server Agent (PLANTSAFETY) is confirmed DISABLED (not just stopped) on
    this dev instance (live-verified 2026-07-19), so the Agent jobs registered
    in Phase 12 (120-backup-jobs-full-diff-log.sql) will never fire on their
    own. This script calls the SAME existing stored procedures
    (msdb.dbo.usp_PlantSafety_BackupFull / Diff / Log) via sqlcmd.exe, so the
    backup logic itself is not duplicated -- only the trigger mechanism
    changes (Windows Task Scheduler instead of SQL Agent).

    Intended to be invoked by a registered Scheduled Task
    (see Register-PlantSafetyBackupTasks.ps1), but is safe to run manually too.

.PARAMETER BackupType
    Full | Diff | Log -- selects which usp_PlantSafety_Backup<Type> proc to run.

.PARAMETER ServerInstance
    SQL Server instance name. Defaults to (local)\PLANTSAFETY.

.OUTPUTS
    Writes a timestamped log file under
    C:\Users\dross\SQLBackups\PlantSafety\TaskLogs\Backup-<Type>-<stamp>.log
    and exits non-zero on any sqlcmd failure so Task Scheduler correctly
    records a failed run (surfaced in Get-ScheduledTaskInfo LastTaskResult).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Full', 'Diff', 'Log')]
    [string]$BackupType,

    [string]$ServerInstance = '(local)\PLANTSAFETY'
)

$ErrorActionPreference = 'Stop'

$logRoot = 'C:\Users\dross\SQLBackups\PlantSafety\TaskLogs'
if (-not (Test-Path $logRoot)) {
    New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
}

$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$logFile = Join-Path $logRoot "Backup-$BackupType-$stamp.log"

$procName = "msdb.dbo.usp_PlantSafety_Backup$BackupType"
$sql = "EXEC $procName;"

"[$([DateTime]::UtcNow.ToString('o'))] Starting $BackupType backup via $procName on $ServerInstance" | Tee-Object -FilePath $logFile -Append | Out-Null

try {
    # -b: abort batch and return a non-zero exit code to the OS on a SQL
    #     error, so Task Scheduler correctly marks the run as failed.
    # Note: BACKUP DATABASE emits carriage-return-driven "percent complete"
    # progress control characters via sqlcmd's default STATS behavior; strip
    # non-printable characters so the log file stays human-readable instead
    # of a wall of control-character garbage.
    $rawOutput = & sqlcmd -S $ServerInstance -E -Q $sql -b 2>&1
    $exitCode = $LASTEXITCODE

    $output = $rawOutput | ForEach-Object { ($_ -replace '[^\x09\x0A\x0D\x20-\x7E]', '').Trim() } | Where-Object { $_ -ne '' }

    $output | Out-File -FilePath $logFile -Append -Encoding utf8

    if ($exitCode -ne 0) {
        "[$([DateTime]::UtcNow.ToString('o'))] FAILED: sqlcmd exited with code $exitCode" | Out-File -FilePath $logFile -Append -Encoding utf8
        Write-Error "PlantSafety $BackupType backup FAILED (sqlcmd exit code $exitCode). See $logFile"
        exit $exitCode
    }

    "[$([DateTime]::UtcNow.ToString('o'))] SUCCESS: $BackupType backup completed." | Out-File -FilePath $logFile -Append -Encoding utf8
    Write-Host "PlantSafety $BackupType backup completed successfully. Log: $logFile"
    exit 0
}
catch {
    "[$([DateTime]::UtcNow.ToString('o'))] EXCEPTION: $($_.Exception.Message)" | Out-File -FilePath $logFile -Append -Encoding utf8
    Write-Error "PlantSafety $BackupType backup FAILED with exception: $($_.Exception.Message). See $logFile"
    exit 1
}
