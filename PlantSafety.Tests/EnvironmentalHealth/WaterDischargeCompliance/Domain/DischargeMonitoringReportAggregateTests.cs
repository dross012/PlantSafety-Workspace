// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain
//  Layer:          Tests (Unit)
//  File:           DischargeMonitoringReportAggregateTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Lifecycle/invariant tests for the DischargeMonitoringReport (DMR) aggregate root,
//                  including the on-time-vs-late submission classification and the 5+ year regulatory
//                  retention clock invariant.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Exceptions;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Domain;

public sealed class DischargeMonitoringReportAggregateTests : UnitTestBase
{
    private static Agg PreparedReport(DateTime dueAtUtc) => Agg.Create(
        createdBy: TestUserId,
        createdAtUtc: FixedTimestamp,
        correlationId: TestCorrelationId,
        causationId: TestCausationId,
        actor: TestActor,
        tenantId: TestTenantId.Value,
        siteId: Guid.NewGuid(),
        dischargePermitId: Guid.NewGuid(),
        reportingPeriodStartUtc: UtcTimestamp.From(FixedUtcNow.AddMonths(-1)),
        reportingPeriodEndUtc: FixedTimestamp,
        dueAtUtc: UtcTimestamp.From(dueAtUtc));

    [Fact]
    public void Create_WithValidData_PreparesDraftReportAndRaisesEvent()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(28));

        report.Status.Should().Be(DischargeMonitoringReportStatus.Draft);
        report.SamplesWithinLimitCount.Should().Be(0);
        report.TotalSamplesCount.Should().Be(0);
        report.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Submit_OnOrBeforeDueDate_TransitionsToSubmittedAndStartsRetentionClock()
    {
        var report = PreparedReport(dueAtUtc: FixedUtcNow.AddDays(1));
        var submittedAt = UtcTimestamp.From(FixedUtcNow);

        report.Submit(TestUserId, submittedAt, samplesWithinLimitCount: 28, totalSamplesCount: 30, TestActor, TestCorrelationId, TestCausationId);

        report.Status.Should().Be(DischargeMonitoringReportStatus.Submitted);
        report.RetentionExpiresAtUtc!.Value.Value.Should().Be(submittedAt.Value.AddYears(Agg.MinimumRetentionYears));
        report.SubmittedByUserId.Should().Be(TestUserId);
    }

    [Fact]
    public void Submit_AfterDueDate_TransitionsToLate()
    {
        var report = PreparedReport(dueAtUtc: FixedUtcNow.AddDays(-1));

        report.Submit(TestUserId, FixedTimestamp, samplesWithinLimitCount: 25, totalSamplesCount: 30, TestActor, TestCorrelationId, TestCausationId);

        report.Status.Should().Be(DischargeMonitoringReportStatus.Late);
    }

    [Fact]
    public void Submit_WhenNotDraft_ThrowsInvalidStatusTransition()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));
        report.Submit(TestUserId, FixedTimestamp, 30, 30, TestActor, TestCorrelationId, TestCausationId);

        var act = () => report.Submit(TestUserId, FixedTimestamp, 30, 30, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargeMonitoringReportInvalidStatusTransitionException>();
    }

    [Fact]
    public void Accept_WhenSubmitted_TransitionsToAccepted()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));
        report.Submit(TestUserId, FixedTimestamp, 30, 30, TestActor, TestCorrelationId, TestCausationId);

        report.Accept(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        report.Status.Should().Be(DischargeMonitoringReportStatus.Accepted);
    }

    [Fact]
    public void Accept_WhenLate_TransitionsToAccepted()
    {
        var report = PreparedReport(dueAtUtc: FixedUtcNow.AddDays(-1));
        report.Submit(TestUserId, FixedTimestamp, 25, 30, TestActor, TestCorrelationId, TestCausationId);

        report.Accept(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        report.Status.Should().Be(DischargeMonitoringReportStatus.Accepted);
    }

    [Fact]
    public void Accept_WhenStillDraft_ThrowsInvalidStatusTransition()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));

        var act = () => report.Accept(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargeMonitoringReportInvalidStatusTransitionException>();
    }

    [Fact]
    public void Reject_WhenSubmitted_TransitionsToRejectedAndRecordsReason()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));
        report.Submit(TestUserId, FixedTimestamp, 30, 30, TestActor, TestCorrelationId, TestCausationId);

        report.Reject("Missing lab certification attachments.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        report.Status.Should().Be(DischargeMonitoringReportStatus.Rejected);
        report.RejectionReason.Should().Be("Missing lab certification attachments.");
    }

    [Fact]
    public void Reject_WhenAlreadyAccepted_ThrowsInvalidStatusTransition()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));
        report.Submit(TestUserId, FixedTimestamp, 30, 30, TestActor, TestCorrelationId, TestCausationId);
        report.Accept(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        var act = () => report.Reject("too late", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        act.Should().Throw<DischargeMonitoringReportInvalidStatusTransitionException>();
    }
}
