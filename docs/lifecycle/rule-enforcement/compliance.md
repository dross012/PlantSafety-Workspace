# Rule Enforcement Validation — Compliance

See `_shared-findings.md` and F13 (`RegulatoryCalendar`'s write-repository interface has zero
implementations at all — the worst-confirmed persistence gap in the solution). 7 create-only
aggregates, batched.

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Certification — Create only (batched) | Fail — minimal marker guard | Unconfirmed | Pass (no other-module reference found) | N/A | Fail (F1, pattern) | Unconfirmed — no genuine write proc found, only subtype `_Get*` reports | Low | Plausibly dead/unwired |
| Compliance — Create only (batched) | Fail — minimal marker guard | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Unconfirmed | Low | `usp_ComplianceResources_BulkUpdate` is the closest write-path candidate, target table unconfirmed |
| ManagementOfChange — Create only (batched) | Fail — minimal marker guard | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Unconfirmed | Low | Only read-only MOC sub-concept reports found |
| Permits (Compliance) — Create only (batched) | Fail — minimal marker guard | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Unconfirmed | Low | `usp_PermitsToWork_List` read-only only |
| Policy — Create only (batched) | Fail — minimal marker guard | Unconfirmed | Pass | N/A | Fail (F1, pattern) | Unconfirmed | Low | Largest SQL footprint (12 `usp_Policy*` procs) of any create-only aggregate, still no confirmed base-entity write path |
| Procedures — Create only (batched) | Fail — minimal marker guard | Unconfirmed | Pass | N/A | Fail (F1, pattern) | N/A — read-only `usp_Procedures_GetItems` only | Low | |
| RegulatoryCalendar — Create only (batched) | **Fail** — factory exists but **the write repository interface has zero implementations anywhere in the solution (F13)**; even if a handler called `Create`, DI would fail to resolve `IRegulatoryCalendarRepository` | **Fail** — moot; nothing downstream of Create can execute | N/A — moot | N/A — moot, would throw at DI-resolution before any transition logic ran | Fail (F1, pattern) — also moot | **Fail/N/A** — the one "Infrastructure" class sharing this name doesn't implement the interface and calls two more procs (`usp_RegulatoryCalendar_GetById`/`_GetAll`) that don't exist as `.sql` files either (F13) | High (F13 is directly confirmed) | **Worst-confirmed gap of the whole audit**: broken one level deeper than "no SQL" — the C# write path itself cannot be constructed |
| Auditing (Compliance marker) — Create only (batched) | Fail — marker only | Fail — no write proc | N/A | N/A — dead code | Fail (F1, pattern) | N/A — read-only `usp_Auditing_GetLogs` | High | Duplicate of live `AuditEngagement`/`AuditFinding`, see `auditing.md` |
| All 7 above — Update/Delete/Cancel/status | N/A | N/A | N/A | N/A | N/A | N/A | High | None exist for any of the 7 |
