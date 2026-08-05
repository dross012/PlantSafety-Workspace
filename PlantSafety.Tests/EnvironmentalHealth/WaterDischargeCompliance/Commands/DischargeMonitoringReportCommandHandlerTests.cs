// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands
//  Layer:          Tests (Unit)
//  File:           DischargeMonitoringReportCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for the DischargeMonitoringReport (DMR) aggregate:
//                  Prepare, Submit, Accept, Reject.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.ValueObjects.Ids;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates.DischargeMonitoringReport;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands;

public sealed class DischargeMonitoringReportCommandHandlerTests : UnitTestBase
{
    private readonly IDischargeMonitoringReportRepository _repo = Substitute.For<IDischargeMonitoringReportRepository>();

    private static Agg PreparedReport(DateTime dueAtUtc) => Agg.Create(
        createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
        causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
        dischargePermitId: Guid.NewGuid(), reportingPeriodStartUtc: UtcTimestamp.From(FixedUtcNow.AddMonths(-1)),
        reportingPeriodEndUtc: FixedTimestamp, dueAtUtc: UtcTimestamp.From(dueAtUtc));

    [Fact]
    public async Task Prepare_WithValidCommand_PersistsNewReportAsDraft()
    {
        var sut = new PrepareDischargeMonitoringReportCommandHandler(_repo, NullLogger<PrepareDischargeMonitoringReportCommandHandler>());
        var command = new PrepareDischargeMonitoringReportCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
            Guid.NewGuid(), FixedUtcNow.AddMonths(-1), FixedUtcNow, FixedUtcNow.AddDays(28), TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargeMonitoringReportStatus.Draft));
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_OnTime_TransitionsToSubmittedAndPersists()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));
        _repo.GetByIdAsync(Arg.Any<DischargeMonitoringReportId>(), Arg.Any<CancellationToken>()).Returns(report);
        var sut = new SubmitDischargeMonitoringReportCommandHandler(_repo, NullLogger<SubmitDischargeMonitoringReportCommandHandler>());
        var command = new SubmitDischargeMonitoringReportCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            report.DischargeMonitoringReportId.Value, FixedUtcNow, 30, 30, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargeMonitoringReportStatus.Submitted));
        await _repo.Received(1).UpdateAsync(report, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_AfterDueDate_TransitionsToLate()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(-1));
        _repo.GetByIdAsync(Arg.Any<DischargeMonitoringReportId>(), Arg.Any<CancellationToken>()).Returns(report);
        var sut = new SubmitDischargeMonitoringReportCommandHandler(_repo, NullLogger<SubmitDischargeMonitoringReportCommandHandler>());
        var command = new SubmitDischargeMonitoringReportCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            report.DischargeMonitoringReportId.Value, FixedUtcNow, 25, 30, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargeMonitoringReportStatus.Late));
    }

    [Fact]
    public async Task Accept_WhenSubmitted_TransitionsToAccepted()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));
        report.Submit(TestUserId, FixedTimestamp, 30, 30, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<DischargeMonitoringReportId>(), Arg.Any<CancellationToken>()).Returns(report);
        var sut = new AcceptDischargeMonitoringReportCommandHandler(_repo, NullLogger<AcceptDischargeMonitoringReportCommandHandler>());
        var command = new AcceptDischargeMonitoringReportCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            report.DischargeMonitoringReportId.Value, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargeMonitoringReportStatus.Accepted));
    }

    [Fact]
    public async Task Accept_WhenStillDraft_ReturnsFailureRatherThanThrowing()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));
        _repo.GetByIdAsync(Arg.Any<DischargeMonitoringReportId>(), Arg.Any<CancellationToken>()).Returns(report);
        var sut = new AcceptDischargeMonitoringReportCommandHandler(_repo, NullLogger<AcceptDischargeMonitoringReportCommandHandler>());
        var command = new AcceptDischargeMonitoringReportCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            report.DischargeMonitoringReportId.Value, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Reject_WhenSubmitted_TransitionsToRejectedAndPersistsReason()
    {
        var report = PreparedReport(FixedUtcNow.AddDays(1));
        report.Submit(TestUserId, FixedTimestamp, 30, 30, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<DischargeMonitoringReportId>(), Arg.Any<CancellationToken>()).Returns(report);
        var sut = new RejectDischargeMonitoringReportCommandHandler(_repo, NullLogger<RejectDischargeMonitoringReportCommandHandler>());
        var command = new RejectDischargeMonitoringReportCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            report.DischargeMonitoringReportId.Value, "Missing lab certification attachments.", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(DischargeMonitoringReportStatus.Rejected));
        report.RejectionReason.Should().Be("Missing lab certification attachments.");
    }

    [Fact]
    public async Task Reject_WhenReportDoesNotExist_ReturnsNotFoundFailure()
    {
        _repo.GetByIdAsync(Arg.Any<DischargeMonitoringReportId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new RejectDischargeMonitoringReportCommandHandler(_repo, NullLogger<RejectDischargeMonitoringReportCommandHandler>());
        var command = new RejectDischargeMonitoringReportCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), "reason", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("NotFound");
    }
}
