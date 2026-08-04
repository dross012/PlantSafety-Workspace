// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless rules supporting the "sensor integrity pass rate" KPI and the
//                  recurring integrity-check scheduling cadence.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Enums;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Rules;

/// <summary>Pure rules supporting sensor-integrity KPIs and scheduling.</summary>
public static class SensorIntegrityRules
{
    /// <summary>Default recurring check interval, in days, absent a criticality-specific override.</summary>
    public const int DefaultCheckIntervalDays = 30;

    /// <summary>Computes the pass rate (0.0-1.0) across a set of check results.</summary>
    public static double ComputePassRate(IReadOnlyCollection<SensorIntegrityCheckResult> results)
    {
        if (results.Count == 0)
            return 1.0;

        var passCount = results.Count(r => r == SensorIntegrityCheckResult.Pass);
        return (double)passCount / results.Count;
    }

    /// <summary>Computes the next due date for a recurring check, from the given check's completion timestamp.</summary>
    public static DateTime ComputeNextCheckDueUtc(DateTime checkedAtUtc, int intervalDays = DefaultCheckIntervalDays)
        => checkedAtUtc.AddDays(intervalDays);
}
