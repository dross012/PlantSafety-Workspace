# Wrapper invoked by the scheduled task (see Register-NfpaUpdateTask.ps1) or run manually.
# Publishes once to a stable output path, then runs the compiled exe so repeated scheduled
# runs don't pay `dotnet build` cost. Logs to schedule\logs\<date>.log.

$ErrorActionPreference = "Stop"
$toolDir = Split-Path -Parent $PSScriptRoot
$publishDir = Join-Path $toolDir "publish"
$logDir = Join-Path $PSScriptRoot "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$logFile = Join-Path $logDir ("update-{0}.log" -f (Get-Date -Format "yyyy-MM-dd_HHmmss"))

if (-not (Test-Path (Join-Path $publishDir "NfpaReferenceImport.exe"))) {
    dotnet publish $toolDir -c Release -o $publishDir | Tee-Object -FilePath $logFile -Append
}

& (Join-Path $publishDir "NfpaReferenceImport.exe") 2>&1 | Tee-Object -FilePath $logFile -Append
exit $LASTEXITCODE
