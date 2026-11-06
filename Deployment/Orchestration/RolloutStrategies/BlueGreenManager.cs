// ============================================================================
//  Project:        Deployment
//  Module:         Orchestration.RolloutStrategies
//  File:           BlueGreenManager.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic Blue/Green promotion:
//                  runs every supplied gate, in the exact order supplied,
//                  against the idle slot; traffic switches to the idle slot
//                  only if every gate passes. Any single failed gate is an
//                  explicit rollback trigger -- the active slot stays
//                  unchanged, never a partial switch. Deploying the idle
//                  slot's new version is assumed to already have happened
//                  via DeploymentEngine steps before PromoteAsync is called
//                  -- this manager's job is gating and the traffic switch
//                  decision, not the deploy itself.
// ============================================================================
namespace Deployment.Orchestration.RolloutStrategies;

/// <summary>The two Blue/Green environment slots.</summary>
public enum BlueGreenSlot
{
    /// <summary>The Blue environment slot.</summary>
    Blue,

    /// <summary>The Green environment slot.</summary>
    Green,
}

/// <summary>A single Blue/Green gate check against the idle slot.</summary>
public delegate Task<bool> BlueGreenHealthCheck(BlueGreenSlot idleSlot, CancellationToken cancellationToken);

/// <summary>A single, named Blue/Green promotion gate.</summary>
public sealed record BlueGreenGate(string GateName, BlueGreenHealthCheck Check);

/// <summary>The deterministic outcome of a Blue/Green promotion attempt.</summary>
public sealed record BlueGreenResult(BlueGreenSlot ActiveSlot, bool Promoted, IReadOnlyList<string> FailedGateNames);

/// <summary>Deterministic Blue/Green promotion: gate the idle slot, then switch traffic only if every gate passes.</summary>
public static class BlueGreenManager
{
    /// <summary>Returns the slot opposite <paramref name="activeSlot"/> -- the idle slot a new version deploys to.</summary>
    public static BlueGreenSlot IdleSlot(BlueGreenSlot activeSlot) => activeSlot == BlueGreenSlot.Blue ? BlueGreenSlot.Green : BlueGreenSlot.Blue;

    /// <summary>
    /// Runs every gate in <paramref name="gates"/>, in the order supplied, against the idle slot
    /// (<see cref="IdleSlot"/> of <paramref name="currentActiveSlot"/>). Switches ActiveSlot to the idle
    /// slot only if every gate passes; any failed gate is an explicit rollback trigger that leaves
    /// ActiveSlot unchanged.
    /// </summary>
    public static async Task<BlueGreenResult> PromoteAsync(
        BlueGreenSlot currentActiveSlot,
        IReadOnlyList<BlueGreenGate> gates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gates);

        BlueGreenSlot idleSlot = IdleSlot(currentActiveSlot);
        var failedGateNames = new List<string>();

        foreach (BlueGreenGate gate in gates)
        {
            bool passed;
            try
            {
                passed = await gate.Check(idleSlot, cancellationToken);
            }
            catch (Exception)
            {
                passed = false;
            }

            if (!passed)
            {
                failedGateNames.Add(gate.GateName);
            }
        }

        return failedGateNames.Count > 0
            ? new BlueGreenResult(currentActiveSlot, false, failedGateNames)
            : new BlueGreenResult(idleSlot, true, Array.Empty<string>());
    }
}
