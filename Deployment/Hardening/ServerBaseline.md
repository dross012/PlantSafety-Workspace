# Server Hardening Baseline

Phase 18 (Deployment Modernization). The deterministic baseline every on-prem server must meet
before it may host a PlantSafety deployment. `HardeningOrchestrator.ValidateBaseline` checks exactly
these facts (as `HardeningBaselineFacts`) — this document and that C# rule pack must never drift
apart; if you add a checklist item here, add the matching field and check in
[`HardeningOrchestrator.cs`](HardeningOrchestrator.cs) in the same change.

## Checklist

| # | Control | Linux mechanism | Windows mechanism |
|---|---|---|---|
| 1 | Host firewall enabled, default-deny inbound | `ufw`/`firewalld` enabled | Windows Defender Firewall enabled, all profiles |
| 2 | Remote root/Administrator login disabled | `PermitRootLogin no` in `sshd_config` | Built-in Administrator account disabled for RDP |
| 3 | Password authentication disabled (key-only) | `PasswordAuthentication no` in `sshd_config` | N/A (RDP uses NLA + domain/local auth; see control 7) |
| 4 | Audit logging enabled | `auditd` running and enabled at boot | Advanced Audit Policy: Logon/Object Access enabled |
| 5 | Automatic security updates enabled | `unattended-upgrades` (Debian/Ubuntu) or `dnf-automatic` (RHEL) enabled | Windows Update set to automatic |
| 6 | Minimum TLS 1.2 enforced | OpenSSL/system crypto policy set to disallow TLS 1.0/1.1 | `SchUseStrongCrypto` + disabled TLS 1.0/1.1 registry keys |
| 7 | Guest/anonymous account disabled | No shared/guest accounts present | Guest account disabled |

## Idempotency contract

Both [`HardeningScripts/apply_hardening.sh`](HardeningScripts/apply_hardening.sh) and
[`HardeningScripts/apply_hardening.ps1`](HardeningScripts/apply_hardening.ps1) check each control's
current state before changing it, and print `[OK]`/`[APPLIED]`/`[SKIPPED]` per control. Re-running
either script against an already-hardened host must produce only `[OK]` lines and exit 0 — this is
the idempotency guarantee the acceptance criteria require ("automated baseline scripts apply
idempotently").

## Drift detection

Once a host is hardened, `HardeningOrchestrator.ValidateBaseline` (fed with facts collected by
whichever host-facing tool gathers them — the scripts themselves, a config-management run, or a
manual audit) reports `HardeningReport.CriticalDeviationCount`. A pilot cluster passes acceptance
when this count is 0 across every host.
