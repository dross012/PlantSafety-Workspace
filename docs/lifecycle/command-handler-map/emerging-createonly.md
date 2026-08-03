# Command-Handler Map — Emerging/Create-Only Top-Level Aggregates

See `_shared-findings.md` for F7 (Outbox universal), F8 (none of these 11 aggregates on the
23-aggregate SQL-audit list), F9 (event delivery, [EVT-A]/[EVT-B] depending on whether a named
consumer stub happens to exist), F10. Every aggregate here is Create-only with (per Phase 1's direct
file scan) 0-2 guard sites and 0 domain events except where noted. Command/handler names
pattern-inferred; not individually opened this phase — several of these aggregates may have no
command/handler at all (unconfirmed either way).

| Aggregate | Transition | Command (pattern-inferred) | Handler | Repository Call(s) | Domain Event(s) Published | Outbox? | Audit Entry? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|---|
| AiGovernance | Create only | `CreateAiGovernanceCommand` | Unconfirmed if a handler exists | `AddAsync` — **no SQL of any kind found** | **None found — 0 `AddDomainEvent` sites** (inherited) | **N** (no event to write) | **N** | Directly verified (guard/event counts + SQL absence, inherited) | Thinnest guard shape in the solution (0 throws, 0 events) — even if a command/handler exists and the SQL call fails, there would be no event either, so failure would be silent all the way up unless the repository throws and the handler surfaces it |
| AiSafetyIntelligence | Create only | `CreateAiSafetyIntelligenceCommand` | Unconfirmed | `AddAsync` — no SQL found | None found | **N** | **N** | Directly verified (inherited) | Same shape as AiGovernance |
| ContractorVisitor | Create only | `CreateContractorVisitorCommand` | Unconfirmed | `AddAsync` — no SQL of any kind found (this phase's census; see also `contractorsafety.md`) | Unconfirmed — not re-checked this phase | Unconfirmed | **N** | Directly verified (SQL absence, inherited) | Also appears in `contractorsafety.md` as the 6th ContractorSafety-adjacent concept |
| Governance (top-level) | Create only | `CreateGovernanceCommand` | Unconfirmed | `AddAsync` — no SQL found | None found (0 events) | **N** | **N** | Directly verified (inherited) | Distinct from `Compliance.*`'s regulatory concepts — an enterprise-governance marker with nothing behind it |
| HumanFactors | Create only | `CreateHumanFactorsCommand` | Unconfirmed | `AddAsync` — no SQL found | None found | **N** | **N** | Directly verified (inherited) | |
| OperationalSafety (top-level) | Create only | `CreateOperationalSafetyCommand` | Unconfirmed | `AddAsync` — no SQL found | None found | **N** | **N** | Directly verified (inherited) | Distinct from the `Operational.*` duplicate-marker namespace (which at least has read-only List procs) — this one has nothing at all |
| ProcessManagement (`ProcessDefinition`) | Create only | `CreateProcessDefinitionCommand` | Unconfirmed | `AddAsync` — no SQL found | **None found (0 events) despite 2 real guard/throw sites** — a factory with genuine input validation and nowhere to persist | **N** | **N** | Directly verified (inherited) | Notable outlier: has real domain-level validation, unlike its 0-throw siblings in this cluster, but is equally unpersisted |
| Sustainability | Create only | `CreateSustainabilityCommand` | Unconfirmed | `AddAsync` — no SQL found | None found | **N** | **N** | Directly verified (inherited) | |
| WorkerWellBeing | Create only | `CreateWorkerWellBeingCommand` | Unconfirmed | `AddAsync` — **read-only SQL** (`usp_WorkerWellBeing_GetById`/`_List`), no `_Create`/`_Insert`/`_Update` under any name | None found except 1 throw site | **N** | **N** | Directly verified (inherited) | The single most concrete "would fail/no-op" aggregate in this cluster per Phase 1 — some SQL exists, but none of it can accept a write |
| ManagementOfChange (Compliance) | Create only | See `compliance.md` — same aggregate, cross-listed here | — | — | — | — | **N** | — | Cross-reference only, not double-counted in totals |
| RegulatoryCalendar (Compliance) | Create only | See `compliance.md`/F13 — same aggregate, cross-listed here | — | — | — | — | **N** | — | Cross-reference only; F13 (this phase) found its write-repository interface has zero implementations, the single sharpest finding in either cluster |

**Not in this list despite superficial name-match candidacy (confirmed this phase, inherited):**
`DigitalTwin` (SQL exists as `usp_Twin_*`) and `EnterpriseFabric` (SQL exists as `usp_FabricNode_*`) —
both have real, if shortened-name, SQL persistence and were deliberately excluded after a 2nd-pass
loose-substring check. Do not re-add them without re-confirming the naive grep alone.

**Event delivery reality (F9):** of the 9 top-level markers with any `AddDomainEvent` presence
checked, none showed one (0/9 have any domain event at all per Phase 1's count) — so for this
cluster the Outbox/Audit question is largely moot; there is nothing to write to Outbox even setting
aside whether a command/handler exists to invoke `Create` at all. This is a stronger form of
"silent" than anything else in the inventory: not guarded-but-unreachable (F4/Scheduling's `Cancel`),
not guarded-but-eventless (F3's silent archives) — these are **events that were never designed to
exist in the first place**, on aggregates that additionally have nowhere confirmed to persist to.
