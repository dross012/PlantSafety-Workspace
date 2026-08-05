// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.Resilience
//  Layer:          Tests (Resilience)
//  File:           CraneHoistSystemsResilienceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Resilience tests verifying every CraneHoistSystems command handler degrades
//                  gracefully (returns Result.Failure) rather than letting infrastructure
//                  exceptions (transient SQL failures, timeouts) propagate as unhandled
//                  exceptions out of the MediatR pipeline. Isolated, deterministic, <50ms per
//                  test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.MachineSafety.Commands;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Interfaces;
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.ValueObjects.Ids;
using Agg = PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates.CraneHoistSystems;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.Resilience;

public sealed class CraneHoistSystemsResilienceTests : UnitTestBase
{
    private static Agg RegisteredAsset() => Agg.Create(
        TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
        TestTenantId.Value, null, "RES-1", "Resilience Crane", CraneHoistAssetType.OverheadBridgeCrane, 5000m);

    [Theory]
    [InlineData(typeof(TimeoutException))]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(Exception))]
    public async Task RegisterHandler_WhenRepositoryThrowsTransientException_ReturnsFailureNotException(Type exceptionType)
    {
        var repo = Substitute.For<ICraneHoistSystemsRepository>();
        repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw (Exception)Activator.CreateInstance(exceptionType, "transient failure")!);
        var sut = new RegisterCraneHoistAssetCommandHandler(repo, NullLogger<RegisterCraneHoistAssetCommandHandler>());

        var command = new RegisterCraneHoistAssetCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            null, "RES-2", "Resilient Crane", "Hoist", 1000m, TestUserId.Value);

        var act = async () => await sut.Handle(command, CancellationToken.None);
        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RecordLiftEventHandler_WhenReadThrows_ReturnsFailureNotException()
    {
        var repo = Substitute.For<ICraneHoistSystemsRepository>();
        repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>())
            .Returns<Task<Agg?>>(_ => throw new TimeoutException("connection pool exhausted"));
        var sut = new RecordLiftEventCommandHandler(repo, NullLogger<RecordLiftEventCommandHandler>());

        var command = new RecordLiftEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            Guid.NewGuid(), 1000m, TestUserId.Value);

        var act = async () => await sut.Handle(command, CancellationToken.None);
        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RecordOverloadEventHandler_WhenWriteThrows_ReturnsFailureNotException()
    {
        var repo = Substitute.For<ICraneHoistSystemsRepository>();
        var asset = RegisteredAsset();
        repo.GetByIdAsync(Arg.Any<CraneHoistSystemsId>(), Arg.Any<CancellationToken>()).Returns(asset);
        repo.UpdateAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("deadlock victim"));
        var sut = new RecordOverloadEventCommandHandler(repo, NullLogger<RecordOverloadEventCommandHandler>());

        var command = new RecordOverloadEventCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            asset.CraneHoistSystemsId.Value, 6000m, TestUserId.Value);

        var act = async () => await sut.Handle(command, CancellationToken.None);
        (await act.Should().NotThrowAsync()).Which.IsFailure.Should().BeTrue();
    }
}
