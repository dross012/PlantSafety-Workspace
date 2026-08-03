# File Ingestion Ingress — Ingestion Pipeline Discovery (Phase 1)

Extracted by grepping all `.cs` files for `IFormFile`, `CsvReader`/`CsvHelper`,
`ExcelPackage`/`ClosedXML`/`IXLWorkbook`, and `iTextSharp`/`PdfReader`/`PdfDocument`, then verified
by reading the matched files. File paths are relative to `C:\Users\dross\source` unless already
absolute.

**Headline finding: no `IFormFile`-based HTTP upload endpoint exists anywhere in the scanned tree.**
Zero matches for `IFormFile` across all 14,090 `.cs` files scanned. There is also no `CsvHelper`/
`CsvReader` usage. File-format ingestion is real but happens through a **library-level adapter
pattern**, not an HTTP multipart upload:

- **ExportImport** module has genuine CSV/JSON/XML/Excel *import* adapters — `CsvAdapter`,
  `JsonAdapter`, `XmlAdapter` (custom parsers) and `ExcelAdapter` (via ClosedXML), all implementing
  `ExportImport.Application.Shared.Interfaces.IImportFileAdapter`, resolved through
  `FileAdapterFactory` and invoked by `ImportPipelineService` /
  `StartImportCommandHandler`/`PreviewImportCommandHandler`/`ReplayImportCommandHandler`.
  **However, the ExportImport module has no `*.Api` project and no controller anywhere in the
  solution references `ExportImport.Application`/`ExportImport.Infrastructure`** (verified by
  grepping the whole tree for `ExportImport.Application`/`ExportImport.Infrastructure`/
  `StartImportCommand`/`ImportPipelineService` outside the ExportImport folder itself — zero
  hits). This means: **the file-import capability exists as a library but currently has no wired
  entry point (HTTP, message, or scheduled) anywhere else in the solution.** Flagged as
  **unverified/needs follow-up** — either an entry point exists that this scan's patterns didn't
  catch, or this is dead/unfinished integration work.

- **SafetyTooling**'s `PdfDocument`/`IPdfDocumentBuilder` matches (`SafetyReporting.Pdf` project)
  are a **PDF generation/output library** (building PDFs for reports), not a PDF *ingestion* path —
  there is no code reading or parsing an externally supplied PDF. These are included below for
  transparency but should be read as **false positives for ingestion purposes**, kept only so the
  method is auditable.

No image-upload handling (e.g. `IFormFile` for images) was found anywhere.

Row count: 60 (9 raw ExportImport Excel-comment/using matches + 6 manually-verified ExportImport
adapter/handler class declarations + 45 SafetyTooling PDF-library matches, the latter excluded from
serious consideration per the note above).

| Module | Format/API | File:Line | Snippet |
|---|---|---|---|
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelAdapter.cs:5 | `DESCRIPTION:        IImportFileAdapter implementation for XLSX workbooks, using ClosedXML. Reads the first` |
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelAdapter.cs:9 | `STREAMING NOTE:     ClosedXML parses the OOXML package into an in-memory workbook DOM — unlike the CSV/JSON/` |
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelAdapter.cs:19 | `using ClosedXML.Excel;` |
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelAdapter.cs:27 | `/// Reads raw rows from the first worksheet of an XLSX workbook via ClosedXML.` |
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelFormatter.cs:6 | `ClosedXML. Writes a deterministic header row from the first row's keys, followed by data` |
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelFormatter.cs:13 | `using ClosedXML.Excel;` |
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelFormatter.cs:20 | `/// Formats export-shaped rows into a single-worksheet XLSX workbook via ClosedXML.` |
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Tests\Security\ExportSecurityTests.cs:110 | `because: "ClosedXML's Cell.Value setter stores plain strings as literal text, not as a live formula (only .FormulaA1 cre` |
| ExportImport | Excel | C:\Users\dross\source\ExportImport\ExportImport.Tests\Unit\FileAdapters\ExcelAdapterFormatterTests.cs:5 | `DESCRIPTION:        Unit tests for ExcelAdapter and ExcelFormatter round-tripping via ClosedXML: header row` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\PdfDebugViewInspectorTests.cs:12 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\PdfDebugViewInspectorTests.cs:28 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\PdfDebugViewInspectorTests.cs:52 | `var document = new PdfDocument(Array.Empty<string>(), Array.Empty<PdfParagraph>(), new[] { table });` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\PdfDebugViewInspectorTests.cs:67 | `var document = new PdfDocument(Array.Empty<string>(), Array.Empty<PdfParagraph>(), new[] { table });` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateLinterTests.cs:13 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateLinterTests.cs:27 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateLinterTests.cs:42 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateLinterTests.cs:58 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateLinterTests.cs:74 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateLinterTests.cs:90 | `var document = new PdfDocument(Array.Empty<string>(), Array.Empty<PdfParagraph>(), new[] { table });` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateLinterTests.cs:109 | `var document = new PdfDocument(Array.Empty<string>(), Array.Empty<PdfParagraph>(), new[] { table });` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateLinterTests.cs:121 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateProfilerTests.cs:12 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateProfilerTests.cs:33 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateProfilerTests.cs:57 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Diagnostics\TemplateProfilerTests.cs:71 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\FakePdfDocumentBuilder.cs:9 | `///     <see cref="PdfDocument"/> via <see cref="Build"/>. Tests can assert against` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\FakePdfDocumentBuilder.cs:11 | `///     resulting <see cref="PdfDocument"/> (for content assertions).` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\FakePdfDocumentBuilder.cs:45 | `public PdfDocument Build()` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\FakePdfDocumentBuilder.cs:47 | `return new PdfDocument(_titles, _paragraphs, _tables);` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\FakePdfGenerator.cs:9 | `///     payload derived from the built <see cref="Models.PdfDocument"/>'s title` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Models\PdfDocumentTests.cs:12 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Models\PdfDocumentTests.cs:28 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Models\PdfDocumentTests.cs:43 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Models\PdfDocumentTests.cs:59 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Models\PdfDocumentTests.cs:75 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Models\PdfDocumentTests.cs:94 | `var document = new PdfDocument(` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf.Tests\Models\PdfDocumentTests.cs:116 | `var document = new PdfDocument(titles, paragraphs, tables);` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Abstractions\IPdfDocumentBuilder.cs:7 | `/// produces an immutable <see cref="PdfDocument"/> once building is complete.` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Abstractions\IPdfDocumentBuilder.cs:34 | `PdfDocument Build();` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\DX\TemplatePlaygroundSession.cs:123 | `public PdfDocument? Document { get; }` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\DX\TemplatePlaygroundSession.cs:132 | `PdfDocument? document,` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\DX\TemplatePlaygroundSession.cs:150 | `PdfDocument document,` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\DX\TemplateSandboxRenderer.cs:123 | `public PdfDocument? Document { get; }` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\DX\TemplateSandboxRenderer.cs:133 | `PdfDocument? document,` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\DX\TemplateSandboxRenderer.cs:153 | `PdfDocument document,` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Diagnostics\PdfDebugViewInspector.cs:8 | `///     of a rendered <see cref="PdfDocument"/> — used by the sandbox and` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Diagnostics\PdfDebugViewInspector.cs:18 | `public static PdfDebugView Describe(PdfDocument document)` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Diagnostics\TemplateLinter.cs:9 | `///     <see cref="PdfDocument"/>, independent of the structural checks` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Diagnostics\TemplateLinter.cs:10 | `///     performed by <see cref="PdfDocument.Validate"/>.` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Diagnostics\TemplateLinter.cs:26 | `public PdfValidationResult Run(PdfDocument document)` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Diagnostics\TemplateProfiler.cs:8 | `///     <see cref="PdfDocument"/>, approximating render cost/complexity from` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Diagnostics\TemplateProfiler.cs:23 | `public PdfDebugView Profile(PdfDocument document)` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Models\PdfDocument.cs:20 | `public sealed class PdfDocument` |
| SafetyTooling | PDF | C:\Users\dross\source\SafetyTooling\SafetyReporting.Pdf\Models\PdfDocument.cs:31 | `public PdfDocument(` |
| ExportImport | CSV (custom adapter) | ExportImport\ExportImport.Infrastructure\Adapters\Csv\CsvAdapter.cs:25 | `public sealed class CsvAdapter : IImportFileAdapter` |
| ExportImport | JSON (custom adapter) | ExportImport\ExportImport.Infrastructure\Adapters\Json\JsonAdapter.cs:26 | `public sealed class JsonAdapter : IImportFileAdapter` |
| ExportImport | XML (custom adapter) | ExportImport\ExportImport.Infrastructure\Adapters\Xml\XmlAdapter.cs:27 | `public sealed class XmlAdapter : IImportFileAdapter` |
| ExportImport | Excel (ClosedXML adapter, class decl) | ExportImport\ExportImport.Infrastructure\Adapters\Excel\ExcelAdapter.cs:29 | `public sealed class ExcelAdapter : IImportFileAdapter` |
| ExportImport | Import orchestration entry point | ExportImport\ExportImport.Application\Services\Import\ImportPipelineService.cs:31 | `public sealed class ImportPipelineService : IImportPipelineService` |
| ExportImport | Import orchestration entry point | ExportImport\ExportImport.Application\Handlers\Import\StartImportCommandHandler.cs:27 | `internal sealed class StartImportCommandHandler : IRequestHandler<StartImportCommand, Result<ImportStatusDto>>` |
