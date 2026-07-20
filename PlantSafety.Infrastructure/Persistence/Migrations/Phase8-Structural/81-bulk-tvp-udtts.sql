-- =============================================
-- Phase 8 Structural Additions
-- Script:        81-bulk-tvp-udtts.sql
-- Purpose:       Add User-Defined Table Types (UDTTs) + accompanying bulk
--                insert stored procedures for the two highest-volume,
--                genuinely append-only, single-row-CRUD-only event tables
--                in scope:
--                  - Diagnostics.DiagnosticEvent   (PlantSafety.DiagnosticsDB)
--                  - Integration.IntegrationLogEntry (PlantSafety.IntegrationDB)
--
-- Grounding: both tables were explicitly classified as the highest write-
-- frequency (MONTHLY partition granularity) event tables in Phase 3
-- (30-partitioning-templates.sql) -- "the direct write-path event/telemetry
-- /prediction tables ... the actual highest-frequency write tables in
-- scope." Both are currently served only by single-row sp_<Entity>_Create
-- procedures (Phase 6), which is a real bottleneck for any bulk-ingest path
-- (batch diagnostic sweeps, high-throughput integration message logging).
--
-- Deliberately NOT built (considered, rejected with reasoning -- "don't
-- force this everywhere"):
--   - PredictiveSafety.SafetyPrediction: also MONTHLY-partitioned/high-
--     frequency, but its own ConcurrencyToken is a rowversion column that
--     cannot be supplied via INSERT/TVP, and its column set (Explanation,
--     HumanAgreed, ClosedAtUtc) reflects a per-inference human-review
--     lifecycle rather than a pure fire-and-forget append pattern; single-
--     row create remains the right shape for now.
--   - Diagnostics.DiagnosticEventArchive / Integration.ReplayRecord: Phase 3
--     classified these as QUARTERLY (lower write-frequency, downstream/
--     derived) tables specifically because monthly granularity would be
--     over-engineering for their expected volume. The same reasoning means
--     a bulk-insert TVP is not currently justified for them either.
--
-- Idempotent: UDTTs are re-created only if missing (table types cannot be
-- ALTERed in SQL Server -- must DROP/CREATE, and since these are brand new
-- with zero existing dependents, drop-if-exists-then-create is safe here
-- and remains safe on re-run). Procedures use CREATE OR ALTER. All
-- application tables confirmed at 0 rows before and after.
-- =============================================

------------------------------------------------------------------------------
-- PlantSafety.DiagnosticsDB : Diagnostics.DiagnosticEventTableType +
-- Diagnostics.sp_DiagnosticEvent_BulkCreate
------------------------------------------------------------------------------
USE [PlantSafety.DiagnosticsDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Note: DiagnosticEventTableType intentionally omits CreatedUtc/IsDeleted --
-- both are stamped uniformly by the bulk proc (SYSUTCDATETIME() / 0), same
-- as the existing single-row sp_DiagnosticEvent_Create.
-- Idempotency note: SQL Server table types cannot be ALTERed, and (unlike
-- ordinary tables) cannot be DROPped once a procedure references them
-- (Msg 3732). So, unlike the DROP-then-CREATE pattern used for a first
-- authoring pass, re-runs of this script must be a pure no-op once the type
-- already exists -- guarded by IF NOT EXISTS, never dropped here.
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'DiagnosticEventTableType' AND is_table_type = 1)
BEGIN
    CREATE TYPE [Diagnostics].[DiagnosticEventTableType] AS TABLE (
        DiagnosticEventId  uniqueidentifier    NOT NULL,
        TenantId            uniqueidentifier    NOT NULL,
        CorrelationId        nvarchar(200)       NULL,
        Severity             nvarchar(40)        NOT NULL,
        Category             nvarchar(100)       NOT NULL,
        Module                nvarchar(200)       NOT NULL,
        Operation             nvarchar(200)       NOT NULL,
        OperationStatus       nvarchar(40)        NOT NULL,
        Message               nvarchar(2000)      NOT NULL,
        Detail                nvarchar(MAX)       NULL,
        MetadataJson          nvarchar(MAX)       NULL,
        EventHash             char(64)            NOT NULL,
        OccurredAtUtc         datetime2(3)        NOT NULL,
        CreatedBy             uniqueidentifier    NOT NULL,
        PRIMARY KEY (DiagnosticEventId)
    );
END;
GO

-- =============================================
-- Procedure:     [Diagnostics].sp_DiagnosticEvent_BulkCreate
-- Description:   Bulk-inserts a batch of DiagnosticEvent rows via TVP for
--                high-throughput diagnostic ingest paths. All-or-nothing
--                (XACT_ABORT): a single bad/duplicate row rolls back the
--                whole batch, matching the existing single-row Create's
--                error semantics.
-- Notes:
--   - Filters nothing (INSERT-only entry point) but validates the TVP is
--     non-empty.
--   - Best-effort audit logging for the batch (single summary row, not one
--     row per event -- avoids audit-log amplification on top of a bulk op).
--   - Mandatory error logging with contextual parameters.
--   - XACT_ABORT ensures rollback on error.
--   - StrongId-safe identity usage (relies on caller-supplied StrongIds in
--     the TVP, same as the single-row proc).
-- =============================================
CREATE OR ALTER PROCEDURE [Diagnostics].sp_DiagnosticEvent_BulkCreate
    @Events [Diagnostics].[DiagnosticEventTableType] READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @__RowCount int = (SELECT COUNT(*) FROM @Events);
        IF @__RowCount = 0
            THROW 50001, N'@Events must contain at least one row.', 1;

        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal (one summary row for the batch)
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'DiagnosticEvent', DB_NAME(), N'BULK_INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RowCount=', @__RowCount), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;

        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Diagnostics].[DiagnosticEvent] (
            DiagnosticEventId, TenantId, CorrelationId, Severity, Category, Module,
            Operation, OperationStatus, Message, Detail, MetadataJson, EventHash,
            OccurredAtUtc, CreatedBy, IsDeleted, CreatedUtc
        )
        SELECT
            e.DiagnosticEventId, e.TenantId, e.CorrelationId, e.Severity, e.Category, e.Module,
            e.Operation, e.OperationStatus, e.Message, e.Detail, e.MetadataJson, e.EventHash,
            e.OccurredAtUtc, e.CreatedBy, 0, SYSUTCDATETIME()
        FROM @Events AS e;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RowCount=', (SELECT COUNT(*) FROM @Events), N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'DiagnosticEvent', DB_NAME(), N'BULK_INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO

------------------------------------------------------------------------------
-- PlantSafety.IntegrationDB : Integration.IntegrationLogEntryTableType +
-- Integration.sp_IntegrationLogEntry_BulkCreate
------------------------------------------------------------------------------
USE [PlantSafety.IntegrationDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Note: IntegrationLogEntryTableType intentionally omits CreatedUtc/IsDeleted
-- for the same reason as DiagnosticEventTableType above. Idempotency note:
-- same constraint as DiagnosticEventTableType above -- guarded by
-- IF NOT EXISTS, never dropped on re-run once a procedure references it.
IF NOT EXISTS (SELECT 1 FROM sys.types WHERE name = N'IntegrationLogEntryTableType' AND is_table_type = 1)
BEGIN
CREATE TYPE [Integration].[IntegrationLogEntryTableType] AS TABLE (
    IntegrationLogEntryId  uniqueidentifier    NOT NULL,
    CorrelationId            uniqueidentifier    NOT NULL,
    CausationId              uniqueidentifier    NOT NULL,
    MessageId                uniqueidentifier    NULL,
    TraceId                  nvarchar(200)       NOT NULL,
    SpanId                   nvarchar(200)       NOT NULL,
    TimestampUtc             datetime2(3)        NOT NULL,
    ElapsedMs                bigint              NOT NULL,
    OperationName            nvarchar(400)       NOT NULL,
    ModuleName               nvarchar(400)       NOT NULL,
    Environment              nvarchar(100)       NOT NULL,
    Version                  nvarchar(100)       NOT NULL,
    Success                  bit                 NOT NULL,
    CreatedBy                uniqueidentifier    NOT NULL,
    PRIMARY KEY (IntegrationLogEntryId)
);
END;
GO

-- =============================================
-- Procedure:     [Integration].sp_IntegrationLogEntry_BulkCreate
-- Description:   Bulk-inserts a batch of IntegrationLogEntry rows via TVP
--                for high-throughput cross-module message/telemetry
--                logging. All-or-nothing (XACT_ABORT).
-- Notes: same conventions as sp_DiagnosticEvent_BulkCreate above.
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogEntry_BulkCreate
    @Entries [Integration].[IntegrationLogEntryTableType] READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @__RowCount int = (SELECT COUNT(*) FROM @Entries);
        IF @__RowCount = 0
            THROW 50001, N'@Entries must contain at least one row.', 1;

        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal (one summary row for the batch)
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IntegrationLogEntry', DB_NAME(), N'BULK_INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RowCount=', @__RowCount), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;

        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[IntegrationLogEntry] (
            IntegrationLogEntryId, CorrelationId, CausationId, MessageId, TraceId, SpanId,
            TimestampUtc, ElapsedMs, OperationName, ModuleName, Environment, Version, Success,
            CreatedBy, IsDeleted, CreatedUtc
        )
        SELECT
            e.IntegrationLogEntryId, e.CorrelationId, e.CausationId, e.MessageId, e.TraceId, e.SpanId,
            e.TimestampUtc, e.ElapsedMs, e.OperationName, e.ModuleName, e.Environment, e.Version, e.Success,
            e.CreatedBy, 0, SYSUTCDATETIME()
        FROM @Entries AS e;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RowCount=', (SELECT COUNT(*) FROM @Entries), N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'IntegrationLogEntry', DB_NAME(), N'BULK_INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO
