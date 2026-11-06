#!/usr/bin/env bash
# ============================================================================
# tools/run-governance-drill.sh
#
# CI/local convenience wrapper around Deployment/Tools/GovernanceDrill: runs
# the DeploymentGovernanceEvaluator blocking scenarios (suspended tenant,
# quota exceeded, production without an approval role, plus two compliant
# baselines) and writes the evidence report to the path given (default:
# governance-drill-evidence.json in the repo root). Exits non-zero if any
# scenario's outcome doesn't match its expectation.
# ============================================================================
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

output_path="${1:-governance-drill-evidence.json}"

dotnet run --project Deployment/Tools/GovernanceDrill/GovernanceDrill.csproj -c Release --no-launch-profile -- "$output_path"
