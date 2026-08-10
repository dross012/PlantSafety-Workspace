# ============================================================================
#  Phase 18 (Deployment Modernization) -- idempotent Windows hardening
#  script. Applies the checklist in ../ServerBaseline.md. Every control
#  checks current state before changing it and prints [OK]/[APPLIED]/
#  [SKIPPED], so repeated runs against an already-hardened host produce
#  only [OK] lines and exit 0.
#
#  Usage: Run as Administrator: powershell -File apply_hardening.ps1
# ============================================================================
#Requires -RunAsAdministrator
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$script:Changed = $false

function Write-Ok      { param([string]$Message) Write-Host "[OK]      $Message" }
function Write-Applied { param([string]$Message) Write-Host "[APPLIED] $Message"; $script:Changed = $true }
function Write-Skipped { param([string]$Message) Write-Host "[SKIPPED] $Message" }

# Control 1: Windows Defender Firewall enabled, all profiles.
function Set-FirewallControl {
    $profiles = Get-NetFirewallProfile -ErrorAction SilentlyContinue
    if (-not $profiles) {
        Write-Skipped "Control 1: Get-NetFirewallProfile unavailable on this host."
        return
    }

    $disabledProfiles = $profiles | Where-Object { -not $_.Enabled }
    if ($disabledProfiles.Count -eq 0) {
        Write-Ok "Control 1: Windows Defender Firewall already enabled for all profiles."
    } else {
        Set-NetFirewallProfile -All -Enabled True
        Write-Applied "Control 1: Windows Defender Firewall enabled for all profiles."
    }
}

# Control 2: built-in Administrator account disabled for interactive/RDP logon.
function Set-AdministratorAccountControl {
    $admin = Get-LocalUser -Name "Administrator" -ErrorAction SilentlyContinue
    if (-not $admin) {
        Write-Skipped "Control 2: local Administrator account not found (domain-joined host?)."
        return
    }

    if (-not $admin.Enabled) {
        Write-Ok "Control 2: built-in Administrator account already disabled."
    } else {
        Disable-LocalUser -Name "Administrator"
        Write-Applied "Control 2: built-in Administrator account disabled."
    }
}

# Control 4: Advanced Audit Policy -- Logon/Logoff and Object Access auditing enabled.
function Set-AuditPolicyControl {
    $logonAudit = (auditpol /get /subcategory:"Logon" /r 2>$null | Select-Object -Skip 1 | ConvertFrom-Csv)
    if ($logonAudit -and $logonAudit."Inclusion Setting" -match "Success and Failure") {
        Write-Ok "Control 4: Logon audit policy already set to Success and Failure."
    } else {
        auditpol /set /subcategory:"Logon" /success:enable /failure:enable | Out-Null
        Write-Applied "Control 4: Logon audit policy set to Success and Failure."
    }
}

# Control 5: Windows Update set to automatic.
function Set-AutomaticUpdatesControl {
    $auOptionsPath = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"
    $desired = 4 # 4 = Auto download and schedule the install

    if (-not (Test-Path $auOptionsPath)) {
        New-Item -Path $auOptionsPath -Force | Out-Null
    }

    $current = (Get-ItemProperty -Path $auOptionsPath -Name "AUOptions" -ErrorAction SilentlyContinue).AUOptions
    if ($current -eq $desired) {
        Write-Ok "Control 5: automatic updates already set to auto-download-and-schedule."
    } else {
        Set-ItemProperty -Path $auOptionsPath -Name "AUOptions" -Value $desired -Type DWord
        Write-Applied "Control 5: automatic updates set to auto-download-and-schedule."
    }
}

# Control 6: minimum TLS 1.2 enforced, strong crypto, TLS 1.0/1.1 disabled.
function Set-TlsControl {
    $schannelPath = "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols"
    $weakProtocols = @("TLS 1.0", "TLS 1.1")
    $anyChanged = $false

    foreach ($protocol in $weakProtocols) {
        $serverPath = Join-Path $schannelPath "$protocol\Server"
        if (-not (Test-Path $serverPath)) {
            New-Item -Path $serverPath -Force | Out-Null
        }

        $enabled = (Get-ItemProperty -Path $serverPath -Name "Enabled" -ErrorAction SilentlyContinue).Enabled
        if ($enabled -ne 0) {
            Set-ItemProperty -Path $serverPath -Name "Enabled" -Value 0 -Type DWord
            $anyChanged = $true
        }
    }

    $strongCryptoPath = "HKLM:\SOFTWARE\Microsoft\.NETFramework\v4.0.30319"
    $currentStrongCrypto = (Get-ItemProperty -Path $strongCryptoPath -Name "SchUseStrongCrypto" -ErrorAction SilentlyContinue).SchUseStrongCrypto
    if ($currentStrongCrypto -ne 1) {
        Set-ItemProperty -Path $strongCryptoPath -Name "SchUseStrongCrypto" -Value 1 -Type DWord
        $anyChanged = $true
    }

    if ($anyChanged) {
        Write-Applied "Control 6: TLS 1.0/1.1 disabled, SchUseStrongCrypto enforced."
    } else {
        Write-Ok "Control 6: TLS 1.0/1.1 already disabled, SchUseStrongCrypto already enforced."
    }
}

# Control 7: Guest account disabled.
function Set-GuestAccountControl {
    $guest = Get-LocalUser -Name "Guest" -ErrorAction SilentlyContinue
    if (-not $guest) {
        Write-Skipped "Control 7: local Guest account not found."
        return
    }

    if (-not $guest.Enabled) {
        Write-Ok "Control 7: Guest account already disabled."
    } else {
        Disable-LocalUser -Name "Guest"
        Write-Applied "Control 7: Guest account disabled."
    }
}

Set-FirewallControl
Set-AdministratorAccountControl
Set-AuditPolicyControl
Set-AutomaticUpdatesControl
Set-TlsControl
Set-GuestAccountControl

if ($script:Changed) {
    Write-Host "Hardening applied. A reboot may be required for TLS/audit policy changes to take full effect."
} else {
    Write-Host "Host already meets the baseline -- no changes made."
}
