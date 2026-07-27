# Step D — IncidentManagement Aggregate Validation Plan

Covers `PlantSafety.Domain/SafetyEvents/IncidentManagement/Aggregates/IncidentManagement.cs`
(five public members: `Create`, `Rehydrate`, `UpdateDetails`, `Archive`,
`ClearDomainEvents`) and its integration through
`CreateIncidentCommandHandler`/`UpdateIncidentCommandHandler` →
`IncidentDapperRepository` → outbox → `Shared.EventBus` → Reporting's
read model. This is a checklist, not an audit result — items are marked
`[known]` where this session already has direct evidence, and left
unmarked where verification is still needed.

## 1. Invariants

- [known] `Create` rejects empty/whitespace `Title`, `Description`,
  `Severity`, `Location`, and a default `ReportedByWorkerId`/`SiteId`/
  `TenantId` — confirmed present in the aggregate's guard clauses.
- [known] `Create` trims `Title`, `Description`, `Severity`, and
  `Location` before storing them (confirmed by direct inspection —
  `normalizedTitle = (title ?? string.Empty).Trim()` etc., applied to all
  four). The `Severity`-trimming case specifically is a confirmed,
  deliberate behavioral difference from the pre-migration handler (see
  the Step-6 diff-harness design doc,
  `incident-sql-shape-diff-harness-design.md`, which checked git history
  and found the old handler passed `Severity` through untrimmed) — the
  other three fields' old-handler behavior was not separately checked and
  should be, since the same silent-value-change risk applies to each.
  Verify this trim doesn't silently change any *existing* persisted row's
  semantics if `Rehydrate` is ever used to round-trip an
  already-untrimmed legacy value — `Rehydrate` should not re-trim and
  silently rewrite historical data as a side effect of an unrelated
  update. **Action**: confirm `Rehydrate` treats loaded string fields as
  opaque and only the explicit `UpdateDetails(...)` parameter path
  applies a fresh trim, not a blanket re-normalization of untouched
  fields.
- [known] `UpdateDetails` rejects mutation once `Status` is `Archived`
  (throws `IncidentManagementInvalidStatusTransitionException`) — this
  is new behavior added by the aggregate migration; the pre-migration
  handler had no equivalent guard (relied entirely on
  `usp_IncidentRead_GetById`'s `WHERE IsDeleted = 0` filter to make
  archived rows unreachable, which is a *read*-side exclusion, not a
  *write*-side invariant — meaning a direct write to an archived row was
  previously possible if a caller somehow had a stale row reference).
- [known] `Archive` requires a non-empty `DeletionReason`.
- **Not yet independently verified**: whether every invariant enforced by
  `usp_Incident_Create`/`usp_Incident_Update` at the SQL layer (column
  `NOT NULL` constraints, string length limits) has a *matching* aggregate
  guard clause, or whether some invariants exist only at the SQL layer
  and would surface as a raw `SqlException` rather than a domain
  exception if violated. **Action**: cross-reference every `NOT NULL`
  column in `Incident.sql` against the aggregate's constructor/`Create`
  guards; any gap should either get an aggregate-level guard (preferred,
  produces a clean domain exception) or be explicitly documented as
  "SQL-layer-only, by design" if the case is truly unreachable through
  the aggregate's public API.

## 2. State transitions

Confirmed transition set: `(new) → Create → Reported`,
`Reported → UpdateDetails → Reported` (no status change on update),
`Reported → Archive → Archived`, `Archived → (blocked)`.

- [known] There is **no** `Closed` status or transition anywhere in the
  code — confirmed absent from both the aggregate and the SQL schema
  during the Phase 2 aggregate redesign. `IncidentManagementClosedEvent`
  was deliberately not created for this reason. This remains correct as
  of this plan; re-verify only if a future change introduces a `Closed`
  concept.
- **Not yet verified**: whether any *other* status values exist in
  production data (or are referenced anywhere in UI/reporting code) that
  the aggregate doesn't model — e.g. an "InReview" or "Assigned" status
  implied by `AssignedInvestigatorId` being set. **Action**: grep for
  string literal status values compared against `Incident.Status`
  anywhere outside the aggregate (UI, Reporting, Analytics) to confirm
  the aggregate's `IncidentManagementStatus` enum is exhaustive relative
  to what the rest of the system expects.

## 3. Domain events

| Event | Emitted by | Verified real (not speculative) |
|---|---|---|
| `IncidentManagementCreatedEvent` | `Create` | [known] yes — confirmed by direct inspection of the aggregate source (`agg.AddDomainEvent(new IncidentManagementCreatedEvent(...))`), carrying `EventId`, `IncidentManagementId`, `CreatedAtUtc`, `Actor`, `CorrelationId`, `CausationId`. This resolves an open question from an earlier draft of this plan, which had not yet checked the source directly. |
| `IncidentManagementUpdatedEvent` | `UpdateDetails` | [known] yes — grounded in `usp_Incident_Update` |
| `IncidentManagementArchivedEvent` | `Archive` | [known] yes — grounded in `usp_Incident_SoftDelete`, follows `AccidentArchivedEvent`/`RiskArchivedEvent` convention |

- [known] Events are collected on the in-memory aggregate
  (`_domainEvents`) and only enqueued to the outbox **after** a
  successful SQL commit (`IncidentDapperRepository.EnqueuePendingEvents`,
  called post-`tx.Commit()`), then cleared via `ClearDomainEvents()` —
  this ordering is correct and matches the `AccidentDapperRepository`
  precedent; a failed transaction cannot leak a phantom event.
- **Not yet verified**: whether a *partial* failure is possible — e.g.
  the SQL commit succeeds but the process crashes before
  `EnqueuePendingEvents` runs, permanently losing that event with no
  retry path (since it never reached the outbox table at all, as opposed
  to reaching it and failing delivery, which the dead-letter/retry
  infrastructure in `Shared.EventBus` does cover). This is a real,
  narrow window inherent to the "write DB, then write outbox" pattern
  without a single atomic transaction spanning both — **Action**: confirm
  whether `outboxWriter.Enqueue(...)` writes to the *same* database/
  transaction scope as the row it's describing (in which case this risk
  doesn't exist) or a genuinely separate call (in which case it does, and
  should be documented as an accepted risk or addressed by moving the
  outbox write inside the same transaction as the row commit).

## 4. Repository behaviors

- [known] `CreateAsync` builds all SQL parameters from the aggregate's
  *validated* state (post-`Create()`), not from raw command input —
  correct, ensures the invariants that ran can't be bypassed by a
  parameter mismatch.
- [known] `UpdateAsync` still uses the original null-means-unchanged diff
  parameters (not full aggregate state) against the existing
  `ISNULL`/`COALESCE` partial-update stored procedure — deliberately
  preserved, not regressed to a full-column overwrite.
- [known] `UpdateAsync`/`SoftDeleteAsync` throw
  `ConcurrencyConflictException` on zero rows affected (stale/wrong
  ETag) — unchanged behavior from before the migration.
- [known] The `RowVersion`→`NVARCHAR` ETag-projection bug (Step B) is now
  fixed in all six `SafetyEvents.Incident` procedures — this was
  previously a **complete** blocker to any live read/update against a
  real SQL Server; it is fixed but its fix has only been proven at the
  schema-application level, not via a full green integration test run
  (blocked by the unrelated Step A/C gap as of this writing).

## 5. Cross-module interactions

- [known] `IncidentManagementUpdatedEvent`/`ArchivedEvent` flow through
  `CrossModuleEventRelay` → `Shared.EventBus` (SQL queue, retry/backoff,
  dead-letter) → Reporting's `EventBusSubscriberHostedService` → 12
  `IEventHandler<T>` implementations → `InMemorySafetyEventActivityFeedStore`.
- [known] Reporting's read-model mapping has snapshot/pinning test
  coverage (`ActivityFeedHandlerSnapshotTests.cs`, 14 tests, confirmed
  passing) including a tripwire for a future event type shipping with no
  registered handler.
- **Not yet verified end-to-end**: whether an actual `IncidentManagementUpdatedEvent`
  published by the real `IncidentDapperRepository` → outbox path, in a
  live system, is correctly picked up by the SQL-queue-backed
  `Shared.EventBus.Infrastructure` publisher/subscriber and reaches
  Reporting — the event bus's own test coverage (25/25, per the Phase 3
  report) exercises the bus in isolation; the full round trip from a real
  Incident write through to a Reporting read-model update has not been
  proven by an integration test that spans both repos. This would require
  either a true end-to-end environment (both APIs running, shared SQL
  Server, a real event-bus polling loop) or a scoped integration test
  that seeds an outbox row and asserts Reporting's store updates — neither
  currently exists per this plan's research.

## 6. Recommended additional tests

In priority order, based on what's genuinely unverified above rather than
padding for coverage's sake:

1. **Add a Create-event assertion** if one doesn't already exist in
   `PlantSafety.Tests` — a single unit test on `IncidentManagement.Create`
   asserting `IncidentManagementCreatedEvent` (now confirmed to exist,
   per section 3) appears in `DomainEvents` with the correct
   `IncidentManagementId`/`CorrelationId`/`CausationId` immediately after
   construction. Cheapest possible test; confirm it isn't already
   redundant with existing aggregate tests before adding.
2. **`Rehydrate` non-mutation test** — construct via `Rehydrate` with an
   untrimmed legacy `Severity` value, assert no domain event is raised and
   the in-memory `Severity` is *not* silently retrimmed on load (only on
   an explicit subsequent `UpdateDetails` call).
3. **SQL-layer invariant cross-reference** (a documentation/audit task,
   not a test) — the `NOT NULL` column vs. aggregate guard-clause mapping
   described in section 1.
4. **Outbox-write atomicity confirmation** (a documentation/audit task) —
   resolve whether `IOutboxWriter.Enqueue` shares a transaction with the
   row write, per section 3's open question.
5. **End-to-end event-bus round trip** (an integration test, only worth
   building once sections 1–4 are resolved and the Step C test-host
   restoration is complete) — seed a real Incident update through the
   full stack and assert Reporting's read model reflects it, closing the
   section 5 gap.

## 7. Explicitly not re-litigated here

Items already conclusively settled earlier in this session and not
reopened by this plan: the deletion of the dead pre-migration
`IncidentManagement` scaffold (factory/service/policy/snapshot classes),
the decision not to invent an `IncidentManagementClosedEvent`, and the
`CorrectiveAction` (CAPA) aggregate's parallel validation (out of scope —
this plan is Incident-specific per the request).
