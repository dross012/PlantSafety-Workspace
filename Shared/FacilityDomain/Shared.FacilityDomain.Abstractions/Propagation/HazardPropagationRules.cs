// ============================================================================
//  Project:        Shared.FacilityDomain
//  Module:         Abstractions.Propagation
//  File:           HazardPropagationRules.cs
//  Created:        2026-08-08
//  Description:    Phase C.6: deterministic hazard propagation chains --
//                  Fire -> Smoke -> Evacuation, Spill -> Vapor -> Exposure,
//                  MachineFailure -> Injury -> Shutdown, per Path C's spec.
//                  A fixed lookup table, not inference from free text
//                  (mirrors HazardInferenceRules' precedent of deterministic
//                  rules, but here the input is already a classified stage,
//                  not raw text).
// ============================================================================
namespace Shared.FacilityDomain.Abstractions.Propagation;

/// <summary>Deterministic hazard propagation chains from an originating hazard to its downstream consequences.</summary>
public static class HazardPropagationRules
{
    private static readonly IReadOnlyDictionary<PropagationStage, PropagationStage> NextStageByStage = new Dictionary<PropagationStage, PropagationStage>
    {
        [PropagationStage.Fire] = PropagationStage.Smoke,
        [PropagationStage.Smoke] = PropagationStage.Evacuation,
        [PropagationStage.Spill] = PropagationStage.Vapor,
        [PropagationStage.Vapor] = PropagationStage.Exposure,
        [PropagationStage.MachineFailure] = PropagationStage.Injury,
        [PropagationStage.Injury] = PropagationStage.Shutdown,
    };

    /// <summary>Returns the single deterministic next stage after <paramref name="stage"/>, or null if <paramref name="stage"/> is terminal.</summary>
    public static PropagationStage? NextStage(PropagationStage stage) =>
        NextStageByStage.TryGetValue(stage, out PropagationStage next) ? next : null;

    /// <summary>Returns the full deterministic chain starting at <paramref name="originStage"/>, inclusive of the origin, ending at the first terminal stage.</summary>
    public static IReadOnlyList<PropagationStage> BuildChain(PropagationStage originStage)
    {
        var chain = new List<PropagationStage> { originStage };
        PropagationStage? current = originStage;

        while ((current = NextStage(current.Value)) is not null)
        {
            chain.Add(current.Value);
        }

        return chain;
    }

    /// <summary>True if <paramref name="stage"/> has no further propagation (a terminal consequence).</summary>
    public static bool IsTerminal(PropagationStage stage) => !NextStageByStage.ContainsKey(stage);
}
