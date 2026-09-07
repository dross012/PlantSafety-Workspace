param(
    [Parameter(Mandatory = $true)]
    [string] $ServiceName,

    [Parameter(Mandatory = $true)]
    [string] $DisplayName,

    [Parameter(Mandatory = $true)]
    [string] $Description,

    [Parameter(Mandatory = $true)]
    [string] $ExePath,

    [Parameter(Mandatory = $true)]
    [string] $Account,

    [Parameter(Mandatory = $true)]
    [string] $Password
)

Write-Host "------------------------------------------------------------"
Write-Host " RegisterServices.ps1"
Write-Host "------------------------------------------------------------"
Write-Host "ServiceName: $ServiceName"
Write-Host "DisplayName: $DisplayName"
Write-Host "Description: $Description"
Write-Host "ExePath: $ExePath"
Write-Host "Account: $Account"
Write-Host "------------------------------------------------------------"

# ------------------------------------------------------------
# 1. VALIDATION
# ------------------------------------------------------------

if (-not (Test-Path $ExePath)) {
    Write-Host "ERROR: Executable not found: $ExePath"
    exit 1
}

# ------------------------------------------------------------
# 2. CHECK IF SERVICE EXISTS
# ------------------------------------------------------------

$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($existing) {
    Write-Host "Service '$ServiceName' already exists. Skipping installation."
    exit 0
}

# ------------------------------------------------------------
# 3. CREATE SERVICE
# ------------------------------------------------------------

Write-Host "Creating service '$ServiceName'..."

try {
    New-Service `
        -Name $ServiceName `
        -BinaryPathName "`"$ExePath`"" `
        -DisplayName $DisplayName `
        -Description $Description `
        -StartupType Automatic `
        -Credential (New-Object System.Management.Automation.PSCredential($Account, (ConvertTo-SecureString $Password -AsPlainText -Force))) `
        -ErrorAction Stop | Out-Null
}
catch {
    Write-Host "ERROR: Failed to create service '$ServiceName'. $($_.Exception.Message)"
    exit 1
}

Write-Host "Service '$ServiceName' created successfully."

# ------------------------------------------------------------
# 4. START SERVICE
# ------------------------------------------------------------

Write-Host "Starting service '$ServiceName'..."

try {
    Start-Service -Name $ServiceName -ErrorAction Stop
}
catch {
    Write-Host "ERROR: Failed to start service '$ServiceName'. $($_.Exception.Message)"
    exit 1
}

Write-Host "Service '$ServiceName' started successfully."
Write-Host "------------------------------------------------------------"
exit 0
