// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Smoke
//  File:           SmokeTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-SMKE-PSC] Smoke test type (TRD-MAN-2026-001 Ch.3): fast, shallow sanity
//                  checks that the module's core building blocks construct and execute their
//                  trivial happy path without throwing -- one smoke check per aggregate/handler
//                  family, scaled to this 4-aggregate module. Not a substitute for the deeper
//                  Functional suite. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Application.PublicSafetyCommunication.Validation;
using PlantSafety.Tests.PublicSafetyCommunication.TestMatrix.Fakes;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class SmokeTests : UnitTestBase
{
    [Fact]
    public void AllSixValidators_CanBeConstructedWithoutThrowing()
    {
        var act = () =>
        {
            _ = new RegisterPublicSafetyAgencyCommandValidator();
            _ = new DispatchIncidentNotificationCommandValidator();
            _ = new AcknowledgeIncidentNotificationCommandValidator();
            _ = new CloseIncidentNotificationCommandValidator();
            _ = new BroadcastPublicAlertCommandValidator();
            _ = new LogCommunicationEntryCommandValidator();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public async Task RegisterPublicSafetyAgencyCommandHandler_HappyPath_DoesNotThrow()
    {
        var handler = new RegisterPublicSafetyAgencyCommandHandler(
            new InMemoryPublicSafetyAgencyRepository(), NullLogger<RegisterPublicSafetyAgencyCommandHandler>());

        var act = () => handler.Handle(new RegisterPublicSafetyAgencyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Smoke Test Agency", "Fire", "Smoke district", "555-0000", null)
        { ActorId = TestUserId.Value }, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DispatchIncidentNotificationCommandHandler_HappyPath_DoesNotThrow()
    {
        var handler = new DispatchIncidentNotificationCommandHandler(
            new InMemoryIncidentNotificationRepository(), NullLogger<DispatchIncidentNotificationCommandHandler>());

        var act = () => handler.Handle(new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), Guid.NewGuid(), "Phone", "Smoke test message.")
        { ActorId = TestUserId.Value }, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BroadcastPublicAlertCommandHandler_HappyPath_DoesNotThrow()
    {
        var handler = new BroadcastPublicAlertCommandHandler(
            new InMemoryPublicAlertBroadcastRepository(), NullLogger<BroadcastPublicAlertCommandHandler>());

        var act = () => handler.Handle(new BroadcastPublicAlertCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            null, "Advisory", "SocialMedia", "Smoke headline", "Smoke message", "Smoke area")
        { ActorId = TestUserId.Value }, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task LogCommunicationEntryCommandHandler_HappyPath_DoesNotThrow()
    {
        var handler = new LogCommunicationEntryCommandHandler(
            new InMemoryCommunicationLogEntryRepository(), NullLogger<LogCommunicationEntryCommandHandler>());

        var act = () => handler.Handle(new LogCommunicationEntryCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            null, null, null, "Outbound", "Phone", "Smoke Counterparty", "Smoke summary", FixedUtcNow)
        { ActorId = TestUserId.Value }, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
