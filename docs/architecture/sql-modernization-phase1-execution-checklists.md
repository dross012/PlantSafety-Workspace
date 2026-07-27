# SQL Modernization Phase 1 — Execution Checklists (Compliance, Analytics, Operational)

Documentation only — no SQL, code, or UI changes are made by this document. All
table/procedure names below were extracted directly from the current `.sql` files
under `PlantSafety.Infrastructure/{Compliance,Analytics,Operational}/Sql/` at the time
of writing, not estimated from earlier aggregate counts.

## 0. The one finding that reshapes this whole phase

Every procedure in these three modules was checked for `BEGIN TRY` presence. The result:

| Module | Total procedures | Write procedures (have `BEGIN TRY`) | Read procedures (`_List`/`GetById`/`GetAll`, no `BEGIN TRY`) |
|---|---|---|---|
| Compliance | 69 | **5** | 64 |
| Analytics | 25 | **1** | 24 |
| Operational | 26 | **1** | 25 |

Only **7 procedures across all three modules actually write anything** — everything
else is a read-only projection. This means:

- The stored-procedure modernization checklist (Section 2) is short — 7 procedures,
  not 120.
- The vast majority of the 110 tables needing a `RowVersion` column (Section 1) have
  **no write procedure at all** — they're fed by some other process (event-bus
  subscribers, per the Reporting read-model pattern established earlier this session)
  rather than direct API writes. Adding a concurrency column to a table nothing writes
  to today is pure schema hygiene: no procedure change, no repository change, low risk.
- Only **2 of the 110 currently-unprotected tables have an active write procedure**:
  `Compliance.ComplianceResources` (via `usp_ComplianceResources_BulkUpdate`) and
  `Operational.OperationalResources` (via `usp_OperationalResources_BulkUpdate`).
  These two deserve the most scrutiny in this phase — everything else is additive DDL.

## 1. ROWVERSION Concurrency Rollout Checklist

### 1.1 Compliance — 62 tables need `RowVersion` added

(2 of 64 total already have it: `Moc`, `Permit` — both added during the integration
test bring-up, both already `ROWVERSION`-typed, matching the canonical direction. Not
listed below.)

```sql
ALTER TABLE [Compliance].[AuditFindings] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CAPATriggers] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CertificationContractor] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CertificationDOT] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CertificationEPA] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CertificationISO14001] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CertificationISO45001] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CertificationIndustrySpecific] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CertificationOSHACertifications] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[CertificationTraining] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ComplianceAudit] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ComplianceExportItems] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ComplianceResources] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[DeadlinesAnnualAudit] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[DeadlinesEPA] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[InspectionsDOT] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[InspectionsEnvironmentalSampling] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[InspectionsEquipment] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[InspectionsOSHA] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[InternalSafetyAudit] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[MOCApprovals] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[MOCRiskAssessments] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[MOCWorkflows] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[Obligations] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitChemicalHandling] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitColdWork] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitConfinedSpace] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitElectrical] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitExcavation] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitHotTap] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitHotWork] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitLifting] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitLineBreak] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitLockoutTagout] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitPressureTesting] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitRadiography] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PermitWorkatHeight] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyApprovals] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyChemicalHandling] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyContractor] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyDocuments] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyEmergency] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyEnvironmental] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyEquipment] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyHR] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyPPE] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicyRevisions] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[PolicySafetyPolicies] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresEmergencyProcedure] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresHazMatProcedures] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresIncidentResponse] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresInspection] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresLockoutTagoutProcedure] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresMaintenance] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresSafety] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresSOPs] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[ProceduresWorkInstructions] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[RegulatoryRequirements] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[RenewalsDOT] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[RenewalsISO] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[RenewalsTraining] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Compliance].[Tracking] ADD RowVersion ROWVERSION NOT NULL;
```

**Only `ComplianceResources` (bolded here) has an active write procedure** — treat this
ALTER as the one in this list that needs the full verification sequence (1.4). The other
61 are read-only projections with no write path found — schema-only, low-risk.

### 1.2 Analytics — 22 tables need `RowVersion` added

(`Culture` already has it — done in the prior pass.)

```sql
ALTER TABLE [Analytics].[AnalyticsExportItems] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[Behavior] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[ComplianceFailure] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[CultureIndex] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[DashboardTrendAnalysis] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[Department] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[EngagementDataPoints] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[EngagementScoring] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[Executive] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[FatigueRiskModeling] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[IncidentPrediction] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[KPISnapshots] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[PredictiveAnalytics] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[ReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[Reporting] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[RiskForecasting] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[Schedules] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[ScoreCardsCompliance] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[ScoreCardsTraining] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[Supervisor] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[TrendAnalysis] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Analytics].[TrendModeling] ADD RowVersion ROWVERSION NOT NULL;
```

**None of these 22 tables have a write procedure** (confirmed — Section 0's inventory
found exactly one Analytics write procedure, `usp_Culture_Insert`, targeting the
already-done `Culture` table). All 22 ALTERs here are pure schema hygiene.

### 1.3 Operational — 26 tables need `RowVersion` added

```sql
ALTER TABLE [Operational].[AlertsReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[ChecklistsReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[ClassesReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[CompetencyManagementReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[ContractorReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[CoursesReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[EquipmentSafetyReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[ErgonomicsReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[FatigueManagementReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[HazardAssessmentsReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[HazardousMaterialsReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[IndustrialHygieneReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[InspectionsReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[JobSafetyAnalysisReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[LockoutTagoutReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[OperationalExportItems] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[OperationalResources] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[PermitsToWorkReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[PPEManagementReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[PredictiveMaintenanceReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[PreventiveMaintenanceReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[RiskManagementReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[SchedulesReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[SeminarsReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[TrainingReadModel] ADD RowVersion ROWVERSION NOT NULL;
ALTER TABLE [Operational].[WebinarsReadModel] ADD RowVersion ROWVERSION NOT NULL;
```

**Only `OperationalResources` (bolded here) has an active write procedure**
(`usp_OperationalResources_BulkUpdate`) — same elevated-scrutiny treatment as
`ComplianceResources`. The 25 `*ReadModel`/`ExportItems` tables have no write
procedure found — schema-only.

### 1.4 Verification steps per table

**For the 2 write-active tables (`ComplianceResources`, `OperationalResources`)** —
full sequence:

1. Confirm current row count and a sample of existing `PK` values before the ALTER
   (`SELECT COUNT(*) FROM [Schema].[Table];`) — baseline for the "did the ALTER
   change any existing data" check in step 3.
2. Run the `ALTER TABLE ... ADD RowVersion ROWVERSION NOT NULL;` statement.
3. Re-run the same `SELECT COUNT(*)` — must be identical (an `ADD COLUMN` with a
   computed/system type like `ROWVERSION` never touches existing row data, but verify
   anyway rather than assume).
4. Confirm the column exists and is genuinely `ROWVERSION`-typed:
   `SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Schema' AND TABLE_NAME = 'Table' AND COLUMN_NAME = 'RowVersion';`
   — expect `timestamp` (SQL Server's `INFORMATION_SCHEMA` reports `ROWVERSION` as
   `timestamp`, its underlying type name — this is expected, not an error).
5. Run the existing write procedure once against a test row and confirm
   `RowVersion` populates with a non-null, 8-byte value
   (`SELECT RowVersion FROM [Schema].[Table] WHERE <PK> = @TestId;`).
6. Run the module's integration test suite for that specific area (Section 4 names
   the exact test classes) and confirm no regression — the new column must not break
   any existing `SELECT`-with-explicit-column-list procedure (it won't, by
   construction, since none of this codebase's procedures use `SELECT *`, but confirm
   rather than assume, per this whole phase's own standard).

**For the 108 schema-only tables** — lighter sequence, since nothing currently reads
or writes the new column:

1. Run the `ALTER TABLE` statement.
2. Confirm the column exists (`INFORMATION_SCHEMA.COLUMNS` check, same as step 4 above).
3. Run the existing read procedure for that table (e.g. `usp_AuditFindings_GetFindings`)
   and confirm it still returns the same shape/row count as before — proves the new
   column didn't silently appear in a `SELECT *`-style result the C# read model wasn't
   expecting (again, not expected given this codebase's explicit-column convention, but
   cheap to confirm).
4. No repository or procedure change is required beyond the ALTER itself for these 108
   tables in this phase — see Section 3.

## 2. Stored Procedure Modernization Checklist

Only the 7 write procedures identified in Section 0 need work. All read (`_List`/
`GetById`/`GetAll`) procedures in these three modules are intentionally left
unchanged — this matches the established, already-audited codebase convention
(read procedures never carry TRY/CATCH; only write procedures do), not an oversight.

### 2.1 Compliance — 5 procedures

| Procedure | Current state | Needed |
|---|---|---|
| `usp_ComplianceResources_BulkUpdate` | TRY/CATCH ✔ (fixed for the ROWCOUNT/NOCOUNT bug during integration bring-up), AuditLog ✘, ErrorLog ✘ | Add AuditLog + ErrorLog |
| `usp_Moc_Approve` | TRY/CATCH ✔, AuditLog ✘, ErrorLog ✘ | Add AuditLog + ErrorLog |
| `usp_Moc_Create` | TRY/CATCH ✔, AuditLog ✘, ErrorLog ✘ | Add AuditLog + ErrorLog |
| `usp_Permit_Approve` | TRY/CATCH ✔, AuditLog ✘, ErrorLog ✘ | Add AuditLog + ErrorLog |
| `usp_Permit_Create` | TRY/CATCH ✔, AuditLog ✘, ErrorLog ✘ | Add AuditLog + ErrorLog |

**Exact modernization steps, per procedure** (identical shape for all 5 — follow the
`usp_Culture_Insert` rewrite from the prior pass as the worked reference):

1. Locate the existing `BEGIN TRY ... END TRY` block's success path (after the
   `INSERT`/`UPDATE` statement, before any `COMMIT`/end of the TRY block).
2. Insert a nested `BEGIN TRY / INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog
   (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed,
   ErrorContext, Severity, Module) VALUES (...) / END TRY BEGIN CATCH END CATCH;`
   immediately after the write, using the procedure's own real parameters for the
   `ErrorContext` string (e.g. `CONCAT(N'MocId=', @MocId, N'; TenantId=', @TenantId)`)
   — never invent a parameter that doesn't exist on the procedure.
3. In the existing `BEGIN CATCH` block, add the `@__ErrorContext`/`@__ErrorDetails`
   declarations and the nested `BEGIN TRY / INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog
   (...) / END TRY BEGIN CATCH END CATCH;` before the existing `THROW;` — do not
   remove or reorder the `THROW`, it must remain the last statement in the CATCH block.
4. Do not change the procedure's parameter list, return shape, or any existing
   business logic — this is additive-only, matching every other procedure
   modernization in this codebase.
5. Update the embedded copy in `TestDatabaseFactory.cs` (`ApplyComplianceSchemaAsync`)
   to match exactly — this codebase's own prior sessions found drift between embedded
   and real copies to be a real, repeat source of bugs; do not skip this step.

### 2.2 Analytics — 0 procedures need work

`usp_Culture_Insert` is the only write procedure in this module and is already fully
modernized (TRY/CATCH + AuditLog + ErrorLog, done in the prior pass). No further
action needed for Analytics under this checklist item.

### 2.3 Operational — 1 procedure

| Procedure | Current state | Needed |
|---|---|---|
| `usp_OperationalResources_BulkUpdate` | TRY/CATCH ✔ (ROWCOUNT/NOCOUNT-fixed), AuditLog ✘, ErrorLog ✘ | Add AuditLog + ErrorLog |

Same five-step process as Section 2.1, applied to this one procedure, with its embedded
copy in `TestDatabaseFactory.cs`'s `ApplyOperationalSchemaAsync` updated to match.

### 2.4 Regression checks tied to integration tests, per procedure

| Procedure | Test class that must re-run |
|---|---|
| `usp_ComplianceResources_BulkUpdate` | `PlantSafety.IntegrationTests.Compliance.BulkUpdate.BulkUpdateComplianceIntegrationTests` |
| `usp_Moc_Approve`, `usp_Moc_Create` | `PlantSafety.IntegrationTests.Compliance.ManagementOfChange.MocWritePathIntegrationTests` |
| `usp_Permit_Approve`, `usp_Permit_Create` | `PlantSafety.IntegrationTests.Compliance.Permits.PermitLifecycleIntegrationTests` |
| `usp_OperationalResources_BulkUpdate` | `PlantSafety.IntegrationTests.Operational.BulkUpdate.BulkUpdateOperationalIntegrationTests` |

Adding AuditLog/ErrorLog to a procedure that already has TRY/CATCH and a working
rowcount check should not change any of these tests' pass/fail outcome — they test
business behavior (create/approve succeeds, bulk update reports the right count), not
audit-log presence. If any of them start failing after this change, that's a signal
the AuditLog/ErrorLog insert was placed somewhere it interferes with the main
transaction (e.g. inside the same transaction scope in a way that changes rowcount or
error propagation) — stop and investigate rather than adjusting the test to match.

## 3. Repository Modernization Checklist

### 3.1 What actually needs a repository (C#) change

**Nothing, for this phase**, with one caveat below. Reasoning:

- The AuditLog/ErrorLog additions in Section 2 use `SYSTEM_USER`, `DB_NAME()`,
  `@@PROCID`, `ERROR_NUMBER()`/`ERROR_MESSAGE()`/etc., and the procedure's own
  existing parameters — no new parameter is added to any of the 7 procedures'
  signatures, so no repository call site needs to pass anything new.
- The rowcount/concurrency-check correctness for all 7 write procedures was already
  fixed during the integration test bring-up (confirmed in Section 0/2's status
  table — all 7 already show `TRY=Y`, meaning the `ExecuteScalarAsync` fix from that
  pass is already in place). This phase's Section 2 work does not touch that logic.
- The 108 schema-only `RowVersion` additions (Section 1) have no C# consumer at all
  today — no repository method reads or writes them, so nothing to update.

**The one caveat**: if, while doing Section 2's procedure modernization work, the
AuditLog/ErrorLog insert is placed in a way that changes the procedure's transaction
boundaries (e.g. if the existing procedure did NOT wrap the main write in an explicit
transaction and the modernization work introduces one to keep the audit insert
atomic with the write — check the `usp_Culture_Insert` reference: it does **not**
wrap the audit insert in the same transaction as the main write, matching
`usp_Incident_Create`'s pattern, specifically so an audit-log failure can never roll
back a successful business write), then the repository's exception-handling
assumptions should be re-verified against `PersistenceException`'s current wrapping
behavior in `DapperCommandExecutor`/`DapperQueryExecutor` — but if Section 2's steps
are followed exactly as specified (nested `TRY/CATCH` around the audit/error inserts
specifically so they can never surface as the main exception), this caveat should not
actually trigger.

### 3.2 Safe sequencing for updating repositories after SQL changes

Since Section 3.1 concludes no repository changes are needed for this phase, sequencing
is simple:

1. Land Section 1's `ALTER TABLE` statements first (all 110, low individual risk,
   can be done as one batch or module-by-module — no dependency between them).
2. Land Section 2's 6 procedure modernizations (5 Compliance + 1 Operational).
3. Run Section 4's validation checklist.
4. **Only if** validation surfaces an actual repository-level issue (contrary to
   Section 3.1's expectation) should any `.cs` file change — and at that point, treat
   it as a new, scoped bug fix following this session's established pattern (find the
   root cause via a failing test first, fix narrowly, don't preemptively refactor
   working repository code).

## 4. Integration Test Validation Checklist

### 4.1 Tests to re-run after each modernization step

| After... | Re-run |
|---|---|
| Any Compliance `ALTER TABLE` | `PlantSafety.IntegrationTests.Compliance.*` (all classes: `Auditing`, `BulkUpdate`, `Certification`, `Compliance`, `ManagementOfChange`, `Permits`, `Policy`, `Procedures`, `RegulatoryCalendar`) |
| Any Analytics `ALTER TABLE` | `PlantSafety.IntegrationTests.Analytics.Culture.CultureWritePathIntegrationTests` (the only Analytics test class with real write coverage; the rest are read-only smoke tests per the bring-up report) |
| Any Operational `ALTER TABLE` | `PlantSafety.IntegrationTests.Operational.BulkUpdate.BulkUpdateOperationalIntegrationTests` plus the module's other `Operational.*` read-smoke test classes |
| `usp_ComplianceResources_BulkUpdate` modernization | `BulkUpdateComplianceIntegrationTests` |
| `usp_Moc_Approve`/`usp_Moc_Create` modernization | `MocWritePathIntegrationTests` |
| `usp_Permit_Approve`/`usp_Permit_Create` modernization | `PermitLifecycleIntegrationTests` |
| `usp_OperationalResources_BulkUpdate` modernization | `BulkUpdateOperationalIntegrationTests` |
| **All of Phase 1 complete** | Full `PlantSafety.IntegrationTests.exe` run (no filter) — confirm the total count and the specific set of pre-existing, unrelated failures (50, per the most recent verified baseline — Outbox/RLS infrastructure tests and modules outside Compliance/Analytics/Operational/Culture) is unchanged; any new failure name outside that known set is a regression from this phase's work |
| **All of Phase 1 complete** | Full `PlantSafety.Tests.exe` unit suite — must remain 728/728 (no unit test in this codebase currently asserts against these procedures' SQL text, so a 728/728 result confirms no C# compilation/behavior regression, not SQL correctness — that's what the integration suite is for) |

Build command before any test run: `dotnet build PlantSafety.IntegrationTests/PlantSafety.IntegrationTests.csproj -tl:off`
(the `-tl:off` flag is required in this environment — see prior session findings).
Run via the native executable directly
(`PlantSafety.IntegrationTests/bin/Debug/net8.0/PlantSafety.IntegrationTests.exe -class "<FullyQualifiedName>"`),
**not** `dotnet test`. Docker must be running.

### 4.2 How to validate concurrency correctness

For the 2 write-active tables (`ComplianceResources`, `OperationalResources`):

1. After the `ALTER TABLE`, confirm `RowVersion` populates on a real write
   (Section 1.4, step 5).
2. Confirm the value changes on a second write to the same row, if the write
   procedure supports updates to an existing row (check `usp_ComplianceResources_BulkUpdate`/
   `usp_OperationalResources_BulkUpdate`'s actual semantics — these are bulk
   operations, so "the same row written twice" may mean two separate bulk-update
   calls touching the same resource ID).
3. This phase does **not** add a new ETag-based conflict-rejection check to these
   procedures (no `WHERE ... AND RowVersion = @ETag` clause is part of this phase's
   scope — that would be a behavioral change requiring its own, separately-reviewed
   pass, consistent with Section 2's "additive-only" constraint). Validation here is
   limited to confirming the column populates and changes correctly, not that
   conflicts are rejected — that capability doesn't exist yet for these two
   procedures and isn't being added by this phase.

For the 108 schema-only tables: no concurrency *behavior* exists to validate (no
write path consumes the column yet) — validation is limited to Section 1.4's
schema-level checks (column exists, correct type, no regression to existing reads).

### 4.3 How to validate auditing correctness

For each of the 6 modernized procedures (Section 2.1/2.3):

1. Execute the procedure once via its integration test (Section 4.1's table).
2. Query `dbo.AuditLog` (embedded test schema) or
   `[PlantSafety.AuditDB].dbo.AuditLog` (real environment) for a row matching:
   `TableName` = the target table, `Operation` = the expected verb (`INSERT`/`UPDATE`
   as appropriate), and `ErrorContext` containing the specific ID used in that test
   run — mirroring the exact assertion pattern used in
   `CultureWritePathIntegrationTests.CultureLifecycle_...` (query `dbo.AuditLog`
   directly via `SqlConnection`, assert `COUNT(1) > 0` for the matching row).
3. Confirm the audit row's absence does **not** fail the business operation — the
   canonical pattern wraps the audit insert in its own `TRY/CATCH` specifically so an
   audit failure never blocks the write; if you want to test this specific
   resilience property (not required for Phase 1 sign-off, but worth doing once),
   temporarily and locally break the audit insert (e.g. point it at a nonexistent
   table in a scratch/local copy only, never in a committed file) and confirm the
   main write still succeeds — then revert.

### 4.4 How to validate error-handling correctness

For each of the 6 modernized procedures:

1. Force a genuine failure the procedure's `CATCH` block should handle — e.g. call
   the procedure with a parameter that violates a constraint (a duplicate primary
   key, a `NOT NULL` column left `NULL` where the C# layer wouldn't normally allow
   it but a direct SQL call could).
2. Confirm the call still throws (the `THROW;` at the end of `CATCH` must re-raise —
   error logging must never swallow the original exception).
3. Query `dbo.ErrorLog`/`[PlantSafety.ErrorDB].dbo.ErrorLog` for a matching row:
   `ErrorProcedure` = the procedure name, `ErrorContext` containing the parameters
   used in the failing call, `ErrorMessage` containing a recognizable fragment of the
   forced failure (e.g. the constraint name).
4. This is the one validation step not already covered by the existing integration
   tests (Section 4.1's table) — none of `MocWritePathIntegrationTests`/
   `PermitLifecycleIntegrationTests`/`BulkUpdateComplianceIntegrationTests`/
   `BulkUpdateOperationalIntegrationTests` currently force a failure path. Adding a
   dedicated "forced failure → ErrorLog populated" test per modernized procedure is
   the one net-new test-writing recommendation in this checklist — everything else
   reuses existing coverage.
