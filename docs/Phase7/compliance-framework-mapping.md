# Phase 7 §5.2 — Compliance Framework Control Mapping

**Date:** 2026-08-03
**Author:** Security & Compliance Agent

**Purpose:** an honest, accurate mapping of this codebase's REAL, existing mechanisms against
SOC 2, ISO 27001, GDPR, HIPAA (applicability assessed, not assumed), and NIST control families.
For every control family this document states: (a) what the codebase already satisfies, citing
the actual file/mechanism; (b) genuine code-level gaps; (c) what is purely organizational/
procedural and out of scope for a codebase entirely. This is not a certification, an audit
result, or a substitute for a real assessment by a qualified auditor/legal counsel — it is a
grounded engineering reference for what a real SOC 2/ISO 27001 readiness assessment or GDPR/HIPAA
review would find already in place versus still needed.

## 0. Does PlantSafety plausibly fall under HIPAA?

**Short answer: not as a general rule, but not zero risk either — flagged for legal review, not
assumed either way.**

PlantSafety is an industrial/manufacturing safety platform (incidents, accidents, permits, risk
management, equipment/machine safety, compliance/audit tracking). Its core business data is
**not** Protected Health Information under a normal reading of HIPAA — it is occupational-safety
and operational data. However, two real modules in this codebase touch employee-health-adjacent
data closely enough that HIPAA *could* become relevant in specific deployment scenarios, and this
should be resolved by legal counsel, not by code assumption:

- `WorkerWellBeingV1Controller` / the WorkerWellBeing bounded context (Domain classification:
  "Restricted (PII, well-being data, AI training data)" per
  `project_plantsafety_infrastructure_security.md`'s PII & Well-Being Data Isolation section) —
  captures employee wellness/well-being signals.
- `AccidentManagement`'s injury/incident records (`InjuredPersonCount`, injury severity,
  regulatory filing fields) — occupational injury data, which is regulated under OSHA
  recordkeeping rules (a **different**, non-HIPAA regulatory regime) in the U.S., but could
  overlap with HIPAA if a specific customer's deployment integrates this data with a covered
  entity (e.g., an occupational-health clinic or workers'-comp insurer acting as a HIPAA business
  associate).

**Conclusion for this codebase:** HIPAA is **not treated as a default-applicable framework**
for PlantSafety's core safety/compliance domain, but the platform's existing Data Classification
Levels (Public/Internal/Confidential/**Restricted**) and PII/well-being isolation rules already
provide the technical baseline (access logging, masking, encryption) that HIPAA's Security Rule
would also require *if* a specific customer's use of WorkerWellBeing data brought them into HIPAA
scope. This is a genuine "if needed" per the task's own framing — flagged for a real legal/
compliance determination per customer contract, not built out as a full HIPAA compliance program
in this codebase.

## 1. Access Control

**SOC 2:** CC6.1, CC6.2, CC6.3 | **ISO 27001:** A.9 (Access Control) | **GDPR:** Art. 32
(Security of Processing) | **NIST 800-53:** AC-2, AC-3, AC-6

| | Detail |
|---|---|
| **Codebase satisfies** | Role/permission-based authorization on every controller (`docs\architecture\authorization-tiers.md`, `ControllerAuthorizationTierTests.cs` — 363 controllers reflected over, four canonical tiers Viewer/Editor/Approver/PlatformAdmin). Defense-in-depth re-check at the MediatR layer (`AuthorizationBehavior`). Tenant-boundary access control (`TenantIsolationBehavior`, RLS — see §2 Audit Logging and `docs\Phase7\security-compliance.md` §1). Every access decision (allow/deny) is now logged (`PermissionUsageLogRepository`, Phase 6 §5.4) — this pass added the tenant-scoped read side (`GetTenantAuditStreamQuery`). |
| **Genuine code-level gap** | No formal periodic access-review workflow exists in code (e.g. a scheduled job that surfaces "users with permissions unused for 90 days" for human review) — `plantsafety_compliance_checklist.md`'s KPI of "100% quarterly access reviews" has no code-level automation today. |
| **Organizational/out of scope** | MFA enforcement is an Azure AD/Entra ID tenant policy, not application code (Technology Version Matrix already specifies Azure AD/MSAL as the identity provider — MFA policy lives in that IdP's configuration, external to this repo). Background-check-gated account provisioning, HR offboarding triggering account deactivation — procedural. |

## 2. Audit Logging

**SOC 2:** CC7.2 | **ISO 27001:** A.12.4 (Logging and Monitoring) | **GDPR:** Art. 30 (Records of
Processing) | **NIST 800-53:** AU-2, AU-3, AU-6, AU-12

| | Detail |
|---|---|
| **Codebase satisfies** | Canonical structured log envelope with `CorrelationId`/`CausationId`/`ActorId`/`TenantId` on every log entry (Handbook §2.19-2.22, `project_plantsafety_infrastructure_security.md` Part VI). `dbo.AuditLog`/`dbo.ErrorLog` written by every stored procedure's TRY/CATCH pattern (verified directly in `AccidentConcurrencyAndAuditIntegrationTests.AccidentInsertAndUpdate_ShouldWriteAuditLogRows`). `[Auditing].[PermissionUsageLog]` records every authorization decision, allow or deny (`AuthorizationBehavior`). **New this pass:** `GetTenantAuditStreamQuery` (`PlantSafety.Application\Auditing\ComplianceMonitoring\PermissionUsage\Queries\`) makes that access log genuinely queryable **per tenant** — proven by `TenantAuditStreamAndCorrelationIsolationTests.cs`, which also proves CorrelationIds never leak across tenant boundaries through this stream (two tenants' entries in the same physical table, same time window; each tenant's query returns only its own rows and never the other tenant's CorrelationId). `GetDataLineageQuery` (Phase 6 §5.4) reconstructs cross-system event chains from the existing Outbox/DeadLetterEvents infrastructure — "which system/event produced this record". |
| **Genuine code-level gap** | No `ComplianceDashboard` UI surface exists yet specifically for this new tenant audit stream (see §4 Dashboards note below — the existing `AuditDashboard`/`PlatformHealthDashboard` cover adjacent but not identical ground; extending one of them with a compliance-audit-stream widget is a reasonable, not-yet-built follow-up). Log retention automation (`Log Retention & Lifecycle Rule`, Handbook §2.32) is a documented rule; this pass did not verify a scheduled job enforces the stated 7+ year audit-log retention class end-to-end. |
| **Organizational/out of scope** | Actual SIEM ingestion/alerting on these logs (Splunk/Sentinel/etc.) is an operations-team integration, not application code. |

## 3. Encryption at Rest / In Transit

**SOC 2:** CC6.7 | **ISO 27001:** A.10 (Cryptography) | **GDPR:** Art. 32(1)(a) | **NIST 800-53:**
SC-8, SC-28

| | Detail |
|---|---|
| **Codebase satisfies** | Secrets never in source control or config files (`Common\Secrets\*`, Key Vault-backed in production, §5.1 of the companion security-compliance.md doc). Data classification levels (Public/Internal/Confidential/Restricted) documented and referenced by the PII/well-being isolation rules. RLS + tenant-scoped stored procedures reduce blast radius of any single credential compromise (defense-in-depth, not encryption per se). |
| **Genuine code-level gap** | **TLS/HTTPS enforcement is NOT implemented inside `PlantSafety.Api` itself** — `SecurityHeadersMiddleware`'s own file header states TLS termination is delegated to "the Azure Front Door / Application Gateway layer in production" and no `UseHttpsRedirection()`/`UseHsts()` call exists in `PlantSafety.Api\Program.cs`. This is a defensible architecture (TLS termination at the edge is standard for Front Door/App Gateway deployments) **but it is unverified from inside this codebase alone** — a real deployment audit must confirm the edge layer's TLS configuration (cipher suites, certificate rotation, minimum TLS version) since the application itself provides no independent enforcement or fallback. No **Transparent Data Encryption (TDE)**/Always Encrypted configuration was found anywhere in the SQL migration scripts — encryption-at-rest for the SQL Server databases is therefore an infrastructure/DBA-layer configuration this codebase does not control or verify. mTLS for internal service-to-service traffic (`project_plantsafety_infrastructure_security.md`'s Zero-Trust Enforcement Matrix explicitly requires it) has no corresponding code artifact found in this pass — it would be a service-mesh/Kubernetes-ingress-layer concern, not application code, but no evidence of it being configured anywhere in this repo was found either (genuinely unverifiable from source alone). |
| **Organizational/out of scope** | Actual TLS certificate provisioning/rotation at the App Gateway/Front Door layer, Azure Key Vault's own HSM-backed encryption-at-rest for secrets, and SQL Server TDE key management are all infrastructure/cloud-configuration concerns outside this repository. |

## 4. Incident Response

**SOC 2:** CC7.3, CC7.4 | **ISO 27001:** A.16 (Incident Management) | **GDPR:** Art. 33/34 (Breach
Notification) | **NIST 800-53:** IR-4, IR-6, IR-8

| | Detail |
|---|---|
| **Codebase satisfies** | Severity-leveled incident model documented (`Sev1`-`Sev4`, `project_plantsafety_infrastructure_security.md` Part VII) with a defined escalation path. Every incident/exception already carries `CorrelationId` end-to-end (Handbook §2.17-2.18 Canonical Correlation & Causality Lineage Rule), which is the concrete technical prerequisite for any real incident timeline reconstruction — and `GetDataLineageQuery`/`GetTenantAuditStreamQuery` (§2 above) are exactly the tools a real incident responder would use to reconstruct "what happened, for which tenant, in what order" from this codebase's real data. `docs\Phase7\multi-region-dr-runbook.md` (Phase 7 §4) documents the concrete failover sequence for a Sev1 regional outage. |
| **Genuine code-level gap** | No automated Sev1/Sev2 alerting/paging integration (PagerDuty/Opsgenie-style) exists in code — the escalation path is documented policy, not wired automation. GDPR's 72-hour breach-notification clock has no code-level trigger/tracking artifact (e.g., a `BreachNotificationDeadline` domain concept) anywhere in this codebase. |
| **Organizational/out of scope** | The actual human on-call rotation, RCA authorship/review, and regulatory breach-notification filing (the 72-hour GDPR clock, HIPAA's 60-day clock if ever applicable per §0) are procedural/legal actions a codebase cannot perform on its own — it can only supply accurate, timely data (which §2's audit stream work directly supports). |

## 5. Data Retention

**SOC 2:** CC6.5 | **ISO 27001:** A.11.2.7 (equipment/media disposal), A.18.1.3 (records
protection) | **GDPR:** Art. 5(1)(e) (Storage Limitation) | **NIST 800-53:** SI-12, MP-6

| | Detail |
|---|---|
| **Codebase satisfies** | Documented retention classes (Operational 3-7yr, Audit Logs 7+yr, Evidence per-regulatory, PII minimum-necessary — `project_plantsafety_infrastructure_security.md` Part V). `Persistence/Migrations/Phase7-*/74-retention-archival.sql` (referenced by the Phase 9 RLS migration's own ArchiveDB exclusion note) implements real scheduled retention/archival stored procedures. |
| **Genuine code-level gap** | This pass did not find a single canonical "retention policy enforcement" report/job proving every table's actual retention matches its documented class — the archival job's *existence* is confirmed, but a full table-by-table retention-compliance audit was outside this pass's scope. |
| **Organizational/out of scope** | Deciding the correct retention period per regulatory jurisdiction (varies by country/industry) is a legal/compliance-office decision, not a code default. |

## 6. Change Management

**SOC 2:** CC8.1 | **ISO 27001:** A.12.1.2 (Change Management) | **GDPR:** N/A (not a GDPR
control family) | **NIST 800-53:** CM-3, CM-4

| | Detail |
|---|---|
| **Codebase satisfies** | Gated CI/CD pipeline (`.github\workflows\ci.yml`): build → unit tests → SQL migration validation → contract-test gate → Dev/Test/Staging/Prod promotion, each requiring the prior stage green, with manual-approval gating documented for Production (`Operational Runbook` Part VII). Migrations are additive-only, forward-fix (no destructive rollback scripts), applied via a dedicated migration job before new pods start (blue-green pattern, documented). This pass added two further CI gates (`dependency-vulnerability-scan`, `codeql-sast` — see `docs\Phase7\security-compliance.md` §4), directly strengthening this control family's automated-testing evidence. |
| **Genuine code-level gap** | The CI pipeline's actual deploy steps are placeholders (`docs\Phase6\deployment-release-architecture.md`'s flagged open ambiguity: target deploy platform undetermined) — change management's "deployment is gated and auditable" claim is only as strong as whatever real deploy mechanism a human ultimately wires in; today's placeholder cannot be audited for real. |
| **Organizational/out of scope** | The actual change-advisory-board/peer-review process, and enforcement that no one merges without human code review, are procedural (GitHub branch-protection-rule configuration, not this repo's source). |

## 6a. NIST 800-53 cross-cutting note

The families above already map most of the NIST controls this platform's code can plausibly
satisfy (AC-*, AU-*, SC-8/28, IR-*, CM-*, SI-12, MP-6). NIST's broader catalog (hundreds of
controls across 20 families) includes many controls that are inherently organizational
(personnel security PS-*, physical/environmental PE-*, program management PM-*, media protection
device-level controls) — those are out of scope for a codebase by definition and are not
enumerated individually here to avoid manufacturing a false sense of completeness.

## Summary: satisfied vs. gap vs. out-of-scope

| Framework | Satisfied by code | Genuine code gap | Organizational/out of scope |
|---|---|---|---|
| SOC 2 | Access control (tiers+behaviors), audit logging (PermissionUsageLog+lineage+tenant stream), gated CI/CD, secrets management | Periodic access-review automation, TLS enforcement verification, deploy-step placeholders | MFA policy, on-call rotation, change-advisory-board process |
| ISO 27001 | A.9 (access control), A.12.4 (logging), A.12.1.2 (change mgmt), A.10 (secrets handling) | A.10 crypto verification gap (TDE/mTLS unconfirmed), A.16 automated alerting | A.7 personnel security, A.11 physical security |
| GDPR | Art. 32 (RLS+access control+secrets), Art. 30 (audit records via PermissionUsageLog/lineage) | Art. 5(1)(e) full retention audit, Art. 33 72-hour breach-clock tracking | Legal basis determination, DPA contracts, DPO appointment |
| HIPAA | *(if applicable per §0)* same Restricted-data isolation/audit mechanisms as GDPR/SOC2 above | No HIPAA-specific BAA tracking or minimum-necessary enforcement beyond existing PII rules | Applicability determination itself (legal), BAAs, workforce HIPAA training |
| NIST 800-53 | AC-2/3/6, AU-2/3/6/12, CM-3/4 (see families above) | SC-8/28 verification, IR-4/6 automated alerting | PS-*, PE-*, PM-* (personnel/physical/program mgmt) |

## Related documents

- `docs\Phase7\security-compliance.md` — tenant-isolation test results, CI/CD SAST/dependency-scan
  gap closure, and the real pen-test scope document.
- `docs\Phase7\multi-region-architecture.md` — data-residency infrastructure reality check
  (single-region deployment today).
- `project_plantsafety_infrastructure_security.md` / `plantsafety_compliance_checklist.md` (memory
  manuals) — the canonical policy source this document maps code against.
