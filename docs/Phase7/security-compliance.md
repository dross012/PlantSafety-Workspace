# Phase 7 §5.1 — SaaS Security Hardening

**Date:** 2026-08-03
**Author:** Security & Compliance Agent

**Scope note (read first):** real penetration testing, a real formal SOC 2/ISO 27001 audit, and
real HIPAA legal review are **not** things this pass performs or claims to have performed. This
document separates (a) real, working code-level controls and tests delivered this pass, (b) an
honest scope document for what a real human security team's pen-test engagement would need to
cover, and (c) organizational/procedural controls that are genuinely out of a codebase's reach.

## 1. Tenant isolation — real, verified

The highest-value, most concrete deliverable of this pass. New integration test suite:
`PlantSafety.IntegrationTests\Tenancy\`:

- **`CrossTenantDataLeakageTests.cs`** — creates real `Accident` aggregates for two tenants via
  the real, DI-resolved `AccidentDapperRepository`, then proves:
  1. `GetByIdAsync` never returns another tenant's row, even given the correct `AccidentId`.
  2. A real Row-Level-Security FILTER+BLOCK policy (mirroring
     `Security.fn_TenantAccessPredicate` from
     `Persistence/Migrations/Phase9-Security/90-row-level-security.sql`, applied directly to the
     real `[AccidentManagement].[Accident]` table) denies a raw ad-hoc SELECT for a mismatched
     `SESSION_CONTEXT('TenantId')` — DB-layer defense-in-depth independent of the repository's own
     WHERE-clause scoping.
  3. `usp_Accident_Update`'s own existence guard throws (SQL error 50004) rather than silently
     no-op-ing when a caller claims a tenant that does not own the target row.
- **`TenantIsolationBehaviorPipelineTests.cs`** — sends a real `GetAccidentByIdQuery` through the
  actual, DI-registered MediatR pipeline (`ISender` → `LoggingBehavior` → `MetricsBehavior` →
  `PerformanceBehavior` → `TenantIsolationBehavior` → `AuthorizationBehavior` → … →
  `TransactionBehavior`, the exact chain registered by
  `ApplicationServiceRegistration.AddApplicationServices`), simulating the server-resolved tenant
  via a real `HttpContext.Items` stamp (the same convention
  `TenantResolutionMiddleware`/`OperationContextAccessor` use in production). Proves
  `TenantIsolationBehavior` rejects a mismatched request `TenantId` with
  `ApplicationAuthorizationException`/`ErrorCodes.TenantMismatch` **through the real pipeline**,
  not merely in a bare unit test of the behavior class — and a companion test proves a matching
  `TenantId` is *not* falsely rejected (reaches the real handler's `NotFound` result).

**Result at time of writing:** see the parent task's final build/test run — these are real xunit
v3 facts executed against a genuine Testcontainers SQL Server instance, not self-reported.

### Existing tenant-isolation controls (verified, not rebuilt)

- `PlantSafety.Application\Behaviors\Security\TenantIsolationBehavior.cs` — rejects any request
  whose bound `Guid TenantId` property mismatches the server-resolved
  `IOperationContext.TenantId`. Confirmed registered as an open-generic `IPipelineBehavior<,>` in
  `ApplicationServiceRegistration.AddApplicationServices` (line ~101) — this registration style
  means it runs on **every** MediatR request in the entire application, not a per-module opt-in;
  there is no code path that resolves `ISender`/`IMediator` without it. `AuthorizationBehavior` is
  registered immediately after it (line ~102), same universal-open-generic style. Both run
  end-to-end on every request path — verified by reading the registration, not assumed.
- `PlantSafety.Infrastructure\Persistence\Migrations\Phase9-Security\90-row-level-security.sql` —
  dynamically discovers every table with a `NOT NULL uniqueidentifier TenantId` column across
  CoreDB/ArchiveDB/DiagnosticsDB and applies a deny-by-default FILTER+BLOCK security policy.
  `DapperQueryExecutor`/`DapperCommandExecutor` (Infrastructure/Persistence/Dapper) already call
  `TenantSessionContextExtensions.SetTenantSessionContextAsync` before every read/write, so this
  is genuinely wired end-to-end for any repository that uses those two chokepoints (essentially
  the entire codebase — see `TenantSessionContextRlsTests.cs`, Phase 6).
- `PlantSafety.SecurityTests\Identity\TenantIsolationTests.cs` — unit-level tenant-isolation
  coverage for `PermissionService` (horizontal privilege-escalation prevention at the permission
  layer). Complements, does not duplicate, the new integration suite above.

## 2. Secrets rotation / Key Vault — verified, not rebuilt

`PlantSafety.Infrastructure\Common\Secrets\{ISecretsProvider, AzureKeyVaultSecretsProvider,
ConfigurationSecretsProvider}.cs` (Phase 6) is correctly wired: `InfrastructureServiceRegistration`
(lines ~673-700) resolves `ISecretsProvider` based on an explicit `Secrets:Provider` config
override, else falls back to environment (`Production` → `AzureKeyVault`, everything else →
`Configuration`), and always wraps the result in `AuditingSecretsProviderDecorator` so every
retrieval is logged (name + outcome only, never the value). `ConfigurationSecretsProvider` itself
throws in its own constructor if resolved while `IHostEnvironment.IsProduction()` is true — a
second, provider-level guard independent of the selection logic above. No changes made; verified
correct.

## 3. Zero-Trust API model — verified, one gap ruled out

- `docs\architecture\authorization-tiers.md` (Phase 6) already performed a full authorization
  consistency pass across all 363 controllers: every controller requires authentication by
  default via a canonical tier system (`Viewer`/`Editor`/`Approver`/`PlatformAdmin`), with a
  reviewed 3-controller `[AllowAnonymous]` allowlist (`PlatformV1Controller`, `PlatformV2Controller`
  — health/version info only — and `WebhookReceiverV1Controller`, which authenticates via webhook
  signature instead of JWT). `ControllerAuthorizationTierTests.cs` (`PlantSafety.Tests`) enforces
  this via reflection over the real controller surface, not a hand-picked sample.
- This pass additionally verified (see §1 above) that `TenantIsolationBehavior` and
  `AuthorizationBehavior` are both registered as open-generic `IPipelineBehavior<,>` — i.e. they
  run on every MediatR request unconditionally, with no per-module opt-out mechanism in the
  registration itself. No further hardening was required here; this was a verification pass, not
  a fix.
- **Known, deliberately-not-fixed latent risk (flagged for manual review):**
  `AccidentDapperRepository.ListAsync` builds its candidate set from
  `usp_Accident_GetAllActive` with **no `@TenantId` parameter at all**, then filters in-memory via
  an `AccidentSpecification` predicate that has no concrete implementations anywhere in the
  codebase today (confirmed: `grep` for `AccidentSpecification` usage outside its own
  interface/base-class definition returns zero call sites in `PlantSafety.Application`). This
  method is therefore dead code — not reachable from any real Command/Query handler — so it is
  not a live cross-tenant exposure today. It is flagged here rather than "fixed" because touching
  a method with a public interface contract and no current caller, purely to future-proof it,
  risks masking the real issue (this interface method's shape needs a `TenantId` parameter added
  to its contract, which is an `IAccidentRepository` interface change requiring its own reviewed
  pass) — recorded for a human to decide alongside any future caller of `ListAsync`.

## 4. Automated security scanning in CI/CD — genuine gap found and closed

**Before this pass:** `.github\workflows\ci.yml` (Phase 6.6) had **zero** SAST and **zero**
dependency-vulnerability-scanning steps — confirmed by reading the full pipeline file; it covered
build, unit tests, SQL migration validation, and a conditional contract-test gate only.

**Added this pass (real, runnable GitHub Actions steps — same "will not execute end-to-end in
this sandbox, but is a correct, ready-to-run artifact" caveat as the rest of this pipeline, per
its own file header):**

- **`dependency-vulnerability-scan` job** — `dotnet list "<solution>" package --vulnerable
  --include-transitive`, a real .NET SDK command that cross-references every direct and
  transitive NuGet package against the NuGet Package Vulnerability Data feed. No external
  service/secret required. Registered as a soft gate (`continue-on-error: true`) because the
  current vulnerable-package baseline for this ~30-project solution has not yet been triaged by a
  human — flip to a hard gate once that first triage pass completes.
- **`codeql-sast` job** — standard GitHub-native CodeQL static analysis for C#
  (`github/codeql-action/init` + `analyze`). Requires only that "Code scanning" be enabled once
  this repository is pushed to a real GitHub remote (Settings → Code security and analysis) — no
  fabricated results, no external SaaS dependency beyond GitHub itself.

### What a real penetration-testing engagement would still need to cover (scope document for a human security team — no fabricated results here)

A real pen test of this platform, grounded in its actual stack (ASP.NET Core API, SQL Server with
RLS, JWT/Azure AD auth, MediatR pipeline, MassTransit/RabbitMQ messaging), should cover at minimum:

1. **Authentication/session attacks** — JWT signature/algorithm-confusion attacks, token replay,
   refresh-token handling, session fixation against the real Azure AD/Entra ID integration (not
   testable in this sandbox — no real Azure AD tenant is configured here).
2. **Authorization bypass attempts** beyond what `ControllerAuthorizationTierTests`/
   `AuthorizationPolicyRegistrationTests` already check structurally: IDOR probing across the
   ~363 controllers' route parameters, privilege-escalation via role-claim tampering, and the
   specific `ListAsync` latent risk flagged in §3 above once it gains a real caller.
3. **Injection testing** — this codebase's Prohibited Technology rule (Handbook §2.16B) bans
   dynamic SQL platform-wide (all data access is via stored procedures with typed parameters via
   `StoredProcedureBuilder`/Dapper), which structurally reduces classic SQL injection surface, but
   a real pen test should still verify no dynamic-SQL exception exists anywhere in the ~400+
   stored procedures, and probe the few raw-`SqlCommand`/ad-hoc-query code paths used by
   diagnostics/reporting tooling.
4. **RLS bypass attempts** — a real pen test with direct database credentials (not just
   application-layer access) should attempt to read across tenants using an application service
   account's actual production connection string, to confirm the deny-by-default RLS policy (see
   §1) holds even for a compromised app-tier credential, not just via the application's own code
   paths.
5. **Rate limiting / DoS resilience** — not assessed in this pass; no rate-limiting middleware was
   found or added.
6. **Webhook signature verification** (`WebhookReceiverV1Controller`, the one `[AllowAnonymous]`
   controller with external-facing risk) — a real pen test should attempt signature-forgery and
   replay attacks against the real webhook secret/signing scheme once one is configured in a real
   environment (this sandbox has no real webhook provider configured).
7. **Dependency/supply-chain attacks** — the new `dependency-vulnerability-scan` CI job (above)
   covers known-CVE detection; a real engagement should also assess build-pipeline integrity
   (build provenance, package-source pinning) which is outside this pass's scope.
8. **Multi-tenant SaaS-specific tests**: tenant-enumeration via timing/error-message differences,
   cross-tenant correlation-id/log leakage (see `TenantAuditStreamAndCorrelationIsolationTests.cs`,
   §5 of `docs\Phase7\compliance-framework-mapping.md`'s companion audit doc, for the code-level
   guard this pass added), and subdomain/tenant-routing confusion attacks once a real
   multi-tenant routing layer (beyond JWT-claim-based `TenantId` resolution) is provisioned.

None of the above was executed as a real pen test in this pass — this is a scope document for a
human security team, not a report of findings.

## 5. Summary table

| Control | Status |
|---|---|
| Tenant isolation (app-layer + RLS), real integration tests | **Delivered this pass** — `PlantSafety.IntegrationTests\Tenancy\*` |
| `TenantIsolationBehavior` real end-to-end pipeline test | **Delivered this pass** |
| Secrets rotation / Key Vault | **Verified correct** (Phase 6, unchanged) |
| Zero-Trust API authorization tiers | **Verified correct** (Phase 6, unchanged); one latent dead-code risk flagged |
| SAST in CI | **Gap found and closed** this pass (`codeql-sast` job) |
| Dependency vulnerability scanning in CI | **Gap found and closed** this pass (`dependency-vulnerability-scan` job, soft gate) |
| Real penetration test | **Not performed** — scope document above for a human security team |
