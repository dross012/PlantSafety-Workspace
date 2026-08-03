# Event Map — Checklists (ChecklistTemplate, ChecklistCompletion)

See `_shared-findings.md` (F14-F17, esp. F17's one confirmed duplicate-publish) and
`command-handler-map/checklists.md` (esp. F12 — `ChecklistCompletion` never checks
`ChecklistTemplate`'s status). **Fully directly verified this phase**: read of all 4
`ChecklistTemplate` events and all 5 `ChecklistCompletion` events.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| ChecklistTemplate | Create | Y | `ChecklistTemplateCreatedEvent` | Y — identity/actor/correlation only | **N** | Directly verified | |
| ChecklistTemplate | AddItem | Y | `ChecklistTemplateItemAddedEvent` | Y — `ItemId (Guid, untyped), Sequence (int), IsCritical (bool), AddedAtUtc` — `IsCritical` forwarded here is a nice touch, letting a consumer distinguish critical vs. routine items without a callback | **N** | Directly verified | |
| ChecklistTemplate | Publish | Y | `ChecklistTemplatePublishedEvent` | Y — `ItemCount (int), PublishedAtUtc` — rejects publishing an empty template (inherited real rule); the count corroborates that rule downstream | **N** | Directly verified | |
| ChecklistTemplate | Retire | Y | `ChecklistTemplateRetiredEvent` | Y — `Reason (string), RetiredAtUtc` — **per F12 (inherited), nothing in `ChecklistCompletion` ever consults this event or the template's resulting status**, so even though the event itself is complete, the one place in the solution that most needs it (preventing a completion from starting against a Retired template) doesn't listen for it | **N** | Directly verified | The clearest case in the solution where "payload complete, delivery zero" has a concrete, already-diagnosed consequence (F12) rather than being purely hypothetical |
| ChecklistTemplate | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| ChecklistCompletion | Create | Y | `ChecklistCompletionCreatedEvent` | Y — identity/actor/correlation only (**F12 caveat**: stores `ChecklistTemplateId` internally as a bare Guid but this field is not surfaced in the Create event's own payload either) | **N** | Directly verified | |
| ChecklistCompletion | Start | Y | `ChecklistCompletionStartedEvent` | Y — identity/timestamp/actor only | **N** | Directly verified | F12: confirmed no cross-check against `ChecklistTemplate.Status` — can Start against a Retired/never-Published template, and the resulting event gives no indication either way |
| ChecklistCompletion | RecordItemResult | Y | `ChecklistItemResultRecordedEvent` | Y — `ChecklistTemplateItemId (Guid, untyped), Passed (bool), RecordedAtUtc` | **N** | Directly verified | |
| ChecklistCompletion | FlagFailedItem | **Y — TWICE per call, confirmed F17** | `ChecklistItemResultRecordedEvent` (via internal call to `RecordItemResult`) **and** `ChecklistItemCriticalFailureFlaggedEvent` | Both complete for their own scope: 2nd event adds `ChecklistTemplateItemId (Guid), Description (string), FlaggedAtUtc` | **N (both)** | Directly verified | **The one confirmed duplicate-publish (2 Outbox rows from 1 command) in the entire solution (F17).** Deliberate per the method's own doc comment (intends Auditing/SafetyCulture/Alerts to react "immediately") — but per F9 nothing consumes either event, so the doubled write is pure overhead today, not a functioning fan-out |
| ChecklistCompletion | Complete | Y | `ChecklistCompletionCompletedEvent` | Y — **best payload in this module**: `Outcome (string), ItemsTotal (int), ItemsPassed (int), ItemsFailed (int), CompletedAtUtc` — a consumer gets the full pass/fail breakdown without a callback query | **N** | Directly verified | F12: computed purely from its own item results, zero reference back to template state |
| ChecklistCompletion | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | Directly verified (inherited) | |
| Checklists (Operational duplicate) | Create only | Unconfirmed | `ChecklistsCreatedEvent` | Unconfirmed | **N** (own consumer is a stub anyway) | Directly verified (inherited) | |

**Duplicate-publish check (F17):** confirmed — `FlagFailedItem` is the solution's one instance of a
single command producing 2 Outbox rows. No other duplicate found in either aggregate's event-class
inventory (4 for ChecklistTemplate matching 4 transitions, 5 distinct classes for
ChecklistCompletion's 5 transitions plus the FlagFailedItem double-count).

**Module-level summary:** payloads are uniformly complete for their own scope (F14); `Complete`'s
pass/fail breakdown is one of the richest payloads found this phase. The module's real problems are
structural, not payload-shaped: F12's template-status gap means `Retire`'s otherwise-complete event
is exactly the signal nothing downstream (including `ChecklistCompletion` itself) ever listens for,
and `FlagFailedItem`'s 2-events-per-call design is inert overhead given F9's universal
zero-delivery finding.
