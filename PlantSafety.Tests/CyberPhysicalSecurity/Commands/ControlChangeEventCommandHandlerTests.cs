// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Commands
//  Layer:          Tests (Unit)
//  File:           ControlChangeEventCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordControlChangeEventCommandHandler and
//                  FlagUnauthorizedControlChangeCommandHandler. Includes a Security-category
//                  regression asserting the handler correctly chains PriorRecordHash from the
//                  repository's GetLatestForAssetAsync lookup, end-to-end through the handler layer.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Commands;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using ControlChangeEventAggregate = PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Aggregates.ControlChangeEvent;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Commands;

public sealed class RecordControlChangeEventCommandHandlerTests : UnitTestBase
{
    private readonly IControlChangeEventRepository _repo = Substitute.For<IControlChangeEventRepository>();

    private RecordControlChangeEventCommandHandler Sut() => new(_repo, NullLogger<RecordControlChangeEventCommandHandler>());

    private static RecordControlChangeEventCommand ValidCommand(Guid otAssetId) => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), otAssetId,
        "Setpoint drift", "SetPoint.Temperature", "72.0", "95.0", FixedUtcNow, null, false, TestUserId.Value);

    [Fact]
    public async Task Handle_WithNoPriorRecordForAsset_RecordsWithNullPriorHash()
    {
        var otAssetId = Guid.NewGuid();
        _repo.GetLatestForAssetAsync(Arg.Any<OtAssetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ControlChangeEventAggregate?)null);

        ControlChangeEventAggregate? captured = null;
        await _repo.AddAsync(Arg.Do<ControlChangeEventAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        var result = await Sut().Handle(ValidCommand(otAssetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.PriorRecordHash.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithPriorRecordForAsset_ChainsPriorRecordHashFromRepository()
    {
        var otAssetId = Guid.NewGuid();
        var priorRecord = ControlChangeEventAggregate.Record(
            createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
            causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
            otAssetId: OtAssetId.FromGuid(otAssetId), changeDescription: "prior", changedParameter: "SetPoint.Pressure",
            oldValue: "10", newValue: "12", detectedAtUtc: FixedTimestamp, priorRecordHash: null);

        _repo.GetLatestForAssetAsync(Arg.Any<OtAssetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(priorRecord);

        ControlChangeEventAggregate? captured = null;
        await _repo.AddAsync(Arg.Do<ControlChangeEventAggregate>(a => captured = a), Arg.Any<CancellationToken>());

        var result = await Sut().Handle(ValidCommand(otAssetId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured!.PriorRecordHash.Should().Be(priorRecord.RecordHash);
    }

    [Fact]
    public async Task Handle_WithApprovedChangeRecord_ClassifiesAsAuthorized()
    {
        var otAssetId = Guid.NewGuid();
        _repo.GetLatestForAssetAsync(Arg.Any<OtAssetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ControlChangeEventAggregate?)null);

        var result = await Sut().Handle(ValidCommand(otAssetId) with { HasApprovedChangeRecord = true }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AuthorizationStatus.Should().Be("Authorized");
    }

    [Fact]
    public async Task Handle_WithoutApprovalAndUnknownSource_ClassifiesAsUnauthorized()
    {
        var otAssetId = Guid.NewGuid();
        _repo.GetLatestForAssetAsync(Arg.Any<OtAssetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ControlChangeEventAggregate?)null);

        var result = await Sut().Handle(ValidCommand(otAssetId) with { HasApprovedChangeRecord = false, ChangeSourceUserId = null }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AuthorizationStatus.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureInsteadOfPropagating()
    {
        _repo.GetLatestForAssetAsync(Arg.Any<OtAssetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<ControlChangeEventAggregate?>>(_ => throw new TimeoutException("simulated DB timeout"));

        var result = await Sut().Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.ControlChangeEvent.RecordFailed");
    }
}

public sealed class FlagUnauthorizedControlChangeCommandHandlerTests : UnitTestBase
{
    private readonly IControlChangeEventRepository _repo = Substitute.For<IControlChangeEventRepository>();

    private FlagUnauthorizedControlChangeCommandHandler Sut() => new(_repo, NullLogger<FlagUnauthorizedControlChangeCommandHandler>());

    private static ControlChangeEventAggregate PendingRecord() => ControlChangeEventAggregate.Record(
        createdBy: TestUserId, createdAtUtc: FixedTimestamp, correlationId: TestCorrelationId,
        causationId: TestCausationId, actor: TestActor, tenantId: TestTenantId.Value, siteId: Guid.NewGuid(),
        otAssetId: OtAssetId.New(), changeDescription: "desc", changedParameter: "SetPoint.Pressure",
        oldValue: "10", newValue: "12", detectedAtUtc: FixedTimestamp, priorRecordHash: null);

    [Fact]
    public async Task Handle_WithExistingRecord_ReclassifiesAsUnauthorized()
    {
        var record = PendingRecord();
        _repo.GetByIdAsync(record.ControlChangeEventId, TestTenantId.Value, Arg.Any<CancellationToken>()).Returns(record);

        var command = new FlagUnauthorizedControlChangeCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value,
            record.ControlChangeEventId.Value, TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AuthorizationStatus.Should().Be("Unauthorized");
        await _repo.Received(1).UpdateAsync(record, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRecordDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids.ControlChangeEventId>(),
            Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ControlChangeEventAggregate?)null);

        var command = new FlagUnauthorizedControlChangeCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), TestUserId.Value);

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("CyberPhysicalSecurity.ControlChangeEvent.NotFound");
        result.Error.Severity.Should().Be(PlantSafety.Application.Common.Errors.ErrorSeverity.NotFound);
    }
}
