// ============================================================================
//  Test IDs:  PLNT-WKFL-ALERT-001..003 / PLNT-SCEN-ALERT-001
//  Manual:    tests_rules_design_manual.md (WKFL, SCEN types).
//  Purpose:   Drives the FULL Alert aggregate lifecycle state machine
//             end-to-end at the domain layer:
//                 Created -> Acknowledged -> InProgress -> Escalated
//                         -> Resolved -> Closed -> Archived
//             confirming every legal transition succeeds, every domain event
//             is raised in the correct order, and history/audit entries
//             accumulate correctly across the full workflow.
// ============================================================================
namespace PlantSafety.ScenarioTests.Alerts;

public sealed class AlertLifecycleWorkflowTests
{
    private static readonly UserId Actor = UserId.FromGuid(Guid.NewGuid());
    private static PerformedByActor CurrentActor => PerformedByActor.User(Actor);
    private static CorrelationId NewCorrelation() => new(Guid.NewGuid());
    private static CausationId NewCausation() => new(Guid.NewGuid());

    [Fact(DisplayName = "PLNT-WKFL-ALERT-001: Full lifecycle Created->Acknowledged->InProgress->Escalated->Resolved->Closed->Archived succeeds")]
    public void FullLifecycle_ShouldTransitionThroughEveryStatus_InOrder()
    {
        var t0 = UtcTimestamp.Now();

        var alert = Alert.Create(
            AlertType.MachineFailure, AlertSeverity.High,
            "Conveyor motor overheating", "Temperature exceeds safe operating threshold",
            "ScenarioTests", Actor, t0, NewCorrelation(), NewCausation(), CurrentActor);

        alert.Status.Should().Be(AlertStatus.Created);

        alert.Acknowledge(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation(),
            new AlertNotes("Operator has seen the alert."));
        alert.Status.Should().Be(AlertStatus.Acknowledged);

        alert.StartProgress(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Status.Should().Be(AlertStatus.InProgress);

        alert.Escalate(new EscalationReason("Temperature continuing to climb despite intervention"),
            UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Status.Should().Be(AlertStatus.Escalated);

        alert.Resolve(new ResolutionSummary("Motor replaced; temperature nominal."),
            UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Status.Should().Be(AlertStatus.Resolved);

        alert.Close(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Status.Should().Be(AlertStatus.Closed);

        alert.Archive(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Status.Should().Be(AlertStatus.Archived);

        alert.History.Should().HaveCount(7, "Create plus each of the six subsequent transitions must append exactly one audit history entry");
    }

    [Fact(DisplayName = "PLNT-WKFL-ALERT-002: Every transition raises its corresponding domain event, in order")]
    public void FullLifecycle_ShouldRaiseDomainEvents_InTransitionOrder()
    {
        var alert = Alert.Create(
            AlertType.Environmental, AlertSeverity.Critical,
            "Chemical spill detected", "Sensor threshold breach in Zone 4",
            "ScenarioTests", Actor, UtcTimestamp.Now(), NewCorrelation(), NewCausation(), CurrentActor);

        alert.Acknowledge(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.StartProgress(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Escalate(new EscalationReason("Requires HazMat team"), UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Resolve(new ResolutionSummary("Contained and cleaned by HazMat team"), UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Close(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());

        var events = alert.DomainEvents.ToList();

        events.Should().HaveCount(6);
        events[0].Should().BeOfType<AlertCreatedEvent>();
        events[1].Should().BeOfType<AlertAcknowledgedEvent>();
        events[2].Should().BeOfType<AlertInProgressEvent>();
        events[3].Should().BeOfType<AlertEscalatedEvent>();
        events[4].Should().BeOfType<AlertResolvedEvent>();
        events[5].Should().BeOfType<AlertClosedEvent>();
    }

    [Fact(DisplayName = "PLNT-WKFL-ALERT-003: Illegal transitions are rejected at every step of the workflow (invariant enforcement)")]
    public void IllegalTransitions_ShouldBeRejected_AtEveryWorkflowStep()
    {
        var alert = Alert.Create(
            AlertType.PredictiveMaintenance, AlertSeverity.Medium,
            "Bearing wear predicted", "Vibration analysis flags early wear",
            "ScenarioTests", Actor, UtcTimestamp.Now(), NewCorrelation(), NewCausation(), CurrentActor);

        // Cannot resolve/close/archive before acknowledging/progressing.
        var resolveTooEarly = () => alert.Resolve(new ResolutionSummary("n/a"), UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        resolveTooEarly.Should().Throw<InvalidOperationException>();

        alert.Acknowledge(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());

        // Cannot re-acknowledge an already-acknowledged alert.
        var reAcknowledge = () => alert.Acknowledge(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        reAcknowledge.Should().Throw<InvalidOperationException>();

        alert.StartProgress(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Resolve(new ResolutionSummary("Bearing replaced proactively"), UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Close(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());

        // Cannot escalate a closed alert.
        var escalateAfterClose = () => alert.Escalate(new EscalationReason("too late"), UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        escalateAfterClose.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "PLNT-SCEN-ALERT-001: Cross-module scenario -- Incident detected -> Alert triggered -> Assigned for corrective action -> Escalated -> Resolved")]
    public void IncidentToCorrectiveActionScenario_ShouldFlowThroughFullBusinessNarrative()
    {
        // Step 1: A safety incident is detected upstream (e.g. IncidentManagement module) and
        // triggers a WorkerSafety alert -- modeled here as Alert.Create, the shared trigger path.
        var alert = Alert.Create(
            AlertType.IncidentLinked, AlertSeverity.Critical,
            "Worker injury reported on Line 3",
            "Forklift collision reported by floor supervisor",
            "IncidentManagement", Actor, UtcTimestamp.Now(), NewCorrelation(), NewCausation(), CurrentActor);

        alert.Status.Should().Be(AlertStatus.Created);

        // Step 2: Safety officer acknowledges the alert.
        alert.Acknowledge(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation(),
            new AlertNotes("Safety officer dispatched to scene."));

        // Step 3: Corrective action assignment (modeled as AssignTo, since this codebase does
        // not yet expose a literal CorrectiveAction aggregate wired to Alert).
        alert.AssignTo("Safety Officer J. Rivera", UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Assignments.Should().ContainSingle(a => a.AssignedTo == "Safety Officer J. Rivera");

        // Step 4: Investigation begins, then escalates given injury severity.
        alert.StartProgress(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Escalate(new EscalationReason("Injury requires OSHA-recordable investigation"),
            UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());

        // Step 5: Resolution and closure once corrective action is complete.
        alert.Resolve(new ResolutionSummary("Forklift traffic lane re-striped; worker treated and released."),
            UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());
        alert.Close(UtcTimestamp.Now(), CurrentActor, NewCorrelation(), NewCausation());

        alert.Status.Should().Be(AlertStatus.Closed);
        alert.History.Should().HaveCountGreaterThanOrEqualTo(5,
            "the full incident -> alert -> assignment -> escalation -> resolution narrative must be fully auditable");
    }
}
