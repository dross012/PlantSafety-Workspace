<#
.SYNOPSIS
    Registers Windows Task Scheduler tasks that replace the Phase 7 SQL
    Server Agent retention-purge jobs (74-retention-archival.sql), since SQL
    Server Agent (PLANTSAFETY) is confirmed DISABLED on this dev instance
    (2026-07-19). Same daily 02:00 cadence as the original Agent jobs.

.DESCRIPTION
    Registers one task per purge procedure:
      - PlantSafety - Purge AuditLog             (PlantSafety.AuditDB.dbo.sp_AuditLog_Purge)
      - PlantSafety - Purge ErrorLog              (PlantSafety.ErrorDB.dbo.sp_ErrorLog_Purge -- expected to log 'Blocked', see Phase 7 header)
      - PlantSafety - Purge IntegrationLogEntry   (PlantSafety.IntegrationDB.Integration.sp_IntegrationLogEntry_Purge)
      - PlantSafety - Purge ReplayRecord          (PlantSafety.IntegrationDB.Integration.sp_ReplayRecord_Purge)
      - PlantSafety - Purge DiagnosticEvent       (PlantSafety.DiagnosticsDB.Diagnostics.sp_DiagnosticEvent_Purge)

    Idempotent: any existing task with the same name is unregistered and
    recreated so re-running this script is always safe.
#>
[CmdletBinding()]
param(
    [string]$ScriptPath = 'C:\Users\dross\source\PlantSafety\PlantSafety.Infrastructure\Persistence\Migrations\Phase18-ExpressChecklist\Invoke-PlantSafetyPurge.ps1'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ScriptPath)) {
    throw "Purge script not found at $ScriptPath"
}

$purgeDefs = @(
    @{ TaskName = 'PlantSafety - Purge AuditLog';           Database = 'PlantSafety.AuditDB';       ProcName = 'dbo.sp_AuditLog_Purge' }
    @{ TaskName = 'PlantSafety - Purge ErrorLog';            Database = 'PlantSafety.ErrorDB';       ProcName = 'dbo.sp_ErrorLog_Purge' }
    @{ TaskName = 'PlantSafety - Purge IntegrationLogEntry'; Database = 'PlantSafety.IntegrationDB'; ProcName = 'Integration.sp_IntegrationLogEntry_Purge' }
    @{ TaskName = 'PlantSafety - Purge ReplayRecord';        Database = 'PlantSafety.IntegrationDB'; ProcName = 'Integration.sp_ReplayRecord_Purge' }
    @{ TaskName = 'PlantSafety - Purge DiagnosticEvent';     Database = 'PlantSafety.DiagnosticsDB'; ProcName = 'Diagnostics.sp_DiagnosticEvent_Purge' }
)

$trigger = New-ScheduledTaskTrigger -Daily -At '02:00AM'
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -DontStopOnIdleEnd `
    -ExecutionTimeLimit (New-TimeSpan -Hours 1) -MultipleInstances IgnoreNew

foreach ($def in $purgeDefs) {
    $taskName = $def.TaskName

    if (Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue) {
        Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
        Write-Host "Removed existing task: $taskName"
    }

    $action = New-ScheduledTaskAction -Execute 'powershell.exe' `
        -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$ScriptPath`" -Database `"$($def.Database)`" -ProcName `"$($def.ProcName)`""

    Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Settings $settings `
        -Description "Express-safe replacement for the Phase 7 SQL Agent job of the same name. Daily retention purge via $($def.Database).$($def.ProcName). Registered because SQL Server Agent is disabled on this Express instance." | Out-Null

    Write-Host "Registered task: $taskName"
}

Write-Host ""
Write-Host "Verification (Get-ScheduledTask):"
Get-ScheduledTask -TaskName 'PlantSafety - Purge*' | Select-Object TaskName, State | Format-Table -AutoSize
