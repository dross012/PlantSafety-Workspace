// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands
//  Layer:          Tests (Unit)
//  File:           LimitExceedanceCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Command handler tests for the LimitExceedance aggregate: Record,
//                  InitiateResponse, CloseResponse. Includes a regression test asserting
//                  InitiateExceedanceResponseCommandHandler and CloseExceedanceResponseCommandHandler
//                  work correctly end-to-end with a plain Guid ResponseUserId -- both handlers
//                  previously had a Guid-vs-UserId type-mismatch bug that was hand-fixed by wrapping
//                  request.ResponseUserId in UserId.FromGuid(...); this test locks the fix in place.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.Commands;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Enums;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Interfaces;
using PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.ValueObjects.Ids;
using Agg = PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Aggregates.LimitExceedance;

namespace PlantSafety.Tests.EnvironmentalHealth.WaterDischargeCompliance.Commands;

public sealed class LimitExceedanceCommandHandlerTests : UnitTestBase
{
    private readonly ILimitExceedanceRepository _repo = Substitute.For<ILimitExceedanceRepository>();

    private static Agg DetectedExceedance() => Agg.Create(
        createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
        causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value,
        dischargePointId: Guid.NewGuid(), effluentSampleId: Guid.NewGuid(), dischargeLimitId: Guid.NewGuid(),
        parameterName: "pH", measuredValue: 10.5m, limitValue: 9.0m, severity: ExceedanceSeverity.Major,
        detectedAtUtc: FixedTimestamp);

    [Fact]
    public async Task Record_WithValidCommand_PersistsNewExceedanceAsDetected()
    {
        var sut = new RecordLimitExceedanceCommandHandler(_repo, NullLogger<RecordLimitExceedanceCommandHandler>());
        var command = new RecordLimitExceedanceCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "pH", 10.5m, 9.0m, "Major", FixedUtcNow, 0, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(LimitExceedanceStatus.Detected));
        await _repo.Received(1).AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>());
    }

    // ---- Regression: Guid-vs-UserId type-mismatch fix (InitiateExceedanceResponseCommandHandler) ----

    [Fact]
    public async Task InitiateResponse_WithPlainGuidResponseUserId_SucceedsEndToEndAndRecordsResponder()
    {
        var exceedance = DetectedExceedance();
        _repo.GetByIdAsync(Arg.Any<LimitExceedanceId>(), Arg.Any<CancellationToken>()).Returns(exceedance);
        var sut = new InitiateExceedanceResponseCommandHandler(_repo, NullLogger<InitiateExceedanceResponseCommandHandler>());
        var responseUserId = Guid.NewGuid(); // deliberately a plain Guid, as arrives from the API/controller boundary
        var command = new InitiateExceedanceResponseCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            exceedance.LimitExceedanceId.Value, responseUserId, "Investigating root cause.", TestUserId.Value);

        var act = async () => await sut.Handle(command, CancellationToken.None);

        // Prior to the fix, passing a raw Guid straight into LimitExceedance.InitiateResponse
        // (which requires a UserId value object) failed to compile / threw a type-mismatch at the
        // call site. UserId.FromGuid(request.ResponseUserId) resolves this -- this test proves the
        // full command -> handler -> aggregate path now succeeds for an ordinary Guid input.
        var result = await act.Should().NotThrowAsync();
        result.Subject.IsSuccess.Should().BeTrue();
        result.Subject.Value.Status.Should().Be(nameof(LimitExceedanceStatus.ResponseInitiated));
        exceedance.ResponseInitiatedByUserId!.Value.Value.Should().Be(responseUserId);
        await _repo.Received(1).UpdateAsync(exceedance, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InitiateResponse_WhenAlreadyInitiated_ReturnsFailureRatherThanThrowing()
    {
        var exceedance = DetectedExceedance();
        exceedance.InitiateResponse(TestUserId, null, FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<LimitExceedanceId>(), Arg.Any<CancellationToken>()).Returns(exceedance);
        var sut = new InitiateExceedanceResponseCommandHandler(_repo, NullLogger<InitiateExceedanceResponseCommandHandler>());
        var command = new InitiateExceedanceResponseCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            exceedance.LimitExceedanceId.Value, Guid.NewGuid(), null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("InitiateResponseFailed");
    }

    // ---- Regression: Guid-vs-UserId type-mismatch fix (CloseExceedanceResponseCommandHandler) ----

    [Fact]
    public async Task CloseResponse_WithPlainGuidResponseUserId_SucceedsEndToEndAndRecordsCloser()
    {
        var exceedance = DetectedExceedance();
        exceedance.InitiateResponse(TestUserId, "Investigating.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<LimitExceedanceId>(), Arg.Any<CancellationToken>()).Returns(exceedance);
        var sut = new CloseExceedanceResponseCommandHandler(_repo, NullLogger<CloseExceedanceResponseCommandHandler>());
        var responseUserId = Guid.NewGuid(); // deliberately a plain Guid, as arrives from the API/controller boundary
        var command = new CloseExceedanceResponseCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            exceedance.LimitExceedanceId.Value, responseUserId, "Root cause corrected; resampled within limit.", TestUserId.Value);

        var act = async () => await sut.Handle(command, CancellationToken.None);

        var result = await act.Should().NotThrowAsync();
        result.Subject.IsSuccess.Should().BeTrue();
        result.Subject.Value.Status.Should().Be(nameof(LimitExceedanceStatus.Closed));
        exceedance.ClosedByUserId!.Value.Value.Should().Be(responseUserId);
        await _repo.Received(1).UpdateAsync(exceedance, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CloseResponse_WhenStillDetected_ReturnsFailureRatherThanThrowing()
    {
        var exceedance = DetectedExceedance();
        _repo.GetByIdAsync(Arg.Any<LimitExceedanceId>(), Arg.Any<CancellationToken>()).Returns(exceedance);
        var sut = new CloseExceedanceResponseCommandHandler(_repo, NullLogger<CloseExceedanceResponseCommandHandler>());
        var command = new CloseExceedanceResponseCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            exceedance.LimitExceedanceId.Value, Guid.NewGuid(), null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("CloseResponseFailed");
    }

    [Fact]
    public async Task InitiateResponse_WhenExceedanceDoesNotExist_ReturnsNotFoundFailure()
    {
        _repo.GetByIdAsync(Arg.Any<LimitExceedanceId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);
        var sut = new InitiateExceedanceResponseCommandHandler(_repo, NullLogger<InitiateExceedanceResponseCommandHandler>());
        var command = new InitiateExceedanceResponseCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), Guid.NewGuid(), null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Contain("NotFound");
    }
}
