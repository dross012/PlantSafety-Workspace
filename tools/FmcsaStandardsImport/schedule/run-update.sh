#!/usr/bin/env bash
# Cron-friendly wrapper. Example crontab entry for a weekly Sunday 03:00 run:
#   0 3 * * 0 /path/to/tools/FmcsaStandardsImport/schedule/run-update.sh >> /path/to/tools/FmcsaStandardsImport/schedule/logs/cron.log 2>&1
set -euo pipefail
TOOL_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PUBLISH_DIR="$TOOL_DIR/publish"

if [ ! -f "$PUBLISH_DIR/FmcsaStandardsImport" ] && [ ! -f "$PUBLISH_DIR/FmcsaStandardsImport.dll" ]; then
    dotnet publish "$TOOL_DIR" -c Release -o "$PUBLISH_DIR"
fi

dotnet "$PUBLISH_DIR/FmcsaStandardsImport.dll"
