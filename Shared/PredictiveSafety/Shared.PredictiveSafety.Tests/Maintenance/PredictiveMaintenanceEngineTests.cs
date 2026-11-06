// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveMaintenanceEngineTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Maintenance;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Maintenance;

public sealed class PredictiveMaintenanceEngineTests
{
    private static readonly Guid MachineId = Guid.NewGuid();

    private static PredictiveMaintenanceSignals BaseSignals() => new(
        MachineId, IsMaintenanceOverdue: false, RecentFailureCount: 0, SchedulingWorstCaseDueWithinHours: int.MaxValue, RecentReliabilityFailureCount: 0);

    [Fact]
    public void Evaluate_NoTriggers_NoEscalation()
    {
        PredictiveMaintenanceRuleEngine.Evaluate(BaseSignals()).RequiresEscalation.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_Overdue_Escalates()
    {
        PredictiveMaintenanceRuleEngine.Evaluate(BaseSignals() with { IsMaintenanceOverdue = true })
            .AppliedRuleCodes.Should().Contain("MAINTENANCE-OVERDUE-ESCALATION");
    }

    [Fact]
    public void Evaluate_RepeatedFailures_Escalates()
    {
        PredictiveMaintenanceRuleEngine.Evaluate(BaseSignals() with { RecentFailureCount = 2 })
            .AppliedRuleCodes.Should().Contain("REPEATED-FAILURE-ESCALATION");
    }

    [Fact]
    public void Evaluate_DueSoon_Escalates()
    {
        PredictiveMaintenanceRuleEngine.Evaluate(BaseSignals() with { SchedulingWorstCaseDueWithinHours = 12 })
            .AppliedRuleCodes.Should().Contain("SCHEDULING-DUE-SOON-ESCALATION");
    }

    [Fact]
    public void Infer_MultipleTriggers_ClassifiesHighOrCritical()
    {
        PredictiveMaintenanceSignals signals = BaseSignals() with { IsMaintenanceOverdue = true, RecentFailureCount = 3 };

        PredictiveMaintenanceDecision decision = PredictiveMaintenanceInferenceEngine.Infer(signals);

        decision.RequiresImmediateMaintenance.Should().BeTrue();
        decision.Priority.Should().BeOneOf(PredictivePriority.High, PredictivePriority.Critical);
        decision.MachineId.Should().Be(MachineId);
    }

    [Fact]
    public void Infer_NullSignals_Throws()
    {
        Action act = () => PredictiveMaintenanceInferenceEngine.Infer(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
