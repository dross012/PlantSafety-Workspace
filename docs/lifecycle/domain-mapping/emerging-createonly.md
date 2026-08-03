# Domain Mapping — Emerging/Create-Only Top-Level Aggregates

See `_shared-findings.md` §F1 (tenancy). All 11 aggregates are single-transition (Create only);
condensed per the task's Create-only guidance.

| Aggregate | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| AiGovernance | Create only | — | N/A | 0 throws, 0 events — thinnest guard shape found (Phase 1) | None | Not enforced (F1) | Directly verified | No SQL of any kind — likely unwired/aspirational |
| AiSafetyIntelligence | Create only | — | N/A | 0 throws, 0 events | None | Not enforced (F1) | Directly verified | Same shape |
| ManagementOfChange | Create only | — | N/A | — | See `compliance.md` | Not enforced (F1) | Directly verified | Duplicate listing, same aggregate |
| RegulatoryCalendar | Create only | — | N/A | — | See `compliance.md` | Not enforced (F1) | Directly verified | Duplicate listing |
| ContractorVisitor | Create only | — | N/A | — | See `contractorsafety.md` | Not enforced (F1) | Directly verified | Duplicate listing |
| Governance (top-level) | Create only | — | N/A | 0 throws, 0 events | None | Not enforced (F1) | Directly verified | No SQL — distinct from `Compliance.*` |
| HumanFactors | Create only | — | N/A | 0 throws, 0 events | None | Not enforced (F1) | Directly verified | No SQL |
| OperationalSafety (top-level) | Create only | — | N/A | 0 throws, 0 events | None | Not enforced (F1) | Directly verified | No SQL — distinct from `Operational.*` markers |
| ProcessManagement (`ProcessDefinition`) | Create only | 2 throw sites — real input guards despite zero SQL/events (Phase 1 finding) | **Partial** — input guards are real and would fire; but the transition has nowhere to persist to, so a successful in-memory `Create` still cannot complete an end-to-end write | Confirmed 2 guard sites | None | Not enforced (F1) | Directly verified | The one aggregate in this cluster with genuine validation and nowhere to persist — an inverted risk profile from its 10 siblings |
| Sustainability | Create only | — | N/A | 0 throws, 0 events | None | Not enforced (F1) | Directly verified | No SQL |
| WorkerWellBeing | Create only | — | N/A | 1 throw, 0 events | None | Not enforced (F1) | Directly verified | Only read-only SQL (`GetById`/`List`) — no write proc under any name |

**Summary:** No new evidence this phase; this remains the highest-confidence "would fail/no-op if
called" cluster in the solution (10 of 11 have zero confirmed SQL of any kind, the 11th read-only).
`ProcessManagement` is the sole one with real input validation, making it the most "nearly correct"
of the 11 — a working guard clause with nothing behind it to write to.
