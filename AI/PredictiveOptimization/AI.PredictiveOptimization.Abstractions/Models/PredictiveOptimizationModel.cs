// ============================================================================
//  Project:        AI.PredictiveOptimization
//  Module:         Abstractions.Models
//  File:           PredictiveOptimizationModel.cs
//  Created:        2026-08-08
//  Description:    Phase E.6 (Path E: AI-Driven Safety Automation): one
//                  optimization target's deterministic forecast, weighted
//                  score, and anomaly flag. OptimizationTarget covers
//                  exactly the six dimensions Path E's spec requires the
//                  engine to optimize.
// ============================================================================
namespace AI.PredictiveOptimization.Abstractions.Models;

/// <summary>The dimension a <see cref="PredictiveOptimizationModel"/> optimizes.</summary>
public enum OptimizationTarget
{
    /// <summary>Likelihood of another hazard occurrence.</summary>
    HazardLikelihood,

    /// <summary>Likelihood of severity escalation.</summary>
    SeverityLikelihood,

    /// <summary>Likelihood of employee exposure.</summary>
    ExposureLikelihood,

    /// <summary>Likelihood of machine failure.</summary>
    MachineFailureLikelihood,

    /// <summary>Workflow execution efficiency.</summary>
    WorkflowEfficiency,

    /// <summary>Compliance/risk score.</summary>
    ComplianceRiskScore,
}

/// <summary>One optimization target's deterministic forecast, weighted score, and anomaly flag.</summary>
public sealed record PredictiveOptimizationModel(
    OptimizationTarget Target,
    double MovingAverage,
    double TrendSlope,
    double ForecastedValue,
    double Weight,
    double WeightedScore,
    bool IsAnomalous);
