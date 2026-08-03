# Module Inventory — Emerging/Create-Only Top-Level Aggregates

11 single-aggregate, top-level "marker-ish" domain folders, each with exactly one aggregate whose entire
public surface is `Create` + `ClearDomainEvents`, and — the headline finding of this phase — the
**weakest confirmed SQL persistence of any cluster in the solution**.

| Aggregate | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| AiGovernance | Create only | Yes (0 throws, 0 events — thinnest guard shape found this phase) | **No SQL of any kind found**, exact or loose match | Unconfirmed — likely unwired/aspirational | Directly verified | Name suggests a recently-added, forward-looking domain area (AI governance policy) that has a C# shell but no database behind it yet |
| AiSafetyIntelligence | Create only | Yes (0 throws, 0 events) | **No SQL of any kind found** | Unconfirmed — likely unwired/aspirational | Directly verified | Same shape as AiGovernance — likely built in the same pass |
| ManagementOfChange (Compliance) | Create only | Yes | **No genuine write proc found** (see `compliance.md` — only MOC sub-concept read reports exist) | Unconfirmed | Directly verified | Also listed in `compliance.md`; included here because it fits this cluster's "Create with no confirmed write path" shape |
| RegulatoryCalendar (Compliance) | Create only | Yes | **No SQL of any kind found** | Unconfirmed | Directly verified | Also listed in `compliance.md` |
| ContractorVisitor | Create only | Yes | **No SQL of any kind found** | Unconfirmed | Directly verified | |
| Governance (top-level) | Create only | Yes (0 throws, 0 events) | **No SQL of any kind found** | Unconfirmed — likely unwired/aspirational | Directly verified | Distinct from `Compliance.*`'s regulatory-governance concepts — this is an enterprise-governance top-level marker |
| HumanFactors | Create only | Yes (0 throws, 0 events) | **No SQL of any kind found** | Unconfirmed — likely unwired/aspirational | Directly verified | |
| OperationalSafety (top-level) | Create only | Yes (0 throws, 0 events) | **No SQL of any kind found** | Unconfirmed — likely unwired/aspirational | Directly verified | Distinct from the `Operational.*` namespace's duplicate markers (which do have read-only List procs) — this is a separate top-level marker with nothing at all |
| ProcessManagement (`ProcessDefinition`) | Create only | Yes (2 throws, 0 events — the only zero-throw... actually has 2 guard sites but raises no event at all on Create) | **No SQL of any kind found** | Unconfirmed — likely unwired/aspirational | Directly verified | Notable: this one *does* have input guards (2 throw sites) despite having zero SQL and zero events — a domain factory with real validation and nowhere to persist to |
| Sustainability | Create only | Yes (0 throws, 0 events) | **No SQL of any kind found** | Unconfirmed — likely unwired/aspirational | Directly verified | |
| WorkerWellBeing | Create only | Yes (1 throw, 0 events) | **Read-only** — `usp_WorkerWellBeing_GetById`/`_List` exist, but no `_Create`/`_Insert`/`_Update` found under any name | Unconfirmed | Directly verified | The one aggregate in this cluster with *some* SQL — but it's read-only, so a `Create` call still has nowhere confirmed to write |

**Two adjacent folders that looked like candidates for this list but were confirmed NOT to belong here,
included for anyone re-deriving this list**: `DigitalTwin` (SQL exists as `usp_Twin_Create/GetById/List`
— a shortened noun, not `usp_DigitalTwin_*`) and `EnterpriseFabric` (SQL exists as
`usp_FabricNode_Create/GetById/List` — same shortened-noun pattern). Both were caught by this phase's
2nd-pass loose substring check; a naive exact-prefix-only grep would have wrongly added them here.

**Why this matters:** the prior Ingestion Discovery characterized PlantSafety's ~50 Create-only
aggregates as "shallow but real (if intentionally simple)" — this phase's SQL cross-reference adds a
sharper distinction the prior discovery didn't have the SQL-side evidence to draw: **these 11 aggregates
are not just shallow, their one real transition (Create) appears to have no confirmed database
destination at all.** If accurate, calling `Create` on any of these 11 in production would either throw
at the repository layer (no matching stored procedure to call) or — if the repository silently no-ops
or the call site was never wired to begin with — never persist. This should be checked against each
aggregate's actual repository implementation before treating it as fully certain (see `inventory.md`
§2's SQL cross-referencing caveat).
