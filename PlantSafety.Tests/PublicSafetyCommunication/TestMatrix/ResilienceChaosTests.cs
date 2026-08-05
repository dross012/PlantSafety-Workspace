// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Resilience / Chaos
//  File:           ResilienceChaosTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-RESL-PSC / PLNT-CHAO-PSC] Resilience and Chaos test types
//                  (TRD-MAN-2026-001 Ch.3): injects simulated repository/infrastructure faults
//                  (transient exceptions) into each command handler and verifies the handler
//                  degrades gracefully to a Result.Failure with a sanitized error message rather
//                  than propagating an unhandled exception or leaking infrastructure details, and
//                  that a genuine ConcurrencyConflictException is the one deliberate exception
//                  type allowed to propagate. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Tests.PublicSafetyCommunication.TestMatrix.Fakes;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class ResilienceChaosTests : UnitTestBase
{
    [Fact]
    public async Task RegisterPublicSafetyAgency_UnderSimulatedTransientDatabaseFault_DegradesGracefully()
    {
        var repo = new InMemoryPublicSafetyAgencyRepository { FaultOnAdd = new TimeoutException("Simulated transient SQL timeout.") };
        var handler = new RegisterPublicSafetyAgencyCommandHandler(repo, NullLogger<RegisterPublicSafetyAgencyCommandHandler>());

        var result = await handler.Handle(new RegisterPublicSafetyAgencyCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            "Agency", "Fire", "Jurisdiction", "555-0100", null)
        { ActorId = TestUserId.Value }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().NotContain("Simulated transient SQL timeout",
            "infrastructure fault details must never leak into the sanitized ApplicationError message");
    }

    [Fact]
    public async Task DispatchIncidentNotification_UnderSimulatedTransientFault_DegradesGracefully()
    {
        var repo = new InMemoryIncidentNotificationRepository { FaultOnAdd = new InvalidOperationException("Simulated connection pool exhaustion.") };
        var handler = new DispatchIncidentNotificationCommandHandler(repo, NullLogger<DispatchIncidentNotificationCommandHandler>());

        var result = await handler.Handle(new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), Guid.NewGuid(), "Phone", "msg")
        { ActorId = TestUserId.Value }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.IncidentNotification.DispatchFailed");
    }

    [Fact]
    public async Task BroadcastPublicAlert_UnderSimulatedTransientFault_DegradesGracefully()
    {
        var repo = new InMemoryPublicAlertBroadcastRepository { FaultOnAdd = new InvalidOperationException("Simulated outbox write failure.") };
        var handler = new BroadcastPublicAlertCommandHandler(repo, NullLogger<BroadcastPublicAlertCommandHandler>());

        var result = await handler.Handle(new BroadcastPublicAlertCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            null, "Warning", "Ipaws", "headline", "message", "area")
        { ActorId = TestUserId.Value }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.PublicAlertBroadcast.BroadcastFailed");
    }

    [Fact]
    public async Task LogCommunicationEntry_UnderSimulatedTransientFault_DegradesGracefully()
    {
        var repo = new InMemoryCommunicationLogEntryRepository { FaultOnAdd = new InvalidOperationException("Simulated write failure.") };
        var handler = new LogCommunicationEntryCommandHandler(repo, NullLogger<LogCommunicationEntryCommandHandler>());

        var result = await handler.Handle(new LogCommunicationEntryCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            null, null, null, "Outbound", "Phone", "Counterparty", "Summary", FixedUtcNow)
        { ActorId = TestUserId.Value }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.CommunicationLogEntry.LogFailed");
    }

    [Fact]
    public async Task AcknowledgeIncidentNotification_ChaosInjectedUpdateFault_PropagatesAsGenericFailure_NotUnhandled()
    {
        // Chaos: a non-concurrency infrastructure fault raised inside UpdateAsync (distinct from
        // the deliberate ConcurrencyConflictException path already covered elsewhere) must still
        // be caught by the handler's outer try/catch and mapped to a Result.Failure.
        var repo = new InMemoryIncidentNotificationRepository { FaultOnUpdate = new InvalidOperationException("Simulated deadlock victim.") };
        var handler = new AcknowledgeIncidentNotificationCommandHandler(repo, NullLogger<AcknowledgeIncidentNotificationCommandHandler>());

        var dispatchRepo = new InMemoryIncidentNotificationRepository();
        var dispatchHandler = new DispatchIncidentNotificationCommandHandler(dispatchRepo, NullLogger<DispatchIncidentNotificationCommandHandler>());
        var dispatched = await dispatchHandler.Handle(new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), Guid.NewGuid(), "Phone", "msg")
        { ActorId = TestUserId.Value }, CancellationToken.None);

        // Seed the faulty repo with the same aggregate so GetByIdAsync succeeds but UpdateAsync explodes.
        await repo.AddAsync(await dispatchRepo.GetByIdAsync(
            global::PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids.IncidentNotificationId.FromGuid(dispatched.Value.IncidentNotificationId))
            ?? throw new InvalidOperationException("Setup failure."), CancellationToken.None);

        var result = await handler.Handle(new AcknowledgeIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            dispatched.Value.IncidentNotificationId, "Desk 4")
        { ActorId = TestUserId.Value }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.IncidentNotification.AcknowledgeFailed");
    }
}
