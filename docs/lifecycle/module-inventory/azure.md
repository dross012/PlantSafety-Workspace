# Module Inventory — Azure

No local aggregate/state machine exists anywhere in this module (structural, by design — state lives
in Azure's own ARM control plane). All rows below are reformatted from
`docs/ingestion/lifecycle-coverage/azure-lifecycle.md` (directly verified for 5 handlers read in full
across the prior discovery's two passes); not independently re-verified this phase. "Exists in SQL?"
does not apply — this module's "repository" layer is a live ARM API call, not a database.

| Resource | Transition | Exists in C#? | Exists in SQL? | Legacy or Live? | Confidence | Notes |
|---|---|---|---|---|---|---|
| Container App (Compute) | Restart | Yes | N/A (ARM call, not SQL) | Live | Directly verified (inherited) | No local precondition check — delegated to Azure's control plane |
| Cosmos Account (Data) | Update Throughput | Yes | N/A | Live | Directly verified (inherited) | Same shape |
| Azure ML Training Job | Cancel | Yes | N/A | Live | Directly verified (inherited) | Explicitly documented as idempotent-by-design (header comment cites the invariant) — the one "no local guard" instance in the solution that is a stated design decision, not an oversight |
| KeyVault Secret (Security) | Disable | Yes | N/A | Live | Directly verified (inherited) | Same idempotency-by-delegation pattern as Cancel |
| Service Bus Topic (Eventing) | Purge Dead Letter | Yes (file located, not fully re-read) | N/A | Presumed live | Pattern-inferred (inherited) | An irreversible, data-destroying operation not yet confirmed to have any precondition check — flagged by the prior discovery as its top follow-up, still open |
| Any Azure resource | Delete (hard-delete of the underlying resource) | **No** — not found across 15 command handlers listed | N/A | N/A | Pattern-inferred (inherited, not exhaustive) | Consistent with Azure's role as an operator surface for existing resources, not a provisioning/deprovisioning tool |

**Every transition in this module follows the same shape**: log → ARM SDK call → in-process MediatR
notification (published only after the ARM call succeeds) → one log-only consumer. This is uniform
and shallow by explicit design, not asymmetric — there is no "rich vs. Create-only" contrast to draw
within this module.
