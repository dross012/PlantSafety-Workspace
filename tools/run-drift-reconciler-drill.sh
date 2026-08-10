#!/usr/bin/env bash
# ============================================================================
# tools/run-drift-reconciler-drill.sh
#
# CI/local convenience wrapper around Deployment/Tools/DriftReconcilerDrill:
# runs the drift detection and remediation simulation (healthy baseline,
# out-of-band manual drift detected, remediation applied, declared-state
# advance detected as environment staleness, and the environment catching up
# via redeployment) and writes the evidence report to the path given
# (default: drift-reconciler-drill-evidence.json in the repo root). Exits
# non-zero if any step's outcome doesn't match its expectation.
# ============================================================================
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

output_path="${1:-drift-reconciler-drill-evidence.json}"

dotnet run --project Deployment/Tools/DriftReconcilerDrill/DriftReconcilerDrill.csproj -c Release --no-launch-profile -- "$output_path"
