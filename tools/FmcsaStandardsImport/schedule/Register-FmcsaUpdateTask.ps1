# NOT executed automatically by any agent/tool -- run this manually (elevated) if you want
# Windows Task Scheduler to run the FMCSA updater on a recurring schedule.
#
# Usage:
#   .\Register-FmcsaUpdateTask.ps1                 # registers a weekly Sunday 03:00 run
#   .\Register-FmcsaUpdateTask.ps1 -Daily           # registers a daily 03:00 run
#   .\Register-FmcsaUpdateTask.ps1 -Unregister      # removes the task
#
# The task runs schedule\run-update.ps1, which downloads the current govinfo.gov CFR Title 49
# bulk XML, diffs it against Compliance.FMCSAStandard by SHA-256 content hash, and writes
# New/Revised/Rescinded rows to Compliance.FMCSAStandardChangeLog.

param(
    [switch]$Daily,
    [switch]$Unregister
)

$TaskName = "PlantSafety-FMCSAStandardsUpdate"
$ScriptPath = Join-Path $PSScriptRoot "run-update.ps1"

if ($Unregister) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false -ErrorAction SilentlyContinue
    Write-Output "Unregistered task '$TaskName' (if it existed)."
    return
}

$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$ScriptPath`""
$trigger = if ($Daily) {
    New-ScheduledTaskTrigger -Daily -At 3am
} else {
    New-ScheduledTaskTrigger -Weekly -DaysOfWeek Sunday -At 3am
}
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -DontStopOnIdleEnd

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Description "Syncs Compliance.FMCSAStandard/FMCSARequirement in PlantSafety.CoreDB from the govinfo.gov CFR Title 49 bulk XML." -Force

Write-Output "Registered task '$TaskName'."
