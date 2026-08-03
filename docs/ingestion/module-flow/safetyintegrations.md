# Module Flow — SafetyIntegrations

Phase 3 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.

## Status: designed-but-unwired boundary — no live ingress path

Per Phase 1 (`inventory.md`) and Phase 2 (`classification-matrix.md`), SafetyIntegrations has zero
API controllers or minimal-API endpoints, zero scheduled-job/hosted-service classes, and zero file
ingestion adapters. It does have a substantial custom messaging abstraction — an `IEventBus` /
`IEventBusMessageHandler` layer (`SafetyIntegrations\SafetyIntegrations\EventBus\...`, ~29 files:
dispatcher, dispatch policies, backoff policies, idempotency policies, pipeline middleware) — and
a single governed inbound boundary interface, `IInboundAdapter`
(`SafetyIntegrations\SafetyIntegrations\Adapters\Inbound\IInboundAdapter.cs`), implemented by
`EnterpriseGatewayInboundAdapter.cs`, whose own header comment describes it as "the single
governed entry point through which an external system or legacy module surface may present data
to SafetyIntegration." No concrete caller or transport wiring this adapter to an actual HTTP
listener, queue subscription, or scheduled poll was found anywhere in the solution in Phase 1, and
this Phase 3 pass did not find one either.

Because there is no live caller invoking `EnterpriseGatewayInboundAdapter` (or any
`IEventBusMessageHandler`), this module is excluded from the flow-map proper: there is no
controller/consumer/job class actually receiving an inbound request today, so no domain model is
touched, no lifecycle transition occurs, no repository/DbContext is written to, and no event is
published or consumed as part of any live ingress path. The building blocks (dispatcher, retry/
backoff/idempotency policies, the governed adapter interface) are real and reasonably complete —
this reads as infrastructure built ahead of a caller that was never connected, rather than
abandoned or vestigial code — but until something invokes `IInboundAdapter.ReceiveAsync` (or
equivalent), there is nothing to trace. Phase 1 flagged this as its single biggest open question
for cross-module ingestion; this phase did not find new evidence resolving it either way.
