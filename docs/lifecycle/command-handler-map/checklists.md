# Command-Handler Map — Checklists

See `_shared-findings.md` for F7 (Outbox universal), F8 (neither `ChecklistTemplate` nor
`ChecklistCompletion` is on the 23-aggregate SQL-audit list — confirmed absent), F9 (event delivery,
[EVT-B]), F10, **F12 (re-verification: `ChecklistCompletion` never checks `ChecklistTemplate`'s
status — CONFIRMED gap, resolved this phase)**.
**`ChecklistCompletion` fully directly verified this phase** (full read of `ChecklistCompletion.cs`
+ grep of every command handler in `PlantSafety.Application.Checklists` for a template-repository
dependency). `ChecklistTemplate` and the Operational duplicate not independently re-opened.

| Aggregate | Transition | Command | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| ChecklistTemplate | Create | `CreateChecklistTemplateCommand` | matching handler (file layout confirmed: dedicated `Commands/Create...` pair per transition, F10) | `AddAsync` | `ChecklistTemplateCreatedEvent` (pattern-inferred) | Y | **N** | Pattern-inferred | |
| ChecklistTemplate | AddItem | `AddChecklistTemplateItemCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | |
| ChecklistTemplate | Publish | `PublishChecklistTemplateCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | Rejects publishing an empty template (inherited real rule) |
| ChecklistTemplate | Retire | `RetireChecklistTemplateCommand` | matching handler | `GetByIdAsync` → `UpdateAsync` | pattern-inferred name | Y | **N** | Pattern-inferred | **No transition anywhere prevents a `ChecklistCompletion` from later being Started/Completed against this now-Retired template — see F12.** |
| ChecklistTemplate | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| ChecklistCompletion | Create | `CreateChecklistCompletionCommand` | `CreateChecklistCompletionCommandHandler` | `AddAsync` | `ChecklistCompletionCreatedEvent` | Y | **N** | Directly verified (F12 read) | Stores `ChecklistTemplateId` as a bare Guid, no validation it refers to a Published template |
| ChecklistCompletion | Start | `StartChecklistCompletionCommand` | `StartChecklistCompletionCommandHandler` | `GetByIdAsync` → `UpdateAsync` (only `IChecklistCompletionRepository`-shaped dependency confirmed — no template repository injected) | `ChecklistCompletionStartedEvent` | Y | **N** | **Directly verified** | **F12: confirmed no cross-check against `ChecklistTemplate.Status` at all** — can Start against a Retired/never-Published template |
| ChecklistCompletion | RecordItemResult | `RecordChecklistItemResultCommand` (pattern-inferred; not opened) | matching handler | `GetByIdAsync` → `UpdateAsync` | `ChecklistItemResultRecordedEvent` | Y | **N** | Directly verified (domain method read) | |
| ChecklistCompletion | FlagFailedItem | `FlagChecklistFailedItemCommand` (pattern-inferred) | matching handler | `GetByIdAsync` → `UpdateAsync` | `ChecklistItemResultRecordedEvent` **and** `ChecklistItemCriticalFailureFlaggedEvent` (2 events from 1 call — confirmed, `FlagFailedItem` internally calls `RecordItemResult` then raises a 2nd event) | Y (2 outbox rows) | **N** | **Directly verified** | Intended to let Auditing/SafetyCulture/Alerts react immediately per its own doc comment — but per F9, none of those modules has a live consumer for this event, so the "immediate reaction" the doc comment describes does not happen anywhere today |
| ChecklistCompletion | Complete | `CompleteChecklistCompletionCommand` | `CompleteChecklistCompletionCommandHandler` | `GetByIdAsync` → `UpdateAsync` (confirmed, only `_repository` field, no template repository) | `ChecklistCompletionCompletedEvent` | Y | **N** | **Directly verified** | **F12: same gap as Start** — Complete computes Pass/Fail/PartialWithFindings purely from its own item results, with zero reference back to template state |
| ChecklistCompletion | Archive / Cancel / Delete | Missing entirely | N/A | N/A | N/A | N/A | N/A | Directly verified (inherited) | |
| Checklists (Operational duplicate) | Create only | Presumed exists | Unconfirmed | Would call `AddAsync`, but **no write proc exists** (`usp_Checklists_List` read-only, inherited) | Unconfirmed | N/A (fails/no-ops before publish) | **N** | Directly verified (SQL absence, inherited) | Own controller confirmed dead-duplicate shape (inherited) |

**Event delivery reality (F9 [EVT-B]):** `ChecklistsCreatedConsumer` exists in `publishing-map.md`
line 76/`consumption-map.md` line 39 but is bound to `ChecklistsCreatedEvent` — the **Operational
duplicate marker's** event, not `ChecklistTemplateCreatedEvent`/`ChecklistCompletionCreatedEvent`
from the live aggregates. The live aggregates' events (Created and all 5 non-Create types) have
**zero consumer class of any kind.**
