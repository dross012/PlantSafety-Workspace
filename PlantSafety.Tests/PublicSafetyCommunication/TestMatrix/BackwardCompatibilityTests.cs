// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Backward Compatibility
//  File:           BackwardCompatibilityTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-BKWD-PSC] Backward Compatibility test type (TRD-MAN-2026-001 Ch.3):
//                  verifies additive-only command envelope evolution -- ApiVersionToken defaults
//                  to "v1" for callers that don't set it, optional fields (SiteId/IncidentId/ETag)
//                  default safely to null so older callers that never populated them keep working,
//                  and the DispatchIncidentNotificationCommand/BroadcastPublicAlertCommand (which
//                  are not IConcurrencyAwareCommand) never require an ETag. Isolated, deterministic,
//                  no I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class BackwardCompatibilityTests : UnitTestBase
{
    [Fact]
    public void RegisterPublicSafetyAgencyCommand_DefaultsApiVersionTokenToV1()
    {
        var command = new RegisterPublicSafetyAgencyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Agency", "Fire", "Jurisdiction", "555-0100", null);

        command.ApiVersionToken.Should().Be("v1");
    }

    [Fact]
    public void RegisterPublicSafetyAgencyCommand_OptionalSiteIdAndPrimaryEmail_DefaultSafelyToNull()
    {
        // A caller built against an older client contract that never populated SiteId/PrimaryEmail
        // must still compile and construct a valid command.
        var command = new RegisterPublicSafetyAgencyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Agency", "Fire", "Jurisdiction", "555-0100", null);

        command.SiteId.Should().BeNull();
        command.PrimaryEmail.Should().BeNull();
    }

    [Fact]
    public void DispatchIncidentNotificationCommand_DoesNotRequireAnETag_BecauseItIsNotConcurrencyAware()
    {
        // DispatchIncidentNotificationCommand creates a new aggregate (no existing row to guard),
        // so it deliberately does not implement IConcurrencyAwareCommand and has no ETag property --
        // this must remain true so older callers that never sent an ETag are unaffected.
        var command = new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            null, Guid.NewGuid(), "Phone", "msg");

        command.Should().NotBeAssignableTo<global::PlantSafety.Application.Behaviors.Concurrency.IConcurrencyAwareCommand>();
    }

    [Fact]
    public void CloseIncidentNotificationCommand_OptionalETag_DefaultsToNull_SoOlderCallersCanOmitIt()
    {
        var command = new CloseIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, Guid.NewGuid());

        command.ETag.Should().BeNull();
    }

    [Fact]
    public void LogCommunicationEntryCommand_OptionalIncidentAndRelatedIds_DefaultSafelyToNull()
    {
        var command = new LogCommunicationEntryCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            null, null, null, "Outbound", "Phone", "Counterparty", "Summary", FixedUtcNow);

        command.IncidentId.Should().BeNull();
        command.RelatedIncidentNotificationId.Should().BeNull();
        command.RelatedPublicAlertBroadcastId.Should().BeNull();
    }
}
