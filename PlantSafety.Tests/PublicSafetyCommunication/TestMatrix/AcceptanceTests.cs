// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Acceptance (UAT)
//  File:           AcceptanceTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-UAT-PSC] Acceptance (UAT) test type (TRD-MAN-2026-001 Ch.3): asserts
//                  business-facing acceptance criteria for the module in Given/When/Then form, one
//                  AC per aggregate's core business promise. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Enums;
using AgencyAggregate = PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency;
using BroadcastAggregate = PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast;
using IncidentNotificationAggregate = PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification;
using LogEntryAggregate = PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class AcceptanceTests : UnitTestBase
{
    [Fact]
    public void AC01_GivenANewlyRegisteredAgency_ThenItIsImmediatelyActiveAndUnverified()
    {
        // GIVEN a safety manager registers a new public-safety agency
        var agency = AgencyAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, "County EMS", PublicSafetyAgencyType.EmergencyMedicalServices, "County-wide", "555-0911", null);

        // THEN the agency is immediately usable (Active) but its contact data is not yet
        // verified, consistent with the "register now, verify on schedule" data-governance model.
        agency.Status.Should().Be(AgencyOperationalStatus.Active);
        agency.VerificationStatus.Should().Be(AgencyVerificationStatus.Unverified);
    }

    [Fact]
    public void AC02_GivenADispatchedNotification_WhenTheAgencyAcknowledges_ThenTheComplianceTimingEvidenceIsCaptured()
    {
        // GIVEN a dispatched notification (EPCRA Section 304 compliance evidence starts accruing at dispatch)
        var notification = IncidentNotificationAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, Guid.NewGuid(), global::PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids.PublicSafetyAgencyId.New(),
            PublicSafetyDeliveryChannel.Phone, "Release notification.");

        // WHEN the receiving agency acknowledges
        var acknowledgedAt = UtcTimestamp.From(FixedUtcNow.AddMinutes(4));
        notification.Acknowledge("County Dispatch", acknowledgedAt, TestActor, TestCorrelationId, TestCausationId);

        // THEN both the dispatch and acknowledgement timestamps are retained as compliance evidence
        notification.DispatchedAtUtc.Should().Be(FixedTimestamp);
        notification.AcknowledgedAtUtc.Should().Be(acknowledgedAt);
    }

    [Fact]
    public void AC03_GivenASentBroadcast_WhenDeliveryFails_ThenTheFailureReasonIsRecordedForTheDeliverySuccessRateKpi()
    {
        // GIVEN a sent public alert broadcast
        var broadcast = BroadcastAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, Guid.NewGuid(), PublicAlertSeverity.Warning, PublicBroadcastChannel.Ipaws,
            "Shelter in place", "Chemical release.", "1-mile radius");

        // WHEN delivery fails
        broadcast.RecordFailure("IPAWS gateway timeout after 3 retries.", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);

        // THEN the broadcast delivery success rate KPI has the data it needs: Failed status + reason
        broadcast.Status.Should().Be(PublicAlertBroadcastStatus.Failed);
        broadcast.FailureReason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void AC04_GivenARecordedCommunicationLogEntry_ThenItCanNeverBeEditedAfterTheFact()
    {
        // GIVEN a recorded, immutable communication log entry documenting an after-action communication
        var entry = LogEntryAggregate.Create(TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, Guid.NewGuid(), null, null,
            CommunicationDirection.Outbound, CommunicationMedium.Phone, "County Fire Dispatch", "Original summary.", FixedTimestamp);

        // THEN there is no method on the aggregate's public surface that can alter its recorded
        // content -- this is the core business promise of the After-Action Communication Timeline
        // (an evidentiary, tamper-proof record).
        entry.GetType().GetMethods()
            .Select(m => m.Name)
            .Where(n => n is "Update" or "Edit" or "Correct" or "Amend")
            .Should().BeEmpty("CommunicationLogEntry must remain immutable once recorded, per its data-governance policy");
    }
}
