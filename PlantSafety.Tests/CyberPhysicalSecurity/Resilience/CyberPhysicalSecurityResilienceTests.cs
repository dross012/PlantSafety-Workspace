// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.CyberPhysicalSecurity.Resilience
//  Layer:          Tests (Unit)
//  File:           CyberPhysicalSecurityResilienceTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Resilience/Chaos-category unit tests: verify each of the module's five write
//                  command handlers degrades gracefully (returns Result.Failure with a stable error
//                  code) rather than propagating an unhandled exception when the repository
//                  simulates transient infrastructure failures (timeouts, connection resets,
//                  transaction aborts) -- the module-level equivalent of Chaos-testing dependency
//                  failure injection at the unit level (no live SQL Server/Testcontainers dependency
//                  required for this suite).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.CyberPhysicalIncident.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.ControlChangeEvent.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.OtAsset.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Commands;
using PlantSafety.Application.CyberPhysicalSecurity.SensorIntegrityCheck.Commands;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Interfaces;
using OtAssetAggregate = PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Aggregates.OtAsset;
using ControlChangeEventAggregate = PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Aggregates.ControlChangeEvent;
using SensorIntegrityCheckAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Aggregates.SensorIntegrityCheck;
using SabotageIndicatorAggregate = PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates.SabotageIndicator;
using CyberPhysicalIncidentAggregate = PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.Aggregates.CyberPhysicalIncident;

namespace PlantSafety.Tests.CyberPhysicalSecurity.Resilience;

public sealed class CyberPhysicalSecurityResilienceTests : UnitTestBase
{
    private static readonly TimeoutException SimulatedTransientFailure = new("simulated transient SQL connection failure");

    [Fact]
    public async Task RegisterOtAssetCommandHandler_OnTransientRepositoryFailure_DegradesGracefully()
    {
        var repo = Substitute.For<IOtAssetRepository>();
        repo.AddAsync(Arg.Any<OtAssetAggregate>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw SimulatedTransientFailure);
        var sut = new RegisterOtAssetCommandHandler(repo, NullLogger<RegisterOtAssetCommandHandler>());

        var command = new RegisterOtAssetCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
            "PLC-101", "Line 3 PLC", "Plc", "", "", "", "", "Medium", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue("a transient infra failure must degrade to a Result.Failure, never an unhandled exception");
    }

    [Fact]
    public async Task RecordControlChangeEventCommandHandler_OnTransientRepositoryFailure_DegradesGracefully()
    {
        var repo = Substitute.For<IControlChangeEventRepository>();
        repo.GetLatestForAssetAsync(Arg.Any<PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids.OtAssetId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<ControlChangeEventAggregate?>>(_ => throw SimulatedTransientFailure);
        var sut = new RecordControlChangeEventCommandHandler(repo, NullLogger<RecordControlChangeEventCommandHandler>());

        var command = new RecordControlChangeEventCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), Guid.NewGuid(),
            "desc", "SetPoint.Pressure", "10", "12", FixedUtcNow, null, false, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RunSensorIntegrityCheckCommandHandler_OnTransientRepositoryFailure_DegradesGracefully()
    {
        var repo = Substitute.For<ISensorIntegrityCheckRepository>();
        repo.AddAsync(Arg.Any<SensorIntegrityCheckAggregate>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw SimulatedTransientFailure);
        var sut = new RunSensorIntegrityCheckCommandHandler(repo, NullLogger<RunSensorIntegrityCheckCommandHandler>());

        var command = new RunSensorIntegrityCheckCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), Guid.NewGuid(),
            "Calibration", "Pass", "OK", TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RaiseSabotageIndicatorCommandHandler_OnTransientRepositoryFailure_DegradesGracefully()
    {
        var repo = Substitute.For<ISabotageIndicatorRepository>();
        repo.AddAsync(Arg.Any<SabotageIndicatorAggregate>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw SimulatedTransientFailure);
        var sut = new RaiseSabotageIndicatorCommandHandler(repo, NullLogger<RaiseSabotageIndicatorCommandHandler>());

        var command = new RaiseSabotageIndicatorCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
            "ControlChange", "desc", "High", null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task OpenCyberPhysicalIncidentCommandHandler_OnTransientRepositoryFailure_DegradesGracefully()
    {
        var repo = Substitute.For<ICyberPhysicalIncidentRepository>();
        repo.AddAsync(Arg.Any<CyberPhysicalIncidentAggregate>(), Arg.Any<CancellationToken>()).Returns<Task>(_ => throw SimulatedTransientFailure);
        var sut = new OpenCyberPhysicalIncidentCommandHandler(repo, NullLogger<OpenCyberPhysicalIncidentCommandHandler>());

        var command = new OpenCyberPhysicalIncidentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(),
            "Incident", "desc", "High", TestUserId.Value, null, TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CloseCyberPhysicalIncidentCommandHandler_OnRepositoryLookupFailure_DegradesGracefully()
    {
        var repo = Substitute.For<ICyberPhysicalIncidentRepository>();
        repo.GetByIdAsync(Arg.Any<PlantSafety.Domain.CyberPhysicalSecurity.CyberPhysicalIncident.ValueObjects.Ids.CyberPhysicalIncidentId>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<CyberPhysicalIncidentAggregate?>>(_ => throw SimulatedTransientFailure);
        var sut = new CloseCyberPhysicalIncidentCommandHandler(repo, NullLogger<CloseCyberPhysicalIncidentCommandHandler>());

        var command = new CloseCyberPhysicalIncidentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), FixedUtcNow, TestTenantId.Value, Guid.NewGuid(), TestUserId.Value);

        var result = await sut.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
