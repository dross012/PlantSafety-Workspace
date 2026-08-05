// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.AutonomousVehicleSafety
//  Layer:          IntegrationTests
//  File:           AutonomousVehicleSafetySqlTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Repository/DB round-trip integration tests for the AutonomousVehicleSafety
//                  module, exercising the real MediatR pipeline (command/query handlers) through
//                  the real DI container against the Dapper repositories and stored procedures
//                  applied by TestDatabaseFactory.ApplyAutonomousVehicleSafetySchemaAsync.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleAsset.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.AutonomousVehicleIncident.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.Commands;
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.OperatingGeofence.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.SafetyCaseValidation.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.AutonomousVehicleSafety.Sql;

[Collection(nameof(IntegrationTestCollection))]
public sealed class AutonomousVehicleSafetySqlTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Schema_ShouldExist_InDatabase()
    {
        await using var conn = new SqlConnection(ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM sys.schemas WHERE name = 'AutonomousVehicleSafety'");

        count.Should().Be(1, "the AutonomousVehicleSafety schema must exist before running its SQL tests");
    }

    [Theory]
    [InlineData("AutonomousVehicleAsset")]
    [InlineData("AutonomousVehicleIncident")]
    [InlineData("HumanRobotInteractionEvent")]
    [InlineData("OperatingGeofence")]
    [InlineData("SafetyCaseValidation")]
    public async Task Table_ShouldExist_InAutonomousVehicleSafetySchema(string tableName)
    {
        await using var conn = new SqlConnection(ConnectionString);
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM sys.tables WHERE name = @tableName AND schema_id = SCHEMA_ID('AutonomousVehicleSafety')",
            new { tableName });

        count.Should().Be(1, $"[AutonomousVehicleSafety].[{tableName}] must exist");
    }

    [Fact]
    public async Task RegisterAndGetAutonomousVehicleAsset_ShouldRoundTripThroughRealPipeline()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var registerResult = await sender.Send(
                new RegisterAutonomousVehicleAssetCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId,
                    null, "AMR-SQL-001", "Amr", "SemiAutonomous", ActorId),
                CancellationToken.None);

            registerResult.IsSuccess.Should().BeTrue("RegisterAutonomousVehicleAssetCommand must succeed against the real stored procedure");
            registerResult.Value.Status.Should().Be("Registered");

            var getResult = await sender.Send(
                new GetAutonomousVehicleAssetByIdQuery(TenantId, registerResult.Value.AutonomousVehicleAssetId),
                CancellationToken.None);

            getResult.IsSuccess.Should().BeTrue();
            getResult.Value.Should().NotBeNull();
            getResult.Value!.AssetTag.Should().Be("AMR-SQL-001");
            getResult.Value.VehicleType.Should().Be("Amr");
            getResult.Value.Status.Should().Be("Registered");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordModeTransitionThenSuspend_ShouldPersistLifecycleAcrossRoundTrips()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var registerResult = await sender.Send(
                new RegisterAutonomousVehicleAssetCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId,
                    null, "AMR-SQL-002", "Amr", "SemiAutonomous", ActorId),
                CancellationToken.None);
            var assetId = registerResult.Value.AutonomousVehicleAssetId;

            var transitionResult = await sender.Send(
                new RecordAutonomyModeTransitionCommand(Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, assetId, "Autonomous", ActorId),
                CancellationToken.None);
            transitionResult.IsSuccess.Should().BeTrue();
            transitionResult.Value.Status.Should().Be("Active");

            var suspendResult = await sender.Send(
                new SuspendAutonomousOperationCommand(Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, assetId, "Repeated geofence violations", ActorId),
                CancellationToken.None);
            suspendResult.IsSuccess.Should().BeTrue();
            suspendResult.Value.Status.Should().Be("Suspended");

            var getResult = await sender.Send(new GetAutonomousVehicleAssetByIdQuery(TenantId, assetId), CancellationToken.None);
            getResult.Value!.Status.Should().Be("Suspended");
            getResult.Value.CurrentAutonomyMode.Should().Be("Autonomous");
            getResult.Value.ModeTransitionCount.Should().Be(1);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordAutonomyModeTransition_WhenAssetDoesNotExist_ShouldReturnNotFound()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(
                new RecordAutonomyModeTransitionCommand(Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), "Autonomous", ActorId),
                CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Be("AutonomousVehicleSafety.AutonomousVehicleAsset.NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task DefineOperatingGeofenceAndRecordViolation_ShouldSurfaceInActiveViolationsQuery()
    {
        var geofenceId = Guid.NewGuid();
        var fromUtc = FixedUtcNow.AddDays(-1);
        var toUtc = FixedUtcNow.AddDays(1);

        await using (var conn = new SqlConnection(ConnectionString))
        {
            await conn.OpenAsync();

            // Seed an Active geofence with a recorded violation directly (RecordViolation is only
            // reachable via the aggregate, and this module exposes no HTTP endpoint to invoke it;
            // matches the PlanningProjects Sql-test precedent of seeding real rows directly for
            // read-model verification).
            await conn.ExecuteAsync(
                @"INSERT INTO [AutonomousVehicleSafety].[OperatingGeofence]
                    (OperatingGeofenceId, TenantId, SiteId, Status, GeofenceName, ZoneType, BoundaryDefinition, ViolationCount, CreatedAtUtc, CreatedBy)
                  VALUES (@OperatingGeofenceId, @TenantId, NULL, 'Active', @GeofenceName, 'ExclusionZone', 'POLYGON((0 0,1 0,1 1,0 1,0 0))', 2, @CreatedAtUtc, @CreatedBy)",
                new
                {
                    OperatingGeofenceId = geofenceId,
                    TenantId,
                    GeofenceName = "SQL Round-Trip Exclusion Zone",
                    CreatedAtUtc = FixedUtcNow,
                    CreatedBy = ActorId,
                });
        }

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new ListActiveGeofenceViolationsQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().Contain(i => i.OperatingGeofenceId == geofenceId && i.ViolationCount == 2);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ValidateSafetyCase_ShouldRoundTrip_AndSurfaceInStatusQuery()
    {
        var assetId = Guid.NewGuid();
        var validatedByUserId = Guid.NewGuid();

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var validateResult = await sender.Send(
                new ValidateSafetyCaseCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                    assetId, "AnsiRia1508", "v1.0", 180, validatedByUserId, ActorId),
                CancellationToken.None);

            validateResult.IsSuccess.Should().BeTrue();
            validateResult.Value.Status.Should().Be("Validated");
            validateResult.Value.ExpiresAtUtc.Should().NotBeNull();

            var statusResult = await sender.Send(new GetSafetyCaseStatusQuery(TenantId, assetId), CancellationToken.None);

            statusResult.IsSuccess.Should().BeTrue();
            statusResult.Value.Should().NotBeNull();
            statusResult.Value!.Status.Should().Be("Validated");
            statusResult.Value.DocumentVersion.Should().Be("v1.0");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordHumanRobotInteractionEvent_ShouldRoundTrip_AndSurfaceInListQuery()
    {
        var assetId = Guid.NewGuid();
        var fromUtc = FixedUtcNow.AddDays(-1);
        var toUtc = FixedUtcNow.AddDays(1);

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var recordResult = await sender.Send(
                new RecordHumanRobotInteractionEventCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                    assetId, "NearMiss", "High", 0.4m, "Loading dock 3", ActorId),
                CancellationToken.None);

            recordResult.IsSuccess.Should().BeTrue();
            recordResult.Value.Status.Should().Be("Recorded");

            var listResult = await sender.Send(new ListHumanRobotInteractionEventsQuery(TenantId, null, fromUtc, toUtc), CancellationToken.None);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(i => i.HumanRobotInteractionEventId == recordResult.Value.HumanRobotInteractionEventId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordAutonomousVehicleIncident_ShouldRoundTrip_AndSurfaceInHistoryQuery()
    {
        var assetId = Guid.NewGuid();

        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var recordResult = await sender.Send(
                new RecordAutonomousVehicleIncidentCommand(
                    Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                    assetId, "GeofenceBreach", "High", "Vehicle breached the north exclusion zone", ActorId),
                CancellationToken.None);

            recordResult.IsSuccess.Should().BeTrue();
            recordResult.Value.Status.Should().Be("Reported");

            var historyResult = await sender.Send(new GetIncidentHistoryByAssetQuery(TenantId, assetId), CancellationToken.None);

            historyResult.IsSuccess.Should().BeTrue();
            historyResult.Value.Items.Should().ContainSingle(i => i.AutonomousVehicleIncidentId == recordResult.Value.AutonomousVehicleIncidentId);
        }
        finally
        {
            scope.Dispose();
        }
    }
}
