# Event Map — Emerging/Create-Only Top-Level Aggregates (AiGovernance, AiSafetyIntelligence, ContractorVisitor, Governance, HumanFactors, OperationalSafety, ProcessManagement, Sustainability, WorkerWellBeing, + cross-listed ManagementOfChange/RegulatoryCalendar)

See `_shared-findings.md` (F14-F17) and `command-handler-map/emerging-createonly.md`. This cluster
is the sharpest exception to F14's "uniform, complete event shape" finding — **9 of these 11
aggregates have zero domain events of any kind**, confirmed both by this phase's structural review
and by the inherited Phase 1 guard/event count census.

| Aggregate | Transition | Publishes Event? | Event Type | Payload Complete? | Consumer Exists & Registered? | Confidence | Notes |
|---|---|---|---|---|---|---|---|
| AiGovernance | Create only | **N — 0 `AddDomainEvent` sites, confirmed by Phase 1's structural census (inherited)** | N/A — no event class exists to evaluate for completeness | N/A | **N** | Directly verified (inherited) | Thinnest shape in the solution: 0 throws, 0 events. If a command/handler exists and the SQL write fails, there is no event either — failure would be silent all the way up unless the repository itself throws and something surfaces it |
| AiSafetyIntelligence | Create only | **N — 0 events** | N/A | N/A | **N** | Directly verified (inherited) | Same shape as AiGovernance |
| ContractorVisitor | Create only | **Y — event class confirmed to exist this phase** (`ContractorVisitorCreatedEvent.cs`, listed under `ContractorVisitor/Events/`) | `ContractorVisitorCreatedEvent` | Pattern-inferred Y (not opened this phase) — if it follows F14's universal shape, minimal-but-complete | **N** | Directly verified (file existence); body not opened | **Correction to the inherited Phase 1 framing**: this aggregate is not part of the "0 events" majority in this cluster — it has a real event class, unlike AiGovernance/AiSafetyIntelligence/Governance/HumanFactors/OperationalSafety/Sustainability. Also appears in `contractorsafety.md` as the 6th ContractorSafety-adjacent concept |
| Governance (top-level) | Create only | **N — 0 events** | N/A | N/A | **N** | Directly verified (inherited) | Distinct from `Compliance.*`'s regulatory concepts |
| HumanFactors | Create only | **N — 0 events** | N/A | N/A | **N** | Directly verified (inherited) | |
| OperationalSafety (top-level) | Create only | **N — 0 events** | N/A | N/A | **N** | Directly verified (inherited) | Distinct from the `Operational.*` duplicate-marker namespace (which has 0-events-but-at-least-a-List-proc) |
| ProcessManagement (`ProcessDefinition`) | Create only | **Y — event class confirmed to exist this phase** (`ProcessDefinitionCreatedEvent.cs`) | `ProcessDefinitionCreatedEvent` | Pattern-inferred Y (not opened) | **N** | Directly verified (file existence) | **Correction to the inherited framing**: command-handler-map's F1 census noted this aggregate has "2 real guard/throw sites despite 0-throw siblings" but characterized it as having "None found (0 events)" — this phase's direct file listing shows an event class **does** exist. Whether it is actually raised by the domain method (vs. defined-but-unreferenced, the DataLake/Intelligence pattern) was not confirmed this phase and is the natural next check |
| Sustainability | Create only | **N — 0 events** | N/A | N/A | **N** | Directly verified (inherited) | |
| WorkerWellBeing | Create only | **Y — event class confirmed to exist this phase** (`WorkerWellBeingCreatedEvent.cs`) | `WorkerWellBeingCreatedEvent` | Pattern-inferred Y (not opened) | **N** | Directly verified (file existence) | **Correction to the inherited framing**: command-handler-map states "None found except 1 throw site" for this aggregate — an event class does exist. Per F13's sibling finding, this aggregate's SQL surface is read-only (`GetById`/`_List`), so even if the event is correctly raised, the underlying write it would represent has nowhere to persist to — the event would describe a Create that never actually committed |
| ManagementOfChange (Compliance) | Create only | Y — see `compliance.md` | `ManagementOfChangeCreatedEvent` | Y (directly read there) | **N (EVT-A)** | Cross-reference only, not double-counted | |
| RegulatoryCalendar (Compliance) | Create only | Unconfirmed/moot per F13 — see `compliance.md` | `RegulatoryCalendarCreatedEvent` | Y (directly read there) | **N (EVT-A)** | Cross-reference only | |

**Duplicate-publish check (F17):** not applicable to the 6 zero-event aggregates; the 3 aggregates
newly confirmed to have an event class each have exactly 1 event for their 1 transition — no
duplicate possible.

**Module-level summary — a correction to inherited Phase 1 framing, not a re-derivation:** this
phase's direct file-listing pass found **3 of the 11 aggregates in this cluster (`ContractorVisitor`,
`ProcessManagement`, `WorkerWellBeing`) actually have a defined event class**, contradicting the
inherited "0/9 have any domain event at all" summary line in `command-handler-map/
emerging-createonly.md` (which appears to have been counting only the 9 rows it individually
listed, excluding the two cross-referenced Compliance aggregates, and did not itself re-open the
`Events/` folders for `ContractorVisitor`/`ProcessManagement`/`WorkerWellBeing` to check for a file
before writing "None found"). Whether these 3 event classes are actually **raised** by their
aggregates' `Create` methods (vs. defined-but-dead, per the DataLake/Intelligence pattern
elsewhere in the solution) is the natural follow-up this phase did not have scope to close. The
remaining 6 aggregates (`AiGovernance`, `AiSafetyIntelligence`, `Governance`, `HumanFactors`,
`OperationalSafety`, `Sustainability`) remain confirmed at 0 events — this is still a stronger form
of "silent" than anything else in the solution: these are events that were never designed to exist,
on aggregates that additionally have no confirmed SQL destination.
