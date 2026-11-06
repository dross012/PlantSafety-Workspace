// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingRuleEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.Rules;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Rules;

public sealed class SchedulingRuleEngineTests
{
    [Fact]
    public void Evaluate_EmptyRequiredActions_ReturnsZeroCountsAndNoRuleCodes()
    {
        var context = new SchedulingEvaluationContext(Array.Empty<RequiredAction>(), OverdueActionCount: 0, ActiveMachineGroupCount: 0);

        SchedulingRuleDecision decision = SchedulingRuleEngine.Evaluate(context);

        decision.TotalRequiredActionCount.Should().Be(0);
        decision.TotalEstimatedEffortHours.Should().Be(0);
        decision.WorstCaseDueWithinHours.Should().Be(int.MaxValue);
        decision.RequiresImmediateScheduling.Should().BeFalse();
        decision.AppliedRuleCodes.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_AccumulatesCountAndEffortAdditively()
    {
        var actions = new[]
        {
            new RequiredAction("ACT-1", "First action", DueWithinHours: 10),
            new RequiredAction("ACT-2", "Second action", DueWithinHours: 20),
            new RequiredAction("ACT-3", "Third action", DueWithinHours: 5),
        };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);

        SchedulingRuleDecision decision = SchedulingRuleEngine.Evaluate(context);

        decision.TotalRequiredActionCount.Should().Be(3);
        decision.TotalEstimatedEffortHours.Should().Be(35);
        decision.WorstCaseDueWithinHours.Should().Be(5);
        decision.AppliedRuleCodes.Should().Contain("REQUIRED-ACTION-ACCUMULATION");
    }

    [Fact]
    public void Evaluate_WorstCaseDueWithinHoursAtOrBelowThreshold_RequiresImmediateScheduling()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Immediate action", DueWithinHours: 0) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 0);

        SchedulingRuleDecision decision = SchedulingRuleEngine.Evaluate(context);

        decision.RequiresImmediateScheduling.Should().BeTrue();
        decision.AppliedRuleCodes.Should().Contain("IMMEDIATE-SCHEDULING-REQUIRED");
    }

    [Fact]
    public void Evaluate_OverdueActionCount_EscalatesRegardlessOfDueWithinHours()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Distant action", DueWithinHours: 200) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 1, ActiveMachineGroupCount: 0);

        SchedulingRuleDecision decision = SchedulingRuleEngine.Evaluate(context);

        decision.RequiresImmediateScheduling.Should().BeTrue();
        decision.AppliedRuleCodes.Should().Contain("OVERDUE-ACTION-ESCALATION");
    }

    [Fact]
    public void Evaluate_ActiveMachineGroupWithinTwentyFourHours_EscalatesToImmediate()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Near-term action", DueWithinHours: 12) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 2);

        SchedulingRuleDecision decision = SchedulingRuleEngine.Evaluate(context);

        decision.RequiresImmediateScheduling.Should().BeTrue();
        decision.AppliedRuleCodes.Should().Contain("ACTIVE-MACHINE-GROUP-ESCALATION");
    }

    [Fact]
    public void Evaluate_ActiveMachineGroupBeyondTwentyFourHours_DoesNotEscalate()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Distant action", DueWithinHours: 48) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 2);

        SchedulingRuleDecision decision = SchedulingRuleEngine.Evaluate(context);

        decision.RequiresImmediateScheduling.Should().BeFalse();
        decision.AppliedRuleCodes.Should().NotContain("ACTIVE-MACHINE-GROUP-ESCALATION");
    }

    [Fact]
    public void Evaluate_SameInputTwice_ProducesIdenticalResult()
    {
        var actions = new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 6) };
        var context = new SchedulingEvaluationContext(actions, OverdueActionCount: 0, ActiveMachineGroupCount: 1);

        SchedulingRuleDecision first = SchedulingRuleEngine.Evaluate(context);
        SchedulingRuleDecision second = SchedulingRuleEngine.Evaluate(context);

        second.Should().BeEquivalentTo(first);
    }

    [Fact]
    public void Evaluate_NullContext_Throws()
    {
        Action act = () => SchedulingRuleEngine.Evaluate(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
