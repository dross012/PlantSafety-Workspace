// ============================================================================
//  Project:        Shared.WorkflowEngine.Tests
//  File:           WorkflowEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.7 unit tests for WorkflowEngine.Evaluate:
//                  severity-driven evacuation, hazard-specific SOP/permit
//                  triggers, exposure/failure-driven required actions, and
//                  notification routing escalation.
// ============================================================================
using FluentAssertions;

using Shared.EmployeeDomain.Abstractions.Enums;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Notification;
using Shared.WorkflowEngine.Abstractions.Triggers;

using Xunit;

namespace Shared.WorkflowEngine.Tests.Engine;

public sealed class WorkflowEngineTests
{
    private static WorkflowContext MinimalContext(HazardType hazardType = HazardType.Other, SeverityLevel severity = SeverityLevel.Minor) =>
        new(hazardType, severity, EvacuationScope.Zone, EmployeeExposureCount: 0, MachineFailureCount: 0, EventChainLength: 1);

    [Fact]
    public void Evaluate_MinorSeverity_NoEvacuationTriggered()
    {
        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(MinimalContext());

        decision.EvacuationTriggers.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_SevereSeverity_TriggersEvacuationAtAffectedScope()
    {
        WorkflowContext context = MinimalContext(severity: SeverityLevel.Severe) with { AffectedScope = EvacuationScope.Building };

        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(context);

        decision.EvacuationTriggers.Should().ContainSingle(t => t.Scope == EvacuationScope.Building);
    }

    [Fact]
    public void Evaluate_CatastrophicSeverity_EscalatesEvacuationToFacilityScope()
    {
        WorkflowContext context = MinimalContext(severity: SeverityLevel.Catastrophic) with { AffectedScope = EvacuationScope.Zone };

        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(context);

        decision.EvacuationTriggers.Should().ContainSingle(t => t.Scope == EvacuationScope.Facility);
    }

    [Fact]
    public void Evaluate_Fire_TriggersFireResponseSop()
    {
        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(MinimalContext(HazardType.Fire));

        decision.SopTriggers.Should().Contain(t => t.SopCode == "FIRE-RESPONSE-SOP");
    }

    [Fact]
    public void Evaluate_HotWork_TriggersHotWorkPermit()
    {
        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(MinimalContext(HazardType.HotWork));

        decision.PermitTriggers.Should().Contain(t => t.PermitCode == "HOT-WORK-PERMIT");
    }

    [Fact]
    public void Evaluate_MachineFailure_RequiresEquipmentIsolationAndLoto()
    {
        WorkflowContext context = MinimalContext() with { MachineFailureCount = 1 };

        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(context);

        decision.RequiredActions.Should().Contain(a => a.ActionCode == "EQUIPMENT-ISOLATION");
        decision.SopTriggers.Should().Contain(t => t.SopCode == "LOTO-SOP");
    }

    [Fact]
    public void Evaluate_EmployeeExposure_RequiresMedicalEvaluation()
    {
        WorkflowContext context = MinimalContext() with { EmployeeExposureCount = 2 };

        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(context);

        decision.RequiredActions.Should().Contain(a => a.ActionCode == "MEDICAL-EVALUATION");
    }

    [Fact]
    public void Evaluate_LongEventChain_RequiresEscalation()
    {
        WorkflowContext context = MinimalContext() with { EventChainLength = 3 };

        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(context);

        decision.RequiredActions.Should().Contain(a => a.ActionCode == "MULTI-HAZARD-ESCALATION");
    }

    [Fact]
    public void Evaluate_AlwaysRoutesToSafetyViaEmail()
    {
        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(MinimalContext());

        decision.NotificationRoutings.Should().Contain(r => r.RecipientRole == EmployeeRole.Safety && r.Channel == NotificationChannel.Email);
    }

    [Fact]
    public void Evaluate_CatastrophicSeverity_RoutesToAdministratorViaPage()
    {
        WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(MinimalContext(severity: SeverityLevel.Catastrophic));

        decision.NotificationRoutings.Should().Contain(r => r.RecipientRole == EmployeeRole.Administrator && r.Channel == NotificationChannel.Page);
    }
}
