// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.LogisticsTransportSafety.TransportVehicles
//  Layer:          IntegrationTests
//  File:           TransportVehicleLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Full Register -> GetById -> ScheduleInspection -> RecordInspectionResult ->
//                  Retire lifecycle coverage for the TransportVehicle aggregate. Exercises the
//                  real MediatR pipeline, the real TransportVehicleDapperRepository, and the real
//                  [LogisticsTransportSafety].[TransportVehicles] table + stored procedures applied
//                  to the Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.LogisticsTransportSafety.cs). This module's SQL was
//                  only just added and had never been exercised against a live DB before this
//                  pass.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.LogisticsTransportSafety.TransportVehicles;

[Collection(nameof(IntegrationTestCollection))]
public sealed class TransportVehicleLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task TransportVehicleLifecycle_RegisterScheduleRecordRetire_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            // ---- Register ---------------------------------------------------
            var registerResult = await sender.Send(new RegisterTransportVehicleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                "Forklift 12", "Forklift", "FLT-012", ActorId), CancellationToken.None);

            registerResult.IsSuccess.Should().BeTrue("RegisterTransportVehicleCommand must succeed against the real stored procedure");
            var vehicleId = registerResult.Value.TransportVehicleId;

            // ---- GetById (post-register) --------------------------------------
            var afterRegister = await sender.Send(new GetTransportVehicleByIdQuery(TenantId, vehicleId), CancellationToken.None);
            afterRegister.IsSuccess.Should().BeTrue();
            afterRegister.Value.Item.Should().NotBeNull("the vehicle must be readable immediately after registration");
            afterRegister.Value.Item!.VehicleStatus.Should().Be("Registered");
            afterRegister.Value.Item.VehicleIdentifier.Should().Be("FLT-012");

            // ---- ScheduleInspection ---------------------------------------------
            var scheduleResult = await sender.Send(new ScheduleInspectionTransportVehicleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, vehicleId, FixedUtcNow.AddDays(7), ActorId), CancellationToken.None);

            scheduleResult.IsSuccess.Should().BeTrue();
            scheduleResult.Value.Status.Should().Be("InspectionScheduled");

            // ---- RecordInspectionResult (pass) -----------------------------------
            var recordResult = await sender.Send(new RecordInspectionResultTransportVehicleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, vehicleId, true, 94m, ActorId), CancellationToken.None);

            recordResult.IsSuccess.Should().BeTrue();
            recordResult.Value.Status.Should().Be("InspectionPassed");

            var afterInspection = await sender.Send(new GetTransportVehicleByIdQuery(TenantId, vehicleId), CancellationToken.None);
            afterInspection.Value.Item!.VehicleStatus.Should().Be("InspectionPassed");

            // ---- Retire -------------------------------------------------------------
            var retireResult = await sender.Send(new RetireTransportVehicleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, vehicleId, ActorId), CancellationToken.None);

            retireResult.IsSuccess.Should().BeTrue();
            retireResult.Value.Status.Should().Be("Retired");

            var afterRetire = await sender.Send(new GetTransportVehicleByIdQuery(TenantId, vehicleId), CancellationToken.None);
            afterRetire.Value.Item!.VehicleStatus.Should().Be("Retired");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetTransportVehicleById_ForUnknownId_ShouldReturnSuccessWithNullItem()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetTransportVehicleByIdQuery(TenantId, Guid.NewGuid()), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Item.Should().BeNull();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordInspectionResult_WithoutScheduling_ShouldFail()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var registerResult = await sender.Send(new RegisterTransportVehicleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                "Van 4", "Van", "VAN-004", ActorId), CancellationToken.None);
            var vehicleId = registerResult.Value.TransportVehicleId;

            var recordResult = await sender.Send(new RecordInspectionResultTransportVehicleCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, vehicleId, true, 90m, ActorId), CancellationToken.None);

            recordResult.IsFailure.Should().BeTrue(
                "recording an inspection result requires the vehicle to have a scheduled inspection first");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
