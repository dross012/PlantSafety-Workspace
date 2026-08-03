# Module Flow — ExportImport

Phase 3 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.

## Status: built but unwired/orphaned — no live ingress path

Per Phase 1 (`inventory.md`, `file-ingress.md`) and Phase 2 (`classification-matrix.md`),
ExportImport is the one module in the solution with genuine, working file-format import adapters
(`CsvAdapter`, `JsonAdapter`, `XmlAdapter`, `ExcelAdapter` via ClosedXML, all implementing
`IImportFileAdapter`), resolved through `FileAdapterFactory` and orchestrated by
`ImportPipelineService` / `StartImportCommandHandler` / `PreviewImportCommandHandler` /
`ReplayImportCommandHandler`, with a validated command contract
(`StartImportCommandValidator` + `ValidationBehavior`) and events
(`ImportStarted`/`ImportCompleted`/`OperationFailed`) that a real caller could react to. However,
**ExportImport has no `*.Api` project and no controller anywhere in the solution** — confirmed by
Phase 1's grep of the entire tree for `ExportImport.Application`, `ExportImport.Infrastructure`,
`StartImportCommand`, and `ImportPipelineService` outside the `ExportImport` folder itself, which
returned zero hits. There is no HTTP endpoint, message consumer, or scheduled job anywhere in the
solution that ever calls into this module's command handlers.

Because there is no ingress point, this module is excluded from the flow-map proper: there is no
controller/consumer/job to name as a receiving class, so "which module/layer receives the
request" has no answer for ExportImport today — nothing does. The domain model, lifecycle
transitions, repository, and events this pipeline *would* produce if wired are fully described in
`file-ingress.md` and the "Validated?"/"Lifecycle-triggering?" rows of `classification-matrix.md`
(section 3); this document does not repeat that detail, since none of it is currently reachable
from outside the module. If Phase 4 or a later engineering effort wires a controller, consumer, or
scheduled job to `StartImportCommand`, this module should be re-traced as a live flow at that
point — note also (per `classification-matrix.md` section 3) that `StartImportCommand` as it
exists today carries no `TenantId` or actor/user field, so wiring it up would additionally require
adding tenant and actor scoping to the command contract itself.
