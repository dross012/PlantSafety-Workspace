#!/usr/bin/env bash
# ============================================================================
#  Phase 18 (Deployment Modernization) -- idempotent Linux hardening script.
#  Applies the checklist in ../ServerBaseline.md. Every control checks
#  current state before changing it and prints [OK]/[APPLIED]/[SKIPPED], so
#  repeated runs against an already-hardened host produce only [OK] lines
#  and exit 0.
#
#  Usage: sudo ./apply_hardening.sh
# ============================================================================
set -euo pipefail

SSHD_CONFIG="/etc/ssh/sshd_config"
CHANGED=0

log_ok()      { echo "[OK]      $1"; }
log_applied() { echo "[APPLIED] $1"; CHANGED=1; }
log_skipped() { echo "[SKIPPED] $1"; }

require_root() {
  if [[ "${EUID:-$(id -u)}" -ne 0 ]]; then
    echo "This script must be run as root (sudo)." >&2
    exit 1
  fi
}

# Control 1: host firewall enabled, default-deny inbound.
control_firewall() {
  if command -v ufw >/dev/null 2>&1; then
    if ufw status | grep -q "Status: active"; then
      log_ok "Control 1: ufw already active."
    else
      ufw --force enable
      ufw default deny incoming
      log_applied "Control 1: ufw enabled, default-deny inbound set."
    fi
  elif command -v firewall-cmd >/dev/null 2>&1; then
    if systemctl is-active --quiet firewalld; then
      log_ok "Control 1: firewalld already active."
    else
      systemctl enable --now firewalld
      log_applied "Control 1: firewalld enabled and started."
    fi
  else
    log_skipped "Control 1: no supported firewall manager (ufw/firewalld) found."
  fi
}

# Control 2: remote root login disabled.
control_no_root_login() {
  if [[ -f "$SSHD_CONFIG" ]]; then
    if grep -qE "^\s*PermitRootLogin\s+no\s*$" "$SSHD_CONFIG"; then
      log_ok "Control 2: PermitRootLogin already disabled."
    else
      sed -i -E "s/^\s*#?\s*PermitRootLogin.*/PermitRootLogin no/" "$SSHD_CONFIG"
      grep -qE "^\s*PermitRootLogin\s+no\s*$" "$SSHD_CONFIG" || echo "PermitRootLogin no" >> "$SSHD_CONFIG"
      log_applied "Control 2: PermitRootLogin set to no."
    fi
  else
    log_skipped "Control 2: $SSHD_CONFIG not found."
  fi
}

# Control 3: password authentication disabled (key-only).
control_no_password_auth() {
  if [[ -f "$SSHD_CONFIG" ]]; then
    if grep -qE "^\s*PasswordAuthentication\s+no\s*$" "$SSHD_CONFIG"; then
      log_ok "Control 3: PasswordAuthentication already disabled."
    else
      sed -i -E "s/^\s*#?\s*PasswordAuthentication.*/PasswordAuthentication no/" "$SSHD_CONFIG"
      grep -qE "^\s*PasswordAuthentication\s+no\s*$" "$SSHD_CONFIG" || echo "PasswordAuthentication no" >> "$SSHD_CONFIG"
      log_applied "Control 3: PasswordAuthentication set to no."
    fi
  else
    log_skipped "Control 3: $SSHD_CONFIG not found."
  fi
}

# Control 4: audit logging enabled.
control_audit_logging() {
  if command -v auditctl >/dev/null 2>&1; then
    if systemctl is-active --quiet auditd; then
      log_ok "Control 4: auditd already active."
    else
      systemctl enable --now auditd
      log_applied "Control 4: auditd enabled and started."
    fi
  else
    log_skipped "Control 4: auditd not installed."
  fi
}

# Control 5: automatic security updates enabled.
control_auto_updates() {
  if command -v unattended-upgrade >/dev/null 2>&1; then
    if systemctl is-enabled --quiet unattended-upgrades 2>/dev/null; then
      log_ok "Control 5: unattended-upgrades already enabled."
    else
      systemctl enable --now unattended-upgrades || true
      log_applied "Control 5: unattended-upgrades enabled."
    fi
  elif command -v dnf-automatic >/dev/null 2>&1; then
    if systemctl is-enabled --quiet dnf-automatic.timer 2>/dev/null; then
      log_ok "Control 5: dnf-automatic.timer already enabled."
    else
      systemctl enable --now dnf-automatic.timer
      log_applied "Control 5: dnf-automatic.timer enabled."
    fi
  else
    log_skipped "Control 5: no supported automatic-update mechanism found."
  fi
}

# Control 7: no shared/guest accounts present (Linux has no built-in "Guest" account; verify none was added).
control_no_guest_account() {
  if id -u guest >/dev/null 2>&1; then
    log_skipped "Control 7: a \"guest\" account exists -- remove it manually and re-run (userdel is destructive, not run automatically)."
  else
    log_ok "Control 7: no \"guest\" account present."
  fi
}

main() {
  require_root
  control_firewall
  control_no_root_login
  control_no_password_auth
  control_audit_logging
  control_auto_updates
  control_no_guest_account

  if [[ "$CHANGED" -eq 1 ]]; then
    echo "Hardening applied. Restart sshd for SSH-related changes to take effect: systemctl restart sshd"
  else
    echo "Host already meets the baseline -- no changes made."
  fi
}

main "$@"
