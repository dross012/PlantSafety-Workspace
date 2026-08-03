# Gaps — Checklists

`ChecklistTemplate`/`ChecklistCompletion` (rich, live) + `Operational.Checklists.Checklists`
(create-only marker, dead duplicate). See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| ChecklistTemplate | Archive/Cancel/Delete missing `[Inv]` | No — real rule: rejects publishing an empty template `[Inv]` | No — publishes correctly per transition (EVT-B, no working consumer) `[CH-F9]` | **Yes (pattern-inferred)** — classic shape `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation; not AuditLog-covered `[CH-F8]` | **Yes (pattern-inferred)** `[RLS§2]` |
| ChecklistCompletion | Archive/Cancel/Delete missing `[Inv]` | **Yes** — confirmed gap, not merely suspected: none of the 5 lifecycle methods (`Create`/`Start`/`RecordItemResult`/`FlagFailedItem`/`Complete`) check the referenced `ChecklistTemplate`'s status; `ChecklistTemplateId` is a bare unvalidated FK, zero handlers have a template-repository dependency at all `[CH-F12]` — a completion can run against a Retired/never-Published template with nothing able to detect it | **Partial** — `FlagFailedItem` is the solution's one confirmed double-publish (calls `RecordItemResult` internally, raising 2 events from 1 command) `[EV-F17]`; not silent, but a duplication anomaly | **Yes (pattern-inferred)** `[DM-F1]` | **Yes — Bug#1 only** `[Actor§2b]` | No confirmed shape violation | **Yes (pattern-inferred)** `[RLS§2]` |
| Checklists (Operational duplicate marker) | Update/Close/Cancel/Delete missing — Create-only `[Inv]` | No | N/A (inherent) | N/A — read-only SQL (`usp_Checklists_List` only), legacy/dead-ish `[Inv]` | N/A — no confirmed write path | Read-only only, no write proc `[Inv]` | N/A |

**Net for this module**: `ChecklistCompletion`'s unvalidated `ChecklistTemplateId` (F12) is the clearest **confirmed** (not pattern-inferred) instance of the cross-module-reference gap the audit worried recurs throughout the 48 rich aggregates — direct proof the risk is real, not merely theoretical.
