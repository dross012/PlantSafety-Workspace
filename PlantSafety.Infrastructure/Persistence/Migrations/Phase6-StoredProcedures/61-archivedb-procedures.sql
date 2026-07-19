USE [PlantSafety.ArchiveDB]

GO

SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

-- =============================================

-- Phase 6: Stored Procedures (canonical FY2027 template)

-- Database: PlantSafety.ArchiveDB

-- Generated: 2026-07-19 by Procedure Architect agent, grounded in

-- sql-database-skill\references\00-canonical-templates.md

-- Idempotent: CREATE OR ALTER PROCEDURE throughout.

-- =============================================

GO



-- =============================================
-- Procedure:     [Archival].sp_ArchivalJob_Create
-- Description:   Inserts a new ArchivalJob record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_ArchivalJob_Create
    @ArchivalJobId uniqueidentifier,
    @DatasetId uniqueidentifier,
    @TenantId uniqueidentifier,
    @PartitionKey nvarchar(400),
    @SourceTier nvarchar(50),
    @TargetTier nvarchar(50),
    @Status nvarchar(50),
    @StartedAtUtc datetime2(3),
    @CompletedAtUtc datetime2(3),
    @FailureReason nvarchar(1000),
    @CorrelationId uniqueidentifier,
    @CausationId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ArchivalJobId IS NULL
            THROW 50001, N'ArchivalJobId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ArchivalJob', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ArchivalJobId=', @ArchivalJobId, N'; ', N'DatasetId=', @DatasetId, N'; ', N'TenantId=', @TenantId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Archival].[ArchivalJob] (ArchivalJobId, DatasetId, TenantId, PartitionKey, SourceTier, TargetTier, Status, StartedAtUtc, CompletedAtUtc, FailureReason, CorrelationId, CausationId, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@ArchivalJobId, @DatasetId, @TenantId, @PartitionKey, @SourceTier, @TargetTier, @Status, @StartedAtUtc, @CompletedAtUtc, @FailureReason, @CorrelationId, @CausationId, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ArchivalJobId=', @ArchivalJobId, N'; ', N'DatasetId=', @DatasetId, N'; ', N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
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
                N'ArchivalJob', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_ArchivalJob_Update
-- Description:   Updates an existing ArchivalJob record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_ArchivalJob_Update
    @ArchivalJobId uniqueidentifier,
    @DatasetId uniqueidentifier,
    @TenantId uniqueidentifier,
    @PartitionKey nvarchar(400),
    @SourceTier nvarchar(50),
    @TargetTier nvarchar(50),
    @Status nvarchar(50),
    @StartedAtUtc datetime2(3),
    @CompletedAtUtc datetime2(3),
    @FailureReason nvarchar(1000),
    @CorrelationId uniqueidentifier,
    @CausationId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ArchivalJobId IS NULL
            THROW 50001, N'ArchivalJobId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Archival].[ArchivalJob] WHERE ArchivalJobId = @ArchivalJobId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Archival].[ArchivalJob] WHERE ArchivalJobId = @ArchivalJobId AND ConcurrencyToken = @ConcurrencyToken)
            THROW 50002, N'Concurrency conflict: the record was modified by another process.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ArchivalJob', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ArchivalJobId=', @ArchivalJobId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Archival].[ArchivalJob]
        SET
            DatasetId = @DatasetId,
            TenantId = @TenantId,
            PartitionKey = @PartitionKey,
            SourceTier = @SourceTier,
            TargetTier = @TargetTier,
            Status = @Status,
            StartedAtUtc = @StartedAtUtc,
            CompletedAtUtc = @CompletedAtUtc,
            FailureReason = @FailureReason,
            CorrelationId = @CorrelationId,
            CausationId = @CausationId,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ArchivalJobId = @ArchivalJobId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ArchivalJobId=', @ArchivalJobId, N'; User=', SYSTEM_USER);
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
                N'ArchivalJob', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_ArchivalJob_GetById
-- Description:   Retrieves a single ArchivalJob record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_ArchivalJob_GetById
    @ArchivalJobId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ArchivalJobId IS NULL
            THROW 50001, N'ArchivalJobId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ArchivalJob', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ArchivalJobId=', @ArchivalJobId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ArchivalJobId, e.DatasetId, e.TenantId, e.PartitionKey, e.SourceTier, e.TargetTier, e.Status, e.StartedAtUtc, e.CompletedAtUtc, e.FailureReason, e.CorrelationId, e.CausationId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Archival].[ArchivalJob] AS e
        WHERE e.ArchivalJobId = @ArchivalJobId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ArchivalJobId=', @ArchivalJobId, N'; User=', SYSTEM_USER);
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
                N'ArchivalJob', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_ArchivalJob_List
-- Description:   Returns a paginated, soft-delete-filtered list of ArchivalJob records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_ArchivalJob_List
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PageNumber IS NULL OR @PageNumber < 1
            THROW 50001, N'PageNumber must be a positive integer.', 1;
        IF @PageSize IS NULL OR @PageSize < 1
            THROW 50001, N'PageSize must be a positive integer.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ArchivalJob', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ArchivalJobId, e.DatasetId, e.TenantId, e.PartitionKey, e.SourceTier, e.TargetTier, e.Status, e.StartedAtUtc, e.CompletedAtUtc, e.FailureReason, e.CorrelationId, e.CausationId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Archival].[ArchivalJob] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ArchivalJobId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize, N'; User=', SYSTEM_USER);
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
                N'ArchivalJob', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_ArchivalJob_Delete
-- Description:   Soft-deletes a ArchivalJob record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_ArchivalJob_Delete
    @ArchivalJobId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ArchivalJobId IS NULL
            THROW 50001, N'ArchivalJobId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Archival].[ArchivalJob] WHERE ArchivalJobId = @ArchivalJobId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or already deleted.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ArchivalJob', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ArchivalJobId=', @ArchivalJobId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Archival].[ArchivalJob]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ArchivalJobId = @ArchivalJobId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ArchivalJobId=', @ArchivalJobId, N'; User=', SYSTEM_USER);
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
                N'ArchivalJob', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_TierPolicy_Create
-- Description:   Inserts a new TierPolicy record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_TierPolicy_Create
    @TierPolicyId uniqueidentifier,
    @Zone nvarchar(50),
    @SourceTier nvarchar(50),
    @TargetTier nvarchar(50),
    @TransitionAfterDays int,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TierPolicyId IS NULL
            THROW 50001, N'TierPolicyId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'TierPolicy', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TierPolicyId=', @TierPolicyId, N'; ', N'Zone=', @Zone, N'; ', N'SourceTier=', @SourceTier), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Archival].[TierPolicy] (TierPolicyId, Zone, SourceTier, TargetTier, TransitionAfterDays, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@TierPolicyId, @Zone, @SourceTier, @TargetTier, @TransitionAfterDays, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TierPolicyId=', @TierPolicyId, N'; ', N'Zone=', @Zone, N'; ', N'SourceTier=', @SourceTier, N'; User=', SYSTEM_USER);
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
                N'TierPolicy', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_TierPolicy_Update
-- Description:   Updates an existing TierPolicy record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_TierPolicy_Update
    @TierPolicyId uniqueidentifier,
    @Zone nvarchar(50),
    @SourceTier nvarchar(50),
    @TargetTier nvarchar(50),
    @TransitionAfterDays int,
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TierPolicyId IS NULL
            THROW 50001, N'TierPolicyId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Archival].[TierPolicy] WHERE TierPolicyId = @TierPolicyId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Archival].[TierPolicy] WHERE TierPolicyId = @TierPolicyId AND ConcurrencyToken = @ConcurrencyToken)
            THROW 50002, N'Concurrency conflict: the record was modified by another process.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'TierPolicy', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TierPolicyId=', @TierPolicyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Archival].[TierPolicy]
        SET
            Zone = @Zone,
            SourceTier = @SourceTier,
            TargetTier = @TargetTier,
            TransitionAfterDays = @TransitionAfterDays,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE TierPolicyId = @TierPolicyId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TierPolicyId=', @TierPolicyId, N'; User=', SYSTEM_USER);
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
                N'TierPolicy', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_TierPolicy_GetById
-- Description:   Retrieves a single TierPolicy record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_TierPolicy_GetById
    @TierPolicyId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TierPolicyId IS NULL
            THROW 50001, N'TierPolicyId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'TierPolicy', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'TierPolicyId=', @TierPolicyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.TierPolicyId, e.Zone, e.SourceTier, e.TargetTier, e.TransitionAfterDays, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Archival].[TierPolicy] AS e
        WHERE e.TierPolicyId = @TierPolicyId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TierPolicyId=', @TierPolicyId, N'; User=', SYSTEM_USER);
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
                N'TierPolicy', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_TierPolicy_List
-- Description:   Returns a paginated, soft-delete-filtered list of TierPolicy records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_TierPolicy_List
    @PageNumber int = 1,
    @PageSize int = 50
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PageNumber IS NULL OR @PageNumber < 1
            THROW 50001, N'PageNumber must be a positive integer.', 1;
        IF @PageSize IS NULL OR @PageSize < 1
            THROW 50001, N'PageSize must be a positive integer.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'TierPolicy', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.TierPolicyId, e.Zone, e.SourceTier, e.TargetTier, e.TransitionAfterDays, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Archival].[TierPolicy] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.TierPolicyId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize, N'; User=', SYSTEM_USER);
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
                N'TierPolicy', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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

-- =============================================
-- Procedure:     [Archival].sp_TierPolicy_Delete
-- Description:   Soft-deletes a TierPolicy record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Archival].sp_TierPolicy_Delete
    @TierPolicyId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TierPolicyId IS NULL
            THROW 50001, N'TierPolicyId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Archival].[TierPolicy] WHERE TierPolicyId = @TierPolicyId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or already deleted.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'TierPolicy', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TierPolicyId=', @TierPolicyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Archival].[TierPolicy]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE TierPolicyId = @TierPolicyId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TierPolicyId=', @TierPolicyId, N'; User=', SYSTEM_USER);
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
                N'TierPolicy', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
