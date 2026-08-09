// ============================================================================
//  Project:        AI.HazardUnderstanding.Tests
//  File:           HazardUnderstandingEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.2 unit tests for HazardUnderstandingEngine.Understand:
//                  category classification, propagation chain, escalating
//                  multi-hazard chain, severity, and regulatory impact.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

using AI.HazardUnderstanding.Abstractions.Engine;
using AI.HazardUnderstanding.Abstractions.Semantics;

using Xunit;

namespace AI.HazardUnderstanding.Tests.Engine;

public sealed class HazardUnderstandingEngineTests
{
    [Fact]
    public void Understand_Fire_ClassifiesAsPhysicalSafety()
    {
        var input = new HazardUnderstandingInput(HazardType.Fire, PropagationStage.Fire, 0, 0, 1);

        HazardUnderstandingOutput output = HazardUnderstandingEngine.Understand(input);

        output.Model.Category.Should().Be(HazardCategory.PhysicalSafety);
    }

    [Fact]
    public void Understand_Fire_IncludesRelatedHazards()
    {
        var input = new HazardUnderstandingInput(HazardType.Fire, PropagationStage.Fire, 0, 0, 1);

        HazardUnderstandingOutput output = HazardUnderstandingEngine.Understand(input);

        output.Model.RelatedHazards.Should().Contain(HazardType.ChemicalSpill);
    }

    [Fact]
    public void Understand_Fire_ProducesFireSmokeEvacuationPropagation()
    {
        var input = new HazardUnderstandingInput(HazardType.Fire, PropagationStage.Fire, 0, 0, 1);

        HazardUnderstandingOutput output = HazardUnderstandingEngine.Understand(input);

        output.Model.PropagationChain.Should().Equal(PropagationStage.Fire, PropagationStage.Smoke, PropagationStage.Evacuation);
    }

    [Fact]
    public void Understand_MultiHazardChain_MatchesPropagationChainOrderAndEscalatesSeverity()
    {
        var input = new HazardUnderstandingInput(HazardType.Other, PropagationStage.MachineFailure, 0, 0, 1);

        HazardUnderstandingOutput output = HazardUnderstandingEngine.Understand(input);

        output.Model.MultiHazardChain.Nodes.Select(n => n.Stage).Should().Equal(output.Model.PropagationChain);
        (output.Model.MultiHazardChain.Nodes[0].Severity <= output.Model.MultiHazardChain.Nodes[^1].Severity).Should().BeTrue();
    }

    [Fact]
    public void Understand_MultipleInjuries_AssessesCatastrophicSeverity()
    {
        var input = new HazardUnderstandingInput(HazardType.Injury, PropagationStage.Injury, EmployeeInjuryCount: 3, MachineFailureCount: 0, EventChainLength: 1);

        HazardUnderstandingOutput output = HazardUnderstandingEngine.Understand(input);

        output.Model.AssessedSeverity.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void Understand_ChemicalSpill_IncludesEpaInRegulatoryImpact()
    {
        var input = new HazardUnderstandingInput(HazardType.ChemicalSpill, PropagationStage.Spill, 0, 0, 1);

        HazardUnderstandingOutput output = HazardUnderstandingEngine.Understand(input);

        output.Model.RegulatoryImpact.Should().Contain(RegulatoryJurisdiction.EPA);
    }
}
