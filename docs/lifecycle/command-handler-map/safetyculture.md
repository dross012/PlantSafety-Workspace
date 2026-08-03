# Command-Handler Map — SafetyCulture

See `_shared-findings.md` for F7 (Outbox universal), F8 (neither `CultureAssessment` nor
`StopWorkEvent` is on the 23-aggregate SQL-audit list), F9 (event delivery, [EVT-B]), F10.
Command/handler names pattern-inferred; not individually opened this phase.

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| CultureAssessment | Create | `CreateCultureAssessmentCommand` | matching handler | `AddAsync` | `CultureAssessmentCreatedEvent` (pattern-inferred) | Y | **N** | Pattern-inferred | |
| CultureAssessment | RecordScore | `RecordCultureAssessmentScoreCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | Real 0-100 range-check on both score fields (inherited) — a positive control at the domain layer even without downstream audit/event coverage |
| CultureAssessment | PublishReport | `PublishCultureAssessmentReportCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | |
| CultureAssessment | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| StopWorkEvent | Create | `CreateStopWorkEventCommand` | matching handler | `AddAsync` | `StopWorkEventCreatedEvent` (pattern-inferred) | Y | **N** | Pattern-inferred | |
| StopWorkEvent | Investigate | `InvestigateStopWorkEventCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | |
| StopWorkEvent | Resolve | `ResolveStopWorkEventCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | A `StopWorkEvent` is exactly the kind of record `ChecklistCompletion.FlagFailedItem`'s doc comment (see `checklists.md`) implies should be notified automatically — no such wiring exists; the two aggregates have no cross-reference to each other in either direction |
| StopWorkEvent | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |

**Event delivery reality (F9 [EVT-B]):** no consumer class exists for any event in this module
(Created or non-Created) — confirmed no `CultureAssessment*Consumer`/`StopWorkEvent*Consumer` in
`publishing-map.md`/`consumption-map.md`. Net: zero live delivery for every transition in this module.
