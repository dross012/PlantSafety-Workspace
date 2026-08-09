// ============================================================================
//  Project:        Shared.EmployeeDomain
//  Module:         Abstractions.Scoring
//  File:           EmployeeSafetyScore.cs
//  Created:        2026-08-08
//  Description:    Phase C.1: a deterministic, explainable safety score for
//                  an employee, recomputed by EmployeeAggregate whenever
//                  training/certification/exposure/injury data changes.
//                  Deterministic arithmetic only (no ML) per this program's
//                  standing rule.
// ============================================================================
namespace Shared.EmployeeDomain.Abstractions.Scoring;

/// <summary>A deterministic safety score for an employee, in the range [0, 100].</summary>
public sealed record EmployeeSafetyScore(
    double ScoreValue,
    DateTime ComputedAtUtc)
{
    /// <summary>
    /// Computes a deterministic safety score: starts at 100, subtracts 15 per recorded
    /// injury and 2 per recorded exposure, adds 3 per active certification and 1 per
    /// completed training code, then clamps to [0, 100].
    /// </summary>
    public static EmployeeSafetyScore Compute(int injuryCount, int exposureCount, int certificationCount, int completedTrainingCount, DateTime nowUtc)
    {
        double raw = 100 - (injuryCount * 15) - (exposureCount * 2) + (certificationCount * 3) + completedTrainingCount;
        double clamped = Math.Clamp(raw, 0, 100);
        return new EmployeeSafetyScore(clamped, nowUtc);
    }
}
