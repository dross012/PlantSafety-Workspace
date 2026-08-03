# Command-Handler Map — RiskManagement

See `_shared-findings.md` for F7 (Outbox universal), F8 (`Risk` **is** on the 23-aggregate SQL-audit
list — `usp_Risk_Insert`/`usp_Risk_Update`), F9 (event delivery, [EVT-B]), F10 (naming convention).
Command/handler names pattern-inferred (F10); not individually opened this phase.

| Aggregate | Transition | Command (pattern-inferred) | Handler (pattern-inferred) | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| Risk | Create (Identify hazard) | `CreateRiskCommand` | `CreateRiskCommandHandler` | `AddAsync` | `RiskCreatedEvent` | Y | **Y** (SQL, F8) | Pattern-inferred (command name confirmed by inherited Phase 1 table; handler body not opened) | |
| Risk | ScoreRisk | `ScoreRiskCommand` | `ScoreRiskCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `RiskScoredEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| Risk | Prioritize | `PrioritizeRiskCommand` | `PrioritizeRiskCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `RiskPrioritizedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| Risk | ApplyControlMeasure | `ApplyRiskControlMeasureCommand` | `ApplyRiskControlMeasureCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `RiskControlMeasureAppliedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | Real cross-field rule confirmed (inherited): justification required for lower-effectiveness control hierarchy |
| Risk | ValidateEffectiveness | `ValidateRiskEffectivenessCommand` | `ValidateRiskEffectivenessCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `RiskEffectivenessValidatedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| Risk | StartReviewCycle / CompleteReview | `StartRiskReviewCycleCommand` / `CompleteRiskReviewCommand` | matching handlers | `GetByIdAsync` → `UpdateAsync` | `RiskReviewCycleStartedEvent` / `RiskReviewCompletedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| Risk | Close | `CloseRiskCommand` | `CloseRiskCommandHandler` | `GetByIdAsync` → `UpdateAsync` | `RiskClosedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| Risk | Archive | `ArchiveRiskCommand` (existence not independently re-confirmed) | matching handler | `GetByIdAsync` → `UpdateAsync` | **N — confirmed no `AddDomainEvent` call** (F3, directly verified in prior discovery) | **N** (silent transition, F7) | **Y** (SQL row still written — F8 is per-aggregate, not per-event; the generic `_Update` proc fires and logs regardless of whether the domain method raised an event) | Directly verified (event absence, inherited); command/handler existence pattern-inferred | **Notable split result**: this transition IS captured in `AuditLog` (best-effort, anonymous, per F8) even though it raises zero domain event and therefore zero Outbox row — an example of the audit and event-delivery mechanisms diverging in coverage for the same transition. |
| Risk | Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| Operational.RiskManagement (duplicate marker) | Create | Presumed exists (factory-only aggregate) | Unconfirmed | Would call `AddAsync`, but **no `_Insert`/`_Create`/`_Update` proc exists under this name — only `usp_RiskManagement_List` (read)** | Unconfirmed | If ever called, would fail/no-op at the SQL layer (Phase 1 finding, inherited) | **N** (not on F8 list; would fail before an audit row could be written) | Directly verified (SQL absence, inherited) | Own controller confirmed GET-only (inherited, full controller read) — this whole row is moot in production: no route creates one |
| Operational.RiskManagement (duplicate marker) | Update/Delete/Cancel/status | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9 [EVT-B]):** `RiskCreatedEvent` is technically among the 22 "Created" event
types the ingestion discovery's publishing-map cites with a named, if unregistered, consumer stub
(`RiskManagementCreatedConsumer` — but note this consumer is bound to `RiskManagementCreatedEvent`,
the **Operational duplicate marker's** event, not `RiskCreatedEvent` from the live `Risk` aggregate —
cross-referencing `publishing-map.md` line 78 shows the consumer naming tracks the Operational/marker
event, reinforcing that the live `Risk` aggregate's own `RiskCreatedEvent` has **no consumer of any
kind**, same as its 7 non-Create siblings). Net: zero live delivery for any `Risk` transition.
