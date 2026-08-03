# Rule Enforcement Validation — Checklists

See `_shared-findings.md` and F12 (`ChecklistCompletion` never checks its `ChecklistTemplate`'s status —
re-confirmed, not merely repeated, by this phase's G5).

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| ChecklistTemplate — Create | Pass | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) — `ChecklistTemplateDapperRepository` confirmed hardcoded site | High | |
| ChecklistTemplate — AddItem | Pass — guard confirmed (inherited); event pattern-inferred | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| ChecklistTemplate — Publish | **Pass** — real rule: rejects publishing an empty template (zero items), a genuine invariant (inherited) | Pass (inherited) | Pass | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| ChecklistTemplate — Retire | Pass — guard confirmed (inherited) | Pass (inherited) | **Fail** — nothing prevents Retire while `ChecklistCompletion` rows still reference this template as active (see F12's converse: the gap is bidirectional — neither side checks the other) | Pass (own status machine) | Fail (F1, pattern) | Fail (G2) — `ChecklistTemplateDapperRepository` confirmed hardcoded site | High | |
| ChecklistTemplate — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| ChecklistCompletion — Create | Pass | Pass (inherited) | **Fail (F12, re-confirmed)** — `ChecklistTemplateId` stored as a bare `Guid`, no repository dependency exists in any Checklists command handler capable of loading the template | Pass | Fail (F1, pattern) | Fail (G2) — `ChecklistCompletionDapperRepository` confirmed hardcoded site | High | |
| ChecklistCompletion — Start | Pass — own-status guard | Pass (inherited) | **Fail (F12)** — can Start against a Retired/never-Published template | Pass (own status machine only) | Fail (F1, pattern) | Fail (G2) | High | |
| ChecklistCompletion — RecordItemResult | Pass | Pass (inherited) | **Fail (F12)** | Pass | Fail (F1, pattern) | Fail (G2) | High | |
| ChecklistCompletion — FlagFailedItem | Pass — guard pattern-inferred, not independently confirmed | Unconfirmed | **Fail (F12)** | Pass | Fail (F1, pattern) | Fail (G2) | Medium | |
| ChecklistCompletion — Complete | Pass — own-status guard | Pass (inherited) | **Fail (F12)** — a `ChecklistCompletion` can be fully completed against a template that has since been Retired, with nothing in either layer able to detect it | Pass (own status machine only — completing does not itself violate `ChecklistCompletion`'s own sequence, only the cross-aggregate rule) | Fail (F1, pattern) | Fail (G2) | High | Headline cross-module gap for this module |
| ChecklistCompletion — Archive/Cancel/Delete | N/A | N/A | N/A | N/A | N/A | N/A | High | |
| Checklists (Operational duplicate) — Create only | Fail — marker only | Fail — no write SQL exists | N/A | N/A — dead code | Fail (F1, pattern) | N/A — no write path exists at all | High | Confirmed dead duplicate (read-only `usp_Checklists_List` only) |
