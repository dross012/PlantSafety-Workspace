// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.TestMatrix
//  Layer:          Tests (Unit) -- Failover / Recovery
//  File:           FailoverRecoveryTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    [PLNT-FAIL-PSC / PLNT-RCVR-PSC] Failover and Recovery test types
//                  (TRD-MAN-2026-001 Ch.3): a write that loses its optimistic-concurrency race
//                  (stale ETag, simulating a failover to a competing writer) must fail loudly with
//                  ConcurrencyConflictException and leave the aggregate's persisted state
//                  untouched; a subsequent retry with the current ETag (Recovery) must then
//                  succeed and converge to the intended state. Isolated, deterministic, no I/O.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;
using PlantSafety.Tests.PublicSafetyCommunication.TestMatrix.Fakes;

namespace PlantSafety.Tests.PublicSafetyCommunication.TestMatrix;

public sealed class FailoverRecoveryTests : UnitTestBase
{
    [Fact]
    public async Task Acknowledge_WithStaleEtag_FailsOverWithoutMutatingPersistedState_ThenRecoversOnRetryWithFreshEtag()
    {
        var repo = new InMemoryIncidentNotificationRepository();
        var dispatchHandler = new DispatchIncidentNotificationCommandHandler(repo, NullLogger<DispatchIncidentNotificationCommandHandler>());
        var ackHandler = new AcknowledgeIncidentNotificationCommandHandler(repo, NullLogger<AcknowledgeIncidentNotificationCommandHandler>());

        var dispatched = await dispatchHandler.Handle(new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), Guid.NewGuid(), "Phone", "msg")
        { ActorId = TestUserId.Value }, CancellationToken.None);
        var notificationId = dispatched.Value.IncidentNotificationId;
        var freshEtag = repo.GetETag(notificationId);

        // Failover: a competing writer's stale ETag must be rejected, not silently applied.
        var act = () => ackHandler.Handle(new AcknowledgeIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            notificationId, "Stale writer")
        { ActorId = TestUserId.Value, ETag = "definitely-stale" }, CancellationToken.None);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(act);

        // The aggregate's persisted status must remain untouched after the failed failover attempt.
        var afterFailedAttempt = await repo.GetByIdAsync(
            global::PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids.IncidentNotificationId.FromGuid(notificationId));
        afterFailedAttempt!.Status.Should().Be(IncidentNotificationStatus.Dispatched);

        // Recovery: retrying with the current (fresh) ETag succeeds and converges to the intended state.
        var recovered = await ackHandler.Handle(new AcknowledgeIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value,
            notificationId, "Recovering writer")
        { ActorId = TestUserId.Value, ETag = freshEtag }, CancellationToken.None);

        recovered.IsSuccess.Should().BeTrue();
        recovered.Value.Status.Should().Be("Acknowledged");
    }

    [Fact]
    public async Task Close_AfterTransientRepositoryOutage_RecoversOnNextAttempt()
    {
        // Simulates a transient outage on the first attempt (e.g. a failed-over SQL replica not
        // yet caught up) followed by a successful retry once the outage clears.
        var repo = new InMemoryIncidentNotificationRepository();
        var dispatchHandler = new DispatchIncidentNotificationCommandHandler(repo, NullLogger<DispatchIncidentNotificationCommandHandler>());
        var closeHandler = new CloseIncidentNotificationCommandHandler(repo, NullLogger<CloseIncidentNotificationCommandHandler>());

        var dispatched = await dispatchHandler.Handle(new DispatchIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, null,
            Guid.NewGuid(), Guid.NewGuid(), "Phone", "msg")
        { ActorId = TestUserId.Value }, CancellationToken.None);
        var notificationId = dispatched.Value.IncidentNotificationId;

        repo.FaultOnUpdate = new TimeoutException("Simulated transient outage.");
        var firstAttempt = await closeHandler.Handle(new CloseIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, notificationId)
        { ActorId = TestUserId.Value, ETag = repo.GetETag(notificationId) }, CancellationToken.None);
        firstAttempt.IsFailure.Should().BeTrue();

        repo.FaultOnUpdate = null; // outage clears
        var secondAttempt = await closeHandler.Handle(new CloseIncidentNotificationCommand(
            Guid.NewGuid(), TestCorrelationId.Value, TestCausationId.Value, TestTenantId.Value, notificationId)
        { ActorId = TestUserId.Value, ETag = repo.GetETag(notificationId) }, CancellationToken.None);

        secondAttempt.IsSuccess.Should().BeTrue();
        secondAttempt.Value.Status.Should().Be("Closed");
    }
}
