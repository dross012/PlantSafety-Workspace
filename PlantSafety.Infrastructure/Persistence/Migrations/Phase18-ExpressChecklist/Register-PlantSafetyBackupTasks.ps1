<#
.SYNOPSIS
    Registers Windows Task Scheduler tasks that replace the Phase 12 SQL
    Server Agent backup jobs, since SQL Server Agent (SQLEXPRESS) is
    confirmed DISABLED on this dev instance (2026-07-19).

.DESCRIPTION
    Mirrors the exact cadence already defined in Phase 12
    (120-backup-jobs-full-diff-log.sql) so the two mechanisms stay in sync:
      - PlantSafety - Full Backup        : weekly, Sunday 01:00
      - PlantSafety - Differential Backup: daily Mon-Sat, 01:00
      - PlantSafety - Log Backup         : every 15 minutes (correctly a
        continual no-op today -- all 18 PlantSafety.* databases are SIMPLE
        recovery model, confirmed live -- registered now so it activates
        automatically the moment a database is deliberately switched to
        FULL recovery, same rationale as the Phase 12 Agent job).

    Idempotent: any existing task with the same name is unregistered and
    recreated so re-running this script is always safe.

.NOTES
    Does not require elevation -- Register-ScheduledTask under the current
    user context was live-verified to succeed without admin rights on this
    machine (2026-07-19).
#>
[CmdletBinding()]
param(
    [string]$ScriptPath = 'C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations\Phase18-ExpressChecklist\Invoke-PlantSafetyBackup.ps1'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ScriptPath)) {
    throw "Backup script not found at $ScriptPath"
}

function Register-PlantSafetyTask {
    param(
        [string]$TaskName,
        [string]$Argument,
        [Microsoft.Management.Infrastructure.CimInstance[]]$Triggers,
        [string]$Description
    )

    if (Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue) {
        Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
        Write-Host "Removed existing task: $TaskName"
    }

    $action = New-ScheduledTaskAction -Execute 'powershell.exe' `
        -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$ScriptPath`" $Argument"

    $settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -DontStopOnIdleEnd `
        -ExecutionTimeLimit (New-TimeSpan -Hours 2) -MultipleInstances IgnoreNew

    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $Triggers `
        -Settings $settings -Description $Description | Out-Null

    Write-Host "Registered task: $TaskName"
}

# 1) Full backup -- weekly, Sunday 01:00 (matches Phase 12 Agent schedule)
$fullTrigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Sunday -At '01:00AM'
Register-PlantSafetyTask -TaskName 'PlantSafety - Full Backup (All Databases)' `
    -Argument '-BackupType Full' -Triggers $fullTrigger `
    -Description 'Express-safe replacement for Phase 12 SQL Agent job of the same name. Weekly FULL checksummed backup of every live PlantSafety.* database via msdb.dbo.usp_PlantSafety_BackupFull.'

# 2) Differential backup -- daily Mon-Sat, 01:00 (matches Phase 12 Agent schedule)
$diffTrigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Monday, Tuesday, Wednesday, Thursday, Friday, Saturday -At '01:00AM'
Register-PlantSafetyTask -TaskName 'PlantSafety - Differential Backup (All Databases)' `
    -Argument '-BackupType Diff' -Triggers $diffTrigger `
    -Description 'Express-safe replacement for Phase 12 SQL Agent job of the same name. Daily (Mon-Sat) DIFFERENTIAL backup via msdb.dbo.usp_PlantSafety_BackupDiff. Sunday reserved for FULL.'

# 3) Log backup -- every 15 minutes (matches Phase 12 Agent schedule; currently
#    a correct no-op on every database, all SIMPLE recovery model)
$logTrigger = New-ScheduledTaskTrigger -Once -At (Get-Date).Date -RepetitionInterval (New-TimeSpan -Minutes 15) -RepetitionDuration (New-TimeSpan -Days 3650)
Register-PlantSafetyTask -TaskName 'PlantSafety - Transaction Log Backup (All Databases)' `
    -Argument '-BackupType Log' -Triggers $logTrigger `
    -Description 'Express-safe replacement for Phase 12 SQL Agent job of the same name. Every-15-minute LOG backup via msdb.dbo.usp_PlantSafety_BackupLog. Currently a correct no-op: all PlantSafety.* databases are SIMPLE recovery model (live-verified). Activates automatically if a database is deliberately switched to FULL recovery.'

Write-Host ""
Write-Host "Verification (Get-ScheduledTask):"
Get-ScheduledTask -TaskName 'PlantSafety - *Backup*' | Select-Object TaskName, State | Format-Table -AutoSize
