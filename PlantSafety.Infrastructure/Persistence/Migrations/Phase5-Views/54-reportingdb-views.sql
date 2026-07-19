USE [PlantSafety.ReportingDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 5 Views & Summaries
-- Script:        54-reportingdb-views.sql
-- Author:        View & Summary Builder (unattended agent run)
-- Purpose:       Active-record views for all 5 ReportingDB tables, plus cross-table
--                reporting views over the real FKs (ReportDocument->ReportType,
--                ReportSection->ReportDocument, ReportMetadata->ReportDocument).
--
-- SECTION 2 includes the vw_ReportDocumentWithSections view explicitly named as the
-- worked example in scope.
--
-- Deliberately NOT created:
--   - No materialized summary table: report generation volume/cadence is not documented
--     anywhere in memory as an aggregation-worthy time series yet (no "reports generated
--     per day" dashboard is grounded). RECOMMENDED optional summary if/when a Reporting
--     operations dashboard is scoped: Reporting.ReportDocumentSummary (SummaryDateKey,
--     ReportTypeId, DocumentCount) refreshed from ReportDocument.GeneratedAtUtc, same
--     full-rebuild pattern as CoreDB/Diagnostics -- not built here, no consumer grounded.
--   - No tenant-scoped view: none of the 5 ReportingDB tables carry a TenantId column
--     (reports are scoped by ReportDocumentId/ReportTypeId, not tenant), so the CoreDB
--     TVF pattern does not apply.
-- =============================================

-- ===================== SECTION 1: Active-record views =====================

CREATE OR ALTER VIEW Reporting.vw_ReportDefinition_Active
AS
SELECT
    e.ReportDefinitionId, e.ReportTypeId, e.Description, e.SupportedOptionNamesJson,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Reporting.ReportDefinition AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Reporting.vw_ReportDocument_Active
AS
SELECT
    e.ReportDocumentId, e.ReportTypeId, e.GeneratedAtUtc,
    e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Reporting.ReportDocument AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Reporting.vw_ReportMetadata_Active
AS
SELECT
    e.ReportMetadataId, e.ReportDocumentId, e.RunId, e.TimestampUtc, e.GeneratorName,
    e.GeneratorVersion, e.PlatformVersion, e.CorrelationId, e.Classification, e.SchemaVersion,
    e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Reporting.ReportMetadata AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Reporting.vw_ReportSection_Active
AS
SELECT
    e.ReportSectionId, e.ReportDocumentId, e.SectionType, e.Title, e.SortOrder, e.PayloadJson,
    e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Reporting.ReportSection AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Reporting.vw_ReportType_Active
AS
SELECT
    e.ReportTypeId, e.Code, e.DisplayName, e.IsComplianceReport,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Reporting.ReportType AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 2: Cross-table reporting views =====================

-- =============================================
-- View:          Reporting.vw_ReportDocument_WithType
-- Purpose:       Active ReportDocument joined to its ReportType via the real
--                FK_Reporting_ReportDocument_ReportType FK. One row per document.
-- =============================================
CREATE OR ALTER VIEW Reporting.vw_ReportDocument_WithType
AS
SELECT
    doc.ReportDocumentId, doc.GeneratedAtUtc,
    ty.ReportTypeId, ty.Code AS ReportTypeCode, ty.DisplayName AS ReportTypeDisplayName,
    ty.IsComplianceReport,
    doc.CreatedUtc, doc.CreatedBy
FROM Reporting.ReportDocument AS doc
INNER JOIN Reporting.ReportType AS ty
    ON doc.ReportTypeId = ty.ReportTypeId
WHERE ISNULL(doc.IsDeleted, 0) = 0
  AND ISNULL(ty.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Reporting.vw_ReportDocumentWithSections
-- Purpose:       Active ReportDocument joined to its active ReportSection rows via the
--                real FK_Reporting_ReportSection_ReportDocument FK -- named explicitly
--                as the worked example for this module. One row per section (documents
--                with zero sections still appear once via OUTER APPLY... actually LEFT
--                JOIN below preserves parentless documents too).
-- =============================================
CREATE OR ALTER VIEW Reporting.vw_ReportDocumentWithSections
AS
SELECT
    doc.ReportDocumentId, doc.ReportTypeId, doc.GeneratedAtUtc,
    sec.ReportSectionId, sec.SectionType, sec.Title, sec.SortOrder, sec.PayloadJson,
    doc.CreatedUtc AS DocumentCreatedUtc
FROM Reporting.ReportDocument AS doc
LEFT JOIN Reporting.ReportSection AS sec
    ON sec.ReportDocumentId = doc.ReportDocumentId
    AND ISNULL(sec.IsDeleted, 0) = 0
WHERE ISNULL(doc.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Reporting.vw_ReportDocument_WithMetadata
-- Purpose:       Active ReportDocument joined to its active ReportMetadata row (1:1 in
--                practice) via the real FK_Reporting_ReportMetadata_ReportDocument FK.
-- =============================================
CREATE OR ALTER VIEW Reporting.vw_ReportDocument_WithMetadata
AS
SELECT
    doc.ReportDocumentId, doc.ReportTypeId, doc.GeneratedAtUtc,
    md.ReportMetadataId, md.RunId, md.TimestampUtc, md.GeneratorName, md.GeneratorVersion,
    md.PlatformVersion, md.CorrelationId, md.Classification, md.SchemaVersion,
    doc.CreatedUtc AS DocumentCreatedUtc
FROM Reporting.ReportDocument AS doc
LEFT JOIN Reporting.ReportMetadata AS md
    ON md.ReportDocumentId = doc.ReportDocumentId
    AND ISNULL(md.IsDeleted, 0) = 0
WHERE ISNULL(doc.IsDeleted, 0) = 0;
GO
