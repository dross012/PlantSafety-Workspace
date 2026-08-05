// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.LogisticsTransportSafety.Carriers
//  Layer:          IntegrationTests
//  File:           CarrierLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Full Register -> Qualify -> Suspend -> Retire lifecycle coverage for the
//                  Carrier aggregate, plus the GetCarriers read-model query. Exercises the real
//                  MediatR pipeline, the real CarrierDapperRepository, and the real
//                  [LogisticsTransportSafety].[Carriers] table + stored procedures applied to the
//                  Testcontainers SQL Server instance.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.LogisticsTransportSafety.Carriers.Queries;
using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.LogisticsTransportSafety.Carriers;

[Collection(nameof(IntegrationTestCollection))]
public sealed class CarrierLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CarrierLifecycle_RegisterQualifySuspendRetire_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var registerResult = await sender.Send(new RegisterCarrierCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                "Acme Freight Lines", "ThirdPartyContractCarrier", FixedUtcNow.AddYears(1), ActorId), CancellationToken.None);

            registerResult.IsSuccess.Should().BeTrue();
            registerResult.Value.Status.Should().Be("PendingQualification");
            var carrierId = registerResult.Value.CarrierId;

            var qualifyResult = await sender.Send(new QualifyCarrierCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, carrierId, 91m, ActorId), CancellationToken.None);

            qualifyResult.IsSuccess.Should().BeTrue();
            qualifyResult.Value.Status.Should().Be("Qualified");

            var suspendResult = await sender.Send(new SuspendCarrierCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, carrierId, "Insurance lapsed", ActorId), CancellationToken.None);

            suspendResult.IsSuccess.Should().BeTrue();
            suspendResult.Value.Status.Should().Be("Suspended");

            var retireResult = await sender.Send(new RetireCarrierCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, carrierId, ActorId), CancellationToken.None);

            retireResult.IsSuccess.Should().BeTrue();
            retireResult.Value.Status.Should().Be("Retired");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetCarriers_AfterRegistration_ShouldContainTheNewCarrier()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var registerResult = await sender.Send(new RegisterCarrierCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                "Beta Transport Co", "InHouseFleet", null, ActorId), CancellationToken.None);
            var carrierId = registerResult.Value.CarrierId;

            var listResult = await sender.Send(new GetCarriersQuery(
                TenantId, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)), CancellationToken.None);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(c => c.CarrierId == carrierId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task QualifyCarrier_ForUnknownId_ShouldFail()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new QualifyCarrierCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), 90m, ActorId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.Carriers.NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
