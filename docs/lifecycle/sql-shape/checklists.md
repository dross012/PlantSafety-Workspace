# SQL Shape Audit — Checklists

Phase 6 of 10. Covers `Checklists.ChecklistTemplate`/`ChecklistCompletion` (live). `Operational.
Checklists.Checklists` (create-only marker, read-only SQL) is out of scope here (no write path exists to
shape-audit).

## Worst issues in this module

1. **`CreateChecklistTemplateCommand.TemplateTitle` — confirmed truncation risk, same 256-vs-200 pattern
   as Auditing.** `CreateChecklistTemplateCommandValidator.cs:21`: `MaximumLength(256)`.
   `ChecklistTemplateDapperRepository.cs` calls `.AddString("TemplateTitle", aggregate.TemplateTitle,
   200)` at **both** `AddAsync` (line 118, Create) **and** `UpdateAsync` (line 159, every subsequent
   write: AddItem/Publish/Retire) — meaning a title in the 201–256 range is truncated on Create and then
   **re-truncated identically on every later update** since the in-memory aggregate's `TemplateTitle`
   never changes after Create. Directly verified (both call sites read in full).
2. **`ChecklistCompletion` never checks the referenced `ChecklistTemplate`'s status** (inherited, F12,
   directly re-confirmed) — a `ChecklistCompletion` can be started/completed against a Retired or
   never-Published template with nothing in the domain or application layer detecting it. This is a
   cross-aggregate integrity gap, not a column-shape gap, but it means `ChecklistTemplateId` is
   effectively an unenforced foreign key at every layer, not just SQL.
3. **`ChecklistCompletion.FlagFailedItem` double-publishes** (inherited, F17) — internally calls
   `RecordItemResult` then raises its own `ChecklistItemCriticalFailureFlaggedEvent`, 2 Outbox rows per
   command. Not a shape bug, but relevant to anyone consuming this transition's persisted event history.
4. **No CHECK constraint found on either aggregate's `Status` column** — same structural enum-drift
   exposure as the rest of the solution outside Alert/Dashboard/Widget.
5. **`AddChecklistTemplateItemCommand.Description` (512) vs `RegulatoryReference` (256, optional,
   `MaximumLength(256)` with no `NotEmpty`)** — both validated, both plausibly aligned; not
   individually cross-checked against the child-table column sizes this phase (flagged, not confirmed,
   as a follow-up).

## Method

Direct read this phase: `CreateChecklistTemplateCommandValidator.cs`, `ChecklistTemplateDapperRepository.cs`
(`AddAsync`/`UpdateAsync` in full). Cross-referenced against the solution-wide `AddString`/`MaximumLength`
sweep used in `auditing.md`. `F12` (template-status never checked by Completion) re-cited from
`command-handler-map/_shared-findings.md`, not re-derived.

| Transition | Field-Set Match? | Nullability Match? | Enum Drift? | Truncation Risk? | Confidence | Notes |
|---|---|---|---|---|---|---|
| ChecklistTemplate.Create | Yes | Not individually re-verified | Structurally possible, no CHECK found | **Yes — confirmed** (`TemplateTitle` 256 validator vs 200 param) | Directly verified | Worst-issue #1 |
| ChecklistTemplate.AddItem | Yes (`Description` 512, `RegulatoryReference` 256) | Not verified | Same | Not individually re-verified against child-table column sizes | Directly verified (validator), pattern-inferred (column) | Worst-issue #5 |
| ChecklistTemplate.Publish | Yes (`Status`, `TemplateTitle` re-sent on every Update per issue #1) | Not verified | Same | **Yes — confirmed** (same `TemplateTitle` re-truncation on every Update call) | Directly verified | Real business rule: rejects publishing an empty template (inherited) |
| ChecklistTemplate.Retire | Same as Publish | Not verified | Same | **Yes — confirmed**, same mechanism | Directly verified | |
| ChecklistTemplate.Archive/Cancel/Delete | N/A — transitions do not exist | N/A | N/A | N/A | Directly verified | |
| ChecklistCompletion.Create | Field set not re-opened this phase | Not verified | N/A | Not individually re-verified | Pattern-inferred | `ChecklistTemplateId` stored as bare `Guid`, no navigation (F12) |
| ChecklistCompletion.Start | Same | Not verified | N/A | Not individually re-verified | Pattern-inferred | Does not check template status (F12) |
| ChecklistCompletion.RecordItemResult | Same | Not verified | N/A | Not individually re-verified | Pattern-inferred | |
| ChecklistCompletion.FlagFailedItem | Same | Not verified | N/A | Not individually re-verified | Pattern-inferred (guard); Directly verified (double-publish, F17 inherited) | Worst-issue #3 |
| ChecklistCompletion.Complete | Same | Not verified | N/A | Not individually re-verified | Pattern-inferred | |
| ChecklistCompletion.Archive/Cancel/Delete | N/A | N/A | N/A | N/A | Directly verified | |
