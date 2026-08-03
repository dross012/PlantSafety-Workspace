# Domain Mapping — Azure

Not independently re-verified this phase — inherited from the prior Ingestion Discovery. No local
aggregate/tenancy model exists (state lives in Azure's ARM control plane), so F1 (PlantSafety
tenancy gap) does not directly apply in the same shape here — see the Tenancy column per row.

| Resource | Transition | Invariants (stated) | Enforced? | Validation Rules | Cross-Module Rules | Tenancy Rules | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Container App (Compute) | Restart | No local precondition check — delegated to Azure's control plane (Phase 1 finding) | N/A by design — Azure ARM enforces its own state machine | Not independently confirmed | None | Azure subscription/RBAC scoping presumed to be the tenancy boundary, not independently confirmed this phase | Directly verified (inherited) | |
| Cosmos Account (Data) | Update Throughput | Same shape | N/A by design | — | None | Same as above | Directly verified (inherited) | |
| Azure ML Training Job | Cancel | Explicitly documented as idempotent-by-design (header comment cites the invariant) — Phase 1 finding | **Y as a stated design decision**, not an oversight — the one "no local guard" instance that is intentional | — | None | Same as above | Directly verified (inherited) | Positive control: an explicitly-reasoned absence, not a gap |
| KeyVault Secret (Security) | Disable | Same idempotency-by-delegation pattern | Y (by design) | — | None | Same as above | Directly verified (inherited) | |
| Service Bus Topic (Eventing) | Purge Dead Letter | Irreversible, data-destroying — no precondition check confirmed (Phase 1's top follow-up, still open) | **Unconfirmed — not verified this phase either** | Not confirmed | None | Same as above | Pattern-inferred (inherited) | Still the top open item in this module; not resolved by either phase |
| Any Azure resource | Delete | N/A — not found across 15 handlers | N/A | N/A | N/A | N/A | Pattern-inferred (inherited, not exhaustive) | Consistent with Azure's operator-surface role |

**Summary:** `Service Bus Topic.Purge Dead Letter` remains the single highest-risk unconfirmed
transition in this module — an irreversible, data-destroying operation whose precondition-check
status is still open after two phases of review.
