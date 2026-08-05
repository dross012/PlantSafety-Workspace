// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HazardousWasteManagement.Commands
//  Layer:          Tests (Unit)
//  File:           CloseAccumulationContainerCommandHandlerTests.cs
//  Created:        2026-08-04
//  Author:         Testing Agent
//  SchemaVersion:  2026.2
//  Description:    Handler tests for CloseAccumulationContainerCommandHandler, including the
//                  ErrorSeverity.NotFound regression this test suite surfaced and fixed (missing
//                  severity previously defaulted to ErrorSeverity.Error, mapping to HTTP 500
//                  instead of the documented HTTP 404).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;
using Agg = PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Aggregates.WasteContainer;

namespace PlantSafety.Tests.HazardousWasteManagement.Commands;

public sealed class CloseAccumulationContainerCommandHandlerTests : UnitTestBase
{
    private readonly IWasteContainerRepository _repo = Substitute.For<IWasteContainerRepository>();

    private CloseAccumulationContainerCommandHandler Sut() => new(_repo, NullLogger<CloseAccumulationContainerCommandHandler>());

    private static Agg OpenContainer() =>
        Agg.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, Guid.NewGuid(), Guid.NewGuid(), "DRUM-001", 55m, 90);

    private static CloseAccumulationContainerCommand CommandFor(Guid wasteContainerId) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        WasteContainerId: wasteContainerId,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithExistingOpenContainer_ClosesAndReturnsSuccess()
    {
        var container = OpenContainer();
        _repo.GetByIdAsync(Arg.Any<WasteContainerId>(), Arg.Any<CancellationToken>()).Returns(container);

        var result = await Sut().Handle(CommandFor(container.WasteContainerId.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Closed");
        await _repo.Received(1).UpdateAsync(container, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenContainerDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<WasteContainerId>(), Arg.Any<CancellationToken>()).Returns((Agg?)null);

        var result = await Sut().Handle(CommandFor(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.WasteContainer.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound, "a missing WasteContainer must map to HTTP 404, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenAlreadyClosed_ReturnsFailureWithoutPropagating()
    {
        var container = OpenContainer();
        container.Close(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _repo.GetByIdAsync(Arg.Any<WasteContainerId>(), Arg.Any<CancellationToken>()).Returns(container);

        var result = await Sut().Handle(CommandFor(container.WasteContainerId.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HazardousWasteManagement.WasteContainer.CloseFailed");
    }
}
