#!/usr/bin/env bash
# ============================================================================
# tools/validate-manifest.sh
#
# CI/local convenience wrapper around Deployment/Tools/ManifestValidator.
# Validates every sample manifest under Deployment/ConfigSchema/SampleManifests
# (or the files passed as arguments) against the real ConfigValidator/
# RoleValidator pipeline, and runs DeterministicSerializer twice per file to
# confirm the serialized output is byte-for-byte stable. Exits non-zero on
# the first failure so it can gate CI directly.
# ============================================================================
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

manifests=("$@")
if [ ${#manifests[@]} -eq 0 ]; then
    mapfile -t manifests < <(find Deployment/ConfigSchema/SampleManifests -name '*.json' | sort)
fi

echo "== ManifestValidator =="
dotnet run --project Deployment/Tools/ManifestValidator/ManifestValidator.csproj -c Release --no-launch-profile -- "${manifests[@]}"

echo
echo "== DeterministicSerializer (round-trip check) =="
for manifest in "${manifests[@]}"; do
    out1="$(mktemp)"
    out2="$(mktemp)"
    dotnet run --project Deployment/Tools/DeterministicSerializer/DeterministicSerializer.csproj -c Release --no-launch-profile -- "$manifest" > "$out1"
    dotnet run --project Deployment/Tools/DeterministicSerializer/DeterministicSerializer.csproj -c Release --no-launch-profile -- "$manifest" > "$out2"

    if ! cmp -s "$out1" "$out2"; then
        echo "[NON-DETERMINISTIC] $manifest -- two serialization runs produced different output:"
        diff "$out1" "$out2" || true
        rm -f "$out1" "$out2"
        exit 1
    fi

    echo "[DETERMINISTIC] $manifest"
    rm -f "$out1" "$out2"
done
