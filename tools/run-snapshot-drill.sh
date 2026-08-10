#!/usr/bin/env bash
# ============================================================================
# tools/run-snapshot-drill.sh
#
# CI/local convenience wrapper around Deployment/Tools/SnapshotDrill: runs the
# ConfigRegistry publish -> diff -> rollback -> audit-chain-verify drill and
# writes the evidence report to the path given (default: snapshot-drill-
# evidence.json in the repo root). Exits non-zero if any drill step fails.
# ============================================================================
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

output_path="${1:-snapshot-drill-evidence.json}"

dotnet run --project Deployment/Tools/SnapshotDrill/SnapshotDrill.csproj -c Release --no-launch-profile -- "$output_path"
