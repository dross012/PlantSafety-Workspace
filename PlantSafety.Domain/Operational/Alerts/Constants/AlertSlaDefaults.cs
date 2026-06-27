// ============================================================================
//  File:           AlertSlaDefaults.cs
//  Created:        2026-05-26
//  Description:
//      Default SLA durations for Alerts based on severity.
// ============================================================================

using System;

namespace PlantSafety.Domain.Operational.Alerts.Constants;

public static class AlertSlaDefaults
{
    public static readonly TimeSpan Emergency = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Critical = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan High = TimeSpan.FromHours(1);
    public static readonly TimeSpan Medium = TimeSpan.FromHours(4);
    public static readonly TimeSpan Low = TimeSpan.FromHours(24);
}
