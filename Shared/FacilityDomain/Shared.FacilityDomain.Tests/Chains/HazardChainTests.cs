// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           HazardChainTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.9 unit tests for HazardChainBuilder/HazardChain:
//                  the three named example chains from Path C's spec, plus
//                  Length/OriginStage/TerminalStage/OverallSeverity.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Chains;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

using Xunit;

namespace Shared.FacilityDomain.Tests.Chains;

public sealed class HazardChainTests
{
    [Fact]
    public void Build_SpillFireInjuryChain_PreservesOrder()
    {
        HazardChain chain = new HazardChainBuilder()
            .AddStage(PropagationStage.Spill, SeverityLevel.Moderate, DateTime.UtcNow)
            .AddStage(PropagationStage.Fire, SeverityLevel.Severe, DateTime.UtcNow.AddMinutes(2))
            .AddStage(PropagationStage.Injury, SeverityLevel.Catastrophic, DateTime.UtcNow.AddMinutes(5))
            .Build();

        chain.Nodes.Select(n => n.Stage).Should().Equal(PropagationStage.Spill, PropagationStage.Fire, PropagationStage.Injury);
        chain.Length.Should().Be(3);
        chain.OriginStage.Should().Be(PropagationStage.Spill);
        chain.TerminalStage.Should().Be(PropagationStage.Injury);
    }

    [Fact]
    public void Build_FireSmokeEvacuationChain_MatchesHazardPropagationRulesChain()
    {
        IReadOnlyList<PropagationStage> propagationChain = HazardPropagationRules.BuildChain(PropagationStage.Fire);
        var builder = new HazardChainBuilder();
        foreach (PropagationStage stage in propagationChain)
        {
            builder.AddStage(stage, SeverityLevel.Moderate, DateTime.UtcNow);
        }

        HazardChain chain = builder.Build();

        chain.Nodes.Select(n => n.Stage).Should().Equal(propagationChain);
    }

    [Fact]
    public void Build_MachineFailureInjuryShutdownChain_PreservesOrder()
    {
        HazardChain chain = new HazardChainBuilder()
            .AddStage(PropagationStage.MachineFailure, SeverityLevel.Moderate, DateTime.UtcNow)
            .AddStage(PropagationStage.Injury, SeverityLevel.Severe, DateTime.UtcNow.AddMinutes(1))
            .AddStage(PropagationStage.Shutdown, SeverityLevel.Severe, DateTime.UtcNow.AddMinutes(3))
            .Build();

        chain.Nodes.Select(n => n.Stage).Should().Equal(PropagationStage.MachineFailure, PropagationStage.Injury, PropagationStage.Shutdown);
    }

    [Fact]
    public void OverallSeverity_ReturnsWorstCaseAcrossNodes()
    {
        HazardChain chain = new HazardChainBuilder()
            .AddStage(PropagationStage.Spill, SeverityLevel.Minor, DateTime.UtcNow)
            .AddStage(PropagationStage.Fire, SeverityLevel.Catastrophic, DateTime.UtcNow.AddMinutes(1))
            .AddStage(PropagationStage.Injury, SeverityLevel.Moderate, DateTime.UtcNow.AddMinutes(2))
            .Build();

        chain.OverallSeverity.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void Build_NoStagesAdded_Throws()
    {
        var builder = new HazardChainBuilder();

        Action act = () => builder.Build();

        act.Should().Throw<InvalidOperationException>();
    }
}
