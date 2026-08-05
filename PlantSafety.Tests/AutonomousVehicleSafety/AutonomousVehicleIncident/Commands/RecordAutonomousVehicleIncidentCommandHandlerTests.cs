// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.AutonomousVehicleIncident.Commands
//  Layer:          Tests (Unit)
//  File:           RecordAutonomousVehicleIncidentCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RecordAutonomousVehicleIncidentCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Interfaces;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Aggregates.AutonomousVehicleIncident;

namespace PlantSafety.Tests.AutonomousVehicleSafety.AutonomousVehicleIncident.Commands;

public sealed class RecordAutonomousVehicleIncidentCommandHandlerTests : UnitTestBase
{
    private readonly IAutonomousVehicleIncidentRepository _repo = Substitute.For<IAutonomousVehicleIncidentRepository>();

    private RecordAutonomousVehicleIncidentCommandHandler Sut() => new(_repo, NullLogger<RecordAutonomousVehicleIncidentCommandHandler>());

    private static RecordAutonomousVehicleIncidentCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        AutonomousVehicleAssetId: Guid.NewGuid(),
        IncidentType: "GeofenceBreach",
        SeverityLevel: "High",
        DescriptionSummary: "Vehicle breached the north exclusion zone",
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistIncident_AndReturnSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Reported");
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.DescriptionSummary == "Vehicle breached the north exclusion zone"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedIncidentType_ShouldDefaultToCollisionNearMiss()
    {
        var command = ValidCommand() with { IncidentType = "NotARealType" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.IncidentType.ToString() == "CollisionNearMiss"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithRecordFailedErrorCode()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.AutonomousVehicleIncident.RecordFailed");
    }
}
