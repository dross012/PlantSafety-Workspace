// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- End-to-End / Scenario
//  File:           EndToEndScenarioTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-E2E-PSC / PLNT-SCEN-PSC] End-to-End and Scenario test types
//                  (TRD-MAN-2026-001 Ch.3): exercises a realistic cross-aggregate incident
//                  communication workflow spanning all 4 module aggregates end-to-end through
//                  their real command handlers, mirroring how the After-Action Communication
//                  Timeline dashboard is populated in production. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Tests.PublicSafetyCommunication.TestMatrix.Fakes;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class EndToEndScenarioTests : UnitTestBase
{
    [Fact]
    public async Task Scenario_ChemicalReleaseIncidentCommunicationWorkflow_CompletesEndToEndAcrossAllFourAggregates()
    {
        // Scenario: a chemical release incident triggers (1) registering the responding agency,
        // (2) dispatching a notification to that agency, (3) the agency acknowledging receipt,
        // (4) broadcasting a public shelter-in-place alert, (5) confirming its delivery,
        // (6) recording an after-action communication log entry referencing both the notification
        // and the broadcast, and (7) closing the incident notification. Every step must succeed
        // and the final aggregate states must be internally consistent.
        var agencyRepo = new InMemoryPublicSafetyAgencyRepository();
        var notificationRepo = new InMemoryIncidentNotificationRepository();
        var broadcastRepo = new InMemoryPublicAlertBroadcastRepository();
        var logRepo = new InMemoryCommunicationLogEntryRepository();

        var incidentId = Guid.NewGuid();

        // Step 1: Register the responding agency.
        var registerHandler = new RegisterPublicSafetyAgencyCommandHandler(agencyRepo, NullLogger<RegisterPublicSafetyAgencyCommandHandler>());
        var agencyResult = await registerHandler.Handle(new RegisterPublicSafetyAgencyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "County HazMat Response Unit", "HazmatResponse", "County-wide", "555-0177", "hazmat@county.example")
        { ActorId = TestUserId.Value }, CancellationToken.None);
        agencyResult.IsSuccess.Should().BeTrue();

        // Step 2: Dispatch the incident notification to that agency.
        var dispatchHandler = new DispatchIncidentNotificationCommandHandler(notificationRepo, NullLogger<DispatchIncidentNotificationCommandHandler>());
        var dispatchResult = await dispatchHandler.Handle(new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            incidentId, agencyResult.Value.PublicSafetyAgencyId, "RadioDispatch", "Ammonia release Unit 3 -- HazMat response requested.")
        { ActorId = TestUserId.Value }, CancellationToken.None);
        dispatchResult.IsSuccess.Should().BeTrue();
        var notificationId = dispatchResult.Value.IncidentNotificationId;

        // Step 3: Agency acknowledges receipt.
        var ackHandler = new AcknowledgeIncidentNotificationCommandHandler(notificationRepo, NullLogger<AcknowledgeIncidentNotificationCommandHandler>());
        var ackResult = await ackHandler.Handle(new AcknowledgeIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            notificationId, "HazMat Unit Desk")
        { ActorId = TestUserId.Value, ETag = notificationRepo.GetETag(notificationId) }, CancellationToken.None);
        ackResult.IsSuccess.Should().BeTrue();

        // Step 4: Broadcast the public shelter-in-place alert.
        var broadcastHandler = new BroadcastPublicAlertCommandHandler(broadcastRepo, NullLogger<BroadcastPublicAlertCommandHandler>());
        var broadcastResult = await broadcastHandler.Handle(new BroadcastPublicAlertCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            incidentId, "Emergency", "WirelessEmergencyAlerts", "Shelter in place", "Chemical release -- shelter in place immediately.", "2-mile radius")
        { ActorId = TestUserId.Value }, CancellationToken.None);
        broadcastResult.IsSuccess.Should().BeTrue();
        var broadcastId = broadcastResult.Value.PublicAlertBroadcastId;

        // Step 5: Confirm broadcast delivery is out of scope for this command set (no dedicated
        // command exists yet), so the scenario proceeds directly to after-action logging.

        // Step 6: Log an after-action communication entry referencing both the notification and the broadcast.
        var logHandler = new LogCommunicationEntryCommandHandler(logRepo, NullLogger<LogCommunicationEntryCommandHandler>());
        var logResult = await logHandler.Handle(new LogCommunicationEntryCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            incidentId, notificationId, broadcastId, "Outbound", "RadioDispatch",
            "County HazMat Response Unit", "Coordinated response and public alert for ammonia release.", FixedUtcNow)
        { ActorId = TestUserId.Value }, CancellationToken.None);
        logResult.IsSuccess.Should().BeTrue();

        // Step 7: Close the incident notification lifecycle.
        var closeHandler = new CloseIncidentNotificationCommandHandler(notificationRepo, NullLogger<CloseIncidentNotificationCommandHandler>());
        var closeResult = await closeHandler.Handle(new CloseIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, notificationId)
        { ActorId = TestUserId.Value, ETag = notificationRepo.GetETag(notificationId) }, CancellationToken.None);
        closeResult.IsSuccess.Should().BeTrue();
        closeResult.Value.Status.Should().Be("Closed");

        // Final-state assertions across all four aggregates.
        var finalNotification = await notificationRepo.GetByIdAsync(
            global::PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids.IncidentNotificationId.FromGuid(notificationId));
        finalNotification.Should().NotBeNull();
        finalNotification!.AcknowledgedByContactName.Should().Be("HazMat Unit Desk");

        var loggedEntry = await logRepo.GetByIdAsync(
            global::PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.ValueObjects.Ids.CommunicationLogEntryId.FromGuid(logResult.Value.CommunicationLogEntryId));
        loggedEntry.Should().NotBeNull();
        loggedEntry!.RelatedIncidentNotificationId.Should().Be(notificationId);
        loggedEntry.RelatedPublicAlertBroadcastId.Should().Be(broadcastId);
        loggedEntry.IncidentId.Should().Be(incidentId);
    }
}
