# Recommendations — Observability

Top 5 fixes, in priority order, with sequencing dependencies called out explicitly. All 5 aggregates
here are in the 23-aggregate worst-case actor-attribution cluster, and this module holds one of only
4 confirmed (not merely suspected) cross-module-reference gaps in the entire solution — that item
takes priority over this module's share of the generic cross-cutting work.

1. **Fix `AutomatedIncidentDetection.EscalateToIncident`'s unverified cross-module reference.**
   `IncidentReferenceId` is a raw caller-supplied Guid stored on faith — no repository call verifies
   the referenced Incident exists, no status check, no write-back. This is a confirmed, not
   suspected, cross-module gap — direct evidence the pattern worried about across the solution's 48
   rich aggregates is real. Fix: add a repository dependency on the Incident-tracking module (whether
   that's `AccidentManagement.Accident` or a dedicated IncidentManagement concept — resolve which is
   canonical first, see item 3 below), verify the referenced record exists and is in an appropriate
   status before allowing the escalation to complete, and consider writing back a
   confirmation/acknowledgment. Cite: `gaps/_severity-ranking.md` #11,
   `domain-mapping/_shared-findings.md` F11, `gaps/observability.md`. Effort: M (adds a second
   repository dependency, same architectural pattern as the `AuditEngagement.Close` fix in
   `auditing.md`). Risk: **Data integrity** — an escalation can reference a nonexistent or
   already-closed incident today with nothing able to detect it. Sequencing: none, though resolving
   item 3 first clarifies which module's repository to depend on.

2. **Add a Close/Resolve transition to `MachineProcessMonitoring`** — this aggregate genuinely grew
   past Create (has `Acknowledge`/`RaiseAnomalyFlag`/`RecordSensorReading`) but has no confirmed way
   to ever mark a raised anomaly flag as resolved. Unlike the 45 genuinely Create-only aggregates
   elsewhere in the solution, this is a real, product-relevant lifecycle gap, not a completeness
   nice-to-have — an anomaly that can be flagged but never resolved is an operational dead-end for
   whoever's tracking it. Fix: add a guarded `Close`/`Resolve` method following this module's
   existing guard patterns (require `Acknowledged` status, capture resolution notes). Cite:
   `gaps/_severity-ranking.md` #16, `gaps/observability.md`. Effort: S-M. Risk: **Availability/
   operational** — flagged anomalies genuinely cannot be closed out today. Sequencing: none.

3. **Apply XC-1 (tenancy + RLS engagement) to all 5 aggregates** — pattern-inferred classic
   repository shape across the cluster (all 5 named explicitly in the 23-aggregate audit-logged
   population). Cite: `domain-mapping/_shared-findings.md` F1, `gaps/observability.md`. Effort: part
   of XC-1's solution-wide pass. Risk: Critical (see XC-1). **Sequencing: must land before XC-3
   (auth-policy registration) proceeds for this module's endpoints.**

4. **Apply XC-2 (actor/correlation propagation) to all 5 aggregates** — the named-23 cluster hit by
   both the C#-outbox hardcode and the SQL `SYSTEM_USER` bug simultaneously. Cite:
   `security/actor-propagation.md` §2a, `gaps/observability.md`. Effort: part of XC-2's pass. Risk:
   Critical (see XC-2). Sequencing: none — independent, run any time.

5. **Wire the 3 dormant `_Update` stored procedures** (`EnvironmentalAssetMonitoring`,
   `FacilityTrafficMonitoring`, `PPEBehavioralMonitoring`) if these Create-only aggregates are
   actually meant to support updates — the SQL already exists and is simply unused, a much smaller
   lift than XC-4's zero-SQL cluster. Fix: confirm with product whether update capability is needed
   for these 3 monitoring types; if yes, add the domain-layer `Update` method and command handler to
   call the existing dormant proc rather than writing new SQL. Cite: `gaps/observability.md`,
   `gaps/_severity-ranking.md` #16. Effort: S per aggregate if confirmed needed (SQL already exists).
   Risk: Product-completeness gap, not a defect. Sequencing: none.

See `_cross-cutting.md` for XC-1 (tenancy+RLS, required before XC-3) and XC-2 (actor propagation).
