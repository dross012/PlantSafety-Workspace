// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.LogisticsTransportSafety.LoadSecurementChecks
//  Layer:          IntegrationTests
//  File:           LoadSecurementCheckLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Full Record(fail) -> CompleteCorrectiveAction lifecycle coverage for the
//                  LoadSecurementCheck aggregate, plus the ListLoadSecurementFailures read-model
//                  query. Exercises the real MediatR pipeline, the real
//                  LoadSecurementCheckDapperRepository, and the real
//                  [LogisticsTransportSafety].[LoadSecurementChecks] table + stored procedures
//                  applied to the Testcontainers SQL Server instance.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.LoadSecurementChecks.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.LogisticsTransportSafety.LoadSecurementChecks;

[Collection(nameof(IntegrationTestCollection))]
public sealed class LoadSecurementCheckLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task LoadSecurementCheckLifecycle_RecordFailedThenCompleteCorrectiveAction_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var vehicleId = Guid.NewGuid();

            var recordResult = await sender.Send(new RecordLoadSecurementCheckCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, null,
                vehicleId, Guid.NewGuid(), false, 3, 55m, ActorId)
            {
                // RequestedAtUtc defaults to the real wall clock; pin it to the fixed test clock
                // so the recorded check's timestamp falls inside the +/-1 day failures window
                // asserted below regardless of what day this test actually runs.
                RequestedAtUtc = FixedUtcNow,
            }, CancellationToken.None);

            recordResult.IsSuccess.Should().BeTrue();
            recordResult.Value.Status.Should().Be("Failed");
            var checkId = recordResult.Value.LoadSecurementCheckId;

            var failuresResult = await sender.Send(new ListLoadSecurementFailuresQuery(
                TenantId, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)), CancellationToken.None);

            failuresResult.IsSuccess.Should().BeTrue();
            failuresResult.Value.Items.Should().Contain(c => c.LoadSecurementCheckId == checkId);

            var completeResult = await sender.Send(new CompleteCorrectiveActionLoadSecurementCheckCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, checkId, ActorId), CancellationToken.None);

            completeResult.IsSuccess.Should().BeTrue();
            completeResult.Value.Status.Should().Be("CorrectiveActionCompleted");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task RecordLoadSecurementCheck_WhenPassed_ShouldNotAppearInFailuresList()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var recordResult = await sender.Send(new RecordLoadSecurementCheckCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, null,
                Guid.NewGuid(), Guid.NewGuid(), true, 0, 100m, ActorId), CancellationToken.None);
            var checkId = recordResult.Value.LoadSecurementCheckId;

            var failuresResult = await sender.Send(new ListLoadSecurementFailuresQuery(
                TenantId, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)), CancellationToken.None);

            failuresResult.Value.Items.Should().NotContain(c => c.LoadSecurementCheckId == checkId);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CompleteCorrectiveAction_ForUnknownId_ShouldFail()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new CompleteCorrectiveActionLoadSecurementCheckCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), ActorId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.LoadSecurementChecks.NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
