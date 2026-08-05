// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Commands
//  Layer:          Tests (Unit)
//  File:           RecordCognitiveLoadReadingCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordCognitiveLoadReadingCommandHandler: get-or-create ledger
//                   flow (AddAsync vs UpdateAsync), invalid shift type mapping, and unexpected
//                   exception mapping. Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Commands;

public sealed class RecordCognitiveLoadReadingCommandHandlerTests : UnitTestBase
{
    private readonly ICognitiveLoadLedgerRepository _repo = Substitute.For<ICognitiveLoadLedgerRepository>();

    private RecordCognitiveLoadReadingCommandHandler Sut() => new(_repo, NullLogger<RecordCognitiveLoadReadingCommandHandler>());

    private static RecordCognitiveLoadReadingCommand Command(decimal score = 50m, string shiftType = "Day") =>
        RecordCognitiveLoadReadingCommand.FromDto(
            tenantId: TestTenantId.Value, siteId: null, shiftType: shiftType, shiftDateUtc: FixedUtcNow,
            workerUserId: TestUserId.Value, taskComplexityProfileId: null, cognitiveLoadScore: score,
            measurementMethod: "NASA-TLX", measuredAtUtc: FixedUtcNow, requesterUserId: TestUserId.Value,
            correlationId: TestCorrelationId.Value, causationId: TestCausationId.Value);

    [Fact]
    public async Task Handle_WhenNoOpenLedgerExists_CreatesLedgerAndCallsAddAsync()
    {
        _repo.GetOpenLedgerAsync(TestTenantId.Value, null, ShiftType.Day, Arg.Any<UtcTimestamp>(), Arg.Any<CancellationToken>())
            .Returns((CognitiveLoadLedger?)null);

        var result = await Sut().Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsOverload.Should().BeFalse();
        await _repo.Received(1).AddAsync(Arg.Any<CognitiveLoadLedger>(), Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<CognitiveLoadLedger>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOpenLedgerExists_RecordsReadingOnItAndCallsUpdateAsync()
    {
        var existingLedger = CognitiveLoadLedger.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, ShiftType.Day, FixedTimestamp);
        _repo.GetOpenLedgerAsync(TestTenantId.Value, null, ShiftType.Day, Arg.Any<UtcTimestamp>(), Arg.Any<CancellationToken>())
            .Returns(existingLedger);

        var result = await Sut().Handle(Command(score: 85m), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsOverload.Should().BeTrue();
        result.Value.CognitiveLoadLedgerId.Should().Be(existingLedger.CognitiveLoadLedgerId.Value);
        await _repo.Received(1).UpdateAsync(existingLedger, Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().AddAsync(Arg.Any<CognitiveLoadLedger>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidShiftType_ReturnsValidationFailure()
    {
        var result = await Sut().Handle(Command(shiftType: "NotAShift"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.InvalidShiftType");
        result.Error.Severity.Should().Be(ErrorSeverity.Validation);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailureWithoutLeakingException()
    {
        _repo.GetOpenLedgerAsync(Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<ShiftType>(), Arg.Any<UtcTimestamp>(), Arg.Any<CancellationToken>())
            .Returns<Task<CognitiveLoadLedger?>>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(Command(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.RecordCognitiveLoadReadingFailed");
        result.Error.Message.Should().NotContain("boom", "raw exception details must never leak into the sanitized ApplicationError message");
    }
}
