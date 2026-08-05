// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Application.PhysicalAccessControl
//  Layer:          Tests (Unit)
//  File:           AccessEventCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for IngestAccessEventCommandHandler and
//                  FlagAccessEventAnomalyCommandHandler against a mocked IAccessEventRepository.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.PhysicalAccessControl.Commands;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces;
using AccessEventAggregate = PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates.AccessEvent;

namespace PlantSafety.Tests.PhysicalAccessControl.Application;

public sealed class AccessEventCommandHandlerTests : UnitTestBase
{
    private readonly IAccessEventRepository _repository = Substitute.For<IAccessEventRepository>();

    private IngestAccessEventCommandHandler IngestSut() => new(_repository, NullLogger<IngestAccessEventCommandHandler>());
    private FlagAccessEventAnomalyCommandHandler FlagSut() => new(_repository, NullLogger<FlagAccessEventAnomalyCommandHandler>());

    private static IngestAccessEventCommand ValidIngestCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AccessPointId: Guid.NewGuid(),
        CredentialId: Guid.NewGuid(),
        SafetyZoneId: Guid.NewGuid(),
        EventType: "Granted",
        Direction: "In",
        OccurredAtUtc: FixedUtcNow,
        PerformedByUserId: TestUserId.Value);

    // ---- Ingest --------------------------------------------------------------------

    [Fact]
    public async Task Ingest_WithValidCommand_ShouldPersistAggregate()
    {
        await IngestSut().Handle(ValidIngestCommand(), CancellationToken.None);
        await _repository.Received(1).AddAsync(Arg.Any<AccessEventAggregate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ingest_WithValidCommand_ShouldReturnSuccessResult()
    {
        var result = await IngestSut().Handle(ValidIngestCommand(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Ingest_WithValidCommand_ShouldReturnIngestedStatus()
    {
        var result = await IngestSut().Handle(ValidIngestCommand(), CancellationToken.None);
        result.Value.Status.Should().Be("Ingested");
    }

    [Fact]
    public async Task Ingest_WithUnknownEventTypeAndDirection_ShouldDefaultGracefully()
    {
        var command = ValidIngestCommand() with { EventType = "Bogus", Direction = "Sideways" };

        AccessEventAggregate? captured = null;
        await _repository.AddAsync(Arg.Do<AccessEventAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        await IngestSut().Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.EventType.Should().Be(PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Enums.AccessEventType.Granted);
        captured.Direction.Should().Be(PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Enums.AccessEventDirection.In);
    }

    [Fact]
    public async Task Ingest_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        _repository.AddAsync(Arg.Any<AccessEventAggregate>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("boom"));

        var result = await IngestSut().Handle(ValidIngestCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessEvent.IngestFailed");
    }

    // ---- FlagAnomaly -----------------------------------------------------------------

    private static FlagAccessEventAnomalyCommand FlagCommand(Guid accessEventId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        AccessEventId: accessEventId,
        AnomalyType: "Tailgating",
        PerformedByUserId: TestUserId.Value);

    private static AccessEventAggregate IngestedAggregate() =>
        AccessEventAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            tenantId: TestTenantId.Value, accessPointId: Guid.NewGuid());

    [Fact]
    public async Task FlagAnomaly_WhenFound_ShouldTransitionAggregateAndUpdate()
    {
        var aggregate = IngestedAggregate();
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids.AccessEventId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await FlagSut().Handle(FlagCommand(aggregate.AccessEventId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Flagged");
        await _repository.Received(1).UpdateAsync(aggregate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FlagAnomaly_WhenNotFound_ShouldReturnFailureWithNotFoundErrorCode()
    {
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids.AccessEventId>(), Arg.Any<CancellationToken>())
            .Returns((AccessEventAggregate?)null);

        var result = await FlagSut().Handle(FlagCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessEvent.NotFound");
    }

    [Fact]
    public async Task FlagAnomaly_WhenAlreadyFlagged_ShouldReturnFailureResult()
    {
        var aggregate = IngestedAggregate();
        aggregate.FlagAnomaly("ForcedEntry", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repository.GetByIdAsync(Arg.Any<PlantSafety.Domain.PhysicalAccessControl.AccessEvents.ValueObjects.Ids.AccessEventId>(), Arg.Any<CancellationToken>())
            .Returns(aggregate);

        var result = await FlagSut().Handle(FlagCommand(aggregate.AccessEventId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PhysicalAccessControl.AccessEvent.FlagAnomalyFailed");
    }
}
