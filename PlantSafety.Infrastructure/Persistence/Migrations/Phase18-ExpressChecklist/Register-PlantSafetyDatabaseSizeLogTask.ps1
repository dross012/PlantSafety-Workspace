<#
.SYNOPSIS
    Registers a daily Windows Task Scheduler task that runs the Phase 18
    database size snapshot (msdb.dbo.usp_PlantSafety_LogDatabaseSizes),
    since SQL Server Agent (SQLEXPRESS) is confirmed DISABLED on this dev
    instance (2026-07-19) and this new monitoring routine has the same
    reliability problem as the Phase 12 backup/maintenance jobs.

.DESCRIPTION
    Idempotent: any existing task with the same name is unregistered and
    recreated so re-running this script is always safe. Does not require
    elevation (live-verified 2026-07-19).
#>
[CmdletBinding()]
param(
    [string]$ScriptPath = 'C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations\Phase18-ExpressChecklist\Invoke-PlantSafetyDatabaseSizeLog.ps1',
    [string]$TaskName = 'PlantSafety - Database Size Snapshot'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ScriptPath)) {
    throw "Database size log script not found at $ScriptPath"
}

if (Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
    Write-Host "Removed existing task: $TaskName"
}

$action = New-ScheduledTaskAction -Execute 'powershell.exe' `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$ScriptPath`""

# Daily 00:30 -- ahead of the 01:00 backup cadence so a size trend is
# captured before any backup-driven file growth for that day.
$trigger = New-ScheduledTaskTrigger -Daily -At '00:30AM'

$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -DontStopOnIdleEnd `
    -ExecutionTimeLimit (New-TimeSpan -Minutes 30) -MultipleInstances IgnoreNew

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings `
    -Description 'Daily snapshot of data/log file size and used space for every live PlantSafety.* database via msdb.dbo.usp_PlantSafety_LogDatabaseSizes (Phase 18 / 180-database-size-log.sql). Registered because SQL Server Agent is disabled on this Express instance.' | Out-Null

Write-Host "Registered task: $TaskName"
Write-Host ""
Write-Host "Verification (Get-ScheduledTask):"
Get-ScheduledTask -TaskName $TaskName | Select-Object TaskName, State | Format-Table -AutoSize
