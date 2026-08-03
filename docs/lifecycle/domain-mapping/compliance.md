# Domain Mapping — Compliance

See `_shared-findings.md` §F1 (tenancy). 7 create-only aggregates, condensed per the task's
Create-only guidance; no lifecycle beyond Create exists on any of them.

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Certification | Create only | — | N/A | Not individually confirmed | Plausibly should validate against `AccidentManagement`/`RiskManagement` if certifications gate work eligibility — no confirmed link | Not enforced (F1) | Directly verified (file-name census, Phase 1) | No genuine write proc found — only subtype read reports |
| Compliance | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified | No genuine write proc found |
| ManagementOfChange | Create only | — | N/A | — | Plausibly should link to `Auditing`/`Risk` if a change requires a risk re-assessment — no confirmed link | Not enforced (F1) | Directly verified | No genuine write proc found; also listed in `emerging-createonly.md` |
| Permits (Compliance) | Create only | — | N/A | — | Distinct SQL-visible concept name (`usp_PermitsToWork_List`) from the C# class `Permits` — possible naming collision with the `PermitToWork` family, not confirmed | Not enforced (F1) | Directly verified | Read-only only |
| Policy | Create only | — | N/A | — | Largest SQL footprint of any create-only aggregate (12 `usp_Policy*` procs), still no confirmed write path for the base entity | Not enforced (F1) | Directly verified | See Phase 1 — every proc is a child/category report, none is `Policy_Create/Update` |
| Procedures | Create only | — | N/A | — | None | Not enforced (F1) | Directly verified | Read-only only |
| RegulatoryCalendar | Create only | — | N/A | — | Should plausibly be checked by `Accident.FileRegulatoryReport`/`CloseInvestigation` for filing deadlines — no confirmed cross-module call in either direction | Not enforced (F1) | Directly verified | No SQL of any kind found — candidate zero-SQL orphan |
| Auditing (Compliance marker dup) | Create only | — | N/A | — | Duplicate of live `Auditing.AuditEngagement`/`AuditFinding` | Not enforced (F1) | Directly verified | Read-only (`GetLogs`), legacy/dead-ish |

**Summary:** No new evidence this phase; the `RegulatoryCalendar`↔`Accident` non-relationship
remains the most concerning cross-module absence in this cluster given the real-world stakes
(regulatory filing deadlines), still unconfirmed either way since neither module's repository
implementation was opened this phase.
