// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Commands
//  Layer:          Tests (Unit)
//  File:           GenerateEsgDisclosureReportCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for GenerateEsgDisclosureReportCommandHandler: happy
//                  path (multi-framework parsing), source-ledger-not-found mapping, invalid
//                  framework rejection, and the generic failure-envelope fallback. Isolated,
//                  deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;
using LedgerAggregate = PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates.AdvancedMetricsLedger;
using ReportAggregate = PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates.EsgDisclosureReport;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Commands;

public sealed class GenerateEsgDisclosureReportCommandHandlerTests : UnitTestBase
{
    private readonly IAdvancedMetricsLedgerRepository _ledgerRepo = Substitute.For<IAdvancedMetricsLedgerRepository>();
    private readonly IEsgDisclosureReportRepository _reportRepo = Substitute.For<IEsgDisclosureReportRepository>();

    private GenerateEsgDisclosureReportCommandHandler Sut() => new(_ledgerRepo, _reportRepo, NullLogger<GenerateEsgDisclosureReportCommandHandler>());

    private static readonly DateTime PeriodStart = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime PeriodEnd = new(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc);

    private LedgerAggregate SourceLedger() => LedgerAggregate.Create(
        TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
        TestTenantId.Value, null, UtcTimestamp.From(PeriodStart), UtcTimestamp.From(PeriodEnd));

    private GenerateEsgDisclosureReportCommand ValidCommand(Guid sourceLedgerId, string frameworks = "GhgProtocol, Csrd") => new(
        CommandId: Guid.NewGuid(), CorrelationId: TestCorrelationId.Value, CausationId: TestCausationId.Value,
        RequestedAtUtc: FixedUtcNow, ApiVersionToken: "v1", TenantId: TestTenantId.Value, SiteId: null,
        SourceLedgerId: sourceLedgerId, Frameworks: frameworks, MethodologyVersion: "GHG-Protocol-2026.1",
        RequesterUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidSourceLedgerAndMultipleFrameworks_GeneratesVersion1DraftReport()
    {
        var ledger = SourceLedger();
        _ledgerRepo.GetByIdAsync(Arg.Any<AdvancedMetricsLedgerId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(ledger);

        var result = await Sut().Handle(ValidCommand(ledger.AdvancedMetricsLedgerId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be(1);
        result.Value.Status.Should().Be("Draft");
        await _reportRepo.Received(1).AddAsync(Arg.Any<ReportAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSourceLedgerNotFound_ReturnsNotFoundWithoutTouchingReportRepository()
    {
        _ledgerRepo.GetByIdAsync(Arg.Any<AdvancedMetricsLedgerId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((LedgerAggregate?)null);

        var result = await Sut().Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.SourceLedgerNotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
        await _reportRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_WithUnrecognizedFramework_ReturnsValidationFailureWithoutTouchingLedgerRepository()
    {
        var result = await Sut().Handle(ValidCommand(Guid.NewGuid(), frameworks: "NotARealFramework"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.InvalidDisclosureFramework");
        result.Error.Severity.Should().Be(ErrorSeverity.Validation);
        await _ledgerRepo.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default, default);
    }

    [Fact]
    public async Task Handle_WithBlankFrameworks_ReturnsValidationFailure()
    {
        var result = await Sut().Handle(ValidCommand(Guid.NewGuid(), frameworks: ""), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.InvalidDisclosureFramework");
    }

    [Fact]
    public async Task Handle_WhenReportRepositoryThrows_ReturnsGenericFailureNotRawException()
    {
        var ledger = SourceLedger();
        _ledgerRepo.GetByIdAsync(Arg.Any<AdvancedMetricsLedgerId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(ledger);
        _reportRepo.AddAsync(Arg.Any<ReportAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("simulated repository outage"));

        var result = await Sut().Handle(ValidCommand(ledger.AdvancedMetricsLedgerId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("Sustainability.AdvancedMetrics.GenerateEsgDisclosureReportFailed");
    }
}
