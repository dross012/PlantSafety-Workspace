// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.LogisticsTransportSafety.DriverQualifications
//  Layer:          IntegrationTests
//  File:           DriverQualificationLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Full Qualify -> Suspend -> Reinstate -> Retire lifecycle coverage for the
//                  DriverQualification aggregate, plus the GetDriverQualificationStatus
//                  read-model query. Exercises the real MediatR pipeline, the real
//                  DriverQualificationDapperRepository, and the real
//                  [LogisticsTransportSafety].[DriverQualifications] table + stored procedures
//                  applied to the Testcontainers SQL Server instance.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.LogisticsTransportSafety.DriverQualifications;

[Collection(nameof(IntegrationTestCollection))]
public sealed class DriverQualificationLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task DriverQualificationLifecycle_QualifySuspendReinstateRetire_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var driverUserId = Guid.NewGuid();

            var qualifyResult = await sender.Send(new QualifyDriverCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                driverUserId, "CommercialDriverLicense", "CDL-99110", FixedUtcNow.AddYears(-1), FixedUtcNow.AddYears(1), ActorId),
                CancellationToken.None);

            qualifyResult.IsSuccess.Should().BeTrue();
            qualifyResult.Value.Status.Should().Be("Qualified");
            var dqId = qualifyResult.Value.DriverQualificationId;

            var statusAfterQualify = await sender.Send(new GetDriverQualificationStatusQuery(TenantId, driverUserId), CancellationToken.None);
            statusAfterQualify.IsSuccess.Should().BeTrue();
            statusAfterQualify.Value.Items.Should().Contain(i => i.DriverQualificationId == dqId && i.QualificationStatus == "Qualified");

            var suspendResult = await sender.Send(new SuspendDriverQualificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, dqId, "Failed drug test", ActorId), CancellationToken.None);

            suspendResult.IsSuccess.Should().BeTrue();
            suspendResult.Value.Status.Should().Be("Suspended");

            var reinstateResult = await sender.Send(new ReinstateDriverQualificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, dqId, ActorId), CancellationToken.None);

            reinstateResult.IsSuccess.Should().BeTrue();
            reinstateResult.Value.Status.Should().Be("Qualified");

            var retireResult = await sender.Send(new RetireDriverQualificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, dqId, ActorId), CancellationToken.None);

            retireResult.IsSuccess.Should().BeTrue();
            retireResult.Value.Status.Should().Be("Retired");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task GetDriverQualificationStatus_ForUnknownDriver_ShouldReturnEmptyItems()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new GetDriverQualificationStatusQuery(TenantId, Guid.NewGuid()), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Items.Should().BeEmpty();
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task ReinstateDriverQualification_WhenNotSuspended_ShouldFail()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var qualifyResult = await sender.Send(new QualifyDriverCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                Guid.NewGuid(), "ForkliftCertified", "FLC-1", FixedUtcNow.AddYears(-1), FixedUtcNow.AddYears(1), ActorId),
                CancellationToken.None);
            var dqId = qualifyResult.Value.DriverQualificationId;

            var reinstateResult = await sender.Send(new ReinstateDriverQualificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, dqId, ActorId), CancellationToken.None);

            reinstateResult.IsFailure.Should().BeTrue();
        }
        finally
        {
            scope.Dispose();
        }
    }
}
