USE [PlantSafety.IntegrationDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 5 Views & Summaries
-- Script:        55-integrationdb-views.sql
-- Author:        View & Summary Builder (unattended agent run)
-- Purpose:       Active-record views for all 7 IntegrationDB tables, cross-table
--                reporting views over the real ContractSchema<->ContractField/
--                ContractVersion FKs, and 2 materialized daily summary tables +
--                refresh procedures for the module's genuinely time-series event tables.
--
-- SECTION 1 -- Active-record views (7). IntegrationLogEntry, ModuleHealthSnapshot and
--   ReplayRecord are standalone append-only event/log tables (no HistoryEntry parent-FK
--   pattern, no audit-trail-of-an-aggregate shape), so -- unlike CoreDB's HistoryEntry
--   tables -- they get their own active views, consistent with how DiagnosticEvent was
--   treated in DiagnosticsDB.
--
-- SECTION 2 -- Cross-table reporting views (2), over the real FKs
--   FK_Integration_ContractField_ContractSchema and FK_Integration_ContractVersion_
--   ContractSchema.
--
-- SECTION 3 -- Materialized summary tables + refresh procedures (2)
--   Integration.ModuleHealthDailySummary -- module health-check outcome counts by day/
--     module/status; genuinely time-series (ModuleHealthSnapshot.CheckedAtUtc), mirrors
--     the Diagnostics/CoreDB event-count pattern, directly useful for an integration
--     ops dashboard.
--   Integration.IntegrationLogDailySummary -- integration call-volume/success counts by
--     day/module/operation; genuinely time-series (IntegrationLogEntry.TimestampUtc).
--
-- Deliberately NOT created:
--   - No summary for ContractSchema/ContractField/ContractVersion/RoutingTableEntry/
--     ReplayRecord: these are contract-catalog/replay-audit tables queried by key
--     lookup, not aggregated by time -- no genuine rollup need found.
--   - No tenant-scoped view: none of the 7 tables carry a TenantId column.
-- =============================================

-- ===================== SECTION 1: Active-record views =====================

CREATE OR ALTER VIEW Integration.vw_ContractField_Active
AS
SELECT
    e.ContractFieldId, e.ContractSchemaId, e.Name, e.FieldType, e.IsRequired, e.AllowedValuesJson,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Integration.ContractField AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Integration.vw_ContractSchema_Active
AS
SELECT
    e.ContractSchemaId, e.SchemaName, e.MessageType, e.Version,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Integration.ContractSchema AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Integration.vw_ContractVersion_Active
AS
SELECT
    e.ContractVersionId, e.ContractSchemaId, e.SchemaName, e.Version, e.LifecycleState,
    e.ReleasedAtUtc, e.DeprecatedAtUtc,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Integration.ContractVersion AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Integration.vw_IntegrationLogEntry_Active
AS
SELECT
    e.IntegrationLogEntryId, e.CorrelationId, e.CausationId, e.MessageId, e.TraceId, e.SpanId,
    e.TimestampUtc, e.ElapsedMs, e.OperationName, e.ModuleName, e.Environment, e.Version, e.Success,
    e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Integration.IntegrationLogEntry AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Integration.vw_ModuleHealthSnapshot_Active
AS
SELECT
    e.ModuleHealthSnapshotId, e.ModuleName, e.Status, e.CheckedAtUtc, e.ResponseLatencyMs,
    e.DegradationReason,
    e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Integration.ModuleHealthSnapshot AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Integration.vw_ReplayRecord_Active
AS
SELECT
    e.ReplayRecordId, e.MessageId, e.CorrelationId, e.CausationId, e.SchemaName,
    e.ContractVersion, e.TimestampUtc, e.SerializedPayload,
    e.CreatedUtc, e.CreatedBy, e.IsDeleted
FROM Integration.ReplayRecord AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW Integration.vw_RoutingTableEntry_Active
AS
SELECT
    e.RoutingTableEntryId, e.SchemaName, e.MessageType, e.SubscribersJson,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM Integration.RoutingTableEntry AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 2: Cross-table reporting views =====================

-- =============================================
-- View:          Integration.vw_ContractSchema_WithFieldCount
-- Purpose:       Active ContractSchema with an aggregated count of its active
--                ContractField rows, via the real FK_Integration_ContractField_
--                ContractSchema FK. One row per schema (no fan-out).
-- =============================================
CREATE OR ALTER VIEW Integration.vw_ContractSchema_WithFieldCount
AS
SELECT
    cs.ContractSchemaId, cs.SchemaName, cs.MessageType, cs.Version,
    ISNULL(f.FieldCount, 0) AS FieldCount,
    ISNULL(f.RequiredFieldCount, 0) AS RequiredFieldCount,
    cs.CreatedUtc, cs.CreatedBy, cs.UpdatedUtc, cs.UpdatedBy
FROM Integration.ContractSchema AS cs
OUTER APPLY (
    SELECT COUNT(*) AS FieldCount, SUM(CASE WHEN cf.IsRequired = 1 THEN 1 ELSE 0 END) AS RequiredFieldCount
    FROM Integration.ContractField AS cf
    WHERE cf.ContractSchemaId = cs.ContractSchemaId
      AND ISNULL(cf.IsDeleted, 0) = 0
) AS f
WHERE ISNULL(cs.IsDeleted, 0) = 0;
GO

-- =============================================
-- View:          Integration.vw_ContractVersion_WithSchema
-- Purpose:       Active ContractVersion joined to its parent ContractSchema via the real
--                FK_Integration_ContractVersion_ContractSchema FK.
-- =============================================
CREATE OR ALTER VIEW Integration.vw_ContractVersion_WithSchema
AS
SELECT
    cv.ContractVersionId, cv.SchemaName AS VersionSchemaName, cv.Version, cv.LifecycleState,
    cv.ReleasedAtUtc, cv.DeprecatedAtUtc,
    cs.ContractSchemaId, cs.SchemaName AS CurrentSchemaName, cs.MessageType, cs.Version AS CurrentSchemaVersion,
    cv.CreatedUtc, cv.CreatedBy
FROM Integration.ContractVersion AS cv
INNER JOIN Integration.ContractSchema AS cs
    ON cv.ContractSchemaId = cs.ContractSchemaId
WHERE ISNULL(cv.IsDeleted, 0) = 0
  AND ISNULL(cs.IsDeleted, 0) = 0;
GO

-- ===================== SECTION 3: Materialized summary tables + refresh procedures =====================

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = 'Integration' AND t.name = 'ModuleHealthDailySummary')
BEGIN
    CREATE TABLE Integration.ModuleHealthDailySummary
    (
        ModuleHealthDailySummaryId uniqueidentifier NOT NULL CONSTRAINT DF_ModuleHealthDailySummary_Id DEFAULT NEWSEQUENTIALID(),
        SummaryDateKey              int              NOT NULL,
        ModuleName                  nvarchar(200)    NOT NULL,
        Status                      nvarchar(50)     NOT NULL,
        SnapshotCount               int              NOT NULL,
        AverageResponseLatencyMs    int              NULL,
        CreatedUtc                  datetime2(7)     NOT NULL CONSTRAINT DF_ModuleHealthDailySummary_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy                   uniqueidentifier NOT NULL,
        IsDeleted                   bit              NOT NULL CONSTRAINT DF_ModuleHealthDailySummary_IsDeleted DEFAULT (0),
        CONSTRAINT PK_ModuleHealthDailySummary PRIMARY KEY CLUSTERED (ModuleHealthDailySummaryId)
    );
    CREATE NONCLUSTERED INDEX IX_ModuleHealthDailySummary_Key
        ON Integration.ModuleHealthDailySummary (SummaryDateKey, ModuleName, Status);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
               WHERE s.name = 'Integration' AND t.name = 'IntegrationLogDailySummary')
BEGIN
    CREATE TABLE Integration.IntegrationLogDailySummary
    (
        IntegrationLogDailySummaryId uniqueidentifier NOT NULL CONSTRAINT DF_IntegrationLogDailySummary_Id DEFAULT NEWSEQUENTIALID(),
        SummaryDateKey                int              NOT NULL,
        ModuleName                    nvarchar(200)    NOT NULL,
        OperationName                 nvarchar(200)    NOT NULL,
        SuccessCount                  int              NOT NULL,
        FailureCount                  int              NOT NULL,
        AverageElapsedMs              int              NULL,
        CreatedUtc                    datetime2(7)     NOT NULL CONSTRAINT DF_IntegrationLogDailySummary_CreatedUtc DEFAULT SYSUTCDATETIME(),
        CreatedBy                     uniqueidentifier NOT NULL,
        IsDeleted                     bit              NOT NULL CONSTRAINT DF_IntegrationLogDailySummary_IsDeleted DEFAULT (0),
        CONSTRAINT PK_IntegrationLogDailySummary PRIMARY KEY CLUSTERED (IntegrationLogDailySummaryId)
    );
    CREATE NONCLUSTERED INDEX IX_IntegrationLogDailySummary_Key
        ON Integration.IntegrationLogDailySummary (SummaryDateKey, ModuleName, OperationName);
END
GO

-- =============================================
-- Procedure:     Integration.sp_ModuleHealthDailySummary_Refresh
-- Description:   Full-rebuild refresh from Integration.ModuleHealthSnapshot, grouped by
--                checked-date key, module name, status.
-- =============================================
CREATE OR ALTER PROCEDURE Integration.sp_ModuleHealthDailySummary_Refresh
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM Integration.ModuleHealthDailySummary;

        INSERT INTO Integration.ModuleHealthDailySummary
            (SummaryDateKey, ModuleName, Status, SnapshotCount, AverageResponseLatencyMs, CreatedUtc, CreatedBy)
        SELECT
            CONVERT(int, CONVERT(char(8), e.CheckedAtUtc, 112)) AS SummaryDateKey,
            e.ModuleName,
            e.Status,
            COUNT(*) AS SnapshotCount,
            AVG(e.ResponseLatencyMs) AS AverageResponseLatencyMs,
            SYSUTCDATETIME(),
            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)
        FROM Integration.ModuleHealthSnapshot AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        GROUP BY
            CONVERT(int, CONVERT(char(8), e.CheckedAtUtc, 112)),
            e.ModuleName, e.Status;

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ModuleHealthDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
                0, N'Full rebuild via sp_ModuleHealthDailySummary_Refresh',
                0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorContext nvarchar(200) = CONCAT(N'Proc=Integration.sp_ModuleHealthDailySummary_Refresh; User=', SYSTEM_USER);
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
            N'ModuleHealthDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
            NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
        );

        THROW;
    END CATCH
END
GO

-- =============================================
-- Procedure:     Integration.sp_IntegrationLogDailySummary_Refresh
-- Description:   Full-rebuild refresh from Integration.IntegrationLogEntry, grouped by
--                log-timestamp date key, module name, operation name, with success/
--                failure split and average elapsed time.
-- =============================================
CREATE OR ALTER PROCEDURE Integration.sp_IntegrationLogDailySummary_Refresh
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM Integration.IntegrationLogDailySummary;

        INSERT INTO Integration.IntegrationLogDailySummary
            (SummaryDateKey, ModuleName, OperationName, SuccessCount, FailureCount, AverageElapsedMs, CreatedUtc, CreatedBy)
        SELECT
            CONVERT(int, CONVERT(char(8), e.TimestampUtc, 112)) AS SummaryDateKey,
            e.ModuleName,
            e.OperationName,
            SUM(CASE WHEN e.Success = 1 THEN 1 ELSE 0 END) AS SuccessCount,
            SUM(CASE WHEN e.Success = 0 THEN 1 ELSE 0 END) AS FailureCount,
            AVG(e.ElapsedMs) AS AverageElapsedMs,
            SYSUTCDATETIME(),
            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier)
        FROM Integration.IntegrationLogEntry AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        GROUP BY
            CONVERT(int, CONVERT(char(8), e.TimestampUtc, 112)),
            e.ModuleName, e.OperationName;

        COMMIT TRANSACTION;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IntegrationLogDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
                0, N'Full rebuild via sp_IntegrationLogDailySummary_Refresh',
                0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;

        DECLARE @ErrorContext nvarchar(200) = CONCAT(N'Proc=Integration.sp_IntegrationLogDailySummary_Refresh; User=', SYSTEM_USER);
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
            N'IntegrationLogDailySummary', DB_NAME(), N'REFRESH', SYSTEM_USER, SYSUTCDATETIME(),
            NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
        );

        THROW;
    END CATCH
END
GO
