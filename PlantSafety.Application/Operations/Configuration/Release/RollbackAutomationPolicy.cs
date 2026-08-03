// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration.Release
//  Layer:          Application
//  File:           RollbackAutomationPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default IRollbackAutomationPolicy implementation. Pure threshold + minimum
//                  sample-size check -- rollback triggers only when BOTH the observed error
//                  rate exceeds the threshold AND enough requests were observed to trust the
//                  signal. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.Configuration.Release;

/// <summary>Default <see cref="IRollbackAutomationPolicy"/> implementation.</summary>
public sealed class RollbackAutomationPolicy : IRollbackAutomationPolicy
{
    /// <inheritdoc/>
    public bool ShouldRollback(double observedErrorRate, double errorRateThreshold, int observedRequestCount, int minimumSampleSize)
    {
        if (observedRequestCount < minimumSampleSize)
        {
            // Not enough traffic to trust the signal -- e.g. 1 failed request out of 1
            // total would otherwise look like a 100% error rate.
            return false;
        }

        return observedErrorRate > errorRateThreshold;
    }
}
