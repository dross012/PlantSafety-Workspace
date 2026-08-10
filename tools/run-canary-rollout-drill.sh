#!/usr/bin/env bash
# ============================================================================
# tools/run-canary-rollout-drill.sh
#
# CI/local convenience wrapper around Deployment/Tools/CanaryRolloutDrill:
# runs the canary rollout simulation (full promotion, DeploymentEngine
# finalization, zero-drift registry reconciliation, a transient gate-flake
# retry, and an error-rate threshold breach that triggers reverse-order
# compensation) and writes the evidence report to the path given (default:
# canary-rollout-drill-evidence.json in the repo root). Exits non-zero if any
# step's outcome doesn't match its expectation.
# ============================================================================
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

output_path="${1:-canary-rollout-drill-evidence.json}"

dotnet run --project Deployment/Tools/CanaryRolloutDrill/CanaryRolloutDrill.csproj -c Release --no-launch-profile -- "$output_path"
