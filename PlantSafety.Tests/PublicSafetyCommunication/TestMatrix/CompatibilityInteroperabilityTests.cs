// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Compatibility / Interoperability
//  File:           CompatibilityInteroperabilityTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-COMPAT-PSC / PLNT-INTEROP-PSC] Compatibility and Interoperability test
//                  types (TRD-MAN-2026-001 Ch.3). Compatibility: the module's smart-enum "Code"
//                  values (the wire/persistence contract) round-trip stably via From/TryParse, so
//                  existing persisted rows and external API callers keep working across releases.
//                  Interoperability: CommunicationLogEntry correctly cross-references
//                  IncidentNotification and PublicAlertBroadcast identities from sibling
//                  aggregates, and enum string values match across the Domain/Application
//                  boundary. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Enums;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class CompatibilityInteroperabilityTests : UnitTestBase
{
    [Theory]
    [InlineData("Phone")]
    [InlineData("Fax")]
    [InlineData("Email")]
    [InlineData("Sms")]
    [InlineData("RadioDispatch")]
    [InlineData("CadIntegration")]
    [InlineData("InPersonLiaison")]
    public void PublicSafetyDeliveryChannel_AllPersistedCodes_RoundTripThroughFrom(string code)
    {
        var channel = PublicSafetyDeliveryChannel.From(code);
        channel.Code.Should().Be(code);
    }

    [Theory]
    [InlineData("Advisory")]
    [InlineData("Watch")]
    [InlineData("Warning")]
    [InlineData("Emergency")]
    public void PublicAlertSeverity_AllPersistedCodes_RoundTripThroughFrom(string code)
    {
        var severity = PublicAlertSeverity.From(code);
        severity.Code.Should().Be(code);
    }

    [Theory]
    [InlineData("EmergencyAlertSystem")]
    [InlineData("WirelessEmergencyAlerts")]
    [InlineData("Ipaws")]
    [InlineData("SirenNetwork")]
    [InlineData("SocialMedia")]
    public void PublicBroadcastChannel_AllPersistedCodes_RoundTripThroughFrom(string code)
    {
        var channel = PublicBroadcastChannel.From(code);
        channel.Code.Should().Be(code);
    }

    [Fact]
    public void PublicSafetyDeliveryChannel_UnknownPersistedCode_ThrowsRatherThanSilentlyDefaulting()
    {
        // Compatibility guard: a code that no longer exists (e.g. a retired legacy value read
        // back from an old row) must fail loudly via TryParse, not silently coerce to Default --
        // silent coercion would corrupt the delivery-channel KPI without anyone noticing.
        var act = () => PublicSafetyDeliveryChannel.From("LegacyTelex");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CommunicationLogEntry_InteroperatesWithIncidentNotificationAndPublicAlertBroadcast_ViaGuidCorrelation()
    {
        // Interoperability: CommunicationLogEntry does not hold typed references to the sibling
        // aggregates (by design -- cross-aggregate boundaries stay at the Guid level), but its
        // Related*Id fields must correctly carry the sibling aggregates' real identity values
        // end-to-end so the After-Action Communication Timeline dashboard can join across them.
        var notificationId = global::PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, Guid.NewGuid(),
            global::PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids.PublicSafetyAgencyId.New(),
            PublicSafetyDeliveryChannel.Phone, "msg").IncidentNotificationId.Value;

        var broadcastId = global::PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, Guid.NewGuid(), PublicAlertSeverity.Warning, PublicBroadcastChannel.Ipaws,
            "headline", "message", "area").PublicAlertBroadcastId.Value;

        var entry = global::PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates.CommunicationLogEntry.Create(
            TestUserId, FixedTimestamp, TestCorrelationId, TestCausationId, TestActor,
            TestTenantId.Value, null, Guid.NewGuid(), notificationId, broadcastId,
            global::PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Enums.CommunicationDirection.Outbound,
            global::PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Enums.CommunicationMedium.Phone,
            "County Fire Dispatch", "Cross-referenced entry.", FixedTimestamp);

        entry.RelatedIncidentNotificationId.Should().Be(notificationId);
        entry.RelatedPublicAlertBroadcastId.Should().Be(broadcastId);
    }
}
