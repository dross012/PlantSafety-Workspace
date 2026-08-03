# Rule Enforcement Validation — Azure

See `_shared-findings.md` and **G1** (Azure checked directly this phase for a PlantSafety-style tenancy
gap — none of that shape exists; recorded as N/A, not Pass, since there is no local tenant-scoped
aggregate to enforce a check on in the first place). No local aggregate/state machine exists anywhere —
state lives in Azure's own ARM control plane.

| Transition | Invariants Pass/Fail | Constraints Pass/Fail | Cross-Module Pass/Fail | Lifecycle-Sequence Pass/Fail | Tenancy Pass/Fail | Actor/Correlation Pass/Fail | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Container App (Compute) — Restart | **Fail** — no local precondition check; delegated entirely to Azure's control plane (inherited) | Unconfirmed | Pass — no PlantSafety-module reference | N/A — no local state machine to sequence-check; ARM itself is the source of truth for the resource's state | **N/A (G1)** — no PlantSafety-tenant-scoped record exists for this resource; the only "TenantId"-shaped concept found anywhere in `Azure/` is the Entra/Azure-AD tenant id on `ManagedIdentity`, a different concept from cross-tenant data isolation, and it is a property of a distinct resource (`Identity.Domain.Models.ManagedIdentity`), not of Compute/Data/ML/Eventing resources | Unconfirmed — actor/correlation propagation for ARM calls not re-verified this phase | Medium | |
| Cosmos Account (Data) — Update Throughput | Fail — same shape as Restart (inherited) | Unconfirmed | Pass | N/A | N/A (G1) | Unconfirmed | Medium | |
| Azure ML Training Job — Cancel | **Pass (by design)** — explicitly documented as idempotent-by-design, a stated design decision, not an oversight (inherited) | Unconfirmed | Pass | N/A — idempotency-by-design substitutes for a local sequence guard here | N/A (G1) | Unconfirmed | High | The one "no local guard" instance in the solution confirmed to be intentional rather than an omission |
| KeyVault Secret (Security) — Disable | Pass (by design) — same idempotency-by-delegation pattern as Cancel (inherited) | Unconfirmed | Pass | N/A | N/A (G1) | Unconfirmed | High | |
| Service Bus Topic (Eventing) — Purge Dead Letter | **Fail (unconfirmed, flagged)** — an irreversible, data-destroying operation not yet confirmed to have any precondition check (inherited; not re-read in full this phase — still open) | Unconfirmed | Pass | Unconfirmed — cannot rule out being callable regardless of topic/queue state without a full-body read | N/A (G1) | Unconfirmed | Low | Still the top open follow-up in this module, reiterated from the prior discovery, not resolved this phase either |
| Any Azure resource — Delete (hard-delete) | N/A — not found across 15 command handlers | N/A | N/A | N/A | N/A | N/A | Medium (not exhaustive) | Consistent with Azure's role as an operator surface, not a provisioning/deprovisioning tool |
