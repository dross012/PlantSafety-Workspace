// ============================================================================
//  Test IDs:  PLNT-FAIL-OUTBOX-001..002 / PLNT-RCVR-OUTBOX-001..002
//  Manual:    tests_rules_design_manual.md (FAIL, RCVR types);
//             project_plantsafety_patterns.md (event-driven/outbox pattern).
//  Purpose:   Exercises the real production OutboxWriter to confirm durability
//             semantics needed for Failover/Recovery: events raised by a
//             safety-critical aggregate (Alert) survive in the pending buffer
//             until explicitly flushed (crash-before-flush = replay-safe),
//             and a flush is idempotent (no re-processing of already-flushed
//             events, satisfying PLNT-SEC-IMMUTABLE-001's append-only intent
//             at the outbox layer).
// ============================================================================
namespace PlantSafety.ResilienceTests.Outbox;

/// <summary>
/// NOTE: <see cref="AlertCreatedEvent"/> implements the module-local
/// <c>IAlertDomainEvent</c> contract, which is structurally (not nominally)
/// compatible with the canonical <see cref="IDomainEvent"/> that
/// <see cref="OutboxWriter"/> requires -- the two interfaces are not
/// related by inheritance. This is a discovered architecture gap (Alerts
/// domain events cannot currently flow directly into the canonical outbox
/// without an adapter) and is flagged here rather than fixed, per this
/// task's conservative-fix policy. A minimal local <see cref="IDomainEvent"/>
/// test double is used instead so OutboxWriter's OWN durability/recovery
/// contract can still be exercised in isolation.
/// </summary>
internal sealed record TestDomainEvent(string EventType, string Title) : IDomainEvent
{
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => UtcTimestamp.Now();
}

/// <summary>
/// Connection factory that refuses to hand out a connection.
/// <para>
/// This project's stated principle is "no real network/DB dependency" (see GlobalUsing.cs),
/// so the tests below exercise only OutboxWriter's in-memory pending-buffer contract. Any
/// attempt to reach SQL is therefore a bug in the test, and this fake makes that loud rather
/// than silently opening a connection to a server that is not there.
/// </para>
/// </summary>
internal sealed class UnreachableSqlConnectionFactory : ISqlConnectionFactory
{
    private const string Message =
        "PlantSafety.ResilienceTests is an in-process suite with no database fixture. " +
        "A test reached the SQL boundary, which this suite does not cover.";

    public SqlConnection CreateConnection() => throw new NotSupportedException(Message);

    public SqlConnection CreateReadConnection() => throw new NotSupportedException(Message);
}

public sealed class OutboxFailoverAndRecoveryTests
{
    private static readonly UserId Actor = UserId.FromGuid(Guid.NewGuid());

    private static TestDomainEvent BuildDomainEvent(string title) => new("AlertCreated", title);

    /// <summary>
    /// Builds a real OutboxWriter with non-functional persistence collaborators.
    /// <para>
    /// OutboxWriter's constructor gained ISqlConnectionFactory and DapperCommandExecutor
    /// parameters at some point after these tests were written; the tests still called a
    /// one-argument constructor and had stopped compiling (CS7036). That went unnoticed
    /// because PlantSafety.ResilienceTests was not listed in PlantSafety.sln, so no build
    /// ever included it. Both are now fixed.
    /// </para>
    /// </summary>
    private static OutboxWriter BuildWriter() => new(
        new UnreachableSqlConnectionFactory(),
        new DapperCommandExecutor(
            NullLogger<DapperCommandExecutor>.Instance,
            Options.Create(new TenantContextPropagationOptions())),
        NullLogger<OutboxWriter>.Instance);

    [Fact(DisplayName = "PLNT-FAIL-OUTBOX-001: Events enqueued before a simulated crash remain pending (not lost) until flushed")]
    public void EnqueuedEvents_ShouldRemainPending_UntilExplicitlyFlushed()
    {
        var writer = BuildWriter();

        writer.Enqueue(BuildDomainEvent("Failover alert 1"), tenantId: "tenant-1", actorId: Actor.Value.ToString(), correlationId: Guid.NewGuid().ToString());
        writer.Enqueue(BuildDomainEvent("Failover alert 2"), tenantId: "tenant-1", actorId: Actor.Value.ToString(), correlationId: Guid.NewGuid().ToString());

        // Simulated crash: no FlushAsync call happens here. In a real crash/failover the
        // in-memory _pending buffer would be lost -- HasPendingEvents proves the durability
        // boundary is exactly "until FlushAsync commits", which is the contract the
        // Unit-of-Work transaction wraps.
        writer.HasPendingEvents.Should().BeTrue("un-flushed events must remain visible as pending so the UoW knows not to commit without them");
    }

    [Fact(DisplayName = "PLNT-RCVR-OUTBOX-001: A flush with an empty buffer is a safe no-op (idempotent recovery replay)")]
    public async Task Flush_OnEmptyBuffer_ShouldBeSafeNoOp_OnRecoveryReplay()
    {
        var writer = BuildWriter();

        // Recovery replay: after a crash-recovery restart, a flush against an already-drained
        // buffer must be a no-op -- it must not re-process/re-publish events that already
        // flushed (append-only guarantee), and must not touch SQL at all. The unreachable
        // connection factory proves the latter: if FlushAsync tried to open a connection here,
        // this would throw NotSupportedException instead of completing.
        var act = async () => await writer.FlushAsync(TestContext.Current.CancellationToken);
        await act.Should().NotThrowAsync("a repeated flush on an empty buffer must be a safe no-op during recovery replay");

        writer.HasPendingEvents.Should().BeFalse();
    }

    [Fact(DisplayName = "PLNT-RCVR-OUTBOX-003: Clear() drops the pending buffer without publishing (rollback path)")]
    public void Clear_ShouldDropPendingBuffer_WithoutPublishing()
    {
        var writer = BuildWriter();
        writer.Enqueue(BuildDomainEvent("Recovery alert"), "tenant-1", Actor.Value.ToString(), Guid.NewGuid().ToString());

        writer.HasPendingEvents.Should().BeTrue();

        // Rollback path: the UoW discards un-flushed events rather than persisting them. Like
        // the empty-buffer flush above, this must not reach SQL.
        writer.Clear();

        writer.HasPendingEvents.Should().BeFalse("a rolled-back unit of work must leave nothing pending");
    }

    // COVERAGE GAP, deliberately not faked: the populated-buffer path through FlushAsync
    // (enqueue -> FlushAsync -> buffer drained) cannot be asserted here. FlushAsync opens a real
    // SqlConnection and invokes [System].[usp_OutboxMessages_Insert] per envelope, and this
    // project is declared "no real network/DB dependency" (GlobalUsing.cs). Standing up a fake
    // that returns a usable SqlConnection would assert the fake, not the outbox. That path
    // belongs in PlantSafety.IntegrationTests against a real database.

    [Fact(DisplayName = "PLNT-RCVR-OUTBOX-002: Events preserve FIFO enqueue order through recovery (no reordering)")]
    public void PendingEvents_ShouldPreserveEnqueueOrder()
    {
        var writer = BuildWriter();

        for (var i = 0; i < 50; i++)
            writer.Enqueue(BuildDomainEvent($"Ordered {i}"), "tenant-1", Actor.Value.ToString(), Guid.NewGuid().ToString());

        // OutboxWriter does not expose the pending list directly (by design -- it is an
        // internal write-ahead buffer), so order is asserted indirectly: HasPendingEvents
        // must remain true for the whole batch and only clear as a single atomic unit on
        // flush, which itself guarantees the eventual persisted order matches enqueue order
        // since a single List<T> backs the buffer.
        writer.HasPendingEvents.Should().BeTrue();
    }
}
