// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.LogisticsTransportSafety.TransportIncidents
//  Layer:          IntegrationTests
//  File:           TransportIncidentLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Full Record -> BeginInvestigation -> Close lifecycle coverage for the
//                  TransportIncident aggregate, plus the GetTransportIncidentHistory read-model
//                  query. Exercises the real MediatR pipeline, the real
//                  TransportIncidentDapperRepository, and the real
//                  [LogisticsTransportSafety].[TransportIncidents] table + stored procedures
//                  applied to the Testcontainers SQL Server instance.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;

using PlantSafety.Application.LogisticsTransportSafety.Commands;
using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.LogisticsTransportSafety.TransportIncidents;

[Collection(nameof(IntegrationTestCollection))]
public sealed class TransportIncidentLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task TransportIncidentLifecycle_RecordBeginInvestigationClose_ShouldRoundTripThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var vehicleId = Guid.NewGuid();
            var driverUserId = Guid.NewGuid();

            var recordResult = await sender.Send(new RecordTransportIncidentCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, null,
                vehicleId, driverUserId, "Collision", "Major", "Loading dock 4", ActorId)
            {
                // RequestedAtUtc defaults to the real wall clock; pin it to the fixed test clock
                // so the recorded incident's timestamp falls inside the +/-1 day history window
                // asserted below regardless of what day this test actually runs.
                RequestedAtUtc = FixedUtcNow,
            }, CancellationToken.None);

            recordResult.IsSuccess.Should().BeTrue();
            recordResult.Value.Status.Should().Be("Reported");
            var incidentId = recordResult.Value.TransportIncidentId;

            var historyAfterRecord = await sender.Send(new GetTransportIncidentHistoryQuery(
                TenantId, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)), CancellationToken.None);

            historyAfterRecord.IsSuccess.Should().BeTrue();
            historyAfterRecord.Value.Items.Should().Contain(i => i.TransportIncidentId == incidentId && i.IncidentStatus == "Reported");

            var beginResult = await sender.Send(new BeginInvestigationTransportIncidentCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, incidentId, ActorId), CancellationToken.None);

            beginResult.IsSuccess.Should().BeTrue();
            beginResult.Value.Status.Should().Be("UnderInvestigation");

            var closeResult = await sender.Send(new CloseTransportIncidentCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, incidentId, ActorId), CancellationToken.None);

            closeResult.IsSuccess.Should().BeTrue();
            closeResult.Value.Status.Should().Be("Closed");

            var historyAfterClose = await sender.Send(new GetTransportIncidentHistoryQuery(
                TenantId, null, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)), CancellationToken.None);
            historyAfterClose.Value.Items.Should().Contain(i => i.TransportIncidentId == incidentId && i.IncidentStatus == "Closed");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task CloseTransportIncident_BeforeInvestigationBegan_ShouldFail()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var recordResult = await sender.Send(new RecordTransportIncidentCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null, null,
                Guid.NewGuid(), Guid.NewGuid(), "NearMiss", "Minor", "Yard entrance", ActorId), CancellationToken.None);
            var incidentId = recordResult.Value.TransportIncidentId;

            var closeResult = await sender.Send(new CloseTransportIncidentCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, incidentId, ActorId), CancellationToken.None);

            closeResult.IsFailure.Should().BeTrue();
            closeResult.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportIncidents.CloseFailed");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task BeginInvestigation_ForUnknownId_ShouldFail()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var result = await sender.Send(new BeginInvestigationTransportIncidentCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, Guid.NewGuid(), ActorId), CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.TransportIncidents.NotFound");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
