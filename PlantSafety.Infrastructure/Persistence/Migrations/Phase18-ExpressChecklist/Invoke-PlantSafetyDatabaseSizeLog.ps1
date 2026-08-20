<#
.SYNOPSIS
    Runs the Phase 18 database size snapshot procedure
    (msdb.dbo.usp_PlantSafety_LogDatabaseSizes, see 180-database-size-log.sql)
    via sqlcmd. Intended to be invoked by a registered Scheduled Task
    (see Register-PlantSafetyDatabaseSizeLogTask.ps1), but safe to run
    manually too.

.PARAMETER ServerInstance
    SQL Server instance name. Defaults to (local)\PLANTSAFETY.
#>
[CmdletBinding()]
param(
    [string]$ServerInstance = '(local)\PLANTSAFETY'
)

$ErrorActionPreference = 'Stop'

$logRoot = 'C:\Users\dross\SQLBackups\PlantSafety\TaskLogs'
if (-not (Test-Path $logRoot)) {
    New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
}

$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$logFile = Join-Path $logRoot "DatabaseSizeLog-$stamp.log"

"[$([DateTime]::UtcNow.ToString('o'))] Starting database size snapshot via msdb.dbo.usp_PlantSafety_LogDatabaseSizes on $ServerInstance" | Out-File -FilePath $logFile -Append -Encoding utf8

try {
    $rawOutput = & sqlcmd -S $ServerInstance -E -Q "EXEC msdb.dbo.usp_PlantSafety_LogDatabaseSizes;" -b 2>&1
    $exitCode = $LASTEXITCODE

    $output = $rawOutput | ForEach-Object { ($_ -replace '[^\x09\x0A\x0D\x20-\x7E]', '').Trim() } | Where-Object { $_ -ne '' }
    $output | Out-File -FilePath $logFile -Append -Encoding utf8

    if ($exitCode -ne 0) {
        "[$([DateTime]::UtcNow.ToString('o'))] FAILED: sqlcmd exited with code $exitCode" | Out-File -FilePath $logFile -Append -Encoding utf8
        Write-Error "PlantSafety database size snapshot FAILED (sqlcmd exit code $exitCode). See $logFile"
        exit $exitCode
    }

    "[$([DateTime]::UtcNow.ToString('o'))] SUCCESS: database size snapshot completed." | Out-File -FilePath $logFile -Append -Encoding utf8
    Write-Host "PlantSafety database size snapshot completed successfully. Log: $logFile"
    exit 0
}
catch {
    "[$([DateTime]::UtcNow.ToString('o'))] EXCEPTION: $($_.Exception.Message)" | Out-File -FilePath $logFile -Append -Encoding utf8
    Write-Error "PlantSafety database size snapshot FAILED with exception: $($_.Exception.Message). See $logFile"
    exit 1
}
