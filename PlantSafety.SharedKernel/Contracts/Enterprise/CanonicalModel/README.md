# Enterprise Canonical Model

This folder holds the platform's canonical, enterprise-stable, interchange-ready projections of
the six cross-cutting concepts every ERP/MES/CMMS integration adapter, the event mesh, and any
future external consumer needs a single stable shape for:

- `CanonicalAsset` -- equipment/machine identity (grounded in `PlantSafety.Application.CrossDomain.Machines`
  and `PlantSafety.Domain.Operational.EquipmentSafety`, the platform's real equipment-identity sources).
- `CanonicalWorkOrder` -- grounded in `PlantSafety.Domain.Operational.WorkOrders.Aggregates.WorkOrder`.
- `CanonicalProductionOrder` -- grounded in `PlantSafety.Domain.Operational.ProductionTracking.Aggregates.ProductionRun`.
- `CanonicalRiskEvent` -- grounded in `PlantSafety.Domain.RiskManagement.Aggregates.Risk`.
- `CanonicalImprovementAction` -- grounded in `PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates.CorrectiveAction`.
- `CanonicalSchedulingBlock` -- grounded in `PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates.OptimizationRun`
  and its `ScheduleAssignment` value objects.

These are **projections**, not a parallel domain model. No canonical type owns state, raises
domain events, or is persisted as a system of record -- each module's own aggregate remains the
single source of truth. Mapping FROM a module's real aggregate TO its canonical DTO lives in that
module's own Application (or Infrastructure) layer (e.g.
`PlantSafety.Application.Operational.WorkOrders.Mappings.WorkOrderToCanonicalMapper`), never here.
This project (`PlantSafety.SharedKernel`) must never reference any module's Domain or Application
layer -- that is what makes these contracts safely referenceable by every module and by future
integration adapters without creating cross-module coupling.

## Versioning convention

Each canonical concept is expressed as three distinct, immutable record types with an explicit
version suffix: `CanonicalXV1`, `CanonicalXV2`, `CanonicalXV3`. This mirrors the platform's existing
API versioning precedent (`Controllers/V1`, `Controllers/V2` -- distinct types/classes per version,
not one flexible type with runtime-conditional shape) and keeps each version's field set mechanically
verifiable by reflection (see the "Backward compatibility rules" section below) rather than relying
on convention alone.

- **V1 (stable)** -- the minimal, current-state shape: identity, key attributes, status, and the
  timestamps every consumer needs today. This is the contract external ERP/MES/CMMS adapters should
  target first.
- **V2 (extended)** -- V1 plus additional optional attributes that exist on the real source
  aggregate but were not essential for a V1 consumer (e.g. assignment/completion detail, richer
  lifecycle timestamps).
- **V3 (predictive)** -- V2 plus forward-looking fields. These are wired to real Phase 5 predictor
  infrastructure (`PlantSafety.Domain.Analytics.PredictiveAnalytics.Predictors`,
  `PlantSafety.Domain.Intelligence.PredictiveSafety.Predictors`) wherever a genuine model exists
  (`CanonicalAssetV3`, `CanonicalWorkOrderV3`, `CanonicalSchedulingBlockV3`). Where no genuine ML/
  heuristic predictor currently exists for a concept (`CanonicalRiskEventV3`,
  `CanonicalImprovementActionV3`), the V3 fields are deterministic forward-looking projections of
  data the real aggregate already tracks (e.g. `Risk.NextReviewDueUtc`, the same overdue guard
  condition `CorrectiveAction.MarkOverdue` already enforces) -- clearly documented as such, never
  fabricated numbers dressed up as model output.

Every canonical type also exposes a `SchemaVersion` constant (`1`, `2`, or `3`) for runtime
introspection by consumers that deserialize into a weakly-typed envelope.

## Backward compatibility rules

1. `CanonicalXV2` must only **ADD** properties relative to `CanonicalXV1`. It must never remove,
   rename, or retype a `CanonicalXV1` property.
2. `CanonicalXV3` must only **ADD** properties relative to `CanonicalXV2`, under the same rule.
3. All new properties introduced by a later version must be optional (nullable, or defaulted) --
   a V1 producer must never be required to populate a V2/V3-only field.
4. These rules are enforced both here (as documentation) and mechanically, by
   `PlantSafety.Tests.SharedKernel.Contracts.Enterprise.CanonicalModelVersioningTests`, which
   reflects over every `Vn`/`Vn+1` pair and asserts every `V(n)` property name + type still exists
   on `V(n+1)`.
5. Contract-testing snapshots (`PlantSafety.Tests/SharedKernel/Contracts/Enterprise/Snapshots/`)
   capture each version's serialized JSON shape. A silent field addition/removal/rename that is not
   accompanied by an explicit snapshot update (and, for a breaking change, a new `Vn+1` type) will
   fail `CanonicalModelSnapshotTests`.
