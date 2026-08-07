// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.PublicSafetyCommunication
//  Layer:          IntegrationTests
//  File:           PublicSafetyCommunicationLifecycleIntegrationTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Testing Agent)
//  SchemaVersion:  2026.1
//  Description:    Exercises the PublicSafetyCommunication module's 4 aggregates end-to-end
//                  through the real MediatR pipeline, the real Dapper repositories, and the real
//                  [PublicSafetyCommunication] schema + stored procedures applied to the
//                  Testcontainers SQL Server instance (see
//                  Setup\TestDatabaseFactory.ApplyPublicSafetyCommunicationSchemaAsync). Mirrors
//                  PermitLifecycleIntegrationTests's Create-&gt;GetById-&gt;Approve-&gt;List shape.
//                  Deterministic, parallel-safe, additive-only.
// ============================================================================
using MediatR;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Queries;
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Queries;
using PlantSafety.Application.PublicSafetyCommunication.IncidentNotificationDispatch.Queries;
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Queries;
using PlantSafety.IntegrationTests.Common.TestExtensions;
using PlantSafety.IntegrationTests.Common.TestFixtures;

namespace PlantSafety.IntegrationTests.PublicSafetyCommunication;

[Collection(nameof(IntegrationTestCollection))]
public sealed class PublicSafetyCommunicationLifecycleIntegrationTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RegisterPublicSafetyAgency_ThenGetById_RoundTripsThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var registerResult = await sender.Send(new RegisterPublicSafetyAgencyCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                "County Fire Department", "Fire", "County district 4", "555-0100", "dispatch@countyfire.example")
            { ActorId = ActorId }, CancellationToken.None);

            registerResult.IsSuccess.Should().BeTrue();
            registerResult.Value.PublicSafetyAgencyId.Should().NotBe(Guid.Empty);
            registerResult.Value.Status.Should().Be("Active");

            var listResult = await sender.Send(
                new ListPublicSafetyAgenciesQuery(TenantId, null, null, false), CancellationToken.None);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(i => i.PublicSafetyAgencyId == registerResult.Value.PublicSafetyAgencyId
                && i.AgencyName == "County Fire Department"
                && i.VerificationStatus == "Unverified");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task DispatchIncidentNotification_ThenAcknowledge_ThenClose_RoundTripsThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var incidentId = Guid.NewGuid();

            var dispatchResult = await sender.Send(new DispatchIncidentNotificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                incidentId, Guid.NewGuid(), "Phone", "Ammonia release -- HazMat requested.")
            {
                ActorId = ActorId,
                // RequestedAtUtc defaults to the real wall clock; pin it to the fixed test clock
                // so the notification's timestamp falls inside the +/-1 day status window queried
                // below regardless of what day this test actually runs.
                RequestedAtUtc = FixedUtcNow,
            }, CancellationToken.None);

            dispatchResult.IsSuccess.Should().BeTrue();
            dispatchResult.Value.Status.Should().Be("Dispatched");
            var notificationId = dispatchResult.Value.IncidentNotificationId;

            var statusAfterDispatch = await sender.Send(new GetIncidentNotificationStatusQuery(
                TenantId, null, incidentId, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)), CancellationToken.None);
            statusAfterDispatch.IsSuccess.Should().BeTrue();
            var dispatchedItem = statusAfterDispatch.Value.Items.Should().ContainSingle(i => i.IncidentNotificationId == notificationId).Subject;
            var eTagAfterDispatch = dispatchedItem.ETag;
            eTagAfterDispatch.Should().NotBeNullOrWhiteSpace();

            var ackResult = await sender.Send(new AcknowledgeIncidentNotificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, notificationId, "County Dispatch Desk 4")
            { ActorId = ActorId, ETag = eTagAfterDispatch }, CancellationToken.None);

            ackResult.IsSuccess.Should().BeTrue();
            ackResult.Value.Status.Should().Be("Acknowledged");

            var statusAfterAck = await sender.Send(new GetIncidentNotificationStatusQuery(
                TenantId, null, incidentId, FixedUtcNow.AddDays(-1), FixedUtcNow.AddDays(1)), CancellationToken.None);
            var ackedItem = statusAfterAck.Value.Items.Should().ContainSingle(i => i.IncidentNotificationId == notificationId).Subject;
            ackedItem.AcknowledgedByContactName.Should().Be("County Dispatch Desk 4");
            ackedItem.ETag.Should().NotBe(eTagAfterDispatch, "a successful acknowledgement must advance the ROWVERSION-backed ETag");

            var closeResult = await sender.Send(new CloseIncidentNotificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, notificationId)
            { ActorId = ActorId, ETag = ackedItem.ETag }, CancellationToken.None);

            closeResult.IsSuccess.Should().BeTrue();
            closeResult.Value.Status.Should().Be("Closed");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task AcknowledgeIncidentNotification_WithStaleEtag_ShouldThrowConcurrencyConflict()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var dispatchResult = await sender.Send(new DispatchIncidentNotificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                Guid.NewGuid(), Guid.NewGuid(), "Phone", "msg")
            { ActorId = ActorId }, CancellationToken.None);
            dispatchResult.IsSuccess.Should().BeTrue();

            var act = async () => await sender.Send(new AcknowledgeIncidentNotificationCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId,
                dispatchResult.Value.IncidentNotificationId, "Desk 4")
            { ActorId = ActorId, ETag = "0x0000000000000000" }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>(
                "a stale/incorrect ETag must be rejected by usp_IncidentNotification_Update's ROWVERSION match and surfaced as a concurrency failure");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task BroadcastPublicAlert_ThenListActiveBroadcasts_RoundTripsThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var broadcastResult = await sender.Send(new BroadcastPublicAlertCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                Guid.NewGuid(), "Warning", "Ipaws", "Shelter in place", "A chemical release has occurred.", "1-mile radius")
            { ActorId = ActorId }, CancellationToken.None);

            broadcastResult.IsSuccess.Should().BeTrue();
            broadcastResult.Value.Status.Should().Be("Sent");

            var listResult = await sender.Send(new ListActiveBroadcastsQuery(TenantId, null), CancellationToken.None);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Value.Items.Should().Contain(i => i.PublicAlertBroadcastId == broadcastResult.Value.PublicAlertBroadcastId
                && i.Headline == "Shelter in place");
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public async Task LogCommunicationEntry_ThenGetByIncident_RoundTripsThroughRealStoredProcedures()
    {
        var (scope, sender) = Factory.Services.GetScopedService<ISender>();
        try
        {
            var incidentId = Guid.NewGuid();

            var logResult = await sender.Send(new LogCommunicationEntryCommand(
                Guid.NewGuid(), CorrelationId, Guid.NewGuid(), TenantId, null,
                incidentId, null, null, "Outbound", "Phone", "County Fire Dispatch", "Notified dispatch of the release.", FixedUtcNow)
            { ActorId = ActorId }, CancellationToken.None);

            logResult.IsSuccess.Should().BeTrue();

            var byIncidentResult = await sender.Send(new GetCommunicationLogByIncidentQuery(TenantId, incidentId), CancellationToken.None);

            byIncidentResult.IsSuccess.Should().BeTrue();
            byIncidentResult.Value.Items.Should().ContainSingle(i => i.CommunicationLogEntryId == logResult.Value.CommunicationLogEntryId
                && i.CounterpartyName == "County Fire Dispatch");
        }
        finally
        {
            scope.Dispose();
        }
    }
}
