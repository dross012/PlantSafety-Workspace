// ============================================================================
//  File:           AlertRebuildMapper.cs
//  Created:        2026-05-26
//  Description:
//      Rebuilds an Alert aggregate from a snapshot. Used by rebuild pipelines.
// ============================================================================

using PlantSafety.Domain.Operational.Alerts.Snapshots;

namespace PlantSafety.Domain.Operational.Alerts.Mappers;

public static class AlertRebuildMapper
{
    public static Alert Rebuild(AlertSnapshot snapshot)
    {
        var alert = new Alert();

        alert.RebuildFromSnapshot(snapshot);

        return alert;
    }
}
