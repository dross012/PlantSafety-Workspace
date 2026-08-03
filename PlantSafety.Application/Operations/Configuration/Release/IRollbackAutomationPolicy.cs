// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operations.Configuration.Release
//  Layer:          Application
//  File:           IRollbackAutomationPolicy.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Phase 7 (SaaS Productization Blueprint §7.2 Release Pipeline) pure,
//                  deterministic policy deciding whether a bad-release signal (an elevated
//                  error rate, as reported by the Phase 6 PlatformHealthDashboard) warrants
//                  an automatic feature-flag rollback. Deliberately I/O-free -- callers
//                  (see Handlers\TriggerRollbackCommandHandler or a scheduled job) fetch the
//                  health signal and pass it in, so this contract is fully unit-testable.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Operations.Configuration.Release;

/// <summary>
/// Deterministically decides whether an observed error rate warrants triggering an
/// automatic feature-flag rollback.
/// </summary>
public interface IRollbackAutomationPolicy
{
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="observedErrorRate"/> exceeds
    /// <paramref name="errorRateThreshold"/> and enough samples were observed to trust the
    /// signal (a single anomalous request should not trigger a rollback).
    /// </summary>
    /// <param name="observedErrorRate">The observed error rate, 0.0-1.0 (matches <c>PlatformHealthDashboardItemDto.ErrorRate</c>).</param>
    /// <param name="errorRateThreshold">The configured error-rate threshold, 0.0-1.0, above which a rollback should trigger.</param>
    /// <param name="observedRequestCount">The total number of requests the error rate was computed over.</param>
    /// <param name="minimumSampleSize">The minimum request count required to trust the signal (guards against a single request producing a 100% error rate).</param>
    bool ShouldRollback(double observedErrorRate, double errorRateThreshold, int observedRequestCount, int minimumSampleSize);
}
