# Event Map — SafetyCulture (CultureAssessment, StopWorkEvent)

See `_shared-findings.md` (F14-F17) and `command-handler-map/safetyculture.md`. **Fully directly
verified this phase**: read of all 3 `CultureAssessment` events and all 3 `StopWorkEvent` events.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| CultureAssessment | Create | Pattern-inferred Y | `CultureAssessmentCreatedEvent` | Y — identity/actor/correlation only | **N** | Pattern-inferred (event body directly read) | |
| CultureAssessment | RecordScore | Pattern-inferred Y | `CultureAssessmentScoreRecordedEvent` | Y — `OverallCultureScore (decimal), LeadershipEngagementScore (decimal?), RecordedAtUtc` — carries both the headline score and a secondary dimension; real 0-100 range-check on both fields at the domain layer (inherited) | **N** | Pattern-inferred (event body directly read) | |
| CultureAssessment | PublishReport | Pattern-inferred Y | `CultureAssessmentReportPublishedEvent` | Y — `OverallCultureScore (decimal?), PublishedAtUtc` — restates the score at publish time, a small but genuine mitigation of F15 for anyone comparing Record vs. Publish snapshots | **N** | Pattern-inferred (event body directly read) | |
| CultureAssessment | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| StopWorkEvent | Create | Pattern-inferred Y | `StopWorkEventCreatedEvent` | Y — identity/actor/correlation only | **N** | Pattern-inferred (event body directly read) | |
| StopWorkEvent | Investigate | Pattern-inferred Y | `StopWorkEventInvestigatedEvent` | Y — `Findings (string), InvestigatedAtUtc` | **N** | Pattern-inferred (event body directly read) | |
| StopWorkEvent | Resolve | Pattern-inferred Y | `StopWorkEventResolvedEvent` | Y — `ResolutionSummary (string), WasCelebrated (bool), ResolvedAtUtc` — `WasCelebrated` is a genuinely distinctive, safety-culture-specific field (whether the stop-work act was publicly recognized) not seen in any other module's events this phase | **N** | Pattern-inferred (event body directly read) | This is exactly the kind of record `ChecklistCompletion.FlagFailedItem`'s doc comment (see `checklists.md`) implies should trigger a notification — no cross-reference exists between the two aggregates in either direction, and per F9 neither has a working consumer regardless |
| StopWorkEvent | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |

**Duplicate-publish check (F17):** event-class inventory shows exactly 1 distinct event type per
named transition for both aggregates (3 each) — no naming evidence of a double-publish; aggregate
method bodies not individually re-opened this phase.

**Module-level summary:** all 6 non-Create-missing transitions publish with complete, genuinely
distinctive payloads (`WasCelebrated` stands out as the most safety-culture-specific field found in
this phase's entire review) — F15's "no old value" caveat applies least severely here since neither
aggregate has a clear "changed from X to Y" concept to miss. Zero live delivery for any transition
(F9) — no `CultureAssessment*Consumer`/`StopWorkEvent*Consumer` exists anywhere.
