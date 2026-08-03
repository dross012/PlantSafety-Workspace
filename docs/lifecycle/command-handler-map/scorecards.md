# Command-Handler Map — Scorecards

See `_shared-findings.md` for F7 (Outbox universal), F8 (`Scorecard` **is** on the 23-aggregate
SQL-audit list), F9 (event delivery, [EVT-A] for the Analytics duplicate marker's collision, [EVT-B]
for the live aggregate's own events), F10. See `analytics.md` for the `Analytics.ScoreCards`
duplicate marker. Command/handler names pattern-inferred; not individually opened this phase.

| Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Create (Draft) | `CreateScorecardCommand` | matching handler | `AddAsync` | `ScorecardCreatedEvent` (pattern-inferred) | Y | **Y** | Pattern-inferred | |
| RecordMetric | `RecordScorecardMetricCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| CompleteCompilation | `CompleteScorecardCompilationCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | Real rule: rejects compiling with zero recorded measurements (inherited) |
| Publish | `PublishScorecardCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| RecordDistribution | `RecordScorecardDistributionCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | Exception-message copy-paste bug (inherited) — cosmetic only, does not affect guard/audit/outbox behavior |
| Close | `CloseScorecardCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **Y** | Pattern-inferred | |
| Archive | `ArchiveScorecardCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | **N — confirmed no `AddDomainEvent` call** (4th confirmed silent-archive instance, inherited F3) | **N** | **Y** (SQL row still fires per F8/`usp_Scorecard_Update`) | Directly verified (event absence, inherited) | Same audit/event divergence pattern as `Risk.Archive` (see `riskmanagement.md`): captured in `AuditLog`, invisible to every event-based consumer |
| Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9):** `ScoreCardsCreatedConsumer` (`publishing-map.md` line 66/
`consumption-map.md` line 29) is bound to the **`Analytics.ScoreCards` duplicate marker's** event,
not this live `Scorecard` aggregate's own `ScorecardCreatedEvent` — reinforcing that the live
aggregate's events (Created and all 6 non-Create types) have **zero consumer class of any kind**.
Net: zero live delivery for every transition in this module.
