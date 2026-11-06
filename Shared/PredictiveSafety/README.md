# Shared.PredictiveSafety — Phase 15 (Predictive Safety Engine)

A deterministic, ML-free inference subsystem that predicts hazard, maintenance, routing,
scheduling, and audit outcomes by composing real, existing rule engines (Facility's Phase 4
inference rules, Routing's Phase 13 `EventRoutingResult`, Scheduling's Phase 14
`SchedulingRuleDecision`) with the same decorator-chain/authorization/tenancy/telemetry/governance
conventions every phase since Phase 8 has used.

## Why "prediction" here means something specific

Every rule engine in this module is a pure, deterministic function: fixed threshold comparisons
and additive rule-code accumulation, never a weighted score, never an average, never a model. A
"prediction" is a same-input-same-output classification over already-known signals — not a
forecast derived from historical training data. This is a hard constraint from the phase's own
prompt (no ML, no probabilistic inference, no statistical anomaly detection), and it is enforced
structurally: every `RuleEngine.Evaluate` and `InferenceEngine.Infer` method in this module takes
a plain record of already-known values and returns a plain record — no `Random`, no external
model file, no statistical library dependency anywhere in the two projects.

## Layout

```
Shared/PredictiveSafety/
  Shared.PredictiveSafety.Abstractions/
    Common/PredictivePriority.cs           -- Low/Medium/High/Critical, fixed threshold classification
    Common/PredictiveEscalation.cs         -- PredictiveEscalationOutcome + PredictiveEscalationClassifier,
                                               shared across all five domains
    Hazard/        (Signals, Decision, RuleEngine, InferenceEngine)
    Maintenance/    (Signals, Decision, RuleEngine, InferenceEngine)
    Routing/        (Signals, Decision, RuleEngine, InferenceEngine)
    Scheduling/     (Signals, Decision, RuleEngine, InferenceEngine)
    Audit/          (Signals, PredictiveAuditAnomaly, RuleEngine, InferenceEngine)
    Contracts/      (PredictiveSafetyContractMetadata/Request/Result, Dto, Errors)
    Governance/PredictiveGovernanceRules.cs -- pure governance validation
  Shared.PredictiveSafety.Infrastructure/
    Orchestration/PredictiveSafetyOrchestrator.cs           -- the one public entry point
    Orchestration/PredictiveSafetyEvaluationPipeline.cs     -- fixed Hazard->Maintenance->Routing->Scheduling->Audit composition
    Orchestration/PredictiveSafetyEventEmitter.cs           -- unified event schema publication
    Authorization/PredictiveAuthorization.cs                -- Phase 9 integration
    Authentication/PredictiveIdentity.cs                    -- Phase 10 integration
    Tenant/PredictiveTenant.cs                               -- Phase 11 + Phase 12 integration
    Telemetry/{PredictiveTelemetry,PredictiveMetrics,PredictiveTracing}.cs -- Phase 8 integration
    Governance/{PredictiveGovernanceEvaluator,PredictiveGovernanceEvents}.cs
    DependencyInjection/PredictiveSafetyServiceCollectionExtensions.cs
  Shared.PredictiveSafety.Tests/
    ... one test class per file above, mirroring Shared.Scheduling.Tests' structure
```

## Each domain, in one line

- **Hazard** — `PredictiveHazardInferenceEngine` calls Facility's real, unchanged Phase 4 rules
  (`HazardInferenceRules.InferHazardTypeFromSafetyEvent`, `SeverityInferenceRules.Infer`,
  `RegulatoryInferenceRules.Infer`) to determine *what* is predicted, then
  `PredictiveHazardRuleEngine` decides whether cross-module signals (Routing audit triggers,
  Scheduling immediate-action flags, reliability failure counts) require immediate escalation.
- **Maintenance** — escalates on `MachineMaintenanceProfile.IsOverdue` (Phase C.2, unchanged),
  repeated failure counts, and a near-term Scheduling due time.
- **Routing** — escalates on `EventRoutingResult.TriggeredAudits`/`ChangedComplianceElements`
  (Phase 13, unchanged) plus the upstream Hazard/Maintenance predictions.
- **Scheduling** — escalates on `SchedulingRuleDecision.WorstCaseDueWithinHours`/
  `TotalRequiredActionCount` (Phase 14, unchanged) plus the upstream Hazard/Maintenance/Routing
  predictions.
- **Audit** — fixed-threshold counts over `LogEvent` volume/severity/governance-violation counts
  (see the honest substitution note below), never a statistical measure.

`PredictiveSafetyEvaluationPipeline.EvaluateAsync` runs these five in that fixed order and threads
each decision's escalation flag forward into every later domain — this is how "consume hazard and
maintenance predictions" (the Routing/Scheduling requirements) is satisfied without any domain
re-deriving another domain's decision.

## The decorator chain

Identical in shape to `SchedulingOrchestrator` (Phase 14):

```
PredictiveSafetyOrchestrator.EvaluateAsync
  -> PredictiveTenant.EvaluateWithTenantEnforcementAsync      (Phase 11 tenant isolation + Phase 12 contract governance)
    -> PredictiveAuthorization.EvaluateWithAuthorizationAsync (Phase 9 authorization check, "PredictiveSafety.Read")
      -> PredictiveSafetyEvaluationPipeline.EvaluateAsync     (the fixed five-domain composition)
        -> PredictiveTelemetry.Infer*WithTelemetryAsync       (Phase 8 reliability/logging/metrics, one per domain)
          -> Predictive*InferenceEngine.Infer                 (pure domain logic, this phase)
```

`PredictiveSafetyOrchestrator.EvaluateAsync` then runs governance validation
(`PredictiveGovernanceEvaluator`), publishes the unified event (`PredictiveSafetyEventEmitter.Emit`),
and records a deterministic trace span (`PredictiveTracing.RecordSpan`) — all in the same fixed
order for every call.

## Two honest substitutions (same convention Phase 13 already established)

This phase's own prompt asks for "Diagnostics signals (Phase 12)" and "Audit search metadata."
Neither a Diagnostics domain nor an Audit domain exists anywhere in this repository — confirmed by
direct search before writing any code, the same investigation discipline Phase 13's own header
comments document when it found "Diagnostics" didn't exist and retargeted to Routing:

- **"Diagnostics signals"** — the closest real, existing signal is `ReliabilityHooks`' failure
  count, which every phase's telemetry decorator already produces for every wrapped operation.
  `RecentReliabilityFailureCount` in `PredictiveHazardSignals`/`PredictiveMaintenanceSignals` is
  this substitute, documented in each Signals file's own header comment.
- **"Audit search metadata"** — no `IAuditLogRepository`/`AuditTelemetry` implementation exists
  anywhere (only a stray metric-name reference in an early `PlantSafetyMetrics.cs` comment). The
  closest real, existing structured-record shape every module already produces is
  `Shared.Telemetry`'s `LogEvent` (Phase 2.4). `PredictiveAuditSignals` is therefore a set of
  deterministic counts over `LogEvent` volume/severity/governance-violation totals — still fixed
  thresholds, never statistics, per this phase's own prohibition.

## Cross-module dependencies

`Shared.PredictiveSafety.Abstractions` takes `ProjectReference`s on
`Shared.FacilityDomain.Abstractions` (hazard/severity/regulatory rules),
`Shared.MachineDomain.Abstractions` (`MachineMaintenanceProfile`),
`Shared.SafetyData.Abstractions` (`EventRoutingResult`), and `Shared.Scheduling.Abstractions`
(`SchedulingRuleDecision`) — every one of these types is consumed, never re-implemented.

## Metrics and log categories added this phase

- `LogCategory.PredictiveSafety` (`Shared.Telemetry.Abstractions.Events.LogCategory`)
- `LogCategoryClassifier.Classify("predictive")` -> `LogCategory.PredictiveSafety`
- `PlantSafetyMetrics.PredictiveEvaluationsComputed`
- `PlantSafetyMetrics.PredictiveEscalationsRequired`
- `PlantSafetyMetrics.PredictiveGovernanceViolationsDetected`

No new Meter instance was created — every Predictive Safety counter is published through the same
`PlantSafetyMetrics.Meter` every other module's telemetry already uses.
