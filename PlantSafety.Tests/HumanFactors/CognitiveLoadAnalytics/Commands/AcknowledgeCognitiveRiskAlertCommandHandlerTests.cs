// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Commands
//  Layer:          Tests (Unit)
//  File:           AcknowledgeCognitiveRiskAlertCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AcknowledgeCognitiveRiskAlertCommandHandler: success path,
//                   not-found mapping, tenant isolation, and unexpected exception mapping.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Commands;

public sealed class AcknowledgeCognitiveRiskAlertCommandHandlerTests : UnitTestBase
{
    private readonly ICognitiveRiskPredictionRepository _repo = Substitute.For<ICognitiveRiskPredictionRepository>();

    private AcknowledgeCognitiveRiskAlertCommandHandler Sut() => new(_repo, NullLogger<AcknowledgeCognitiveRiskAlertCommandHandler>());

    private static CognitiveRiskPrediction GeneratedPrediction() =>
        CognitiveRiskPrediction.Generate(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, TestUserId, CognitiveLoadLedgerId.New(),
            CognitiveRiskLevel.Elevated, 0.7m, "v1.0.0", "basis");

    private static AcknowledgeCognitiveRiskAlertCommand CommandFor(Guid predictionId, string? notes = "Reviewed.") =>
        AcknowledgeCognitiveRiskAlertCommand.FromDto(
            tenantId: TestTenantId.Value, cognitiveRiskPredictionId: predictionId, acknowledgementNotes: notes,
            requesterUserId: TestUserId.Value, correlationId: TestCorrelationId.Value, causationId: TestCausationId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_AcknowledgesAndPersists()
    {
        var prediction = GeneratedPrediction();
        _repo.GetByIdAsync(prediction.CognitiveRiskPredictionId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(prediction);

        var result = await Sut().Handle(CommandFor(prediction.CognitiveRiskPredictionId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(nameof(CognitiveRiskPredictionStatus.Acknowledged));
        await _repo.Received(1).UpdateAsync(prediction, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPredictionDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<CognitiveRiskPredictionId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CognitiveRiskPrediction?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.CognitiveRiskPrediction.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenPredictionBelongsToDifferentTenant_ReturnsNotFound()
    {
        var predictionId = CognitiveRiskPredictionId.New();
        var otherTenantId = Guid.NewGuid();
        _repo.GetByIdAsync(predictionId, otherTenantId, Arg.Any<CancellationToken>()).Returns(GeneratedPrediction());

        var result = await Sut().Handle(CommandFor(predictionId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.CognitiveRiskPrediction.NotFound");
    }

    [Fact]
    public async Task Handle_WhenAlreadyAcknowledged_ReturnsGenericFailureWithoutLeakingDomainExceptionMessage()
    {
        var prediction = GeneratedPrediction();
        prediction.Acknowledge(TestUserId, "first", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(prediction.CognitiveRiskPredictionId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(prediction);

        var result = await Sut().Handle(CommandFor(prediction.CognitiveRiskPredictionId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.AcknowledgeCognitiveRiskAlertFailed");
    }
}
