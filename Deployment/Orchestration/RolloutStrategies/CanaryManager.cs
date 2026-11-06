// ============================================================================
//  Project:        Deployment
//  Module:         Orchestration.RolloutStrategies
//  File:           CanaryManager.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic canary rollout: walks
//                  the supplied stage percentages in strictly ascending
//                  order (matching ConfigValidator's own validation of
//                  DeploymentConfig.CanaryStagePercentages), running one
//                  gate check per stage. The first failed stage is an
//                  explicit rollback trigger that stops all further
//                  promotion -- traffic never advances past a failed gate.
// ============================================================================
namespace Deployment.Orchestration.RolloutStrategies;

/// <summary>A single canary stage gate check, evaluated at a given traffic percentage.</summary>
public delegate Task<bool> CanaryStageGate(int trafficPercentage, CancellationToken cancellationToken);

/// <summary>The deterministic outcome of a single canary stage.</summary>
public sealed record CanaryStageResult(int TrafficPercentage, bool Passed, string? FailureReason);

/// <summary>The deterministic outcome of a full canary rollout.</summary>
public sealed record CanaryResult(int ReachedTrafficPercentage, bool FullyPromoted, IReadOnlyList<CanaryStageResult> StageResults);

/// <summary>Deterministic canary rollout: gate each ascending traffic stage, stop at the first failure.</summary>
public static class CanaryManager
{
    /// <summary>
    /// Walks <paramref name="stagePercentages"/> in ascending order, running <paramref name="gate"/>
    /// once per stage. Stops at the first failed stage (rollback trigger) -- traffic never advances
    /// past a failed gate. FullyPromoted is true only if every stage passed and the final stage was
    /// 100%.
    /// </summary>
    public static async Task<CanaryResult> ExecuteAsync(
        IReadOnlyList<int> stagePercentages,
        CanaryStageGate gate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stagePercentages);
        ArgumentNullException.ThrowIfNull(gate);

        List<int> orderedStages = stagePercentages.OrderBy(p => p).ToList();
        var stageResults = new List<CanaryStageResult>();
        int reachedTrafficPercentage = 0;

        foreach (int stage in orderedStages)
        {
            bool passed;
            string? failureReason = null;
            try
            {
                passed = await gate(stage, cancellationToken);
                if (!passed)
                {
                    failureReason = $"Gate failed at {stage}% traffic.";
                }
            }
            catch (Exception ex)
            {
                passed = false;
                failureReason = ex.Message;
            }

            stageResults.Add(new CanaryStageResult(stage, passed, failureReason));

            if (!passed)
            {
                break;
            }

            reachedTrafficPercentage = stage;
        }

        bool fullyPromoted = stageResults.Count > 0 && stageResults[^1].Passed && reachedTrafficPercentage == 100;

        return new CanaryResult(reachedTrafficPercentage, fullyPromoted, stageResults);
    }
}
