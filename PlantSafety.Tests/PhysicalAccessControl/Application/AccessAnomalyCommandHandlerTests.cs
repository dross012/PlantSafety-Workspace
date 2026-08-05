// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           AccessAnomalyCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DetectAccessAnomalyCommandHandler and
//                  AcknowledgeAccessAnomalyCommandHandler against a mocked IAccessAnomalyRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces;
using AccessAnomalyAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates.AccessAnomaly;

namespace PlantSafety.Tests.PhysicalAccessControl.Application;

public sealed class AccessAnomalyCommandHandlerTests : UnitTestBase
{
    private readonly IAccessAnomalyRepository _repository = Substitute.For<IAccessAnomalyRepository>();

    private DetectAccessAnomalyCommandHandler DetectSut() => new(_repository, NullLogger<DetectAccessAnomalyCommandHandler>());
    private AcknowledgeAccessAnomalyCommandHandler AcknowledgeSut() => new(_repository, NullLogger<AcknowledgeAccessAnomalyCommandHandler>());

    private static DetectAccessAnomalyCommand ValidDetectCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AccessEventId: Guid.NewGuid(),
        AccessPointId: Guid.NewGuid(),
        CredentialId: Guid.NewGuid(),
        SafetyZoneId: Guid.NewGuid(),
        AnomalyType: "Tailgating",
        SeverityScore: 65m,
        PerformedByUserId: TestUserId.Value);

    // ---- Detect --------------------------------------------------------------------

    [Fact]
    public async Task Detect_WithValidCommand_ShouldPersistAggregate()
    {
        await DetectSut().Handle(ValidDetectCommand(), CancellationToken.None);
        await _repository.Received(1).AddAsync(Arg.Any<AccessAnomalyAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Detect_WithValidCommand_ShouldReturnSuccessResult()
    {
        var result = await DetectSut().Handle(ValidDetectCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Detect_WithValidCommand_ShouldReturnDetectedStatus()
    {
        var result = await DetectSut().Handle(ValidDetectCommand(), CancellationToken.None);
        result.Value.Status.Should().Be("Detected");
    }

    [Fact]
    public async Task Detect_WithUnknownAnomalyType_ShouldDefaultToUnauthorizedAccess()
    {
        var command = ValidDetectCommand() with { AnomalyType = "NotARealType" };

        AccessAnomalyAggregate? captured = null;
        await _repository.AddAsync(Arg.Do<AccessAnomalyAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        await DetectSut().Handle(command, CancellationToken.None);

        captured!.AnomalyType.Should().Be(PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums.AccessAnomalyType.UnauthorizedAccess);
    }

    [Fact]
    public async Task Detect_WithSeverityScoreOutOfRange_ShouldReturnFailureResult()
    {
        var command = ValidDetectCommand() with { SeverityScore = 150m };

        var result = await DetectSut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessAnomaly.DetectFailed");
    }

    [Fact]
    public async Task Detect_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.AddAsync(Arg.Any<AccessAnomalyAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await DetectSut().Handle(ValidDetectCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessAnomaly.DetectFailed");
    }

    // ---- Acknowledge -----------------------------------------------------------------

    private static AcknowledgeAccessAnomalyCommand AcknowledgeCommand(Guid accessAnomalyId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        AccessAnomalyId: accessAnomalyId,
        AcknowledgedByUserId: Guid.NewGuid(),
        PerformedByUserId: TestUserId.Value);

    private static AccessAnomalyAggregate DetectedAggregate() =>
        AccessAnomalyAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, accessPointId: Guid.NewGuid(), severityScore: 40m);

    [Fact]
    public async Task Acknowledge_WhenFound_ShouldTransitionAggregateAndUpdate()
    {
        var aggregate = DetectedAggregate();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids.AccessAnomalyId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await AcknowledgeSut().Handle(AcknowledgeCommand(aggregate.AccessAnomalyId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Acknowledged");
        await _repository.Received(1).UpdateAsync(aggregate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Acknowledge_WhenNotFound_ShouldReturnFailureWithNotFoundErrorCode()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids.AccessAnomalyId>(), Arg.Any<CancellationToken>())
            .Returns((AccessAnomalyAggregate?)null);

        var result = await AcknowledgeSut().Handle(AcknowledgeCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessAnomaly.NotFound");
    }

    [Fact]
    public async Task Acknowledge_WhenAlreadyAcknowledged_ShouldReturnFailureResult()
    {
        var aggregate = DetectedAggregate();
        aggregate.Acknowledge(Guid.NewGuid(), FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.ValueObjects.Ids.AccessAnomalyId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await AcknowledgeSut().Handle(AcknowledgeCommand(aggregate.AccessAnomalyId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessAnomaly.AcknowledgeFailed");
    }
}
