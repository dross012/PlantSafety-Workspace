// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.PHMSA.Incidents
//  Layer:          Tests (Unit)
//  File:           IncidentReportTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Functional/lifecycle unit tests for the IncidentReport aggregate root
//                  (PHMSA Incidents subdomain -- DOT Form 5800.1). Covers Create validation,
//                  computed IsReportableToPhmsa flag, and the Draft -> Submitted ->
//                  UnderReview -> Closed lifecycle including invalid transitions.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Events;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Exceptions;
using IncidentReportAggregate = PlantSafety.Domain.Compliance.PHMSA.Incidents.Aggregates.IncidentReport;

namespace PlantSafety.Tests.Compliance.PHMSA.Incidents;

public sealed class IncidentReportTests : UnitTestBase
{
    private static IncidentReportAggregate CreatedReport(
        IncidentType incidentType = IncidentType.Spill,
        int injuries = 0,
        bool fatalitiesInvolved = false,
        decimal? propertyDamageEstimateUsd = null,
        string narrative = "A minor release was observed and contained.") =>
        IncidentReportAggregate.Create(
            incidentType: incidentType,
            materialInvolvedId: null,
            materialInvolvedDescription: "Sodium hydroxide solution",
            injuries: injuries,
            fatalitiesInvolved: fatalitiesInvolved,
            propertyDamageEstimateUsd: propertyDamageEstimateUsd,
            narrative: narrative,
            incidentDateUtc: FixedUtcNow.AddHours(-2),
            location: "Loading Dock 3",
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor);

    [Fact]
    public void Create_WithValidData_CreatesDraftReportAndRaisesEvent()
    {
        var report = CreatedReport();

        report.Status.Should().Be(IncidentReportStatus.Draft);
        report.CreatedBy.Should().Be(TestUserId);
        report.Narrative.Should().Be("A minor release was observed and contained.");
        report.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentReportedEvent>();
    }

    [Fact]
    public void Create_WithFatalitiesInvolved_IsReportableToPhmsa()
    {
        var report = CreatedReport(incidentType: IncidentType.Injury, fatalitiesInvolved: true);

        report.IsReportableToPhmsa.Should().BeTrue();
        var evt = report.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentReportedEvent>().Subject;
        evt.IsReportableToPhmsa.Should().BeTrue();
    }

    [Fact]
    public void Create_SpillWithNoInjuriesFatalitiesOrDamage_IsNotReportableToPhmsa()
    {
        var report = CreatedReport(incidentType: IncidentType.Spill, injuries: 0, fatalitiesInvolved: false, propertyDamageEstimateUsd: null);

        report.IsReportableToPhmsa.Should().BeFalse();
    }

    [Fact]
    public void Create_FireOrExplosion_IsAlwaysReportableToPhmsa()
    {
        var report = CreatedReport(incidentType: IncidentType.FireOrExplosion, injuries: 0, fatalitiesInvolved: false, propertyDamageEstimateUsd: null);

        report.IsReportableToPhmsa.Should().BeTrue();
    }

    [Fact]
    public void Create_PropertyDamageAboveIllustrativeThreshold_IsReportableToPhmsa()
    {
        var report = CreatedReport(incidentType: IncidentType.PropertyDamage, propertyDamageEstimateUsd: 75_000m);

        report.IsReportableToPhmsa.Should().BeTrue();
    }

    [Fact]
    public void Create_PropertyDamageBelowIllustrativeThreshold_IsNotReportableToPhmsa()
    {
        var report = CreatedReport(incidentType: IncidentType.PropertyDamage, propertyDamageEstimateUsd: 1_000m);

        report.IsReportableToPhmsa.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyNarrative_ThrowsInvalidIncidentReportDataException(string? narrative)
    {
        var act = () => CreatedReport(narrative: narrative!);

        act.Should().Throw<InvalidIncidentReportDataException>();
    }

    [Fact]
    public void Create_WithNegativeInjuries_ThrowsInvalidIncidentReportDataException()
    {
        var act = () => CreatedReport(injuries: -1);

        act.Should().Throw<InvalidIncidentReportDataException>();
    }

    [Fact]
    public void Create_WithNegativePropertyDamageEstimate_ThrowsInvalidIncidentReportDataException()
    {
        var act = () => CreatedReport(propertyDamageEstimateUsd: -1m);

        act.Should().Throw<InvalidIncidentReportDataException>();
    }

    [Fact]
    public void FullLifecycle_DraftToSubmittedToUnderReviewToClosed_Succeeds()
    {
        var report = CreatedReport();
        report.ClearDomainEvents();

        report.Submit(TestCorrelationId, TestCausationId, TestActor);
        report.Status.Should().Be(IncidentReportStatus.Submitted);
        report.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentReportSubmittedEvent>();
        report.ClearDomainEvents();

        report.BeginReview(TestCorrelationId, TestCausationId, TestActor);
        report.Status.Should().Be(IncidentReportStatus.UnderReview);
        report.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentReportReviewStartedEvent>();
        report.ClearDomainEvents();

        report.Close("No further action required.", TestCorrelationId, TestCausationId, TestActor);
        report.Status.Should().Be(IncidentReportStatus.Closed);
        report.ResolutionNotes.Should().Be("No further action required.");
        report.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<IncidentReportClosedEvent>();
    }

    [Fact]
    public void Close_FromDraft_ThrowsInvalidStatusTransition()
    {
        var report = CreatedReport();

        var act = () => report.Close("Attempted premature close.", TestCorrelationId, TestCausationId, TestActor);

        act.Should().Throw<IncidentReportInvalidStatusTransitionException>();
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ThrowsInvalidStatusTransition()
    {
        var report = CreatedReport();
        report.Submit(TestCorrelationId, TestCausationId, TestActor);

        var act = () => report.Submit(TestCorrelationId, TestCausationId, TestActor);

        act.Should().Throw<IncidentReportInvalidStatusTransitionException>();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllPendingEvents()
    {
        var report = CreatedReport();
        report.DomainEvents.Should().NotBeEmpty();

        report.ClearDomainEvents();

        report.DomainEvents.Should().BeEmpty();
    }
}
