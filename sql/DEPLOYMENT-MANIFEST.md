# PlantSafety SQL Deployment Manifest

Deterministic deploy order for every SQL folder under `Shared/`, `Enterprise/`, and `AI/`,
derived by grepping actual `REFERENCES` foreign-key clauses across the tree (see
`Bash` command used to build this file — not guessed). Within a tier, order between
folders does not matter; folders in a later tier must run after every folder in every
earlier tier.

Each folder is deployed in its own internal numeric file order (`00-`, `01-`, `10-`, ...).

## Tier 0 — no cross-module dependencies

Deployable in any order, first:

- `Shared/FacilityDomain/sql/Phase1-Lookup` — canonical `SeverityLevel`/`HazardType`/`RegulatoryJurisdiction` lookup tables. Almost everything downstream needs this.
- `Shared/SafetyData/sql/Phase1-StoredProcedures` through `Phase5-ComplianceRisk` — Path A, already deployed in production; listed for completeness only.
- `Shared/PredictiveModels/sql/Phase1-PredictionLog`
- `Enterprise/DataLake/sql/Phase1-DataLake`
- `Enterprise/DigitalTwin/sql/Phase1-DigitalTwin`
- `Enterprise/PredictiveModeling/sql/Phase1-PredictiveModeling`
- `Enterprise/Integration/sql/Phase1-Integration`
- `Enterprise/Integration/sql/IncidentMgt-Integration` — no downstream FKs from other PlantSafety modules; own `IncidentMgtIntegration` schema. Deployed live to `PlantSafety.CoreDB` and verified (`usp_ValidateIncidentMgtIntegration`: 44/44 checks passing) 2026-09-06 — brought to parity with `EmergencyMgt-Integration` below at deploy time (added `21-usp-validateintegration.sql`, `22-usp-reads.sql`, `50-governance.sql`, `51-usp-validateintegration-extensions.sql`, mirroring that folder's own file set; the original `00`/`01`/`02`/`10`/`20`/`30`/`40` files were unchanged).
- `Enterprise/Integration/sql/EmergencyMgt-Integration` — mirrors `IncidentMgt-Integration`'s own pattern exactly (own `EmergencyMgtIntegration` schema, no FKs into any other PlantSafety schema). Deployed live to `PlantSafety.CoreDB` and verified (`usp_ValidateEmergencyMgtIntegration`: 23/23 checks passing) 2026-08-28 — the first schema of this integration shape actually deployed live in this repo.
- `Enterprise/Graph/sql/Phase1-Graph`
- `Enterprise/RulesEngine/sql/Phase1-RulesEngine`
- `Enterprise/WorkflowOrchestration/sql/Phase1-WorkflowOrchestration`
- `AI/WorkflowAutomation/sql/Phase1-WorkflowAutomationLog`
- `AI/DigitalTwinLearning/sql/Phase1-DigitalTwinLearningLog`
- `AI/PredictiveOptimization/sql/Phase1-PredictiveOptimizationLog`
- `AI/SafetyCopilot/sql/Phase1-SafetyCopilotLog`

## Tier 1 — depends on `Lookup` only (not `Facility`)

- `Shared/EmployeeDomain/sql/Phase1-Employee` — FKs to `Lookup.HazardType` and self (`Employee`); no `FacilityId` link in this schema.
- `Shared/WorkflowEngine/sql/Phase1-WorkflowLog` — FKs to `Lookup.HazardType`/`Lookup.SeverityLevel` only.
- `AI/Reasoning/sql/Phase1-ReasoningLog`
- `AI/HazardUnderstanding/sql/Phase1-HazardUnderstandingLog`
- `AI/RegulatoryInterpretation/sql/Phase1-RegulatoryInterpretationLog`
- `AI/SeverityAssessment/sql/Phase1-SeverityAssessmentLog`
- `AI/EnterpriseOrchestration/sql/Phase1-EnterpriseOrchestrationLog`
- `Shared/SafetyData/sql/Phase6-MultiFacilityTenant` — FKs to `Lookup.SeverityLevel` and to Path A's own `Facilities.FacilityProfile` (already deployed in Tier 0's Path A scripts), **not** the real `FacilityDomain.Facility` below.

## Tier 2 — depends on `Lookup` (real Facility identity)

- `Shared/FacilityDomain/sql/Phase2-Facility` — `Facility` + `FacilityBuilding`/`FacilityArea` hierarchy. Everything in Tier 3 depends on this.

## Tier 3 — depends on `FacilityDomain.Facility`

- `Shared/MachineDomain/sql/Phase1-Machine` — FKs to `FacilityDomain.Facility` and `Lookup.HazardType`.
- `Shared/FacilityDomain/sql/Phase3-Propagation` — FKs to `FacilityDomain.Facility` and `Lookup.SeverityLevel`.

## Correction to prior assumption

Earlier guidance assumed *every* downstream module depends on `Facility`. The actual FK
graph is narrower: `Employee`, `WorkflowEngine`, and all five Tier-1 AI log tables only
need `Lookup` — they can deploy immediately after Tier 0/Lookup without waiting on
`Facility`. Only `Machine` and `Propagation` (Tier 3) actually require `Facility` to exist
first. Keeping the manifest accurate to the real FK graph (not a conservative
over-assumption) avoids unnecessary deploy-order coupling as new modules are added.

## How this was derived

```
grep -rhoE "REFERENCES \[?[A-Za-z]+\]?\.\[?[A-Za-z]+\]?" Shared Enterprise AI --include=*.sql | sort -u
```

Re-run this whenever a new module's SQL is added, and re-derive tiers from the actual
output rather than assuming — this file should stay a description of what the FKs say,
not a prediction of what they might say.
