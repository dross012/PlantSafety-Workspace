// ============================================================================
//  Project:        AI.HazardUnderstanding
//  Module:         Abstractions.Engine
//  File:           HazardUnderstandingEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.2 (Path E: AI-Driven Safety Automation): builds
//                  a full HazardSemanticModel by composing the real,
//                  already-built deterministic rule engines from Paths
//                  B/C -- no hazard classification/propagation/severity/
//                  regulatory logic is reimplemented here:
//                    - classify   -> Shared.FacilityDomain's HazardCategoryCatalog (B.2);
//                                    HazardOntology (Phase E.2, new) adds the related-hazards
//                                    semantic network a flat category rollup can't express
//                    - propagation-> Shared.FacilityDomain's HazardPropagationRules (C.6)
//                    - chains     -> Shared.FacilityDomain's HazardChainBuilder (C.9),
//                                    with severity escalating one step per
//                                    propagation stage (capped at Catastrophic)
//                    - severity   -> Shared.FacilityDomain's SeverityInferenceRules (C.3)
//                    - regulatory -> Shared.FacilityDomain's RegulatoryInferenceRules (C.4)
// ============================================================================
using Shared.FacilityDomain.Abstractions.Chains;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

using AI.HazardUnderstanding.Abstractions.Ontology;
using AI.HazardUnderstanding.Abstractions.Semantics;

namespace AI.HazardUnderstanding.Abstractions.Engine;

/// <summary>Builds a deterministic <see cref="HazardSemanticModel"/> by composing Path B/C's real rule engines.</summary>
public static class HazardUnderstandingEngine
{
    /// <summary>Builds a full semantic understanding of the given hazard-understanding input.</summary>
    public static HazardUnderstandingOutput Understand(HazardUnderstandingInput input, DateTime? nowUtc = null)
    {
        DateTime generatedAtUtc = nowUtc ?? DateTime.UtcNow;

        HazardCategory category = HazardCategoryCatalog.ForHazardType(input.HazardType);
        IReadOnlyList<HazardType> relatedHazards = HazardOntology.RelatedHazardsOf(input.HazardType);
        SeverityLevel severity = SeverityInferenceRules.Infer(input.HazardType, input.EmployeeInjuryCount, input.MachineFailureCount, input.EventChainLength);
        IReadOnlyList<PropagationStage> propagationChain = HazardPropagationRules.BuildChain(input.OriginPropagationStage);
        IReadOnlyList<RegulatoryJurisdiction> regulatoryImpact = RegulatoryInferenceRules.Infer(input.HazardType, severity);
        HazardChain multiHazardChain = BuildEscalatingChain(propagationChain, severity, generatedAtUtc);

        var model = new HazardSemanticModel(input.HazardType, category, relatedHazards, severity, propagationChain, multiHazardChain, regulatoryImpact);
        return new HazardUnderstandingOutput(model, generatedAtUtc);
    }

    private static HazardChain BuildEscalatingChain(IReadOnlyList<PropagationStage> stages, SeverityLevel baselineSeverity, DateTime startAtUtc)
    {
        var builder = new HazardChainBuilder();
        for (int i = 0; i < stages.Count; i++)
        {
            SeverityLevel stageSeverity = (SeverityLevel)Math.Min((int)SeverityLevel.Catastrophic, (int)baselineSeverity + i);
            builder.AddStage(stages[i], stageSeverity, startAtUtc.AddMinutes(i));
        }

        return builder.Build();
    }
}
