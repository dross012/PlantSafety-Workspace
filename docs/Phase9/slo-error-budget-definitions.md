# SLO / Error-Budget Definitions -- Phase 9 §4 Observability & Telemetry Expansion

## Status

No SLO/error-budget concept existed anywhere in this codebase prior to this document. Phase 6/7
built `RpoRtoOptions`/`RegionalRpoRtoTarget` for **disaster recovery** (how much data/time is lost
in a regional outage) -- a real, adjacent, but distinct concept from an **availability/latency
SLO** (how the platform performs during normal operation). This document is genuinely new
conceptual work, not a code-gap fix, per the Phase 9 §4 task scope.

Every SLO below is grounded in a metric this platform **already collects today** -- no new
collection pipeline is proposed. Where a genuine collection gap existed (residency-check
aggregate telemetry, outbound integration latency), those gaps were closed as part of this same
Phase 9 §4 pass (see `PlantSafety.Application/Enterprise/MultiRegion/DataResidency/DataResidencyEnforcer.cs`
and `PlantSafety.Infrastructure/Messaging/EnterpriseMesh/HttpExternalSystemPublisher.cs`) so the
SLOs proposed here are measurable from day one, not aspirational.

## 1. API latency SLO

**Source of truth:** `[PlatformHealth].[RequestMetricEvent]`, aggregated per tenant/hour by
`usp_PlatformHealthDashboard_GetSummary` (P50/P95/P99 via `PERCENTILE_CONT`). Populated by
`RequestLoggingMiddleware` -> `IRequestMetricsRecorder` -> `ChannelRequestMetricsRecorder` ->
`RequestMetricsCaptureService`, covering every non-`/health*` HTTP request platform-wide
(verified in this pass to include the Phase 8 Global Safety Dashboard's 3 endpoints -- no
controller-specific bypass exists; `RequestLoggingMiddleware` is a single global pipeline stage).

| SLO | Target | Measurement window | Error budget |
|---|---|---|---|
| API P95 latency | P95LatencyMs <= 500ms | Rolling 30 days, evaluated per business hour bucket | 5% of hourly buckets may exceed 500ms P95 before the budget is exhausted for the window |
| API P99 latency | P99LatencyMs <= 2000ms | Rolling 30 days, per business hour bucket | 1% of hourly buckets may exceed 2000ms P99 |
| API availability (error rate) | ErrorRate (StatusCode >= 500) <= 0.1% | Rolling 30 days, per business hour bucket | 99.9% of requests succeed (non-5xx); the remaining 0.1% is the error budget |

**Rationale for thresholds:** 500ms/2000ms mirror the existing, already-enforced
`PerformanceBehavior.SlowRequestThresholdMs` defaults (500ms commands / 200ms queries) scaled up
to a full HTTP-request-inclusive P95/P99 (which also includes auth, routing, and serialization
overhead the MediatR-pipeline-only `PerformanceBehavior` threshold does not measure) -- not an
arbitrary industry number. 99.9% mirrors the `ErrorRate` column already computed by
`usp_PlatformHealthDashboard_GetSummary`; a stricter number is not proposed because this
platform's real error-rate baseline has not yet been observed over a full 30-day window.

**Burn-rate alerting (proposed, not yet implemented):** a fast-burn alert (e.g. 2% of the 30-day
P95 budget consumed in 1 hour) and a slow-burn alert (e.g. 10% of the 30-day error-rate budget
consumed in 6 hours) against the same `RequestMetricEvent` aggregation, following the standard
Google SRE multi-window burn-rate pattern -- flagged as a follow-up, not built in this pass, since
it requires an alerting/paging integration that does not yet exist in this codebase.

## 2. Event-mesh consumer reliability SLO

**Source of truth:** `[System].[DeadLetterEvents]`, queried via
`usp_DeadLetterEvents_GetCountByEventType` (count + oldest/newest fault age + avg retry count per
EventType, tenant-scoped, lookback-windowed). Populated by `DeadLetterFaultObserver` whenever a
MassTransit consumer's retry policy (`cfg.UseMessageRetry`) is exhausted.

| SLO | Target | Measurement window | Error budget |
|---|---|---|---|
| Event-mesh dead-letter rate | Dead-lettered events <= 0.5% of published events per EventType | Rolling 7 days (`@LookbackDays = 7`, the procedure's own default) | 0.5% of events for a given EventType may permanently fail delivery before the budget is exhausted |
| Dead-letter age (undetected backlog) | OldestAgeHours <= 24h for any EventType with a non-zero DeadLetterCount | Continuous (evaluated on every dashboard/alert poll) | A dead-lettered event sitting unreplayed/untriaged for more than 24h is itself the budget-exhaustion signal, independent of volume |

**Rationale:** there is no existing "published event count" denominator table to compute a precise
percentage against (MassTransit does not currently emit a queryable per-EventType publish-count
metric in this codebase), so the dead-letter *rate* SLO is directionally correct but its
denominator is a known measurement gap -- flagged here explicitly rather than presented as more
precise than it is. The dead-letter *age* SLO has no such gap: `OldestFaultedAt`/`OldestAgeHours`
are exact, already-computed columns.

## 3. Global Safety Dashboard query latency SLO

**Source of truth:** the same `[PlatformHealth].[RequestMetricEvent]` table as SLO §1, filtered to
`Path LIKE '/api/v%/global-safety-dashboard/%'` -- the dashboard's 3 endpoints (`summary`,
`detail`, `cross-site-actions`) are ordinary MediatR-routed controller actions captured by the
same global `RequestLoggingMiddleware`, verified in this pass to have no bypass.

| SLO | Target | Measurement window | Error budget |
|---|---|---|---|
| Dashboard query P95 latency | P95 ElapsedMs <= 1500ms for Path LIKE '/api/v%/global-safety-dashboard/%' | Rolling 30 days | 5% of dashboard requests may exceed 1500ms |

**Rationale:** 1500ms (3x the general API P95 target) reflects that this endpoint aggregates
across the multi-site `GlobalSafetyInsightsAggregate` (KPI computation, anomaly detection,
cross-site coordination) rather than a single-entity CRUD read -- a materially different workload
shape than the platform-wide API SLO in §1, not a weaker target chosen arbitrarily.

## 4. Data-residency enforcement health (monitoring signal, not a traditional SLO)

**Source of truth:** the `plantsafety.data_residency.checks_total` counter added in this pass
(`DataResidencyEnforcer`, tagged by `result=satisfied|violation`), exported via the OpenTelemetry
metrics pipeline (`TracingServiceRegistration.AddPlantSafetyTracing`'s new `WithMetrics()`
registration, `AddMeter("PlantSafety.*")`).

| Signal | Target | Rationale |
|---|---|---|
| Residency-check counter non-zero over any 1-hour window with active tenant traffic | Counter increasing | Not a percentage SLO -- a residency violation should be rare-to-nonexistent by design (policy misconfiguration or a genuine cross-region routing bug), so the actionable signal is "is enforcement running at all," not "what fraction of checks pass." A flatlined counter during active traffic is the alarm condition, not a low pass-rate. |

This is intentionally NOT modeled as a percentage-based SLO/error-budget: per Handbook §2.27
Anti-Noise guidance and the judgment applied in the Phase 9 §4 audit itself, a residency violation
is a rare, individually-audited security/compliance event (already loudly logged per-incident at
every call site), not a load-bearing availability metric where a small ongoing failure rate is
expected and budgeted for.

## Explicitly out of scope for this pass

- **Alerting/paging integration.** No alerting system (PagerDuty, Opsgenie, Azure Monitor
  Alerts, etc.) exists in this codebase today. The burn-rate thresholds above are proposed
  targets for such a system, not wired to one.
- **A precise event-mesh publish-count denominator.** Flagged as a known gap in SLO §2, not
  fixed here (would require new per-EventType publish counters, which is a larger, separate
  observability build-out beyond "close genuine gaps").
- **MAUI/mobile client-side SLOs.** `PlantSafety.UI` is a MAUI client; this document covers only
  the server-side surfaces this platform's real, existing metrics pipelines already observe.
