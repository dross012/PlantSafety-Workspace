# Shared.Scheduling — Phase 14 (Scheduling Modernization)

Scheduling's first real modernized subsystem: a deterministic rule engine, a deterministic due-time
engine, a full orchestration pipeline, dispatcher contract shapes (data only, no dispatcher), and
authorization/authentication/tenancy/telemetry/governance integration, following the exact
decorator-chain convention every phase since Phase 8 has used.

## Why this exists

Before this phase, Scheduling did not exist as code anywhere in this repository — only the
docs/lifecycle/*/scheduling.md investigation artifacts, which describe an unrelated legacy domain
(`PMScheduling`, `WorkOrderPriority`, etc.) that has no code in this worktree. Phase 14 introduces
the first real Scheduling module: `Shared.Scheduling.Abstractions` (pure, deterministic domain logic)
and `Shared.Scheduling.Infrastructure` (telemetry/authorization/tenant/governance decorators and the
orchestrator), plus `Shared.Scheduling.Tests`.

## Layout

```
Shared/Scheduling/
  Shared.Scheduling.Abstractions/
    Rules/SchedulingRuleEngine.cs            -- pure, deterministic rule evaluation
    DueTime/SchedulingDueTimeEngine.cs        -- pure, deterministic due-time calculation
    DueTime/SchedulingDueTimeEvaluator.cs     -- worst-case-wins status rollup
    Schedule/SchedulingScheduleCalculator.cs  -- composes RuleEngine + DueTimeEngine
    Events/SchedulingScheduledEvent.cs        -- unified-schema event payload (Phase 12 precedent)
    Contracts/SchedulingContracts.cs          -- request/result contracts + metadata
    Contracts/SchedulingDto.cs                -- flat DTO projection + mapper
    Contracts/SchedulingErrors.cs             -- error codes + SchedulingValidationException
    Contracts/SchedulingDispatcherContracts.cs -- pure dispatcher contract shapes (no dispatcher)
    Governance/SchedulingGovernanceRules.cs   -- pure governance validation rules
  Shared.Scheduling.Infrastructure/
    Orchestration/SchedulingOrchestrator.cs           -- the one public entry point
    Orchestration/SchedulingEvaluationPipeline.cs     -- deterministic result composition
    Orchestration/SchedulingEventEmitter.cs           -- unified event schema publication
    Authorization/SchedulingAuthorization.cs          -- Phase 9 integration
    Authentication/SchedulingIdentity.cs              -- Phase 10 integration
    Tenant/SchedulingTenant.cs                        -- Phase 11 + Phase 12 integration
    Telemetry/{SchedulingTelemetry,SchedulingMetrics,SchedulingTracing}.cs -- Phase 8 integration
    Governance/{SchedulingGovernanceEvaluator,SchedulingGovernanceEvents}.cs
    DependencyInjection/SchedulingServiceCollectionExtensions.cs
  Shared.Scheduling.Tests/
    ... one test class per file above, mirroring Shared.WorkflowEngine.Tests' structure
```

## The decorator chain

Every mutating/checked call to Scheduling flows through the same fixed order every phase since
Phase 9 established:

```
SchedulingOrchestrator.EvaluateAsync
  -> SchedulingTenant.CalculateWithTenantEnforcementAsync      (Phase 11 tenant isolation + Phase 12 contract governance)
    -> SchedulingAuthorization.CalculateWithAuthorizationAsync (Phase 9 authorization check)
      -> SchedulingTelemetry.CalculateWithTelemetryAsync       (Phase 8 reliability/logging/metrics)
        -> SchedulingScheduleCalculator.Calculate              (pure domain logic, this phase)
```

`SchedulingOrchestrator.EvaluateAsync` then composes the unified-schema result contract
(`SchedulingEvaluationPipeline.Compose`), runs governance validation
(`SchedulingGovernanceEvaluator`), publishes the unified event
(`SchedulingEventEmitter.Emit`), and records a deterministic trace span
(`SchedulingTracing.RecordSpan`) — all in the same fixed order for every call, regardless of caller,
tenant, or payload (deterministic rule ordering, Phase 7).

## Deterministic rules, not inference

`SchedulingRuleEngine.Evaluate` and `SchedulingDueTimeEngine.CalculateDueTimes` are pure functions:
same input, same output, every time. No IClock read inside either engine (the caller supplies
`asOfUtc` explicitly), no randomness, no ML model, no probabilistic inference, no weighted scoring or
averaging. Accumulation is strictly additive (`TotalRequiredActionCount`,
`TotalEstimatedEffortHours`); escalation is strictly worst-case-wins (`WorstCaseDueWithinHours` is a
`Min`, `SchedulingDueTimeEvaluator.EvaluateStatus` treats a single overdue entry as overdue for the
whole rollup) — no rule ever cancels or relaxes a prior rule's contribution.

## Dispatcher contracts, not a dispatcher

`ISchedulingDispatcherContract`/`SchedulingDispatchRequest`/`SchedulingDispatchMetadata` are pure,
versioned, serializable data shapes only. Phase 14 does not implement a dispatcher — that is
explicitly Phase 16 or 17's scope. No transport, retry policy, or delivery guarantee is invented here.

## The one thing every downstream phase (9–13) already left unresolved

`Shared/Authorization` — the project every `using Shared.Authorization.Abstractions.*` /
`Shared.Authorization.Infrastructure.*` statement across Phases 9 through 13 (and now 14) resolves
against — does not exist anywhere in this repository or its git history, despite five phases already
having been merged to `main` against it. Phase 14 follows the same convention every merged phase
before it used (referencing `CallerContext`, `TenantContext`, `AuthorizationCheck`,
`EnterprisePermissionResolver`, `ContractMetadata`, `ContractMetadataFactory`,
`AuthorizationGovernanceEvaluator`, `TenantGovernanceEvaluator`, `ContractGovernanceEvaluator`,
`CallerContextFactory`, `TenantContextFactory`, `AuthenticationTelemetry`,
`AuthenticationGovernanceEvaluator` exactly as `WorkflowEngineAuthorization.cs`/`WorkflowEngineTenant.cs`/
`RoutingAuthorization.cs`/`RoutingTenant.cs`/`ApiKeyAuthenticationMiddleware.cs` already do) rather
than inventing a divergent pattern — but this means `Shared.Scheduling.Infrastructure` and
`Shared.Scheduling.Tests` will not compile until `Shared/Authorization` is built, exactly like every
other module's Infrastructure/Tests project since Phase 9. `Shared.Scheduling.Abstractions` has no
such dependency and compiles standalone today.

## Metrics and log categories added this phase

- `LogCategory.Scheduling` (Shared.Telemetry.Abstractions.Events.LogCategory)
- `LogCategoryClassifier.Classify("scheduling")` -> `LogCategory.Scheduling`
- `PlantSafetyMetrics.SchedulingSchedulesComputed`
- `PlantSafetyMetrics.SchedulingDueTimeEvaluationsPerformed`
- `PlantSafetyMetrics.SchedulingImmediateRequiredCount`
- `PlantSafetyMetrics.SchedulingGovernanceViolationsDetected`

No new Meter instance was created — every Scheduling counter is published through the same
`PlantSafetyMetrics.Meter` every other module's telemetry already uses.
