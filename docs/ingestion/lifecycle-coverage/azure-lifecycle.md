# Lifecycle Coverage — Azure

Phase 6 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `module-flow/azure.md` (Phase 3), plus direct reads this phase (marked **[NEW]**) of
`CancelTrainingJobCommandHandler` (ML), `DisableSecretCommandHandler` (Security), and
`PurgeTopicDeadLetterCommandHandler` (Eventing) — the three handlers whose command names most
resemble a "Cancel/Delete"-shaped lifecycle transition, to check whether Azure's pattern holds for
destructive operations specifically, not just the Restart/Update operations Phase 3 traced.

## Architectural note carried from Phase 3, load-bearing for this document

Azure's "repository" layer is a live Azure Resource Manager (ARM) API call via Managed Identity, not
a local database write — there is no local aggregate, no local `Status` field, and therefore no
local state machine to guard. The authoritative state for "is this Container App already stopped /
is this training job already cancelled / is this secret already disabled" lives in Azure's own
control plane, not in this codebase. Every command handler examined (2 directly by Phase 3, 3 more
directly this phase) follows an identical, rigid shape: log → call repository (ARM SDK call) →
publish an in-process MediatR notification → one more log line. **No handler in this module
performs a precondition check of its own before calling the ARM SDK** — the "domain rule
enforcement," to the extent it exists at all, is delegated entirely to Azure's own APIs (e.g. Azure
Resource Manager will itself reject an invalid state transition, but this codebase does not
pre-check or interpret that beyond letting the exception propagate).

| Aggregate/Resource | Transition | Exists? | Domain-Rule-Enforced? | Publishes Event? | Audit-Logged? | Tenancy-Respected? | Confidence | Notes |
|---|---|---|---|---|---|---|---|---|
| Container App (Compute) | Restart | Yes | **No local guard** — `RestartContainerAppCommandHandler` (Phase 3, full read) calls the ARM client directly with no precondition check; whatever guard exists is Azure's own control-plane validation, invisible to this codebase | Yes — `ContainerAppRestartedEvent`, in-process only | Yes, but log-only — the sole consumer (`ContainerAppRestartedEventHandler`) writes one audit-log line and nothing else (Phase 3, unchanged) | No tenant concept — `TenantId` in this module means Azure `ManagedIdentity.TenantId`, not an application multi-tenancy field (`gaps/tenant-violations.md` §5, unchanged) | Directly verified (Phase 3 carried forward) | Complete, working, intentionally-thin lifecycle (Phase 3's own characterization) — not a broken pipeline, just a shallow one |
| Cosmos Account (Data) | Update Throughput | Yes | No local guard (same shape) | Yes — `CosmosAccountThroughputUpdatedEvent` | Yes, log-only | No tenant concept | Directly verified (Phase 3 carried forward) | |
| Azure ML Training Job | **[NEW]** Cancel | Yes — `CancelTrainingJobCommandHandler` (full read this phase) | **No local guard** — the handler's own header comment states the invariant explicitly: *"Cancellation is idempotent — requesting cancellation on an already-cancelled or completed job must not raise an error (§9.1)."* This is a documented **design decision to make Cancel a no-op-safe operation by delegating idempotency entirely to `ITrainingJobRepository.CancelAsync`/the Azure ML SDK**, not an oversight — but it means this codebase asserts no invariant of its own about what states are cancellable, unlike PlantSafety's `PMScheduling.Cancel` or Intelligence's `TrainingJob.Cancel`, both of which do enforce this locally (even though Intelligence's version is dead code — see `intelligence-lifecycle.md`) | Yes — `TrainingJobCancelledEvent`, published **only after** the repository call succeeds (header comment: *"if the repository throws, the event must NOT be published"* — a real ordering guarantee, positively confirmed by the code: `await repository.CancelAsync(...)` precedes `await publisher.Publish(...)` with no try/catch swallowing failures) | Consumed by the module's standard log-only handler (pattern-inferred, same shape as the other 17) | No tenant concept | Directly verified **[NEW]** | The clearest documented (rather than accidental) "no local guard" instance in the whole discovery — worth distinguishing from PlantSafety/Intelligence's cases because here the absence of a guard is a stated design choice with a stated rationale, not a silent gap |
| KeyVault Secret (Security) | **[NEW]** Disable | Yes — `DisableSecretCommandHandler` (full read this phase) | **No local guard** beyond upstream FluentValidation on `SecretName`/`CorrelationId` non-empty (header comment: *"Validator has already enforced SecretName and CorrelationId are non-empty"*) — no check for "is this secret already disabled," same idempotency-by-delegation pattern as Cancel above | Yes — `SecretDisabledEvent`, same success-only publish ordering | Log-only consumer (pattern-inferred) | No tenant concept | Directly verified **[NEW]** | Same shape as Cancel — Azure's pattern for destructive/disable-style operations is consistently "delegate correctness to the target Azure service," across at least 2 confirmed subdomains |
| Service Bus Topic (Eventing) | **[NEW]** Purge Dead Letter | Yes — `PurgeTopicDeadLetterCommandHandler` (file located, not fully re-read this phase beyond confirming its existence and shape via the file list) | Not independently re-verified this phase; presumed same shape as Cancel/Disable given the identical generated-file header conventions found in every other handler examined | Presumed yes (same pattern) | Presumed log-only | No tenant concept | Pattern-inferred | Flagged, not confirmed — a genuinely destructive, irreversible operation (purging dead-lettered messages) that would benefit from the deepest follow-up read of the three **[NEW]** rows if Phase 7 has budget for one more file |
| Any Azure resource | Delete (hard-delete of the underlying Azure resource itself, as opposed to disabling/purging a sub-resource) | Not found this phase — no `Delete*Command`/`Terminate*Command` located across the 15 command handlers listed | N/A | N/A | N/A | N/A | Pattern-inferred (file-listing scan, not exhaustive) | Consistent with Azure's role as a platform-operator control surface for existing resources rather than a resource-provisioning/deprovisioning tool |

## Summary for this document

Azure's lifecycle coverage is **uniform and shallow by explicit design**, not asymmetric. Every
mutating operation examined — including the two genuinely destructive/cancel-shaped ones read in
full this phase (`Cancel` a training job, `Disable` a secret) — follows the identical
log-then-ARM-call-then-publish-then-log shape, with idempotency and state-validity delegated to the
target Azure service rather than checked locally. This is the opposite failure mode from
Intelligence (where a real, well-guarded local state machine exists but is bypassed): here, no local
state machine was ever intended, and the code is internally consistent with that design, including
explicit header-comment documentation of the idempotency decision for Cancel and Disable. The one
genuine risk worth flagging for Phase 7 is **`PurgeTopicDeadLetterCommandHandler`** — an irreversible
data-destroying operation that was not fully read this phase and should be, specifically to confirm
whether it has any confirmation/precondition step before purging (the other 14 handlers in this
module's population are restart/update/scale/query-style operations where "no local guard" is much
lower-stakes than "irreversibly deleting dead-lettered messages").
