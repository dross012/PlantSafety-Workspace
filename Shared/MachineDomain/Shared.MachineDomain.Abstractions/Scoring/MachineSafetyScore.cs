// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Scoring
//  File:           MachineSafetyScore.cs
//  Created:        2026-08-08
//  Description:    Phase C.2: a deterministic, explainable safety score for
//                  a machine, recomputed by MachineAggregate whenever
//                  maintenance/failure data changes. Deterministic
//                  arithmetic only (no ML), mirroring Shared.EmployeeDomain's
//                  EmployeeSafetyScore precedent.
// ============================================================================
namespace Shared.MachineDomain.Abstractions.Scoring;

/// <summary>A deterministic safety score for a machine, in the range [0, 100].</summary>
public sealed record MachineSafetyScore(
    double ScoreValue,
    DateTime ComputedAtUtc)
{
    /// <summary>
    /// Computes a deterministic safety score: starts at 100, subtracts 20 per recorded
    /// failure and 10 if maintenance is overdue, then clamps to [0, 100].
    /// </summary>
    public static MachineSafetyScore Compute(int failureCount, bool maintenanceOverdue, DateTime nowUtc)
    {
        double raw = 100 - (failureCount * 20) - (maintenanceOverdue ? 10 : 0);
        double clamped = Math.Clamp(raw, 0, 100);
        return new MachineSafetyScore(clamped, nowUtc);
    }
}
