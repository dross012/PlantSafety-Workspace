# Rule Enforcement Validation — AccidentManagement

See `_shared-findings.md` (G1-G6). 1 aggregate (`Accident`), 8 real transitions + Delete/Cancel absent.

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Create (Report Accident) | Pass — required fields guarded | Pass (validator confirmed, inherited) | Pass — no other-module reference at Create | Pass (no predecessor state) | **Fail (F1/G1)** — `GetByIdAsync` keys off `AccidentId` alone | **Fail (G2, dual)** — repo hardcodes `"system"`/`Guid.Empty` at `outboxWriter.Enqueue`; SQL `AuditLog.ChangedBy=SYSTEM_USER` is a separate, second bug on the same row | High | Audit-logged (F8); both G2 bugs apply |
| AssignInvestigator (Reported→UnderInvestigation) | Pass — status guard | Pass (inherited) | Pass | Pass — guarded predecessor state | Fail (F1) | Fail (G2, dual) | High | |
| RecordRootCause (UnderInvestigation→RootCauseIdentified) | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2, dual) | High | |
| CreateCorrectiveAction | Pass | Pass (inherited) | Pass — CAPA is a child entity of this same aggregate, not a genuine other-module call | Pass | Fail (F1) | Fail (G2, dual) | High | |
| CompleteCorrectiveAction | **Fail** — does not re-check parent `Accident.Status` before completing a CAPA (inherited finding, reconfirmed) | Pass (inherited) | Pass | **Fail** — a CAPA can be completed regardless of whether the parent Accident has since moved to a terminal/inconsistent state; this is the closest thing to a forceable out-of-order sequence found in this feature area, though it is a child-entity gap, not a full illegal-status transition on `Accident` itself | Fail (F1) | Fail (G2, dual) | High | Flag for remediation: add parent-status guard |
| FileRegulatoryReport | Pass | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2, dual) | High | |
| CloseInvestigation | **Pass** — enforces `RegulatoryFilingRequired` before Close, a real cross-field invariant (best-guarded transition in the solution, inherited) | Pass (inherited) | Pass | Pass | Fail (F1) | Fail (G2, dual) | High | Positive control — model for what a real invariant check looks like |
| Archive (Closed→Archived) | Pass — status-guarded | Pass (inherited) | Pass | Pass | Fail (F1) | N/A — never calls `AddDomainEvent` (silent archive, inherited), so no Enqueue call exists to be right or wrong | High | Silent-archive: guarded but no event/outbox row at all |
| Delete / Cancel | N/A — transition does not exist | N/A | N/A | N/A | N/A | N/A | High | Absence appears appropriate for a regulatory record |
