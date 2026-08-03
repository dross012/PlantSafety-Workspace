# Gaps — Emerging/Create-Only Top-Level Aggregates

11 single-aggregate, marker-ish domain folders (`AiGovernance`, `AiSafetyIntelligence`,
`ManagementOfChange`[Compliance, also in `compliance.md`], `RegulatoryCalendar`[Compliance, also in
`compliance.md`], `ContractorVisitor`, `Governance`, `HumanFactors`, `OperationalSafety`,
`ProcessManagement`/`ProcessDefinition`, `Sustainability`, `WorkerWellBeing`). See `_legend.md`.

| Aggregate | Missing Transitions | Bypasses Domain Rules | Fails to Publish Events | Violates Tenancy | Violates Actor Propagation | Violates SQL Shape | Violates RLS |
|---|---|---|---|---|---|---|---|
| AiGovernance | Update/Close/Cancel/Delete missing — Create-only, 0 throws/0 events, thinnest guard shape found `[Inv]` | No (nothing to bypass — no SQL destination to reach) | N/A (inherent) | N/A — no SQL destination of any kind | **N/A — no repository, no outbox call site exists** `[Inv]`/`[Actor§2c]` | **Yes — no SQL of any kind found**, exact or loose match; likely unwired/aspirational `[Inv]` | N/A — moot |
| AiSafetyIntelligence | Update/Close/Cancel/Delete missing `[Inv]` | No | N/A (inherent) | N/A | **N/A** `[Actor§2c]` | **Yes — no SQL of any kind found** `[Inv]` | N/A — moot |
| ManagementOfChange (Compliance) | Update/Close/Cancel/Delete missing `[Inv]` | No (unreachable) | N/A (inherent) | N/A | **N/A — never reaches a repository** `[SQL-G1]`/`[Actor§2c]` | **Yes — zero implementation** `[SQL-G1]` | N/A — moot |
| RegulatoryCalendar (Compliance) | Update/Close/Cancel/Delete missing `[Inv]` | No (unreachable) | N/A (inherent) | N/A | **N/A — never reaches a repository**, worst-confirmed case: zero implementing classes for the interface at all `[CH-F13]`/`[Actor§2c]` | **Yes — zero implementation, worst case** `[CH-F13]`/`[SQL-G5]` | N/A — moot |
| ContractorVisitor | Update/Close/Cancel/Delete missing `[Inv]` | No | N/A (inherent) | N/A | Unconfirmed | **Yes — no SQL of any kind found** `[Inv]` | Unconfirmed |
| Governance (top-level) | Update/Close/Cancel/Delete missing `[Inv]` | No | N/A (inherent) | N/A | **N/A** `[Actor§2c]`-analog | **Yes — no SQL of any kind found**; likely unwired/aspirational `[Inv]` | N/A — moot |
| HumanFactors | Update/Close/Cancel/Delete missing `[Inv]` | No | N/A (inherent) | N/A | **N/A** | **Yes — no SQL of any kind found** `[Inv]` | N/A — moot |
| OperationalSafety (top-level) | Update/Close/Cancel/Delete missing `[Inv]` | No | N/A (inherent) | N/A | **N/A** | **Yes — no SQL of any kind found** `[Inv]` | N/A — moot |
| ProcessManagement (`ProcessDefinition`) | Update/Close/Cancel/Delete missing `[Inv]` | No — notably *does* have 2 real guard sites despite zero SQL/events — a domain factory with real validation and nowhere to persist to `[Inv]` | N/A (inherent — 0 events on Create even beyond the inherent gap) `[Inv]` | N/A | **N/A** | **Yes — no SQL of any kind found** `[Inv]` | N/A — moot |
| Sustainability | Update/Close/Cancel/Delete missing `[Inv]` | No | N/A (inherent) | N/A | **N/A** | **Yes — no SQL of any kind found** `[Inv]` | N/A — moot |
| WorkerWellBeing | Update/Close/Cancel/Delete missing `[Inv]` | No | N/A (inherent) | N/A | **N/A** | **Yes — read-only SQL only** (`_GetById`/`_List`); no `_Create`/`_Insert`/`_Update` under any name — the one aggregate in this cluster with *some* SQL, but still no confirmed write destination `[Inv]` | N/A — moot |

**Net for this module**: 10 of 11 have **zero confirmed SQL persistence of any kind** — the single most concrete "would fail/no-op if called" finding of Phase 1, now folded in with `compliance.md`'s `[SQL-G1]` zero-implementation finding for the 2 aggregates that overlap both clusters (`ManagementOfChange`, `RegulatoryCalendar`). `DigitalTwin`/`EnterpriseFabric` were checked and confirmed **not** to belong to this list (shortened-noun SQL naming, `Twin`/`FabricNode`) — do not re-add them on a naive re-grep.
