# Shape Audit — Reporting

Phase 5 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.
Input: `module-flow/reporting.md`, `classification-matrix.md`, `gaps/orphaned-ingress.md`
(Phases 1-4).

## Top items for the Reporting module

1. **There is no DTO to domain to SQL chain to audit -- the Reporting module has no persistence
   layer of any kind.** Confirmed by Phase 3 (directly verified: no repository/`DbContext`
   dependency injected into `GenerateReportCommandHandler`) and re-confirmed this phase:
   `find Reporting -iname "*.sql"` returns 0 results, `grep -rl "DbContext" Reporting` returns 0
   results. All 5 endpoints either return static catalog/metadata information or render a PDF from
   data pulled through `IReportGenerationOrchestrator` (internals not opened, per Phase 3) and
   stream it back in the same HTTP response. No row is ever written; there is no schema to drift
   against.
2. **The nearest thing to a "shape" question in this module is whatever `Get{ReportType}
   PreviewQuery`/`GenerateReportCommand` pull *from* other modules' data** (the orchestrator
   presumably queries PlantSafety/other modules' tables to build the rendered document) -- but that
   is a read-side query shape into someone else's already-audited schema, not a
   Reporting-module-owned write path. Tracing that would mean re-auditing whichever PlantSafety
   tables feed into document generation, which is out of scope for a Reporting-specific file and is
   better covered by whichever PlantSafety feature areas are the actual data source (not identified
   this phase -- `IReportGenerationOrchestrator`'s internals were not opened by Phase 3 or this
   phase).
3. **Carried forward from Phase 2/4, restated here because it is adjacent to (not itself) a shape
   issue**: all 5 endpoints have zero authentication wired, meaning an unauthenticated caller can
   trigger document generation. This is an access-control item, not a data-shape item, and is
   already the subject of Phase 4's most severe finding for this module -- not re-litigated here
   beyond this cross-reference.

## Why this module produces no findings table

The deliverable's table format (Field | DTO/Request Type | Domain Type | EF/SQL Column Type |
Nullability Match? | Truncation Risk? | Enum Drift? | Populated By Any Ingestion Path? | Confidence
| Notes) requires a persisted column to compare against. This module persists nothing: `generate`/
`export` return an in-memory PDF byte array directly in the HTTP response body, and the 3 read
endpoints (`types`, `metadata`, `preview`) don't touch storage at all. Fabricating a table here
would either be empty or would have to reach into another module's schema under a misleading
module heading, which this phase avoids per the same discipline `azure-shape.md` and
`datalake-shape.md`/`intelligence-shape.md` apply to their own out-of-scope or unverifiable seams.

## What was directly checked to reach this conclusion

- `find Reporting -iname "*.sql"` returned 0 results.
- `grep -rl "DbContext" Reporting` returned 0 results.
- `Reporting.Api\Controllers\v1\ReportsController.cs` and `GenerateReportCommandHandler` -- read in
  full by Phase 3, re-confirmed this phase: no repository dependency injected, no domain aggregate,
  no event published.

## Recommendation for a future phase

If this module is ever extended to persist generated-document metadata (e.g. an audit trail of
"who generated what document when," which would also address part of the actor-identity gap
Phase 2/4 already flagged for this module), that is the point at which it would need its first real
shape audit. As built today, this module contributes zero rows to the solution-wide shape-drift
items for the same reason as Azure: the seam this phase audits does not exist in this module's
current architecture.
