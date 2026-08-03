# Module Flow — SafetyTooling

Phase 3 of Ingestion Pipeline Discovery. Root: `C:\Users\dross\source`. Date: 2026-07-22.

## Status: no live ingress path (PDF generation library, not an ingestion surface)

Per Phase 1 (`inventory.md`, `file-ingress.md`) and Phase 2 (`classification-matrix.md`),
SafetyTooling has zero API controllers, zero message-broker/event-consumer classes, and zero
scheduled-job/hosted-service classes. Its one substantial piece of production code relevant to
this discovery is `SafetyReporting.Pdf` (`PdfDocument`, `IPdfDocumentBuilder`, `TemplateLinter`,
`TemplateProfiler`, `PdfDebugViewInspector`, etc.), which the Phase 1 scan's `PdfDocument`/
`IPdfDocumentBuilder` keyword search matched 45 times — but this is a PDF **generation/output**
library used to *build* PDF reports from data the caller already has in memory, not a path that
*ingests* an externally supplied PDF, image, or any other file. There is no code in this module
that reads or parses an uploaded/incoming document of any kind. SafetyTooling's other content
(3 `.sql` files under `Analyzers.IntegrationTests\Sql\Scripts\`) are test fixtures, not ingestion
scripts.

Because there is no controller, consumer, or job that receives external or system-initiated input
into this module, it is excluded from the flow-map proper: there is no domain model touched by an
ingress event, no lifecycle transition, no repository/DbContext write, and no event
published/consumed as part of any ingestion flow. This finding was carried forward unchanged from
Phase 1; this phase did not find any code path that would reclassify SafetyTooling's PDF library
as an ingestion surface.
