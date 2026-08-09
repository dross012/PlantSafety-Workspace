// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           HazardPropagationTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.6 unit tests for HazardPropagationRules: the
//                  three fixed chains from Path C's spec and terminal-stage
//                  behavior.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Propagation;

using Xunit;

namespace Shared.FacilityDomain.Tests.Propagation;

public sealed class HazardPropagationTests
{
    [Fact]
    public void BuildChain_Fire_ProducesFireSmokeEvacuation()
    {
        IReadOnlyList<PropagationStage> chain = HazardPropagationRules.BuildChain(PropagationStage.Fire);

        chain.Should().Equal(PropagationStage.Fire, PropagationStage.Smoke, PropagationStage.Evacuation);
    }

    [Fact]
    public void BuildChain_Spill_ProducesSpillVaporExposure()
    {
        IReadOnlyList<PropagationStage> chain = HazardPropagationRules.BuildChain(PropagationStage.Spill);

        chain.Should().Equal(PropagationStage.Spill, PropagationStage.Vapor, PropagationStage.Exposure);
    }

    [Fact]
    public void BuildChain_MachineFailure_ProducesMachineFailureInjuryShutdown()
    {
        IReadOnlyList<PropagationStage> chain = HazardPropagationRules.BuildChain(PropagationStage.MachineFailure);

        chain.Should().Equal(PropagationStage.MachineFailure, PropagationStage.Injury, PropagationStage.Shutdown);
    }

    [Theory]
    [InlineData(PropagationStage.Evacuation)]
    [InlineData(PropagationStage.Exposure)]
    [InlineData(PropagationStage.Shutdown)]
    public void IsTerminal_TrueForChainEndpoints(PropagationStage stage)
    {
        HazardPropagationRules.IsTerminal(stage).Should().BeTrue();
    }

    [Fact]
    public void IsTerminal_FalseForOriginatingStage()
    {
        HazardPropagationRules.IsTerminal(PropagationStage.Fire).Should().BeFalse();
    }

    [Fact]
    public void NextStage_TerminalStage_ReturnsNull()
    {
        HazardPropagationRules.NextStage(PropagationStage.Shutdown).Should().BeNull();
    }
}
