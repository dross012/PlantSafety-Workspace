// ============================================================================
//  Test IDs:  PLNT-UAT-ALERT-001..004
//  Manual:    tests_rules_design_manual.md (UAT type, Appendix A test case
//             template: Preconditions / Steps / Expected Result).
//  Purpose:   Business-readable acceptance criteria a Safety Officer would
//             sign off on before the Alerts feature ships. Each test is
//             written Given/When/Then and names the acceptance criterion in
//             its DisplayName so it can be traced 1:1 back to a UAT sign-off
//             checklist item.
// ============================================================================
namespace PlantSafety.AcceptanceTests.Alerts;

public sealed class SafetyOfficerAlertAcceptanceTests
{
    private static readonly UserId SafetyOfficer = UserId.FromGuid(Guid.NewGuid());
    private static PerformedByActor Officer => PerformedByActor.User(SafetyOfficer);

    [Fact(DisplayName = "PLNT-UAT-ALERT-001: GIVEN a hazard is observed, WHEN it is logged as a Critical WorkerSafety alert, THEN it is immediately visible with Created status")]
    public void LoggingACriticalHazard_ShouldProduceAnImmediatelyVisibleCreatedAlert()
    {
        // GIVEN a Safety Officer has observed a hazardous condition on the floor
        // WHEN they log it as a Critical WorkerSafety alert
        var alert = Alert.Create(
            AlertType.WorkerSafety,
            AlertSeverity.Critical,
            "Unguarded rotating equipment on Line 7",
            "Guard panel found removed during routine walkthrough",
            "SafetyOfficerApp",
            SafetyOfficer,
            UtcTimestamp.Now(),
            new CorrelationId(Guid.NewGuid()),
            new CausationId(Guid.NewGuid()),
            Officer);

        // THEN the alert is immediately visible in Created status with the reported details intact
        alert.Status.Should().Be(AlertStatus.Created);
        alert.Severity.Should().Be(AlertSeverity.Critical);
        alert.Title.Should().Be("Unguarded rotating equipment on Line 7");
        alert.CreatedBy.Should().Be(SafetyOfficer);
    }

    [Fact(DisplayName = "PLNT-UAT-ALERT-002: GIVEN a Created alert, WHEN a Safety Officer acknowledges it with notes, THEN the acknowledgment and notes are recorded and attributable")]
    public void AcknowledgingAnAlert_ShouldRecordAttributableAcknowledgment()
    {
        var alert = Alert.Create(
            AlertType.MachineFailure, AlertSeverity.High,
            "Hydraulic press pressure anomaly", "Pressure reading 20% above nominal",
            "SCADA", SafetyOfficer, UtcTimestamp.Now(),
            new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()), Officer);

        alert.Acknowledge(UtcTimestamp.Now(), Officer,
            new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()),
            new AlertNotes("Dispatched maintenance to inspect press #4."));

        alert.Status.Should().Be(AlertStatus.Acknowledged);
        alert.AcknowledgedBy.Should().Be(Officer);
        alert.Notes!.Value.Should().Contain("press #4");
    }

    [Fact(DisplayName = "PLNT-UAT-ALERT-003: GIVEN an unresolved alert cannot be closed without a resolution summary, WHEN closure is attempted early, THEN it is rejected with a clear business rule violation")]
    public void ClosingAnAlertWithoutResolution_ShouldBeRejected()
    {
        var alert = Alert.Create(
            AlertType.Environmental, AlertSeverity.Medium,
            "Minor coolant leak observed", "Small drip near Tank 2 valve",
            "Inspection", SafetyOfficer, UtcTimestamp.Now(),
            new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()), Officer);

        var prematureClose = () => alert.Close(UtcTimestamp.Now(), Officer,
            new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()));

        prematureClose.Should().Throw<InvalidOperationException>(
            "a Safety Officer must not be able to close an alert that has not gone through acknowledgment/resolution -- this is the acceptance criterion that prevents silently dropped hazards");
    }

    [Fact(DisplayName = "PLNT-UAT-ALERT-004: GIVEN a resolved alert, WHEN it is closed and archived, THEN the full resolution narrative required for regulatory sign-off is preserved")]
    public void ClosingAndArchivingAResolvedAlert_ShouldPreserveTheFullNarrativeForSignOff()
    {
        var alert = Alert.Create(
            AlertType.WorkerSafety, AlertSeverity.High,
            "Slip hazard from spilled lubricant", "Reported by floor worker near Bay 3",
            "SafetyOfficerApp", SafetyOfficer, UtcTimestamp.Now(),
            new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()), Officer);

        alert.Acknowledge(UtcTimestamp.Now(), Officer, new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()));
        alert.StartProgress(UtcTimestamp.Now(), Officer, new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()));

        var summary = new ResolutionSummary("Spill cleaned, absorbent mats placed, area re-inspected and cleared.");
        alert.Resolve(summary, UtcTimestamp.Now(), Officer, new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()));
        alert.Close(UtcTimestamp.Now(), Officer, new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()));
        alert.Archive(UtcTimestamp.Now(), Officer, new CorrelationId(Guid.NewGuid()), new CausationId(Guid.NewGuid()));

        alert.Status.Should().Be(AlertStatus.Archived);
        alert.ResolutionSummary.Should().Be(summary);
        alert.History.Should().NotBeEmpty("the full narrative from Created through Archived must remain queryable for regulatory sign-off");
    }
}
