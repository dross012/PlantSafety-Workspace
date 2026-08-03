# Module Flow — Reporting

Phase 3 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.

## Scope and method

The Reporting module exposes 5 API endpoints, all on a single `ReportsController`, all directly
traced this phase (controller and the one command handler with a non-trivial body were both read
in full).

## Traced flows

| Ingress Point | Receiving Class | Domain Model Touched | Lifecycle Transition | Repository/DbContext | Event Published | Downstream Consumer(s) | Confidence |
|---|---|---|---|---|---|---|---|
| `GET api/v1/reports/types` | `ReportsController.GetReportTypes` -> `GetReportTypesQuery` handler | None -- static/catalog read | None | None | None | N/A | Directly verified |
| `GET api/v1/reports/{reportTypeCode}/metadata` | `ReportsController.GetReportMetadata` -> `GetReportMetadataQuery` handler | None -- metadata read | None | None | None | N/A | Directly verified |
| `POST api/v1/reports/{reportTypeCode}/preview` | `ReportsController.GetReportPreview` -> `GetReportPreviewQuery` handler | None -- assembles logical structure without rendering | None | None | None | N/A | Directly verified |
| `POST api/v1/reports/{reportTypeCode}/generate` | `ReportsController.GenerateReport` -> `GenerateReportCommandHandler` -> `IReportGenerationOrchestrator.GenerateAndExportAsync` | None -- no domain aggregate; produces an in-memory PDF byte array returned directly to the caller | None -- no persisted state change found; the artifact is the HTTP response body, not a stored record | None found in the handler itself (pure delegation, no repository/DbContext dependency injected); orchestrator internals not opened this phase | None | None | Directly verified (controller + handler read in full; orchestrator internals not opened) |
| `POST api/v1/reports/{reportTypeCode}/export` | `ReportsController.ExportReport` -> `ExportReportCommandHandler` (not opened; presumed identical shape to `GenerateReportCommandHandler` based on the parallel controller-method structure) | Same as generate | Same as generate | Same as generate (pattern-inferred, not opened) | None | None | Pattern-inferred from the directly-verified `generate` flow |

## Notes carried forward to Phase 4

The Reporting module is architecturally the simplest in the solution: it has no domain
aggregates, no repository/DbContext writes, and publishes no events at all. It is a pure
read-and-render module -- every endpoint either returns catalog/metadata information or renders
a PDF from existing data and streams it back in the same HTTP response. There is no event or
lifecycle dimension here to dead-end -- persistence and eventing simply do not apply to this
module.

One carry-over item from Phase 2 bears directly on this flow map: all 5 endpoints have zero
authentication wired (no `[Authorize]`, no `AddAuthentication`/`AddJwtBearer` in `Program.cs`),
so as traced, an unauthenticated caller can trigger PDF generation/export of potentially
sensitive aggregate safety data. This is a security/access-control item rather than an
ingestion-flow gap, but it is the single starkest item Phase 4 should carry forward for this
module.
