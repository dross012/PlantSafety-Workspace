# Phase 15 — DevOps/Testing Scaffolding: tSQLt Database Unit Tests

## What's here

| File | Purpose |
|---|---|
| `150-install-tsqlt-coredb.sql` | Installs the official [tSQLt](https://tsqlt.org) framework (v1.0.5873.27393) into `PlantSafety.CoreDB` as the pilot database. |
| `151-tsqlt-tests-coredb.sql` | 6 representative unit tests (test class `SPTests`) covering CRUD, concurrency, soft-delete, CHECK constraints, the Phase 8 sequence generator, and Phase 9 Dynamic Data Masking. |
| `README.md` | This file. |

Verified live on `(local)\SQLEXPRESS` (SQL Server 2025 RTM-CU7): `EXEC tSQLt.RunAll;` → **6/6 tests pass**, 0 residual rows in `SafetyEvents.IncidentManagement` / `Compliance.Permits` after a clean run, and cross-database `AuditDB.dbo.AuditLog` writes made by the SPs under test are confirmed rolled back too (row count identical before/after a clean `RunAll`).

## Why only 6 tests, not all 233 CoreDB procs

Per the Testing Agent's scope rule, this is a *representative, genuinely valuable* pilot sample, not exhaustive coverage. The 6 tests were chosen to protect the patterns most worth regression-protecting across the whole SP layer (they're used identically by all ~40 aggregate-root tables in CoreDB):

1. CRUD happy path (Create → GetById round-trip)
2. Optimistic-concurrency conflict on `_Update` (stale `ConcurrencyToken`)
3. Soft-delete semantics (`_Delete` sets `IsDeleted=1`, never a hard `DELETE`; `_GetById`/`_List` exclude it)
4. CHECK constraint enforcement (invalid enum-like `Status` value)
5. Phase 8 sequence-based number generator (`Compliance.Seq_PermitNumber` via `sp_Permits_GenerateNumber`)
6. Phase 9 Dynamic Data Masking under `app_executor` (has `UNMASK`) vs `readonly_reporting` (does not)

## How to add a new tSQLt test

1. Add a new `CREATE PROCEDURE SPTests.[test <describe the scenario>]` to `151-tsqlt-tests-coredb.sql` (or a new `NNN-tsqlt-tests-<db>.sql` file for a different database — see "Extending to other databases" below).
2. Test procedure names must start with `test` (case-insensitive) — that's how `tSQLt.Tests` discovers them. Use the bracket-quoted, space-separated naming style already used here (`[test <Subject> <does what> <under what condition>]`) — it reads like a spec and shows up verbatim in `tSQLt.RunAll` output.
3. Every test that touches an RLS-scoped table **must** call `EXEC sp_set_session_context @key=N'TenantId', @value=<some GUID>;` as its first executable statement — see "The RLS gotcha" below.
4. Re-run `150-install-tsqlt-coredb.sql` only if tSQLt itself isn't installed yet (check with `SELECT SCHEMA_ID('tSQLt')`). Re-run `151-tsqlt-tests-coredb.sql` any time you add/change tests — it is safe to re-run (each `CREATE PROCEDURE` is preceded by `tSQLt.NewTestClass`, which drops and recreates the `SPTests` schema).
5. Run the suite: `EXEC tSQLt.RunAll;` or a single test: `EXEC tSQLt.Run 'SPTests.[test name]';`.
6. **Never** assert against a parenthesized expression (function call or subquery) as a positional/named argument to `EXEC` on this SQL Server 2025 (RTM-CU7) build — see the parser-quirk gotcha below. Assign to a local variable first.

## Real interaction #1 — the Row-Level Security (RLS) gotcha

Phase 9 enabled deny-by-default RLS (`Security.TenantAccessPolicy`, predicate function `Security.fn_TenantAccessPredicate`) on ~72 tenant-scoped tables in `CoreDB`/`DiagnosticsDB`, including `SafetyEvents.IncidentManagement`. The predicate is:

```sql
CREATE FUNCTION Security.fn_TenantAccessPredicate(@TenantId uniqueidentifier)
RETURNS TABLE WITH SCHEMABINDING AS
RETURN SELECT 1 AS fn_accessResult
WHERE @TenantId = TRY_CAST(SESSION_CONTEXT(N'TenantId') AS uniqueidentifier);
```

It is applied as both a `FILTER PREDICATE` (SELECT) and a `BLOCK PREDICATE` (INSERT/UPDATE/DELETE). If a test (or any caller) never calls `sp_set_session_context`, `SESSION_CONTEXT(N'TenantId')` is `NULL`, the predicate always evaluates to `FALSE`, and:

- `SELECT`s against the table silently return **zero rows** (no error — easy to misdiagnose as "the row wasn't inserted" when it actually was, just invisible).
- `INSERT`/`UPDATE`/`DELETE`s are **blocked outright** (a real error, but a confusing one if you don't know RLS is involved).

**The fix used throughout `151-tsqlt-tests-coredb.sql`:** every test calls `EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId;` as its very first statement, using a `TenantId` value the test controls (`NEWID()`), and uses that same value for every insert/update/select in that test. `sp_set_session_context` requires no special permission for a session to set its own context.

**Secondary finding used by test 6 (masking):** `SESSION_CONTEXT` is **session-scoped, not principal-scoped**. Once set (as `dbo`), it remains visible after `EXECUTE AS USER = '...'` and after `REVERT` — you do not need to re-set it inside an impersonated block. This is what makes it practical to seed test data as `dbo` and then read it back as a low-privilege impersonated user in the same test.

If you ever need to *fake* an RLS-scoped table with `tSQLt.FakeTable` (not used in this pilot — none of the 6 tests needed it), be aware that `tSQLt.FakeTable` recreates the table without carrying over security policies, so the RLS predicate will not apply to the faked table; you'd need to re-apply session-context discipline is moot in that case, but don't assume FakeTable output is RLS-filtered.

## Real interaction #2 — `SET XACT_ABORT ON` + tSQLt's ambient transaction hides the original error number

**This is the most important, non-obvious finding from this pilot.** Every production SP in this codebase begins with `SET XACT_ABORT ON;` and ends its `CATCH` block with an `INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (...)` (unguarded by its own nested `TRY/CATCH` — unlike the audit-log insert a few lines above it, which *is* wrapped in a best-effort `TRY/CATCH`).

`SET XACT_ABORT` is a **session-level** setting that is not scoped to the stored procedure — once any SP in a session sets it, it stays on for the rest of that connection, including for every later `tSQLt` test in the same `RunAll` run.

Under `XACT_ABORT ON`, a run-time error (e.g. a `THROW` or a CHECK-constraint violation) does not just unwind the current statement — it **dooms the entire ambient transaction**, forbidding any further write (including a `ROLLBACK` to a savepoint). Confirmed live:

```sql
BEGIN TRAN;
BEGIN TRY
    EXEC SafetyEvents.sp_IncidentManagement_Create @Status = N'NotARealStatus', ...; -- violates a CHECK constraint (547)
END TRY
BEGIN CATCH
    PRINT ERROR_NUMBER(); -- prints 3930, NOT 547
END CATCH;
```

What happens: the CHECK-constraint violation (547) fires inside the SP → `XACT_ABORT` immediately dooms the transaction → the SP's own `CATCH` block tries to `INSERT INTO ErrorDB.dbo.ErrorLog` to log the *original* error → **that INSERT itself fails** (SQL error 3930, "the current transaction cannot be committed... roll back the transaction") because the transaction is already doomed → **the 3930 from the failed diagnostic write is what actually propagates to the caller, silently replacing the original 547/50002/etc.** The caller (a test, or any real application code that wraps multiple SP calls in its own explicit transaction) can no longer see the true error that occurred.

This reproduces identically whether or not tSQLt is involved — it is a property of `{XACT_ABORT ON + unguarded ErrorLog write in CATCH} + {being invoked inside any pre-existing explicit transaction}`. tSQLt happens to trigger it every time because **every tSQLt test always runs inside an already-open transaction** (that's the mechanism it uses to isolate and roll back test side effects).

**Consequences for how you write negative-path tests here:**
- `tSQLt.ExpectException @ExpectedErrorNumber = ...` **cannot be used directly** against these SPs — it will report a `3930`/doomed-transaction error instead of your expected number, and worse, leaves the test's own transaction unusable for subsequent assertions/cleanup.
- The pattern used in tests 2 and 4 of `151-tsqlt-tests-coredb.sql` instead:
  ```sql
  DECLARE @CaughtErrorNumber int = 0;
  BEGIN TRY
      EXEC SafetyEvents.sp_IncidentManagement_Update ...; -- expected to fail
  END TRY
  BEGIN CATCH
      SET @CaughtErrorNumber = ERROR_NUMBER();
      IF @@TRANCOUNT = 0 BEGIN TRAN; -- XACT_ABORT may have fully closed the transaction; re-open one so tSQLt's own rollback machinery still has something to roll back
  END CATCH;
  EXEC tSQLt.AssertNotEquals 0, @CaughtErrorNumber, N'...'; -- assert AN error occurred
  -- then separately assert the row/column state proves the rule was actually enforced
  ```
  Since the *specific* error code isn't reliably observable, these tests instead assert (a) that an error was thrown at all, and (b) the underlying data-integrity guarantee actually held (row not inserted for the CHECK-constraint test; `Status` unchanged for the concurrency test). That is what a regression test actually needs to protect — the exact SQL error number is a secondary concern.
- **This is arguably a real production reliability/observability defect**, not just a testing inconvenience: any real application code that wraps two or more of these SP calls in its own explicit transaction (a normal pattern for multi-step business operations) will see the same error-number masking in production. Fixing it (wrapping every SP's `ErrorLog` INSERT in its own best-effort `TRY/CATCH`, matching the existing audit-log pattern) is a 233-procedure-wide change and out of scope for this testing pass — flagging it here for a future governance/remediation pass.

## Real interaction #3 — `INSERT ... EXEC` + `EXECUTE AS USER` + a cross-database write inside the callee is unsafe

Confirmed live, and distinct from finding #2 above: capturing a stored procedure's result set via `INSERT INTO #temp EXEC SomeProc ...` **while impersonating a low-privilege `WITHOUT LOGIN` contained user** (`EXECUTE AS USER = '...'`) reliably dooms the transaction (again, SQL error 3930) if `SomeProc` performs its own cross-database write (here: the best-effort `AuditLog` insert every production SP makes) and the impersonated principal has no access to that other database at all (a `WITHOUT LOGIN` contained user, by design, has no cross-database identity unless explicitly provisioned with a matching SID in each database).

- A **plain top-level `EXEC SomeProc ...`** (no `INSERT...EXEC`) under the same impersonation works perfectly fine and correctly reflects masking/permissions.
- `INSERT ... EXEC` against a proc with **no cross-database writes** also works fine under impersonation (verified with a trivial echo proc).
- It is specifically the three-way combination — `INSERT...EXEC` **+** `EXECUTE AS USER` **+** a cross-database write inside the callee by a principal absent from that other database — that is unsafe.

**Workaround used by test 6:** a thin, audit-free helper procedure (`SPTests.__Helper_GetIncidentDescriptionNoAudit`) that performs the exact same `SELECT` against the exact same base table/column, with no audit-log side effect, granted `EXECUTE` only to the `app_executor` test-fixture user. DDM masking is evaluated against the **calling principal's** `UNMASK` permission regardless of which procedure performs the `SELECT`, so this still validates the real Phase 9 masking behavior under test — it just avoids invoking the unrelated cross-database engine limitation.

In production, a real `app_executor` service account would need to be provisioned as a `USER` in `AuditDB`/`ErrorDB` too (see the "onboarding pattern" documented at the bottom of `Phase7-Governance/75-access-control-rbac.sql`) for the full `INSERT...EXEC` combination to work end-to-end under impersonation — out of scope for a CoreDB-pilot unit test.

A **fourth**, related and even narrower interaction was found and intentionally *not* worked around: running a plain (non-`INSERT...EXEC`) permission-denied `EXEC` call under impersonation, as a **third** `EXECUTE AS/REVERT` cycle inside a test whose ambient transaction had already touched `AuditDB` under a *different* security context (`dbo`) earlier in the same test, also dooms the transaction — even though the statement is only a permission check, not a write. This was a "bonus" assertion (confirming `readonly_reporting` is denied `EXECUTE` on the SP layer entirely) that added no coverage beyond what Phase 7's RBAC script already guarantees by construction, so it was removed from the test rather than chased further. See the comment in `151-tsqlt-tests-coredb.sql` (test 6) for the full note.

## Real interaction #4 — this SQL Server 2025 (RTM-CU7) build cannot parse a parenthesized expression as an `EXEC` argument

Independent of tSQLt: on this specific instance (`Microsoft SQL Server 2025 (RTM-CU7) - 17.0.4065.4`), **any** parenthesized expression — a function call or a subquery — passed as a positional or named argument to `EXEC` fails to parse, even the simplest cases:

```sql
EXEC SomeProc @Param = GETDATE();          -- Msg 102: Incorrect syntax near ')'
EXEC SomeProc NEWID();                     -- Msg 102: Incorrect syntax near ')'
EXEC tSQLt.AssertEquals 1, (SELECT COUNT(*) FROM #T), N'msg'; -- Msg 102: Incorrect syntax near '('
```

This is the same family of undocumented parser quirk already noted in `150-install-tsqlt-coredb.sql` for `sp_add_trusted_assembly`'s `HASHBYTES(...)` argument. **Workaround (used everywhere in `151-tsqlt-tests-coredb.sql`):** always assign the expression to a local variable first, then pass the variable:

```sql
DECLARE @Now datetime2(7) = SYSUTCDATETIME();
EXEC SomeProc @Param = @Now;               -- fine

DECLARE @Count int; SELECT @Count = COUNT(*) FROM #T;
EXEC tSQLt.AssertEquals 1, @Count, N'msg'; -- fine
```

## Real interaction #5 — several CoreDB columns are `NOT NULL` despite looking optional

`SafetyEvents.IncidentManagement.RootCause` and `.CorrectiveActions` are both `NOT NULL nvarchar(max)`, even though semantically a brand-new incident wouldn't have a root cause or corrective actions yet. All `_Create`/`_Update` calls in the test suite pass placeholder text (e.g. `N'Root cause pending investigation.'`) for these columns rather than `NULL`. Don't assume a column is nullable from its business meaning — check `sys.columns.is_nullable` first (the No-Guessing Rule applies here too).

## How this relates to `PlantSafety.Tests` / `PlantSafety.IntegrationTests`

tSQLt tests are **not a replacement** for the existing C# test projects — they cover a different layer:

- `PlantSafety.Tests` (unit tests) exercises C# domain/application logic in isolation (handlers, validators, mappers) with mocked repositories — it never touches a real database.
- `PlantSafety.IntegrationTests` exercises the full stack (API → MediatR → EF Core/Dapper → a real or Testcontainers SQL Server instance) — it validates that the C# layer and the database agree, including migrations, but typically only exercises the "happy path" or a handful of scenarios per endpoint, and pays the cost of spinning up a full application host + database per run.
- **tSQLt tests validate the stored-procedure layer itself**, directly, in milliseconds, with zero application-host startup cost, and are the only layer that can practically enumerate T-SQL-specific edge cases like CHECK-constraint rejections, RLS predicate behavior, DDM masking under different roles, and rowversion-based concurrency conflicts — all of which are either impossible or very slow/awkward to provoke reliably from the C# layer. They are the correct place to pin down regressions in the stored-procedure contracts (parameter behavior, error codes, constraint enforcement) that the C# layer calls into but does not itself implement.

Think of it as three complementary layers: **tSQLt** = "does this stored procedure do what its contract says, in isolation" → **`PlantSafety.IntegrationTests`** = "does the whole stack agree end-to-end" → **`PlantSafety.Tests`** = "is the C# business logic correct in isolation from I/O." None of the three subsumes the others.

## Extending to other databases

This pilot only installs tSQLt into `PlantSafety.CoreDB`. To extend to another database (e.g. `PlantSafety.DiagnosticsDB`, which also has RLS from Phase 9), run a copy of `150-install-tsqlt-coredb.sql` with the `USE` statement changed to the target database (tSQLt is a per-database install, not instance-wide), then add a new `NNN-tsqlt-tests-<db>.sql` file following the same test-class/README conventions.

## Fixture objects created by this migration (permanent, by design)

- `tSQLt` schema + ~150 framework objects, `tSQLt.AssertEquals`/etc., and the `tSQLtCLR` assembly (from `150-install-tsqlt-coredb.sql`).
- Two `WITHOUT LOGIN` contained database users used only by test 6: `tSQLt_test_app_executor_user` (member of `app_executor`) and `tSQLt_test_readonly_reporting_user` (member of `readonly_reporting`). These have no server login/credential and cannot connect interactively — they exist solely to be impersonated via `EXECUTE AS USER` inside tests.
- `SPTests` test-class schema and its test procedures + the `SPTests.__Helper_GetIncidentDescriptionNoAudit` helper proc (all recreated on every re-run of `151-tsqlt-tests-coredb.sql` via `tSQLt.NewTestClass`).
- `Compliance.Seq_PermitNumber` is a real, shared production sequence — test 5 permanently consumes 5 values from it each run (small, expected, monotonic gap; `NEXT VALUE FOR` is not transactional/rollback-safe, so this is unavoidable without faking the sequence, which would defeat the point of the test).
