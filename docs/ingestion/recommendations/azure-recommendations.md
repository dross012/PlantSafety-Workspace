# Modernization Recommendations — Azure

Phase 9 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Azure is architecturally distinct from every other module: its "repository" layer calls the real
Azure Resource Manager control plane (Managed Identity, Polly-wrapped), not a local database — so
several categories this discovery applies elsewhere (SQL shape drift, RLS, outbox dormancy) simply
do not exist here. Azure also has the solution's best-wired authentication (real JWT,
`[Authorize]` on every controller). Its main gap is an intentional-looking one: audit-only event
consumers with no cross-module fan-out.

---

## Top 5 fixes, in priority order

1. **Confirm with the team whether the 18 log-only `INotificationHandler<T>` consumers are the
   intended final state, or a stepping stone that was never finished.** Several event doc comments
   explicitly say cross-module fan-out "can be added in future" — this is the one call in this
   document being made rather than a clear-cut bug. See "Is this acceptable by design?" below.
   Effort: N/A (a conversation, not code). Risk of not fixing: **None-just-tech-debt** if
   confirmed intentional; **availability/operational** (missing alerting on ARM-level actions) if
   not.
2. **Add actor identity capture** (a real `ClaimsPrincipal`/user-id claim, not just
   `CorrelationId`) to the audit-log write in each of the 18 event handlers. Effort: S per handler
   (M in aggregate, since it's 18 near-identical files). Risk of not fixing: **Security/audit-trail**
   (an authenticated caller can restart a Container App or purge a dead-letter topic and no code
   anywhere records *who*, beyond an opaque CorrelationId).
3. **Read `PurgeTopicDeadLetterCommandHandler` in full** (the one Azure handler in this discovery
   not yet fully verified) to confirm whether an irreversible, data-destroying operation has any
   confirmation/precondition step before purging. Effort: S (it's a single-file read + a decision
   on whether a guard is needed). Risk of not fixing: **Availability/data-integrity** (unconfirmed
   — this is the one Azure operation whose safety profile is still an open question, not a
   confirmed gap).
4. **Document the intentional "delegate idempotency/precondition-checking to the target Azure
   service" design decision** (already stated in `CancelTrainingJobCommandHandler`'s and
   `DisableSecretCommandHandler`'s own header comments) at the module level, not just per-handler,
   so it reads as a deliberate architecture choice rather than a gap in every future code review.
   Effort: S. Risk of not fixing: **None-just-tech-debt** (a documentation clarity item, not a
   functional gap).
5. **No urgent tenant/RLS work needed** — Azure has no application-level multi-tenancy concept by
   design (it's a platform-operator control surface, not a multi-tenant data surface), so unlike
   DataLake/Intelligence/Reporting there is nothing to bypass here. Recorded as a confirmed
   non-finding, not a recommendation.

---

## Is "audit-only consumers" acceptable by design, or a gap?

**This document's judgment: acceptable as currently scoped, with one caveat.** Azure's 18 event
handlers are the *only* category in the entire solution-wide discovery where a published event
actually reaches running consumer code (`events/publishing-map.md`) — every other module's events
either have no consumer, an unregistered consumer, or a `NullOutboxPublisher`-style silent
swallow. Azure's handlers genuinely execute and genuinely write a real (if trivial) log line, with
an explicit, consistent, per-file documented invariant ("never throws," "log-only by design").
This reads as a deliberately thin, intentionally-scoped design — not a broken pipeline — and every
handler examined is internally consistent with that design.

**The caveat**: the event doc comments (e.g. `ContainerAppRestartedEvent`'s) explicitly say
cross-module fan-out "can be added in future" — meaning the current state was understood at
authoring time as a stepping stone, not necessarily the final intended shape. Whether "future" has
already arrived (i.e., whether product/ops now wants real alerting on Container App
restarts/Cosmos throughput changes/Key Vault secret disables) is a product decision this document
cannot make from static analysis alone. **Recommendation: ask the team directly** whether
operational alerting on these 18 event types is wanted now; if yes, that's the next real
engineering investment (wire to a real alerting/notification channel, not just the log stream); if
no, close the loop by removing the "future work" language from the doc comments so it stops
reading as an open TODO.

---

## Security / Availability

### 1. No actor identity captured anywhere in the Azure event chain
- **Problem**: Every one of Azure's 18 `INotificationHandler<T>` classes writes a structured log
  line containing `CorrelationId` but never a "who" — no `ClaimsPrincipal`/`HttpContext.User`
  usage exists anywhere in the Azure module (confirmed by grep). An authenticated caller can
  restart a Container App, purge a dead-letter topic, or disable a Key Vault secret, and nothing
  in the codebase records which authenticated identity did it, only an opaque tracing GUID.
  (`classification-matrix.md` §Azure, `gaps/tenant-violations.md` §5.)
- **Fix**: Thread the authenticated user's identity (already available via the JWT bearer
  middleware that's genuinely wired here, unlike DataLake/Intelligence/Reporting) from the
  controller through the command to the event, and include it in each handler's log line. This is
  a much smaller fix than the equivalent gap elsewhere in the solution, because the authentication
  layer already exists here — it's purely a matter of actually reading the claim and passing it
  through.
- **Effort**: M (18 near-identical files, but each change is small and mechanical once the first
  is done as a template).
- **Risk of not fixing**: **Security/audit-trail** — genuine gap, though lower urgency than the
  DataLake/Intelligence/Reporting authentication gaps, since Azure at least has a real identity to
  capture; it's just not being captured.

---

## Availability / Operational

### 2. `PurgeTopicDeadLetterCommandHandler` not yet fully verified for a precondition check
- **Problem**: This is the one irreversible, data-destroying Azure operation (purging
  dead-lettered messages) whose handler body was not fully read in this discovery (file located
  and its existence/shape confirmed, but not read line-by-line the way `CancelTrainingJobCommandHandler`
  and `DisableSecretCommandHandler` were). The other 14 Azure command handlers in this population
  are restart/update/scale/query-style operations where "no local guard" is much lower-stakes than
  irreversibly deleting dead-lettered messages. (`lifecycle-coverage/azure-lifecycle.md` summary.)
- **Fix**: Read the handler in full; confirm whether it has any confirmation, dry-run, or
  precondition check before purging. If not, and if the underlying Azure Service Bus API doesn't
  itself provide an undo/soft-delete window, consider adding an application-level confirmation
  step (e.g. requiring a `confirm=true` flag or a dry-run preview endpoint) given the operation is
  irreversible.
- **Effort**: S (read) + S-M (add a guard, if warranted by what the read finds).
- **Risk of not fixing**: **Availability/data-integrity** (unconfirmed severity — this is a
  genuine open question, not a confirmed defect, and is exactly the kind of gap worth closing
  before it matters).

---

## Deliberately not recommended

- **Not recommending building a local database/cache mirror of ARM resource state** — Phase 5's
  shape audit correctly identifies that Azure's architecture has no DTO→SQL seam to audit today,
  and building one speculatively (to enable a shape-drift audit that doesn't currently apply)
  would be exactly the kind of unnecessary new abstraction the task asked not to introduce. Only
  revisit if a future requirement (e.g. offline reporting, avoiding live ARM calls on every read)
  independently justifies it.
- **Not recommending tenant/RLS work for Azure** — confirmed non-finding; Azure has no
  application-level multi-tenancy concept, so there is nothing to enforce or bypass at that layer.
- **Not treating the log-only consumer pattern itself as a defect to fix reflexively** — see "Is
  this acceptable by design?" above; the correct next step is a product conversation, not an
  engineering change made unilaterally.
