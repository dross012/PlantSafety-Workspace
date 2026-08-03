# Severity-Ranked Gap Register — Phase 8 of 10 (Lifecycle Coverage Audit)

Every distinct gap found across the 25 `gaps/*.md` files, deduplicated and ranked by real-world
severity (data integrity / security / availability / compliance / pure tech-debt), not by module or
by row count. A gap affecting 90 aggregates identically appears **once**, with an impact note stating
how many aggregates it touches — this is the single most important input to Phase 9. See `_legend.md`
for citation codes.

---

## Tier 0 — Critical (security/compliance exposure, or a fix-ordering trap that would make things worse)

### 1. Fixing the auth-policy-500 bug before the tenancy/RLS gap converts an availability bug into a live cross-tenant exposure
**Category: Security (sequencing risk).** Scope: ~293+ endpoints currently 500ing due to missing
authorization policy registration (inherited from the Ingestion Discovery). Today's 500 is,
incidentally, a tenant-isolation control. Fixing it naively, before layers 2 (F1 app-layer tenancy
check) and 3 (RLS engagement) are fixed, lets previously-blocked requests reach handlers with **zero**
tenant isolation of any kind for the GetById/Update transition shape. `[RLS§6]` `[Actor§5]`.
**Why #1**: this is not a static gap, it is an ordering hazard — the single finding in the whole audit
where the "obvious" fix (register the missing policies) actively makes the security posture worse if
done first. Any remediation plan must sequence around this explicitly.

### 2. SQL Server RLS never engages for the exact call shape that carries every lifecycle mutation (load-by-ID / update-by-ID)
**Category: Security + Availability (bimodal).** Scope: ~46+ of PlantSafety's 48 rich aggregates,
including all 23 audit-logged ones (Accident, Risk, SafetyPolicy, Scorecard, 4× PermitToWork, 5× PPE,
5× Scheduling, 5× Observability) plus AuditEngagement/AuditFinding and more — every aggregate using
the "classic" `GetByIdAsync(id)`/`UpdateAsync(aggregate)` repository shape. `SetTenantSessionContextAsync`
only fires if the calling method's own `DynamicParameters` happens to include a `"TenantId"` key; this
shape never does. Two possible live states, both bad: **(a)** if the RLS migration is applied, every
`GetByIdAsync` silently returns null and every `UpdateAsync` silently affects 0 rows for every tenant,
unconditionally — a solution-breaking, silent availability bug; **(b)** if not applied, there is zero
SQL-layer enforcement and F1 (below) stands completely undiminished. `[RLS§1]` `[RLS§2]`.
**Why Tier 0, not Tier 1**: unlike F1 (an unenforced app-layer check, which is "merely" broken), this
finding has a branch where the defect isn't a security gap at all but a **totally silent,
solution-wide functional failure** masquerading as a security control — the two are inseparable until
someone checks the live database, and either answer is severe.

### 3. Zero of ~344 transitions in the entire solution have confirmed correct end-to-end actor/correlation propagation
**Category: Compliance + Security (forensic/audit failure).** Scope: literally every transition,
solution-wide — PlantSafety (all 3 buckets: 23-aggregate both-bug cluster, ~35-aggregate Bug#1-only
cluster, ~15-18 zero-implementation aggregates that never reach the bug only because they can't
execute at all), plus DataLake/Intelligence/Azure's independently-worse gaps. Root causes: (1) 58
`outboxWriter.Enqueue(...)` call sites hardcode `"system"`/`Guid.Empty`, discarding the real actor the
command handler already has; (2) the SQL `AuditLog` INSERT in 23 aggregates' stored procedures binds
`ChangedBy = SYSTEM_USER`, a shared service-account identity, never the application-level user.
`[Actor§1-4]` `[RE-G2]`.
**Why Tier 0**: this is the finding that makes "who did this" **unanswerable** for every single
mutating action in the solution — a hard blocker for any compliance/forensic requirement (SOC 2,
incident investigation, regulatory audit trail for accident/incident records) regardless of whether
any individual transition is otherwise correct.

---

## Tier 1 — High (real-world data-integrity or availability exposure, single confirmed root cause, broad blast radius)

### 4. 13+ aggregates' domain write-repository has zero implementing class anywhere in the solution
**Category: Availability (guaranteed runtime failure).** Scope: `RegulatoryCalendar` (Compliance),
`Culture`/`ScoreCards` (Analytics), `Certification`/`Compliance`/`ManagementOfChange`/`Permits`/
`Policy`/`Procedures` (Compliance, 6 more), `EmergencyManagement`/`IncidentManagement`/`Observation`
(SafetyEvents) — 13 confirmed, plus `AiGovernance`/`AiSafetyIntelligence` pattern-inferred. Calling
`Create` on any of these fails at DI-container resolution before a single SQL statement runs — worse
than "the stored procedure doesn't exist," because the C# class that would call any procedure doesn't
exist either. `[SQL-G1]` `[SQL-G5]` `[CH-F13]`.
**Impact note**: ranked by its own source phase as "the single worst — and most solution-wide —
shape-alignment finding of the entire audit" `[SQL-G5]` — not recoverable by fixing a validator or
widening a column; requires writing 13 repository implementations from scratch, 12 of which also need
a stored procedure that doesn't exist yet.

### 5. Systemic app-layer tenancy gap — no transition anywhere checks the caller's TenantId against the loaded aggregate's TenantId
**Category: Security (cross-tenant data exposure).** Scope: every mutating transition on every module
checked (PlantSafety's 48 rich aggregates, DataLake, Intelligence) — directly confirmed for 5
aggregates (Accident, PermitWorkflow's repository, MachineGuarding, DataLake's `IngestionRepository`,
Intelligence's `TrainingJobRepository`), pattern-inferred for the remainder on the strength of an
identical generated repository shape. Tenant isolation today depends entirely on `AggregateId` GUIDs
being unguessable, not on any enforced check. `[DM-F1]` `[RE-G1]`.
**Impact note**: this is the finding gap #2 above sits on top of — RLS not engaging means F1 has zero
defense-in-depth, not merely a weakened one.

### 6. Delete/hard-delete does not exist anywhere in PlantSafety — 93 of 93 aggregates
**Category: Compliance (data-retention / right-to-erasure).** Scope: every PlantSafety aggregate,
confirmed by full non-sampled grep of all `Aggregates/*.cs` files. Plausibly correct-by-design for
regulatory/audit records, but means there is **no path** — C# or SQL — to remove a record from any of
~100 aggregates. `[Inv]`.
**Impact note**: severity is conditional — if any of this data is ever subject to a legal
right-to-erasure or data-retention deletion mandate, this is a hard compliance blocker; if not, this
is arguably correct design, not a gap. Flagged Tier 1 rather than Tier 0 because it is a single,
well-understood, uniform absence rather than a live exposure.

### 7. Auditing (SQL AuditLog) covers only 22-23 of 93 aggregates, and is best-effort/anonymous even there
**Category: Compliance (forensic gap, distinct from the actor-propagation bug above).** Scope: 70 of
93 PlantSafety aggregates get **zero** AuditLog row for any transition (all of EmployeeSafety,
MachineSafety, SafetyProcedures, ContractorSafety, Auditing itself, Checklists, Compliance,
DigitalTwin, AiGovernance, Dashboards, Analytics, SafetyEvents, emerging-createonly). Of the 23 that do
get a row: the INSERT is wrapped in an empty-`CATCH` block (audit-write failures are silently
swallowed) and `Module` is bound to the stored-procedure name, which cannot distinguish which of
several transitions sharing one generic `_Update` proc actually wrote the row. `[CH-F8]`.
**Impact note**: distinct from gap #3 (which is about *who* acted) — this is about *whether a record
of the action exists at all*, for 75% of the aggregate population.

### 8. Guarded-but-100%-dead lifecycles: Operational.Alerts.Alert (9 transitions) and PMScheduling.Cancel
**Category: Availability/tech-debt (wasted engineering, not a live defect).** Scope: `Operational.
Alerts.Alert`'s entire 9-transition, richest-guarded lifecycle in the solution has no HTTP route of
any kind exposed for it `[DM-F4]`; `PMScheduling.Cancel` is fully built, guarded, and event-raising but
unreachable from any route `[Inv]`. Also applies conceptually to Intelligence's 12 domain-guarded
transitions that are reached-and-bypassed rather than merely unreachable (see gap #10).
**Impact note**: not a security or data-integrity risk today (dead code can't misbehave), but
represents real, needless engineering investment sitting idle, and a latent risk if either is ever
wired up without re-auditing the guard logic against current requirements.

---

## Tier 2 — Medium (confirmed, scoped defects with real but bounded blast radius)

### 9. Silent-Archive: 4 aggregates guard the Archive transition but never publish a domain event
**Category: Data integrity (downstream notification gap).** Scope: `Accident`, `Risk`, `Scorecard`
(directly confirmed), `SafetyPolicy` (presumed/suspected, not directly re-verified this audit) — vs. 3
confirmed-correct instances (`PermitTemplate`, `Dashboard`, `Operational.Alerts.Alert`) and 1
unconfirmed either way (`PermitWorkflow.Archive`). `[DM-F3]`.
**Impact note**: moot today because nothing consumes any event solution-wide (see gap #12), but would
become a real gap the moment any consumer is wired up — archival would then silently fail to notify.

### 10. Intelligence's handler-bypass pattern: fully-guarded domain code that never executes at runtime
**Category: Data integrity (rule-enforcement, contained blast radius).** Scope: `TrainingJob` (7
transitions) and `Investigation` (6 transitions) — every reachable command handler calls the
repository directly, never constructing the domain aggregate, so all guard logic (terminal-state
checks, non-empty-reason validation, etc.) is provably dead code at runtime. Confirmed **not** to
recur anywhere else in the solution (checked across 5 representative clusters). `[DM-F2]` `[RE-G6]`.
**Impact note**: severe in isolation (a guard that looks protective and isn't is worse than an
admittedly-absent one) but narrowly scoped to exactly 2 aggregates / 13 transitions — the smallest-
blast-radius Tier 2 item, which is why it doesn't rank in Tier 0/1 despite its conceptual severity.

### 11. Confirmed cross-module/cross-field reference gaps — 4 instances, pattern likely wider
**Category: Data integrity.** Scope, all directly confirmed: `AuditEngagement.Close` accepts raw
caller-supplied `ComplianceScore`/`ComplianceRating` with no cross-module verification and no gate on
open `AuditFinding`s `[DM-F6]`/`[RE-G5]`; `AutomatedIncidentDetection.EscalateToIncident` stores an
`IncidentReferenceId` on faith with no existence/status check `[DM-F11]`; `ChecklistCompletion` never
checks its referenced `ChecklistTemplate`'s status (confirmed — a completion can run against a Retired
or never-Published template) `[CH-F12]`. 2 other candidates (`PPEAssignment`, `MachineGuardingInspections`)
were checked and **cleared** — they do not exhibit this pattern `[RE-G5]`.
**Impact note**: every command handler in the codebase follows a single-repository `Load → mutate →
Save` shape with no second repository dependency — structurally, any aggregate storing a raw Guid FK
to another module is likely to share this gap. Pattern-inferred beyond the 4 confirmed instances, not
individually verified for the full 48-aggregate rich population.

### 12. Zero domain events reach a real, working consumer anywhere in the solution
**Category: Availability/tech-debt (integration gap, not a correctness defect per se).** Scope:
essentially every event raised solution-wide — the 22 "Created" event types have unregistered
consumers and a type mismatch at the Outbox-processor layer even if registered; the ~30+ non-Create
lifecycle events across every rich aggregate have no consumer class of any kind; AccidentManagement's
7 events reconfirmed absent. DataLake's 2 correctly-published events (`IntegrationEventPublishedEvent`,
`DataAccessApprovedEvent`) are also confirmed dead-ends. `[CH-F9]`.
**Impact note**: this is an integration/architecture gap (events are correctly raised and persisted,
just never consumed) rather than a defect in the transitions themselves — lower severity than gaps
that produce wrong or missing data, since no incorrect action results, only a missed opportunity.

### 13. Duplicate-concept clusters — same real-world object modeled as 2+ unrelated aggregates with no shared identity
**Category: Data integrity (compliance-reporting risk).** Scope, confirmed distinct-but-overlapping,
not identical: Alert (2 live-candidate shapes + a 3rd never re-located) `[DM-F4]`; Risk vs.
Operational.RiskManagement (1 live + 1 dead GET-only) `[Inv]`; Dashboard vs. Analytics.Dashboard (1
live + 1 ambiguous) `[Inv]`; Auditing (AuditEngagement/AuditFinding, live) vs. Compliance.Auditing
(dead, read-only); PermitWorkflow vs. ContractorSafety.PermitToWork (2 live, different state machines,
same subject) `[DM-F5]`; LOTO modeled 3 times (EmployeeSafety.LotoPrograms, MachineSafety.LotoSystems,
SafetyProcedures.LotoProcedures); PPE modeled 3 times (PPE.PPEAssignment, EmployeeSafety.Ppe,
SafetyProcedures.PpePolicies); "incident" modeled 3 times; "auditing/inspection" modeled 4 times.
**Impact note**: the same physical entity (e.g., a permit) can be recorded in one aggregate and missed
by a query against its sibling — a real compliance-reporting exposure, not a runtime-crash risk. Only
1 pair (PermitWorkflow/ContractorSafety.PermitToWork) has been individually confirmed as genuinely
distinct-but-risky; the rest are flagged, not resolved.

### 14. Confirmed input-validation gap: ~30 of 220 commands solution-wide have no validator class at all
**Category: Data integrity.** Scope: the FluentValidation pipeline is globally and correctly wired,
but where no `*CommandValidator.cs` file exists, there is nothing for the pipeline to enforce — a
absence-of-artifact gap, not a wiring defect. `[RE-G3]`. `TriggerAlertCommand` (Alerts) is the
directly-confirmed example, additionally compounded by gap #15.
**Impact note**: narrower than it sounds — the pipeline itself is not broken, and 190 of 220 commands
do have real coverage; this is a census of the remaining 14%, not a systemic wiring failure.

### 15. Confirmed silent-truncation bug: TriggerAlertCommand
**Category: Data integrity.** Scope: exactly 1 confirmed instance solution-wide —
`StoredProcedureBuilder.AddString` silently truncates an oversized string to a caller-declared byte
cap before it ever reaches SQL Server, with no rejection or error. `[SQL-G3]`. DataLake/Intelligence
checked and found **not** to share this risk (Dapper infers parameter size from actual value length,
so their failure mode, if any, would be a loud SQL-side error, not a silent truncation).
**Impact note**: severity is contained by its narrow scope (1 confirmed instance across the entire
541-stored-procedure/60-event solution) despite being a genuine silent-data-loss bug where it occurs.

---

## Tier 3 — Low / structural / pure tech-debt (real, but low real-world consequence on their own)

### 16. Missing non-Create transitions on the 45 Create-only aggregates
**Category: Tech-debt / product-completeness.** Scope: 45 of 93 PlantSafety aggregates never grew
past Create — no Update, Close, Cancel, Archive, or status transition of any kind. `[Inv]`.
**Impact note**: for a genuine subset (append-only logs, simple markers) this may be entirely
intentional; for others (e.g., `MachineProcessMonitoring`'s missing Close/Resolve on a real anomaly
lifecycle, `ContractorSafety.IncidentTrackingPerformanceEvaluation`'s missing Close) it is a real
product gap. Ranked low here because it is a coverage gap, not a correctness or security defect —
Phase 9 should triage which of the 45 are intentional vs. genuinely incomplete.

### 17. Missing Archive on 85 of 93 aggregates (only 8 have it at all)
**Category: Tech-debt / product-completeness.** Scope: `Accident`, `Risk`, `Scorecard`, `SafetyPolicy`,
`PermitWorkflow`, `PermitTemplate`, `Dashboards.Dashboard`, `Operational.Alerts.Alert` are the only 8
with any Archive concept. `[Inv]`.
**Impact note**: subsumed in severity by gap #9 (silent-archive) for the aggregates that do have it;
for the 85 that don't, this is a completeness gap, not a defect.

### 18. Missing Cancel on 92 of 93 aggregates (the 1 exception is unreachable — see gap #8)
**Category: Tech-debt / product-completeness.** `[Inv]`. Subsumed by gap #8 for the one instance that
exists; otherwise a pure completeness gap with no confirmed real-world consequence, since most
aggregates have an equivalent "negative path" transition (Reject, Expire, Suspend) covering the same
real-world need under a different name.

### 19. No event carries the pre-transition (old) value — only the post-transition (new) one
**Category: Tech-debt (payload completeness).** Scope: essentially every non-Create event
solution-wide (~50+ read directly, no exceptions found) — e.g. `RiskScoredEvent` carries the new score
only, no baseline to diff against. `[EV-F15]`.
**Impact note**: lowest-ranked confirmed finding in the whole audit — it is moot today because nothing
consumes any event (gap #12), and would only become consequential the moment a consumer is built that
needs to compute a delta (e.g., "risk increased from Medium to High").

### 20. Cosmetic/dead artifacts: `ConcurrencyToken` dead column, `UpdatedBy` always reflecting `CreatedBy`, Scorecard's copy-paste exception message
**Category: Pure tech-debt.** `[SQL-G5]` (context), `[Inv]` (Scorecard). No confirmed functional
consequence for any of these — genuine code-quality items, lowest severity tier.

---

## Summary — gap count by severity tier

| Tier | Count | Gaps |
|---|---|---|
| Tier 0 — Critical | 3 | #1 (auth-fix sequencing trap), #2 (RLS never engages), #3 (zero correct actor propagation solution-wide) |
| Tier 1 — High | 5 | #4 (13+ zero-implementation repos), #5 (systemic tenancy gap), #6 (no Delete anywhere), #7 (AuditLog 22/93, best-effort), #8 (guarded-but-dead lifecycles) |
| Tier 2 — Medium | 7 | #9 (silent-archive ×4), #10 (Intelligence bypass), #11 (cross-module reference gaps ×4 confirmed), #12 (zero working consumers), #13 (duplicate-concept clusters), #14 (~30 missing validators), #15 (1 confirmed truncation) |
| Tier 3 — Low/tech-debt | 5 | #16 (45 Create-only), #17 (Archive missing on 85/93), #18 (Cancel missing on 92/93), #19 (no old-value in events), #20 (cosmetic artifacts) |
| **Total distinct, deduplicated gaps** | **20** | |

Every gap above is written once regardless of how many aggregates it touches (e.g. gap #2/#3/#5 each
span 40-90+ aggregates but are single line items here, exactly per this phase's instruction not to
list a 90-aggregate gap 90 times).
