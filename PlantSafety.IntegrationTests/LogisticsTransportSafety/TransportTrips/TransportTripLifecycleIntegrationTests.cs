// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.LogisticsTransportSafety.TransportTrips
//  Layer:          IntegrationTests
//  File:           TransportTripLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Full Plan -> Complete and Plan -> Cancel lifecycle coverage for the
//                  TransportTrip aggregate, plus the ListActiveTrips read-model query. Exercises
//                  the real MediatR pipeline, the real TransportTripDapperRepository, and the
//                  real [LogisticsTransportSafety].[TransportTrips] table + stored procedures
//                  applied to the Testcontainers SQL Server instance.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.TransportTrips.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.LogisticsTransportSafety.TransportTrips;

[Collection(nameof(IntegrationTestCollection))]
public sealed class TransportTripLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task TransportTripLifecycle_PlanThenComplete_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var vehicleId = Guid.NewGuid();
            var driverUserId = Guid.NewGuid();

            var planResult = await sender.Send(new PlanTransportTripCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                vehicleId, driverUserId, "OffSiteDelivery", "Dock 3 to Warehouse B",
                FixedUtcNow.AddHours(1), FixedUtcNow.AddHours(3), ActorId), CancellationToken.None);

            planResult.IsSuccess.Should().BeTrue();
            planResult.Value.Status.Should().Be("Planned");
            var tripId = planResult.Value.TransportTripId;

            var activeAfterPlan = await sender.Send(new ListActiveTripsQuery(TenantId, null), CancellationToken.None);
            activeAfterPlan.IsSuccess.Should().BeTrue();
            activeAfterPlan.Value.Items.Should().Contain(t => t.TransportTripId == tripId);

            var completeResult = await sender.Send(new CompleteTransportTripCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, tripId,
                FixedUtcNow.AddHours(1), FixedUtcNow.AddHours(2.5), 38.2m, ActorId), CancellationToken.None);

            completeResult.IsSuccess.Should().BeTrue();
            completeResult.Value.Status.Should().Be("Completed");

            var activeAfterComplete = await sender.Send(new ListActiveTripsQuery(TenantId, null), CancellationToken.None);
            activeAfterComplete.Value.Items.Should().NotContain(t => t.TransportTripId == tripId,
                "a completed trip must no longer appear in the active-trips read model");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task TransportTripLifecycle_PlanThenCancel_ShouldRemoveFromActiveList()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var planResult = await sender.Send(new PlanTransportTripCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                Guid.NewGuid(), Guid.NewGuid(), "PersonnelTransport", "Gate 1 to Site Office",
                FixedUtcNow.AddHours(1), FixedUtcNow.AddHours(2), ActorId), CancellationToken.None);
            var tripId = planResult.Value.TransportTripId;

            var cancelResult = await sender.Send(new CancelTransportTripCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, tripId, ActorId), CancellationToken.None);

            cancelResult.IsSuccess.Should().BeTrue();
            cancelResult.Value.Status.Should().Be("Cancelled");

            var activeAfterCancel = await sender.Send(new ListActiveTripsQuery(TenantId, null), CancellationToken.None);
            activeAfterCancel.Value.Items.Should().NotContain(t => t.TransportTripId == tripId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CompleteTransportTrip_ForUnknownId_ShouldFail()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new CompleteTransportTripCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(),
                FixedUtcNow, FixedUtcNow.AddHours(1), 10m, ActorId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportTrips.NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
