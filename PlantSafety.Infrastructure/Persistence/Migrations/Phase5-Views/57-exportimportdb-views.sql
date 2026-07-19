USE [PlantSafety.ExportImportDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 5 Views & Summaries
-- Script:        57-exportimportdb-views.sql
-- Author:        View & Summary Builder (unattended agent run)
-- Purpose:       Active-record views for the 6 soft-deletable ExportImportDB tables,
--                cross-table reporting views over the real ExportOperation/ImportOperation
--                FKs, and 2 materialized daily summary tables + refresh procedures.
--
-- SECTION 1 -- Active-record views (6): ExportOperation, ExportBatch, ExportRecord,
--   ImportOperation, ImportBatch, ImportRecord. ExportOperationError and
--   ImportOperationError are DELIBERATELY EXCLUDED -- verified live they carry no
--   IsDeleted/ConcurrencyToken columns at all (true append-only error logs, same
--   reasoning as CoreDB's HistoryEntry tables: queried by parent OperationId/BatchId,
--   never "active"-filtered).
--
-- SECTION 2 -- Cross-table reporting views (4), over the real FKs
--   FK_ExportImport_ExportBatch_ExportOperation, FK_ExportImport_ExportRecord_
--   ExportOperation, FK_ExportImport_ExportOperationError_ExportOperation (error count
--   only, not a per-row join), and the Import-side mirrors.
--
-- SECTION 3 -- Materialized summary tables + refresh procedures (2)
--   ExportImport.ExportOperationDailySummary / ImportOperationDailySummary -- operation
--   counts by day/format/status, genuinely time-series (StartedAtUtc), directly useful
--   for an ExportImport operations dashboard (matches the module's own Integration &
--   Unit Tests Manual's operational-monitoring intent).
--
-- Deliberately NOT created:
--   - No tenant-scoped view: none of the 10 ExportImportDB tables carry a TenantId
--     column.
-- =============================================

-- ===================== SECTION 1: Active-record views =====================

CREATE OR ALTER VIEW ExportImport.vw_ExportOperation_Active
AS
SELECT
    e.ExportOperationId, e.ExportFormat, e.Status, e.StartedAtUtc, e.CompletedAtUtc,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM ExportImport.ExportOperation AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW ExportImport.vw_ExportBatch_Active
AS
SELECT
    e.ExportBatchId, e.OperationId, e.SequenceNumber,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM ExportImport.ExportBatch AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW ExportImport.vw_ExportRecord_Active
AS
SELECT
    e.ExportRecordId, e.BatchId, e.OperationId, e.DTODataJson, e.ExportShapeDataJson,
    e.FormattedData, e.ErrorCodesJson,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM ExportImport.ExportRecord AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW ExportImport.vw_ImportOperation_Active
AS
SELECT
    e.ImportOperationId, e.FileFormat, e.Status, e.StartedAtUtc, e.CompletedAtUtc,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM ExportImport.ImportOperation AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW ExportImport.vw_ImportBatch_Active
AS
SELECT
    e.ImportBatchId, e.OperationId, e.SequenceNumber,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM ExportImport.ImportBatch AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW ExportImport.vw_ImportRecord_Active
AS
SELECT
    e.ImportRecordId, e.BatchId, e.OperationId, e.NormalizedDataJson, e.MappedDataJson,
    e.ValidationStatus, e.ErrorCodesJson,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM ExportImport.ImportRecord AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 2: Cross-table reporting views =====================

-- =============================================
-- View:          ExportImport.vw_ExportOperation_WithCounts
-- Purpose:       Active ExportOperation with aggregated batch/record/error counts, via
--                the real FK_ExportImport_ExportBatch_ExportOperation,
--                FK_ExportImport_ExportRecord_ExportOperation and
--                FK_ExportImport_ExportOperationError_ExportOperation FKs. One row per
--                operation (no fan-out).
-- =============================================
CREATE OR ALTER VIEW ExportImport.vw_ExportOperation_WithCounts
AS
SELECT
    op.ExportOperationId, op.ExportFormat, op.Status, op.StartedAtUtc, op.CompletedAtUtc,
    ISNULL(b.BatchCount, 0) AS BatchCount,
    ISNULL(r.RecordCount, 0) AS RecordCount,
    ISNULL(err.ErrorCount, 0) AS ErrorCount,
    op.CreatedUtc, op.CreatedBy
FROM ExportImport.ExportOperation AS op
OUTER APPLY (
    SELECT COUNT(*) AS BatchCount FROM ExportImport.ExportBatch AS b
    WHERE b.OperationId = op.ExportOperationId AND ISNULL(b.IsDeleted, 0) = 0
) AS b
OUTER APPLY (
    SELECT COUNT(*) AS RecordCount FROM ExportImport.ExportRecord AS r
    WHERE r.OperationId = op.ExportOperationId AND ISNULL(r.IsDeleted, 0) = 0
) AS r
OUTER APPLY (
    SELECT COUNT(*) AS ErrorCount FROM ExportImport.ExportOperationError AS err
    WHERE err.OperationId = op.ExportOperationId
) AS err
WHERE ISNULL(op.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          ExportImport.vw_ImportOperation_WithCounts
-- Purpose:       Active ImportOperation with aggregated batch/record/error counts, via
--                the real FK_ExportImport_ImportBatch_ImportOperation,
--                FK_ExportImport_ImportRecord_ImportOperation and
--                FK_ExportImport_ImportOperationError_ImportOperation FKs. One row per
--                operation (no fan-out).
-- =============================================
CREATE OR ALTER VIEW ExportImport.vw_ImportOperation_WithCounts
AS
SELECT
    op.ImportOperationId, op.FileFormat, op.Status, op.StartedAtUtc, op.CompletedAtUtc,
    ISNULL(b.BatchCount, 0) AS BatchCount,
    ISNULL(r.RecordCount, 0) AS RecordCount,
    ISNULL(err.ErrorCount, 0) AS ErrorCount,
    op.CreatedUtc, op.CreatedBy
FROM ExportImport.ImportOperation AS op
OUTER APPLY (
    SELECT COUNT(*) AS BatchCount FROM ExportImport.ImportBatch AS b
    WHERE b.OperationId = op.ImportOperationId AND ISNULL(b.IsDeleted, 0) = 0
) AS b
OUTER APPLY (
    SELECT COUNT(*) AS RecordCount FROM ExportImport.ImportRecord AS r
    WHERE r.OperationId = op.ImportOperationId AND ISNULL(r.IsDeleted, 0) = 0
) AS r
OUTER APPLY (
    SELECT COUNT(*) AS ErrorCount FROM ExportImport.ImportOperationError AS err
    WHERE err.OperationId = op.ImportOperationId
) AS err
WHERE ISNULL(op.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 3: Materialized summary tables + refresh procedures =====================

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = 'ExportImport' AND t.name = 'ExportOperationDailySummary')
BEGIN
    CREATE TABLE ExportImport.ExportOperationDailySummary
    (
        ExportOperationDailySummaryId uniqueidentifier NOT NULL CONSTRAINT DF_ExportOperationDailySummary_Id DEFAULT NEWSEQUENTIALID(),
        SummaryDateKey                 int              NOT NULL,
        ExportFormat                   nvarchar(50)     NOT NULL,
        Status                         nvarchar(50)     NOT NULL,
        OperationCount                  int              NOT NULL,
        CreatedUtc                     datetime2(7)     NOT NULL CONSTRAINT DF_ExportOperationDailySummary_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy                      uniqueidentifier NOT NULL,
        IsDeleted                      bit              NOT NULL CONSTRAINT DF_ExportOperationDailySummary_IsDeleted DEFAULT (0),
        CONSTRAINT PK_ExportOperationDailySummary PRIMARY KEY CLUSTERED (ExportOperationDailySummaryId)
    );
    CREATE NONCLUSTERED INDEX IX_ExportOperationDailySummary_Key
        ON ExportImport.ExportOperationDailySummary (SummaryDateKey, ExportFormat, Status);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = 'ExportImport' AND t.name = 'ImportOperationDailySummary')
BEGIN
    CREATE TABLE ExportImport.ImportOperationDailySummary
    (
        ImportOperationDailySummaryId uniqueidentifier NOT NULL CONSTRAINT DF_ImportOperationDailySummary_Id DEFAULT NEWSEQUENTIALID(),
        SummaryDateKey                 int              NOT NULL,
        FileFormat                     nvarchar(50)     NOT NULL,
        Status                         nvarchar(50)     NOT NULL,
        OperationCount                  int              NOT NULL,
        CreatedUtc                     datetime2(7)     NOT NULL CONSTRAINT DF_ImportOperationDailySummary_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy                      uniqueidentifier NOT NULL,
        IsDeleted                      bit              NOT NULL CONSTRAINT DF_ImportOperationDailySummary_IsDeleted DEFAULT (0),
        CONSTRAINT PK_ImportOperationDailySummary PRIMARY KEY CLUSTERED (ImportOperationDailySummaryId)
    );
    CREATE NONCLUSTERED INDEX IX_ImportOperationDailySummary_Key
        ON ExportImport.ImportOperationDailySummary (SummaryDateKey, FileFormat, Status);
END
GO

-- =============================================
-- Procedure:     ExportImport.sp_ExportOperationDailySummary_Refresh
-- Description:   Full-rebuild refresh from ExportImport.ExportOperation, grouped by
--                started-date key, export format, status.
-- =============================================
CREATE OR ALTER PROCEDURE ExportImport.sp_ExportOperationDailySummary_Refresh
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM ExportImport.ExportOperationDailySummary;

        INSERT INTO ExportImport.ExportOperationDailySummary
            (SummaryDateKey, ExportFormat, Status, OperationCount, CreatedUtc, CreatedBy)
        SELECT
            CONVERT(int, CONVERT(char(8), e.StartedAtUtc, 112)) AS SummaryDateKey,
            e.ExportFormat,
            e.Status,
            COUNT(*) AS OperationCount,
            SYSUTCDATETIME(),
            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)
        FROM ExportImport.ExportOperation AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND e.StartedAtUtc IS NOT NULL
        GROUP BY
            CONVERT(int, CONVERT(char(8), e.StartedAtUtc, 112)),
            e.ExportFormat, e.Status;

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ExportOperationDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
                0, N'Full rebuild via sp_ExportOperationDailySummary_Refresh',
                0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorContext nvarchar(200) = CONCAT(N'Proc=ExportImport.sp_ExportOperationDailySummary_Refresh; User=', SYSTEM_USER);
        DECLARE @ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
            TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
            BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
        )
        VALUES (
            N'ExportOperationDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
            NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
        );

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     ExportImport.sp_ImportOperationDailySummary_Refresh
-- Description:   Full-rebuild refresh from ExportImport.ImportOperation, grouped by
--                started-date key, file format, status.
-- =============================================
CREATE OR ALTER PROCEDURE ExportImport.sp_ImportOperationDailySummary_Refresh
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM ExportImport.ImportOperationDailySummary;

        INSERT INTO ExportImport.ImportOperationDailySummary
            (SummaryDateKey, FileFormat, Status, OperationCount, CreatedUtc, CreatedBy)
        SELECT
            CONVERT(int, CONVERT(char(8), e.StartedAtUtc, 112)) AS SummaryDateKey,
            e.FileFormat,
            e.Status,
            COUNT(*) AS OperationCount,
            SYSUTCDATETIME(),
            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)
        FROM ExportImport.ImportOperation AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND e.StartedAtUtc IS NOT NULL
        GROUP BY
            CONVERT(int, CONVERT(char(8), e.StartedAtUtc, 112)),
            e.FileFormat, e.Status;

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ImportOperationDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
                0, N'Full rebuild via sp_ImportOperationDailySummary_Refresh',
                0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorContext nvarchar(200) = CONCAT(N'Proc=ExportImport.sp_ImportOperationDailySummary_Refresh; User=', SYSTEM_USER);
        DECLARE @ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID)
        );

        INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
            TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
            BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
        )
        VALUES (
            N'ImportOperationDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
            NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
        );

        THROW;
    END CATCH
END
GO
