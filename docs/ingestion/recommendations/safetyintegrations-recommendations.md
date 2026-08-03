# Modernization Recommendations — SafetyIntegrations

Phase 9 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
SafetyIntegrations has a substantial custom messaging abstraction (~29 files: `IEventBus`/
`IEventBusMessageHandler` dispatcher, backoff policies, idempotency policies, pipeline middleware)
and a single governed inbound boundary interface (`IInboundAdapter` /
`EnterpriseGatewayInboundAdapter`) — built ahead of a caller that was never connected. This
document makes a different call than ExportImport's: **formally deprecate or explicitly shelve
this one**, not wire it up speculatively. See justification below.

---

## Top 5 fixes, in priority order

1. **Decide and act: do not wire this up speculatively — either find the concrete external system
   it's meant to integrate with, or formally shelve it.** See "Wire up or deprecate?" below.
   Effort: N/A (a decision) — but blocking everything else.
2. **If a concrete external system/caller is identified**, wire `EnterpriseGatewayInboundAdapter`
   to a real transport (HTTP listener, queue subscription, or scheduled poll) as the very next
   step — the dispatcher/policy infrastructure behind it appears reasonably complete and shouldn't
   need to be rebuilt, just connected. Effort: M (transport wiring) once a concrete need exists.
3. **If no concrete external system is identified within a reasonable window**, formally mark the
   module as shelved/deprecated: document the decision in the module's own header comments/README,
   and consider whether the ~29-file `IEventBus` abstraction should be archived or removed to stop
   it from being mistaken for live infrastructure in future audits. Effort: S.
4. **Do not build a duplicate of PlantSafety's real MassTransit+RabbitMQ setup here** — if
   SafetyIntegrations is ever wired up, evaluate reusing the solution's one genuine working message
   broker (once PlantSafety's own consumer-registration gap is fixed — see PlantSafety's
   recommendations doc) rather than building a second, parallel eventing abstraction for the
   solution to maintain. Effort: N/A (an architecture decision to make if/when item #2 happens).
5. **Whichever path is chosen, resolve it explicitly** — the worst outcome is what exists today:
   ~29 files of real, non-trivial code with no caller and no documented decision about its future,
   which is exactly the kind of thing a future engineer might either delete by mistake (if it's
   actually still wanted) or extend by mistake (if it's actually dead). Effort: S (the
   documentation/decision-recording itself).

---

## Wire up or deprecate? This document's call: do not wire up speculatively; get a concrete need first.

**Justification, contrasted directly with ExportImport's "wire it up" call**: ExportImport fills a
concrete, provable solution-wide gap (zero working file-upload path anywhere in 14,090 files) with
adapters that are individually testable and directly useful the moment any single caller exists —
a controller can be added and importing tested end-to-end with real CSV/Excel data immediately.

SafetyIntegrations is different in kind, not just degree: its own header comment describes
`EnterpriseGatewayInboundAdapter` as "the single governed entry point through which an external
system or legacy module surface may present data to SafetyIntegration" — but **no external system
was identified anywhere in this discovery that this adapter is meant to receive from.** Unlike
ExportImport (where the missing piece is simply "add a controller that calls the existing,
self-contained pipeline"), wiring SafetyIntegrations up requires knowing *which* external system,
*what* transport (HTTP? queue? scheduled poll?), and *what* message shape it would send — none of
which this discovery found any evidence of. Building a controller/listener for this module without
that information would mean guessing at an integration contract with no real counterparty to
validate it against — a worse outcome than leaving it explicitly shelved.

**Recommendation**: treat this as a documentation/decision problem first, an engineering problem
second. Ask the team: is there a known, named external system (or "legacy module surface," per the
adapter's own description) this was built for? If yes, that answer determines the wiring work
(item #2). If the answer is "not currently, it was speculative," the responsible action is to
document that explicitly and mark the module shelved (item #3) — not to leave ~29 files of
reasonably complete, non-trivial code in an permanently-ambiguous state.

---

## Event Wiring / Infrastructure

### 1. `IEventBus`/`IEventBusMessageHandler` abstraction — built, never hosted, no live caller
- **Problem**: ~29 files (dispatcher, dispatch policies, backoff policies, idempotency policies,
  pipeline middleware) under `SafetyIntegrations\SafetyIntegrations\EventBus\...` form a
  substantial custom messaging abstraction. Nothing hosts or dispatches it, and no live caller
  invokes `IEventBus.Publish`/equivalent anywhere in the solution. (`gaps/missing-events.md` §2c,
  `module-flow/safetyintegrations.md`.)
- **Fix**: Covered by the module-level decision above (Top-5 #1-3) — this infrastructure's fate
  should be decided as a unit with the inbound adapter, not patched independently.
- **Effort**: N/A pending the decision.
- **Risk of not fixing**: **None-just-tech-debt** — dormant, not broken, but represents real
  unused engineering investment and an ambiguous-intent liability for future audits/maintainers.

### 2. `EnterpriseGatewayInboundAdapter` — the one designed external-boundary implementation, no caller
- **Problem**: Implements `IInboundAdapter`, described in its own header comment as the module's
  single governed entry point for external/legacy data. No concrete caller or transport wiring
  this adapter to an actual HTTP listener, queue subscription, or scheduled poll was found in any
  phase of this discovery. (`inventory.md` §2/§6, `module-flow/safetyintegrations.md`.)
- **Fix**: As above — contingent on identifying (or ruling out) a concrete external system this
  was built for.
- **Effort**: M if a real integration target is identified and wiring proceeds; S if the decision
  is to shelve/document instead.
- **Risk of not fixing**: **None-just-tech-debt** today, same as item #1 — this and item #1
  should be decided and resolved together, not separately, since they're two halves of the same
  designed-but-unconnected boundary.

---

## Deliberately not recommended

- **Not recommending building a controller or transport listener for `IInboundAdapter` without a
  named external-system requirement.** Unlike ExportImport, this module's entire value proposition
  is contingent on a specific external counterparty that this discovery could not identify — building
  speculative wiring here risks guessing at an integration contract nobody has validated, which is a
  worse outcome than leaving the module explicitly shelved pending a real requirement.
- **Not recommending merging this module's `IEventBus` abstraction with PlantSafety's MassTransit
  setup as an immediate action** — that's a legitimate architecture question worth asking *if and
  only if* SafetyIntegrations is reactivated (Top-5 #4), not a change to make while the module
  remains unconnected to anything.
