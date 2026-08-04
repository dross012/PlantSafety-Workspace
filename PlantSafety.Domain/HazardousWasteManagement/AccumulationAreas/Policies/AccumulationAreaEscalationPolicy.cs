// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.AccumulationAreas
//  Layer:          Domain
//  File:           AccumulationAreaEscalationPolicy.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain policy governing escalation of overdue accumulation areas to
//                  RiskManagement and Alerts.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Policies;

/// <summary>Determines whether an AccumulationArea in time-limit warning must escalate.</summary>
public static class AccumulationAreaEscalationPolicy
{
    /// <summary>Returns <see langword="true"/> when the area's warning status should escalate to RiskManagement.</summary>
    public static bool RequiresRiskEscalation(PlantSafety.Domain.HazardousWasteManagement.AccumulationAreas.Aggregates.AccumulationArea aggregate)
        => aggregate.Status == AccumulationAreaStatus.TimeLimitWarning;
}
