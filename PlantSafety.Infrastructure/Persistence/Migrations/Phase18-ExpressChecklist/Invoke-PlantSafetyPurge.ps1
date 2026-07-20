<#
.SYNOPSIS
    Express-safe replacement for the Phase 7 SQL Server Agent retention-purge
    jobs (74-retention-archival.sql). SQL Server Agent (SQLEXPRESS) is
    confirmed DISABLED on this dev instance (2026-07-19), so those jobs never
    fire on their own. This script calls the SAME existing purge stored
    procedures via sqlcmd -- no purge logic is duplicated here.

.PARAMETER Database
    The database containing the purge procedure (e.g. PlantSafety.AuditDB).

.PARAMETER ProcName
    Fully schema-qualified procedure name (e.g. dbo.sp_AuditLog_Purge,
    Integration.sp_ReplayRecord_Purge).

.OUTPUTS
    Writes a timestamped log file under
    C:\Users\dross\SQLBackups\PlantSafety\TaskLogs\Purge-<ProcName>-<stamp>.log
    and exits non-zero on sqlcmd failure. Note: sp_ErrorLog_Purge is expected
    to log a 'Blocked' Archival.ArchivalJob row (TR_ErrorLog_NoDelete) rather
    than throw -- that is treated as sqlcmd/proc SUCCESS at the OS level by
    design (see Phase 7 74-retention-archival.sql header), since it is a
    logged, expected outcome, not an unhandled failure.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Database,

    [Parameter(Mandatory = $true)]
    [string]$ProcName,

    [string]$ServerInstance = '(local)\SQLEXPRESS'
)

$ErrorActionPreference = 'Stop'

$logRoot = 'C:\Users\dross\SQLBackups\PlantSafety\TaskLogs'
if (-not (Test-Path $logRoot)) {
    New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
}

$safeProcName = ($ProcName -replace '[\\/:*?"<>|.]', '_')
$stamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$logFile = Join-Path $logRoot "Purge-$safeProcName-$stamp.log"

"[$([DateTime]::UtcNow.ToString('o'))] Starting purge via $Database.$ProcName on $ServerInstance" | Out-File -FilePath $logFile -Append -Encoding utf8

try {
    $rawOutput = & sqlcmd -S $ServerInstance -E -d $Database -Q "EXEC $ProcName;" -b 2>&1
    $exitCode = $LASTEXITCODE

    $output = $rawOutput | ForEach-Object { ($_ -replace '[^\x09\x0A\x0D\x20-\x7E]', '').Trim() } | Where-Object { $_ -ne '' }
    $output | Out-File -FilePath $logFile -Append -Encoding utf8

    if ($exitCode -ne 0) {
        "[$([DateTime]::UtcNow.ToString('o'))] FAILED: sqlcmd exited with code $exitCode" | Out-File -FilePath $logFile -Append -Encoding utf8
        Write-Error "PlantSafety purge ($Database.$ProcName) FAILED (sqlcmd exit code $exitCode). See $logFile"
        exit $exitCode
    }

    "[$([DateTime]::UtcNow.ToString('o'))] SUCCESS: purge completed (see Archival.ArchivalJob in PlantSafety.ArchiveDB for row-level outcome, including any expected 'Blocked' status)." | Out-File -FilePath $logFile -Append -Encoding utf8
    Write-Host "PlantSafety purge ($Database.$ProcName) completed. Log: $logFile"
    exit 0
}
catch {
    "[$([DateTime]::UtcNow.ToString('o'))] EXCEPTION: $($_.Exception.Message)" | Out-File -FilePath $logFile -Append -Encoding utf8
    Write-Error "PlantSafety purge ($Database.$ProcName) FAILED with exception: $($_.Exception.Message). See $logFile"
    exit 1
}
