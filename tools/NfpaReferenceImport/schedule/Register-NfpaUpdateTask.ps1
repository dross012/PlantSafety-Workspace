# NOT executed automatically by any agent/tool -- run this manually (elevated) if you want
# Windows Task Scheduler to run the NFPA updater on a recurring schedule.
#
# Usage:
#   .\Register-NfpaUpdateTask.ps1                 # registers a weekly Sunday 03:00 run
#   .\Register-NfpaUpdateTask.ps1 -Daily           # registers a daily 03:00 run
#   .\Register-NfpaUpdateTask.ps1 -Unregister      # removes the task
#
# The task runs schedule\run-update.ps1, which reads the mock NFPA reference-data JSON files
# under SampleData/, diffs them against Compliance.NFPA_HazardProfile by SHA-256 content hash,
# and writes New/Revised/Rescinded rows to Compliance.NFPA_HazardChangeLog.

param(
    [switch]$Daily,
    [switch]$Unregister
)

$TaskName = "PlantSafety-NFPAReferenceUpdate"
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

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Description "Syncs Compliance.NFPA_HazardProfile/NFPA_ControlMatrix/NFPA_HazardModuleMapping in PlantSafety.CoreDB from the mock NFPA reference-data JSON files under SampleData/." -Force

Write-Output "Registered task '$TaskName'."
