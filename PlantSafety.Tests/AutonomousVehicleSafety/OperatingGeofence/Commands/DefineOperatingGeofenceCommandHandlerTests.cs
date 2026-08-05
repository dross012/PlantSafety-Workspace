// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.AutonomousVehicleSafety.OperatingGeofence.Commands
//  Layer:          Tests (Unit)
//  File:           DefineOperatingGeofenceCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for DefineOperatingGeofenceCommandHandler.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Interfaces;
using Agg = PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Aggregates.OperatingGeofence;

namespace PlantSafety.Tests.AutonomousVehicleSafety.OperatingGeofence.Commands;

public sealed class DefineOperatingGeofenceCommandHandlerTests : UnitTestBase
{
    private readonly IOperatingGeofenceRepository _repo = Substitute.For<IOperatingGeofenceRepository>();

    private DefineOperatingGeofenceCommandHandler Sut() => new(_repo, NullLogger<DefineOperatingGeofenceCommandHandler>());

    private static DefineOperatingGeofenceCommand ValidCommand() => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        SiteId: null,
        GeofenceName: "North Yard Exclusion",
        ZoneType: "ExclusionZone",
        BoundaryDefinition: "POLYGON((0 0,1 0,1 1,0 1,0 0))",
        MaxSpeedKph: null,
        PerformedByUserId: TestUserId.Value);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistGeofence_AndReturnSuccess()
    {
        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Draft");
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.GeofenceName == "North Yard Exclusion"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnrecognizedZoneType_ShouldDefaultToOperatingZone()
    {
        var command = ValidCommand() with { ZoneType = "NotARealZone" };

        var result = await Sut().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).AddAsync(Arg.Is<Agg>(a => a.ZoneType.ToString() == "OperatingZone"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailureWithDefineFailedErrorCode()
    {
        _repo.AddAsync(Arg.Any<Agg>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException("db unavailable"));

        var result = await Sut().Handle(ValidCommand(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.OperatingGeofence.DefineFailed");
    }
}
