# Modernization Recommendations — ExportImport

Phase 9 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
ExportImport is a fully-built file-import pipeline (CSV/JSON/XML/Excel adapters, validated
command contracts, orchestration service, publish events) with **zero live callers anywhere in the
solution**. This document makes the call: wire it up, don't deprecate it — see the justification
below.

---

## Top 5 fixes, in priority order

1. **Decide and act: wire ExportImport up, don't deprecate it.** See "Wire up or deprecate?"
   below for the justification. This is the prerequisite decision every other item in this
   document depends on. Effort: N/A (a decision, not code) — but blocking.
2. **Add a `*.Api` project with at least a `POST /api/v1/imports` controller** that accepts a
   file upload (this also closes the solution-wide "zero `IFormFile` usage anywhere" gap found in
   Phase 1) and dispatches `StartImportCommand`. Effort: M. Risk of not fixing: **None-just-tech-debt**
   today (the capability sits unused) — but represents real, working engineering effort with zero
   return until this lands.
3. **Add `TenantId` and `ActorId`/`UserId` fields to `StartImportCommand`** before wiring any
   caller to it — the command as it exists today has no tenant or actor scoping at all
   (`CommandId, CorrelationId, CausationId, RequestedAtUtc, ApiVersionToken, FileContent,
   FileFormat, DatasetName, RequiredFields, BatchSize` — no `TenantId`, no actor field). Effort: S,
   but must happen *before* item #2, not after — do not expose an HTTP endpoint to an
   unscoped command.
4. **Register a consumer for `ImportStarted`/`ImportCompleted`/`OperationFailed`** once a live
   caller exists, so the pipeline's own events (which are correctly designed) actually reach
   something. Effort: S-M, and only relevant once items #1-3 land.
5. **If, after this discovery, the team instead decides to formally deprecate this module**,
   remove it explicitly (delete the code, note the decision in the module's own README/header
   comments) rather than leaving it as permanently-orphaned code that will keep confusing future
   audits. Effort: S. This is the alternative to items #1-4, not an additional item.

---

## Wire up or deprecate? This document's call: wire it up.

**Justification**: ExportImport is qualitatively different from SafetyIntegrations' orphaned
adapter (see that module's own recommendations doc). ExportImport has:
- Four genuinely working, well-tested format adapters (`CsvAdapter`, `JsonAdapter`, `XmlAdapter`,
  `ExcelAdapter` via ClosedXML), each implementing a clean `IImportFileAdapter` interface.
- A real orchestration layer (`ImportPipelineService`, `StartImportCommandHandler`,
  `PreviewImportCommandHandler`, `ReplayImportCommandHandler`) with validated command contracts
  (`StartImportCommandValidator` + `ValidationBehavior` wired in DI).
- Correctly-designed lifecycle events (`ImportStarted`/`ImportCompleted`/`OperationFailed`) that a
  real caller could react to.
- It fills a genuine, solution-wide gap: this discovery found **zero `IFormFile`-based HTTP
  upload endpoint anywhere in the entire 14,090-file solution** and **zero `CsvHelper`/`CsvReader`
  usage anywhere** — meaning today, if any module needed to accept a CSV/Excel/JSON/XML file from
  a user, there is no working, wired path to do it. ExportImport is that path, sitting unused.

This is a materially stronger case for investment than either "delete it" or "leave it as-is."
Deprecating fully-built, well-designed, tested infrastructure that fills a real solution-wide
capability gap would be a worse outcome than the modest effort (add one controller, add two
missing fields to one command) needed to make it live.

**The one thing that must happen first, not after**: adding `TenantId`/`ActorId` to
`StartImportCommand` (Top-5 #3) before adding any HTTP entry point (Top-5 #2) — do not expose an
unscoped import pipeline to real HTTP traffic, even briefly.

---

## Data Integrity

### 1. `StartImportCommand` carries no `TenantId` or actor/user field at all
- **Problem**: Full read confirms the command's fields:
  `CommandId, CorrelationId, CausationId, RequestedAtUtc, ApiVersionToken, FileContent, FileFormat,
  DatasetName, RequiredFields, BatchSize`. No `TenantId`, no `ActorId`/`UserId`. Currently N/A in
  terms of live risk (module has no live caller), but this is a concrete, actionable design gap
  for whoever wires this module up. (`gaps/tenant-violations.md` §6, `classification-matrix.md`
  §3.)
- **Fix**: Add `TenantId` (required, non-nullable — follow DataLake's `DataLakeControllerBase`
  pattern of a throwing accessor, not PlantSafety's client-suppliable pattern) and `ActorId`
  (required) to `StartImportCommand`, threaded from whatever authenticated context the new
  controller (Top-5 #2) establishes. Also add corresponding `MaximumLength`/format validation to
  `StartImportCommandValidator`.
- **Effort**: S.
- **Risk of not fixing**: **N/A today** (module unreachable); **security/data-integrity** the
  moment any caller is wired without this fix landing first.

---

## Event Wiring

### 2. `ImportStarted`/`ImportCompleted`/`OperationFailed` are correctly designed, never triggered
- **Problem**: These events would publish correctly if the module were ever invoked — the
  publishing code itself is sound — but nothing calls `StartImportCommandHandler`/
  `ImportPipelineService` anywhere in the solution today, so the code never executes.
  (`events/publishing-map.md` §ExportImport.)
- **Fix**: No consumer needs to be built speculatively before there's a real caller — this is
  naturally resolved once Top-5 #1-3 land and a real business need for reacting to import
  completion/failure is identified (e.g. notifying the initiating user, updating an import-status
  read model).
- **Effort**: S-M, once there's a live caller and a defined downstream need.
- **Risk of not fixing**: **None-just-tech-debt** until the module is wired up.

---

## Deliberately not recommended

- **Not recommending building a generic "file upload" UI/endpoint speculatively wired to
  ExportImport without a concrete first caller in mind.** Wiring this up should be driven by an
  actual product need (which module/workflow needs to accept a CSV/Excel import first?), not
  built as an abstract capability with no consumer in mind — that would repeat exactly the mistake
  this whole discovery found everywhere else (infrastructure built, nothing calls it).
