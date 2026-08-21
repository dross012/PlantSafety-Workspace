-- =============================================
-- Shared.AuditLogging
-- Script:        01-usp-search-auditlog.sql
-- Purpose:       Creates dbo.usp_SearchAuditLog against [PlantSafety.AuditDB], the
--                single canonical, parameterized, read-only query path over
--                dbo.AuditLog for Shared.AuditLogging.Infrastructure's
--                AuditLogDapperRepository (both PlantSafety's Audit Viewer UI and
--                Reporting's Audit Report PDF call through this one procedure).
--
-- LOCATION JUDGMENT CALL: this script lives under Shared\AuditLogging\sql rather
-- than as a new PlantSafety.Infrastructure Phase10+ migration file. dbo.AuditLog is
-- explicitly documented (PlantSafety.Infrastructure's Persistence\Migrations\
-- Phase8-Structural\83-synonyms-audit-error.sql) as shared, cross-database,
-- server-level platform data written to by all 8 module databases -- it is NOT
-- PlantSafety-owned. Since Shared.AuditLogging is now the single canonical read
-- path for this table (see ..\..\docs\README.md), its stored procedure belongs
-- with it rather than continuing to accumulate PlantSafety-module migration
-- history for a table PlantSafety merely has read access to today.
--
-- WHY A STORED PROCEDURE (not inline SQL): the original PlantSafety
-- AuditLogDapperRepository built its SELECT/COUNT text inline in C# (with values
-- always bound as Dapper parameters, never concatenated). This extraction follows
-- the house convention of Dapper + stored procedure for a contract now shared
-- across two modules, converting that inline SQL into this procedure.
--
-- PAGINATION SHAPE JUDGMENT CALL: no existing paginated stored procedure in the
-- PlantSafety.Infrastructure migration scripts returns a total row count (the
-- *_List procedures examined, e.g. [AiGovernance].sp_AiGovernance_List in
-- Phase6-StoredProcedures\60-coredb-procedures.sql, return only the page with no
-- count). Since AuditLog search callers (both the PlantSafety Audit Viewer grid
-- and a future paginated Reporting view) need a total for page-count UI, this
-- procedure returns TWO result sets: (1) a single-row, single-column total count
-- reflecting the same filters, (2) the requested page of rows, newest first. This
-- was chosen over an OUTPUT parameter because Dapper's QueryMultipleAsync maps
-- cleanly onto "count, then rows" with no extra parameter-direction plumbing.
--
-- SAFETY: SELECT only. This procedure must never gain an INSERT/UPDATE/DELETE
-- statement -- dbo.AuditLog is append-only audit infrastructure written
-- exclusively by the canonical stored-procedure template's best-effort audit
-- block in each module database.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.AuditDB]
GO

CREATE OR ALTER PROCEDURE dbo.usp_SearchAuditLog
    @TableName   nvarchar(128) = NULL,
    @Operation   nvarchar(50)  = NULL,
    @ChangedBy   nvarchar(128) = NULL,
    @Module      nvarchar(128) = NULL,
    @Severity    int           = NULL,
    @FromUtc     datetime2     = NULL,
    @ToUtc       datetime2     = NULL,
    @SearchText  nvarchar(400) = NULL,
    @PageNumber  int           = 1,
    @PageSize    int           = 50
AS
BEGIN
    SET NOCOUNT ON;

    IF @PageNumber IS NULL OR @PageNumber < 1
        THROW 50001, N'PageNumber must be a positive integer.', 1;
    IF @PageSize IS NULL OR @PageSize < 1
        THROW 50001, N'PageSize must be a positive integer.', 1;

    ------------------------------------------------------------
    -- Result set 1: total matching row count (same filters as result set 2)
    ------------------------------------------------------------
    SELECT COUNT(*)
    FROM dbo.AuditLog
    WHERE (@TableName  IS NULL OR TableName  = @TableName)
      AND (@Operation  IS NULL OR Operation  = @Operation)
      AND (@ChangedBy  IS NULL OR ChangedBy  = @ChangedBy)
      AND (@Module     IS NULL OR Module     = @Module)
      AND (@Severity   IS NULL OR Severity   = @Severity)
      AND (@FromUtc    IS NULL OR ChangeTime >= @FromUtc)
      AND (@ToUtc      IS NULL OR ChangeTime <= @ToUtc)
      AND (@SearchText IS NULL OR ErrorContext LIKE N'%' + @SearchText + N'%');

    ------------------------------------------------------------
    -- Result set 2: the requested page, newest first
    ------------------------------------------------------------
    SELECT
        TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
        ViewerAccessed, ErrorContext, Severity, Module, BeforeSnapshot, AfterSnapshot
    FROM dbo.AuditLog
    WHERE (@TableName  IS NULL OR TableName  = @TableName)
      AND (@Operation  IS NULL OR Operation  = @Operation)
      AND (@ChangedBy  IS NULL OR ChangedBy  = @ChangedBy)
      AND (@Module     IS NULL OR Module     = @Module)
      AND (@Severity   IS NULL OR Severity   = @Severity)
      AND (@FromUtc    IS NULL OR ChangeTime >= @FromUtc)
      AND (@ToUtc      IS NULL OR ChangeTime <= @ToUtc)
      AND (@SearchText IS NULL OR ErrorContext LIKE N'%' + @SearchText + N'%')
    ORDER BY ChangeTime DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO
