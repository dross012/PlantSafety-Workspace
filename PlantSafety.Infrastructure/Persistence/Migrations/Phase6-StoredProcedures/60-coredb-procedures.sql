USE [PlantSafety.CoreDB]

GO

SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

-- =============================================

-- Phase 6: Stored Procedures (canonical FY2027 template)

-- Database: PlantSafety.CoreDB

-- Generated: 2026-07-19 by Procedure Architect agent, grounded in

-- sql-database-skill\references\00-canonical-templates.md

-- Idempotent: CREATE OR ALTER PROCEDURE throughout.

-- =============================================

GO



-- =============================================
-- Procedure:     [AiGovernance].sp_AiGovernance_Create
-- Description:   Inserts a new AiGovernance record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiGovernance].sp_AiGovernance_Create
    @AiGovernanceId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AiGovernanceId IS NULL
            THROW 50001, N'AiGovernanceId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AiGovernance', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AiGovernanceId=', @AiGovernanceId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [AiGovernance].[AiGovernance] (AiGovernanceId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@AiGovernanceId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiGovernanceId=', @AiGovernanceId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'AiGovernance', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiGovernance].sp_AiGovernance_Update
-- Description:   Updates an existing AiGovernance record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiGovernance].sp_AiGovernance_Update
    @AiGovernanceId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AiGovernanceId IS NULL
            THROW 50001, N'AiGovernanceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [AiGovernance].[AiGovernance] WHERE AiGovernanceId = @AiGovernanceId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [AiGovernance].[AiGovernance] WHERE AiGovernanceId = @AiGovernanceId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'AiGovernance', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AiGovernanceId=', @AiGovernanceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [AiGovernance].[AiGovernance]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE AiGovernanceId = @AiGovernanceId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiGovernanceId=', @AiGovernanceId, N'; User=', SYSTEM_USER);
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
                N'AiGovernance', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiGovernance].sp_AiGovernance_GetById
-- Description:   Retrieves a single AiGovernance record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiGovernance].sp_AiGovernance_GetById
    @AiGovernanceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AiGovernanceId IS NULL
            THROW 50001, N'AiGovernanceId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AiGovernance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'AiGovernanceId=', @AiGovernanceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AiGovernanceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [AiGovernance].[AiGovernance] AS e
        WHERE e.AiGovernanceId = @AiGovernanceId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiGovernanceId=', @AiGovernanceId, N'; User=', SYSTEM_USER);
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
                N'AiGovernance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiGovernance].sp_AiGovernance_List
-- Description:   Returns a paginated, soft-delete-filtered list of AiGovernance records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiGovernance].sp_AiGovernance_List
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
                N'AiGovernance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AiGovernanceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [AiGovernance].[AiGovernance] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.AiGovernanceId
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
                N'AiGovernance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiGovernance].sp_AiGovernance_Delete
-- Description:   Soft-deletes a AiGovernance record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiGovernance].sp_AiGovernance_Delete
    @AiGovernanceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AiGovernanceId IS NULL
            THROW 50001, N'AiGovernanceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [AiGovernance].[AiGovernance] WHERE AiGovernanceId = @AiGovernanceId AND ISNULL(IsDeleted,0) = 0)
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
                N'AiGovernance', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AiGovernanceId=', @AiGovernanceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [AiGovernance].[AiGovernance]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE AiGovernanceId = @AiGovernanceId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiGovernanceId=', @AiGovernanceId, N'; User=', SYSTEM_USER);
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
                N'AiGovernance', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiSafetyIntelligence].sp_AiSafetyIntelligence_Create
-- Description:   Inserts a new AiSafetyIntelligence record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiSafetyIntelligence].sp_AiSafetyIntelligence_Create
    @AiSafetyIntelligenceId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AiSafetyIntelligenceId IS NULL
            THROW 50001, N'AiSafetyIntelligenceId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AiSafetyIntelligence', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [AiSafetyIntelligence].[AiSafetyIntelligence] (AiSafetyIntelligenceId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@AiSafetyIntelligenceId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'AiSafetyIntelligence', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiSafetyIntelligence].sp_AiSafetyIntelligence_Update
-- Description:   Updates an existing AiSafetyIntelligence record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiSafetyIntelligence].sp_AiSafetyIntelligence_Update
    @AiSafetyIntelligenceId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AiSafetyIntelligenceId IS NULL
            THROW 50001, N'AiSafetyIntelligenceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [AiSafetyIntelligence].[AiSafetyIntelligence] WHERE AiSafetyIntelligenceId = @AiSafetyIntelligenceId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [AiSafetyIntelligence].[AiSafetyIntelligence] WHERE AiSafetyIntelligenceId = @AiSafetyIntelligenceId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'AiSafetyIntelligence', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [AiSafetyIntelligence].[AiSafetyIntelligence]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE AiSafetyIntelligenceId = @AiSafetyIntelligenceId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId, N'; User=', SYSTEM_USER);
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
                N'AiSafetyIntelligence', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiSafetyIntelligence].sp_AiSafetyIntelligence_GetById
-- Description:   Retrieves a single AiSafetyIntelligence record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiSafetyIntelligence].sp_AiSafetyIntelligence_GetById
    @AiSafetyIntelligenceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AiSafetyIntelligenceId IS NULL
            THROW 50001, N'AiSafetyIntelligenceId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AiSafetyIntelligence', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AiSafetyIntelligenceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [AiSafetyIntelligence].[AiSafetyIntelligence] AS e
        WHERE e.AiSafetyIntelligenceId = @AiSafetyIntelligenceId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId, N'; User=', SYSTEM_USER);
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
                N'AiSafetyIntelligence', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiSafetyIntelligence].sp_AiSafetyIntelligence_List
-- Description:   Returns a paginated, soft-delete-filtered list of AiSafetyIntelligence records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiSafetyIntelligence].sp_AiSafetyIntelligence_List
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
                N'AiSafetyIntelligence', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AiSafetyIntelligenceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [AiSafetyIntelligence].[AiSafetyIntelligence] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.AiSafetyIntelligenceId
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
                N'AiSafetyIntelligence', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [AiSafetyIntelligence].sp_AiSafetyIntelligence_Delete
-- Description:   Soft-deletes a AiSafetyIntelligence record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [AiSafetyIntelligence].sp_AiSafetyIntelligence_Delete
    @AiSafetyIntelligenceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AiSafetyIntelligenceId IS NULL
            THROW 50001, N'AiSafetyIntelligenceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [AiSafetyIntelligence].[AiSafetyIntelligence] WHERE AiSafetyIntelligenceId = @AiSafetyIntelligenceId AND ISNULL(IsDeleted,0) = 0)
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
                N'AiSafetyIntelligence', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [AiSafetyIntelligence].[AiSafetyIntelligence]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE AiSafetyIntelligenceId = @AiSafetyIntelligenceId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AiSafetyIntelligenceId=', @AiSafetyIntelligenceId, N'; User=', SYSTEM_USER);
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
                N'AiSafetyIntelligence', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Culture_Create
-- Description:   Inserts a new Culture record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Culture_Create
    @CultureId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @SurveyPeriod nvarchar(20),
    @CultureIndexScore decimal(7,4),
    @EngagementScore decimal(7,4),
    @ParticipationRate decimal(7,4),
    @TopRiskTheme nvarchar(300)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CultureId IS NULL
            THROW 50001, N'CultureId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Culture', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'CultureId=', @CultureId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[Culture] (CultureId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, SurveyPeriod, CultureIndexScore, EngagementScore, ParticipationRate, TopRiskTheme, IsDeleted, CreatedUtc)
        VALUES (@CultureId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @SurveyPeriod, @CultureIndexScore, @EngagementScore, @ParticipationRate, @TopRiskTheme, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Culture', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Culture_Update
-- Description:   Updates an existing Culture record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Culture_Update
    @CultureId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @SurveyPeriod nvarchar(20),
    @CultureIndexScore decimal(7,4),
    @EngagementScore decimal(7,4),
    @ParticipationRate decimal(7,4),
    @TopRiskTheme nvarchar(300),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CultureId IS NULL
            THROW 50001, N'CultureId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Culture] WHERE CultureId = @CultureId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Culture] WHERE CultureId = @CultureId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Culture', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'CultureId=', @CultureId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Analytics].[Culture]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            SurveyPeriod = @SurveyPeriod,
            CultureIndexScore = @CultureIndexScore,
            EngagementScore = @EngagementScore,
            ParticipationRate = @ParticipationRate,
            TopRiskTheme = @TopRiskTheme,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE CultureId = @CultureId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; User=', SYSTEM_USER);
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
                N'Culture', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Culture_GetById
-- Description:   Retrieves a single Culture record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Culture_GetById
    @CultureId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CultureId IS NULL
            THROW 50001, N'CultureId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Culture', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'CultureId=', @CultureId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.CultureId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.SurveyPeriod, e.CultureIndexScore, e.EngagementScore, e.ParticipationRate, e.TopRiskTheme
        FROM [Analytics].[Culture] AS e
        WHERE e.CultureId = @CultureId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; User=', SYSTEM_USER);
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
                N'Culture', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Culture_List
-- Description:   Returns a paginated, soft-delete-filtered list of Culture records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Culture_List
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
                N'Culture', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.CultureId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.SurveyPeriod, e.CultureIndexScore, e.EngagementScore, e.ParticipationRate, e.TopRiskTheme
        FROM [Analytics].[Culture] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.CultureId
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
                N'Culture', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Culture_Delete
-- Description:   Soft-deletes a Culture record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Culture_Delete
    @CultureId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CultureId IS NULL
            THROW 50001, N'CultureId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Culture] WHERE CultureId = @CultureId AND ISNULL(IsDeleted,0) = 0)
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
                N'Culture', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'CultureId=', @CultureId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Analytics].[Culture]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE CultureId = @CultureId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; User=', SYSTEM_USER);
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
                N'Culture', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_CultureHistoryEntry_Create
-- Description:   Inserts a new append-only CultureHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_CultureHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @CultureId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @CultureIndexScoreAtTransition decimal(7,4)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'CultureHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[CultureHistoryEntry] (HistoryId, CultureId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, CultureIndexScoreAtTransition)
        VALUES (@HistoryId, @CultureId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @CultureIndexScoreAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'CultureHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_CultureHistoryEntry_GetByParentId
-- Description:   Returns all CultureHistoryEntry records for a given CultureId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_CultureHistoryEntry_GetByParentId
    @CultureId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CultureId IS NULL
            THROW 50001, N'CultureId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'CultureHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'CultureId=', @CultureId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.CultureId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.CultureIndexScoreAtTransition
        FROM [Analytics].[CultureHistoryEntry] AS e
        WHERE e.CultureId = @CultureId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CultureId=', @CultureId, N'; User=', SYSTEM_USER);
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
                N'CultureHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Dashboard_Create
-- Description:   Inserts a new Dashboard record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Dashboard_Create
    @DashboardId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @DashboardName nvarchar(300),
    @DashboardType nvarchar(100),
    @OwnerId uniqueidentifier,
    @RefreshIntervalSeconds int,
    @WidgetCount int,
    @IsPublished bit,
    @LastRefreshedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DashboardId IS NULL
            THROW 50001, N'DashboardId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Dashboard', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'DashboardId=', @DashboardId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[Dashboard] (DashboardId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, DashboardName, DashboardType, OwnerId, RefreshIntervalSeconds, WidgetCount, IsPublished, LastRefreshedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@DashboardId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @DashboardName, @DashboardType, @OwnerId, @RefreshIntervalSeconds, @WidgetCount, @IsPublished, @LastRefreshedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DashboardId=', @DashboardId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Dashboard', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Dashboard_Update
-- Description:   Updates an existing Dashboard record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Dashboard_Update
    @DashboardId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @DashboardName nvarchar(300),
    @DashboardType nvarchar(100),
    @OwnerId uniqueidentifier,
    @RefreshIntervalSeconds int,
    @WidgetCount int,
    @IsPublished bit,
    @LastRefreshedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DashboardId IS NULL
            THROW 50001, N'DashboardId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Dashboard] WHERE DashboardId = @DashboardId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Dashboard] WHERE DashboardId = @DashboardId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Dashboard', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'DashboardId=', @DashboardId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Analytics].[Dashboard]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            DashboardName = @DashboardName,
            DashboardType = @DashboardType,
            OwnerId = @OwnerId,
            RefreshIntervalSeconds = @RefreshIntervalSeconds,
            WidgetCount = @WidgetCount,
            IsPublished = @IsPublished,
            LastRefreshedAtUtc = @LastRefreshedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE DashboardId = @DashboardId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DashboardId=', @DashboardId, N'; User=', SYSTEM_USER);
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
                N'Dashboard', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Dashboard_GetById
-- Description:   Retrieves a single Dashboard record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Dashboard_GetById
    @DashboardId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DashboardId IS NULL
            THROW 50001, N'DashboardId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Dashboard', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'DashboardId=', @DashboardId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.DashboardId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.DashboardName, e.DashboardType, e.OwnerId, e.RefreshIntervalSeconds, e.WidgetCount, e.IsPublished, e.LastRefreshedAtUtc
        FROM [Analytics].[Dashboard] AS e
        WHERE e.DashboardId = @DashboardId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DashboardId=', @DashboardId, N'; User=', SYSTEM_USER);
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
                N'Dashboard', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Dashboard_List
-- Description:   Returns a paginated, soft-delete-filtered list of Dashboard records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Dashboard_List
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
                N'Dashboard', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.DashboardId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.DashboardName, e.DashboardType, e.OwnerId, e.RefreshIntervalSeconds, e.WidgetCount, e.IsPublished, e.LastRefreshedAtUtc
        FROM [Analytics].[Dashboard] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.DashboardId
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
                N'Dashboard', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Dashboard_Delete
-- Description:   Soft-deletes a Dashboard record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Dashboard_Delete
    @DashboardId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DashboardId IS NULL
            THROW 50001, N'DashboardId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Dashboard] WHERE DashboardId = @DashboardId AND ISNULL(IsDeleted,0) = 0)
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
                N'Dashboard', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'DashboardId=', @DashboardId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Analytics].[Dashboard]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE DashboardId = @DashboardId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DashboardId=', @DashboardId, N'; User=', SYSTEM_USER);
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
                N'Dashboard', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_DashboardHistoryEntry_Create
-- Description:   Inserts a new append-only DashboardHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_DashboardHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @DashboardId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @IsPublishedAtTransition bit
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'DashboardHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[DashboardHistoryEntry] (HistoryId, DashboardId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, IsPublishedAtTransition)
        VALUES (@HistoryId, @DashboardId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @IsPublishedAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'DashboardHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_DashboardHistoryEntry_GetByParentId
-- Description:   Returns all DashboardHistoryEntry records for a given DashboardId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_DashboardHistoryEntry_GetByParentId
    @DashboardId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DashboardId IS NULL
            THROW 50001, N'DashboardId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'DashboardHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'DashboardId=', @DashboardId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.DashboardId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.IsPublishedAtTransition
        FROM [Analytics].[DashboardHistoryEntry] AS e
        WHERE e.DashboardId = @DashboardId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DashboardId=', @DashboardId, N'; User=', SYSTEM_USER);
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
                N'DashboardHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Engagement_Create
-- Description:   Inserts a new Engagement record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Engagement_Create
    @EngagementId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @WorkerId uniqueidentifier,
    @EngagementScore decimal(7,4),
    @SurveyPeriod nvarchar(20),
    @ParticipationChannel nvarchar(100),
    @ResponseCount int,
    @LastEngagedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EngagementId IS NULL
            THROW 50001, N'EngagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Engagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EngagementId=', @EngagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[Engagement] (EngagementId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, WorkerId, EngagementScore, SurveyPeriod, ParticipationChannel, ResponseCount, LastEngagedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@EngagementId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @WorkerId, @EngagementScore, @SurveyPeriod, @ParticipationChannel, @ResponseCount, @LastEngagedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EngagementId=', @EngagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Engagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Engagement_Update
-- Description:   Updates an existing Engagement record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Engagement_Update
    @EngagementId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @WorkerId uniqueidentifier,
    @EngagementScore decimal(7,4),
    @SurveyPeriod nvarchar(20),
    @ParticipationChannel nvarchar(100),
    @ResponseCount int,
    @LastEngagedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EngagementId IS NULL
            THROW 50001, N'EngagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Engagement] WHERE EngagementId = @EngagementId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Engagement] WHERE EngagementId = @EngagementId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Engagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EngagementId=', @EngagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Analytics].[Engagement]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            WorkerId = @WorkerId,
            EngagementScore = @EngagementScore,
            SurveyPeriod = @SurveyPeriod,
            ParticipationChannel = @ParticipationChannel,
            ResponseCount = @ResponseCount,
            LastEngagedAtUtc = @LastEngagedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE EngagementId = @EngagementId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EngagementId=', @EngagementId, N'; User=', SYSTEM_USER);
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
                N'Engagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Engagement_GetById
-- Description:   Retrieves a single Engagement record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Engagement_GetById
    @EngagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EngagementId IS NULL
            THROW 50001, N'EngagementId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Engagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'EngagementId=', @EngagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.EngagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.WorkerId, e.EngagementScore, e.SurveyPeriod, e.ParticipationChannel, e.ResponseCount, e.LastEngagedAtUtc
        FROM [Analytics].[Engagement] AS e
        WHERE e.EngagementId = @EngagementId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EngagementId=', @EngagementId, N'; User=', SYSTEM_USER);
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
                N'Engagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Engagement_List
-- Description:   Returns a paginated, soft-delete-filtered list of Engagement records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Engagement_List
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
                N'Engagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.EngagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.WorkerId, e.EngagementScore, e.SurveyPeriod, e.ParticipationChannel, e.ResponseCount, e.LastEngagedAtUtc
        FROM [Analytics].[Engagement] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.EngagementId
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
                N'Engagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_Engagement_Delete
-- Description:   Soft-deletes a Engagement record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_Engagement_Delete
    @EngagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EngagementId IS NULL
            THROW 50001, N'EngagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[Engagement] WHERE EngagementId = @EngagementId AND ISNULL(IsDeleted,0) = 0)
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
                N'Engagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EngagementId=', @EngagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Analytics].[Engagement]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE EngagementId = @EngagementId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EngagementId=', @EngagementId, N'; User=', SYSTEM_USER);
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
                N'Engagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_EngagementHistoryEntry_Create
-- Description:   Inserts a new append-only EngagementHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_EngagementHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @EngagementId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @EngagementScoreAtTransition decimal(7,4)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EngagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[EngagementHistoryEntry] (HistoryId, EngagementId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, EngagementScoreAtTransition)
        VALUES (@HistoryId, @EngagementId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @EngagementScoreAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'EngagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_EngagementHistoryEntry_GetByParentId
-- Description:   Returns all EngagementHistoryEntry records for a given EngagementId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_EngagementHistoryEntry_GetByParentId
    @EngagementId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EngagementId IS NULL
            THROW 50001, N'EngagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EngagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'EngagementId=', @EngagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.EngagementId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.EngagementScoreAtTransition
        FROM [Analytics].[EngagementHistoryEntry] AS e
        WHERE e.EngagementId = @EngagementId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EngagementId=', @EngagementId, N'; User=', SYSTEM_USER);
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
                N'EngagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_PredictiveAnalytics_Create
-- Description:   Inserts a new PredictiveAnalytics record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_PredictiveAnalytics_Create
    @PredictiveAnalyticsId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ModelName nvarchar(200),
    @PredictionType nvarchar(100),
    @TargetEntityId uniqueidentifier,
    @PredictedRiskScore decimal(7,4),
    @ConfidenceLevel decimal(7,4),
    @PredictionHorizonDays int,
    @GeneratedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PredictiveAnalyticsId IS NULL
            THROW 50001, N'PredictiveAnalyticsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PredictiveAnalytics', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[PredictiveAnalytics] (PredictiveAnalyticsId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, ModelName, PredictionType, TargetEntityId, PredictedRiskScore, ConfidenceLevel, PredictionHorizonDays, GeneratedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@PredictiveAnalyticsId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @ModelName, @PredictionType, @TargetEntityId, @PredictedRiskScore, @ConfidenceLevel, @PredictionHorizonDays, @GeneratedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'PredictiveAnalytics', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_PredictiveAnalytics_Update
-- Description:   Updates an existing PredictiveAnalytics record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_PredictiveAnalytics_Update
    @PredictiveAnalyticsId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ModelName nvarchar(200),
    @PredictionType nvarchar(100),
    @TargetEntityId uniqueidentifier,
    @PredictedRiskScore decimal(7,4),
    @ConfidenceLevel decimal(7,4),
    @PredictionHorizonDays int,
    @GeneratedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PredictiveAnalyticsId IS NULL
            THROW 50001, N'PredictiveAnalyticsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[PredictiveAnalytics] WHERE PredictiveAnalyticsId = @PredictiveAnalyticsId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Analytics].[PredictiveAnalytics] WHERE PredictiveAnalyticsId = @PredictiveAnalyticsId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'PredictiveAnalytics', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Analytics].[PredictiveAnalytics]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            ModelName = @ModelName,
            PredictionType = @PredictionType,
            TargetEntityId = @TargetEntityId,
            PredictedRiskScore = @PredictedRiskScore,
            ConfidenceLevel = @ConfidenceLevel,
            PredictionHorizonDays = @PredictionHorizonDays,
            GeneratedAtUtc = @GeneratedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE PredictiveAnalyticsId = @PredictiveAnalyticsId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId, N'; User=', SYSTEM_USER);
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
                N'PredictiveAnalytics', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_PredictiveAnalytics_GetById
-- Description:   Retrieves a single PredictiveAnalytics record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_PredictiveAnalytics_GetById
    @PredictiveAnalyticsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PredictiveAnalyticsId IS NULL
            THROW 50001, N'PredictiveAnalyticsId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PredictiveAnalytics', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PredictiveAnalyticsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ModelName, e.PredictionType, e.TargetEntityId, e.PredictedRiskScore, e.ConfidenceLevel, e.PredictionHorizonDays, e.GeneratedAtUtc
        FROM [Analytics].[PredictiveAnalytics] AS e
        WHERE e.PredictiveAnalyticsId = @PredictiveAnalyticsId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId, N'; User=', SYSTEM_USER);
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
                N'PredictiveAnalytics', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_PredictiveAnalytics_List
-- Description:   Returns a paginated, soft-delete-filtered list of PredictiveAnalytics records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_PredictiveAnalytics_List
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
                N'PredictiveAnalytics', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PredictiveAnalyticsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ModelName, e.PredictionType, e.TargetEntityId, e.PredictedRiskScore, e.ConfidenceLevel, e.PredictionHorizonDays, e.GeneratedAtUtc
        FROM [Analytics].[PredictiveAnalytics] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.PredictiveAnalyticsId
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
                N'PredictiveAnalytics', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_PredictiveAnalytics_Delete
-- Description:   Soft-deletes a PredictiveAnalytics record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_PredictiveAnalytics_Delete
    @PredictiveAnalyticsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PredictiveAnalyticsId IS NULL
            THROW 50001, N'PredictiveAnalyticsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[PredictiveAnalytics] WHERE PredictiveAnalyticsId = @PredictiveAnalyticsId AND ISNULL(IsDeleted,0) = 0)
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
                N'PredictiveAnalytics', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Analytics].[PredictiveAnalytics]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE PredictiveAnalyticsId = @PredictiveAnalyticsId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId, N'; User=', SYSTEM_USER);
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
                N'PredictiveAnalytics', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_PredictiveAnalyticsHistoryEntry_Create
-- Description:   Inserts a new append-only PredictiveAnalyticsHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_PredictiveAnalyticsHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @PredictiveAnalyticsId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @PredictedRiskScoreAtTransition decimal(7,4)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PredictiveAnalyticsHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[PredictiveAnalyticsHistoryEntry] (HistoryId, PredictiveAnalyticsId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, PredictedRiskScoreAtTransition)
        VALUES (@HistoryId, @PredictiveAnalyticsId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @PredictedRiskScoreAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'PredictiveAnalyticsHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_PredictiveAnalyticsHistoryEntry_GetByParentId
-- Description:   Returns all PredictiveAnalyticsHistoryEntry records for a given PredictiveAnalyticsId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_PredictiveAnalyticsHistoryEntry_GetByParentId
    @PredictiveAnalyticsId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PredictiveAnalyticsId IS NULL
            THROW 50001, N'PredictiveAnalyticsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PredictiveAnalyticsHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.PredictiveAnalyticsId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.PredictedRiskScoreAtTransition
        FROM [Analytics].[PredictiveAnalyticsHistoryEntry] AS e
        WHERE e.PredictiveAnalyticsId = @PredictiveAnalyticsId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PredictiveAnalyticsId=', @PredictiveAnalyticsId, N'; User=', SYSTEM_USER);
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
                N'PredictiveAnalyticsHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_ScoreCards_Create
-- Description:   Inserts a new ScoreCards record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_ScoreCards_Create
    @ScoreCardsId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ScoreCardName nvarchar(300),
    @ReportingPeriod nvarchar(20),
    @KpiCategory nvarchar(100),
    @PerformanceIndex decimal(7,4),
    @GeneratedForSiteId uniqueidentifier,
    @GeneratedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ScoreCardsId IS NULL
            THROW 50001, N'ScoreCardsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ScoreCards', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ScoreCardsId=', @ScoreCardsId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[ScoreCards] (ScoreCardsId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, ScoreCardName, ReportingPeriod, KpiCategory, PerformanceIndex, GeneratedForSiteId, GeneratedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@ScoreCardsId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @ScoreCardName, @ReportingPeriod, @KpiCategory, @PerformanceIndex, @GeneratedForSiteId, @GeneratedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ScoreCardsId=', @ScoreCardsId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'ScoreCards', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_ScoreCards_Update
-- Description:   Updates an existing ScoreCards record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_ScoreCards_Update
    @ScoreCardsId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ScoreCardName nvarchar(300),
    @ReportingPeriod nvarchar(20),
    @KpiCategory nvarchar(100),
    @PerformanceIndex decimal(7,4),
    @GeneratedForSiteId uniqueidentifier,
    @GeneratedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ScoreCardsId IS NULL
            THROW 50001, N'ScoreCardsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[ScoreCards] WHERE ScoreCardsId = @ScoreCardsId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Analytics].[ScoreCards] WHERE ScoreCardsId = @ScoreCardsId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ScoreCards', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ScoreCardsId=', @ScoreCardsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Analytics].[ScoreCards]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            ScoreCardName = @ScoreCardName,
            ReportingPeriod = @ReportingPeriod,
            KpiCategory = @KpiCategory,
            PerformanceIndex = @PerformanceIndex,
            GeneratedForSiteId = @GeneratedForSiteId,
            GeneratedAtUtc = @GeneratedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ScoreCardsId = @ScoreCardsId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ScoreCardsId=', @ScoreCardsId, N'; User=', SYSTEM_USER);
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
                N'ScoreCards', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_ScoreCards_GetById
-- Description:   Retrieves a single ScoreCards record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_ScoreCards_GetById
    @ScoreCardsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ScoreCardsId IS NULL
            THROW 50001, N'ScoreCardsId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ScoreCards', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ScoreCardsId=', @ScoreCardsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ScoreCardsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ScoreCardName, e.ReportingPeriod, e.KpiCategory, e.PerformanceIndex, e.GeneratedForSiteId, e.GeneratedAtUtc
        FROM [Analytics].[ScoreCards] AS e
        WHERE e.ScoreCardsId = @ScoreCardsId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ScoreCardsId=', @ScoreCardsId, N'; User=', SYSTEM_USER);
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
                N'ScoreCards', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_ScoreCards_List
-- Description:   Returns a paginated, soft-delete-filtered list of ScoreCards records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_ScoreCards_List
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
                N'ScoreCards', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ScoreCardsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ScoreCardName, e.ReportingPeriod, e.KpiCategory, e.PerformanceIndex, e.GeneratedForSiteId, e.GeneratedAtUtc
        FROM [Analytics].[ScoreCards] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ScoreCardsId
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
                N'ScoreCards', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_ScoreCards_Delete
-- Description:   Soft-deletes a ScoreCards record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_ScoreCards_Delete
    @ScoreCardsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ScoreCardsId IS NULL
            THROW 50001, N'ScoreCardsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Analytics].[ScoreCards] WHERE ScoreCardsId = @ScoreCardsId AND ISNULL(IsDeleted,0) = 0)
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
                N'ScoreCards', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ScoreCardsId=', @ScoreCardsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Analytics].[ScoreCards]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ScoreCardsId = @ScoreCardsId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ScoreCardsId=', @ScoreCardsId, N'; User=', SYSTEM_USER);
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
                N'ScoreCards', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_ScoreCardsHistoryEntry_Create
-- Description:   Inserts a new append-only ScoreCardsHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_ScoreCardsHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @ScoreCardsId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @PerformanceIndexAtTransition decimal(7,4)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ScoreCardsHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Analytics].[ScoreCardsHistoryEntry] (HistoryId, ScoreCardsId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, PerformanceIndexAtTransition)
        VALUES (@HistoryId, @ScoreCardsId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @PerformanceIndexAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'ScoreCardsHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Analytics].sp_ScoreCardsHistoryEntry_GetByParentId
-- Description:   Returns all ScoreCardsHistoryEntry records for a given ScoreCardsId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Analytics].sp_ScoreCardsHistoryEntry_GetByParentId
    @ScoreCardsId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ScoreCardsId IS NULL
            THROW 50001, N'ScoreCardsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ScoreCardsHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ScoreCardsId=', @ScoreCardsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.ScoreCardsId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.PerformanceIndexAtTransition
        FROM [Analytics].[ScoreCardsHistoryEntry] AS e
        WHERE e.ScoreCardsId = @ScoreCardsId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ScoreCardsId=', @ScoreCardsId, N'; User=', SYSTEM_USER);
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
                N'ScoreCardsHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Auditing_Create
-- Description:   Inserts a new Auditing record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Auditing_Create
    @AuditingId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @AuditTitle nvarchar(300),
    @AuditType nvarchar(100),
    @Scope nvarchar(MAX),
    @AuditorId uniqueidentifier,
    @ScheduledAtUtc datetime2(7),
    @CompletedAtUtc datetime2(7),
    @FindingsCount int,
    @Score decimal(7,4),
    @ComplianceRating nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AuditingId IS NULL
            THROW 50001, N'AuditingId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Auditing', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AuditingId=', @AuditingId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[Auditing] (AuditingId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, AuditTitle, AuditType, Scope, AuditorId, ScheduledAtUtc, CompletedAtUtc, FindingsCount, Score, ComplianceRating, IsDeleted, CreatedUtc)
        VALUES (@AuditingId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @AuditTitle, @AuditType, @Scope, @AuditorId, @ScheduledAtUtc, @CompletedAtUtc, @FindingsCount, @Score, @ComplianceRating, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditingId=', @AuditingId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Auditing', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Auditing_Update
-- Description:   Updates an existing Auditing record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Auditing_Update
    @AuditingId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @AuditTitle nvarchar(300),
    @AuditType nvarchar(100),
    @Scope nvarchar(MAX),
    @AuditorId uniqueidentifier,
    @ScheduledAtUtc datetime2(7),
    @CompletedAtUtc datetime2(7),
    @FindingsCount int,
    @Score decimal(7,4),
    @ComplianceRating nvarchar(50),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AuditingId IS NULL
            THROW 50001, N'AuditingId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Auditing] WHERE AuditingId = @AuditingId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Auditing] WHERE AuditingId = @AuditingId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Auditing', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AuditingId=', @AuditingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Compliance].[Auditing]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            AuditTitle = @AuditTitle,
            AuditType = @AuditType,
            Scope = @Scope,
            AuditorId = @AuditorId,
            ScheduledAtUtc = @ScheduledAtUtc,
            CompletedAtUtc = @CompletedAtUtc,
            FindingsCount = @FindingsCount,
            Score = @Score,
            ComplianceRating = @ComplianceRating,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE AuditingId = @AuditingId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditingId=', @AuditingId, N'; User=', SYSTEM_USER);
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
                N'Auditing', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Auditing_GetById
-- Description:   Retrieves a single Auditing record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Auditing_GetById
    @AuditingId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AuditingId IS NULL
            THROW 50001, N'AuditingId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Auditing', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'AuditingId=', @AuditingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AuditingId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.AuditTitle, e.AuditType, e.Scope, e.AuditorId, e.ScheduledAtUtc, e.CompletedAtUtc, e.FindingsCount, e.Score, e.ComplianceRating
        FROM [Compliance].[Auditing] AS e
        WHERE e.AuditingId = @AuditingId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditingId=', @AuditingId, N'; User=', SYSTEM_USER);
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
                N'Auditing', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Auditing_List
-- Description:   Returns a paginated, soft-delete-filtered list of Auditing records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Auditing_List
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
                N'Auditing', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AuditingId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.AuditTitle, e.AuditType, e.Scope, e.AuditorId, e.ScheduledAtUtc, e.CompletedAtUtc, e.FindingsCount, e.Score, e.ComplianceRating
        FROM [Compliance].[Auditing] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.AuditingId
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
                N'Auditing', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Auditing_Delete
-- Description:   Soft-deletes a Auditing record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Auditing_Delete
    @AuditingId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AuditingId IS NULL
            THROW 50001, N'AuditingId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Auditing] WHERE AuditingId = @AuditingId AND ISNULL(IsDeleted,0) = 0)
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
                N'Auditing', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AuditingId=', @AuditingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Compliance].[Auditing]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE AuditingId = @AuditingId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditingId=', @AuditingId, N'; User=', SYSTEM_USER);
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
                N'Auditing', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_AuditingHistoryEntry_Create
-- Description:   Inserts a new append-only AuditingHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_AuditingHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @AuditingId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @FindingsCountAtTransition int,
    @ScoreAtTransition decimal(7,4),
    @ComplianceRatingAtTransition nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AuditingHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[AuditingHistoryEntry] (HistoryId, AuditingId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, FindingsCountAtTransition, ScoreAtTransition, ComplianceRatingAtTransition)
        VALUES (@HistoryId, @AuditingId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @FindingsCountAtTransition, @ScoreAtTransition, @ComplianceRatingAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'AuditingHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_AuditingHistoryEntry_GetByParentId
-- Description:   Returns all AuditingHistoryEntry records for a given AuditingId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_AuditingHistoryEntry_GetByParentId
    @AuditingId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AuditingId IS NULL
            THROW 50001, N'AuditingId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AuditingHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'AuditingId=', @AuditingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.AuditingId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.FindingsCountAtTransition, e.ScoreAtTransition, e.ComplianceRatingAtTransition
        FROM [Compliance].[AuditingHistoryEntry] AS e
        WHERE e.AuditingId = @AuditingId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AuditingId=', @AuditingId, N'; User=', SYSTEM_USER);
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
                N'AuditingHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Certification_Create
-- Description:   Inserts a new Certification record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Certification_Create
    @CertificationId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @HolderWorkerId uniqueidentifier,
    @CertificationName nvarchar(300),
    @CertificationBody nvarchar(150),
    @CertificationNumber nvarchar(100),
    @IssuedAtUtc datetime2(7),
    @ExpiresAtUtc datetime2(7),
    @RenewalRequired bit
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CertificationId IS NULL
            THROW 50001, N'CertificationId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Certification', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'CertificationId=', @CertificationId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[Certification] (CertificationId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, HolderWorkerId, CertificationName, CertificationBody, CertificationNumber, IssuedAtUtc, ExpiresAtUtc, RenewalRequired, IsDeleted, CreatedUtc)
        VALUES (@CertificationId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @HolderWorkerId, @CertificationName, @CertificationBody, @CertificationNumber, @IssuedAtUtc, @ExpiresAtUtc, @RenewalRequired, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CertificationId=', @CertificationId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Certification', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Certification_Update
-- Description:   Updates an existing Certification record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Certification_Update
    @CertificationId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @HolderWorkerId uniqueidentifier,
    @CertificationName nvarchar(300),
    @CertificationBody nvarchar(150),
    @CertificationNumber nvarchar(100),
    @IssuedAtUtc datetime2(7),
    @ExpiresAtUtc datetime2(7),
    @RenewalRequired bit,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CertificationId IS NULL
            THROW 50001, N'CertificationId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Certification] WHERE CertificationId = @CertificationId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Certification] WHERE CertificationId = @CertificationId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Certification', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'CertificationId=', @CertificationId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Compliance].[Certification]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            HolderWorkerId = @HolderWorkerId,
            CertificationName = @CertificationName,
            CertificationBody = @CertificationBody,
            CertificationNumber = @CertificationNumber,
            IssuedAtUtc = @IssuedAtUtc,
            ExpiresAtUtc = @ExpiresAtUtc,
            RenewalRequired = @RenewalRequired,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE CertificationId = @CertificationId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CertificationId=', @CertificationId, N'; User=', SYSTEM_USER);
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
                N'Certification', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Certification_GetById
-- Description:   Retrieves a single Certification record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Certification_GetById
    @CertificationId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CertificationId IS NULL
            THROW 50001, N'CertificationId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Certification', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'CertificationId=', @CertificationId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.CertificationId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.HolderWorkerId, e.CertificationName, e.CertificationBody, e.CertificationNumber, e.IssuedAtUtc, e.ExpiresAtUtc, e.RenewalRequired
        FROM [Compliance].[Certification] AS e
        WHERE e.CertificationId = @CertificationId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CertificationId=', @CertificationId, N'; User=', SYSTEM_USER);
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
                N'Certification', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Certification_List
-- Description:   Returns a paginated, soft-delete-filtered list of Certification records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Certification_List
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
                N'Certification', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.CertificationId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.HolderWorkerId, e.CertificationName, e.CertificationBody, e.CertificationNumber, e.IssuedAtUtc, e.ExpiresAtUtc, e.RenewalRequired
        FROM [Compliance].[Certification] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.CertificationId
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
                N'Certification', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Certification_Delete
-- Description:   Soft-deletes a Certification record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Certification_Delete
    @CertificationId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CertificationId IS NULL
            THROW 50001, N'CertificationId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Certification] WHERE CertificationId = @CertificationId AND ISNULL(IsDeleted,0) = 0)
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
                N'Certification', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'CertificationId=', @CertificationId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Compliance].[Certification]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE CertificationId = @CertificationId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CertificationId=', @CertificationId, N'; User=', SYSTEM_USER);
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
                N'Certification', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_CertificationHistoryEntry_Create
-- Description:   Inserts a new append-only CertificationHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_CertificationHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @CertificationId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @ExpiresAtUtcAtTransition datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'CertificationHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[CertificationHistoryEntry] (HistoryId, CertificationId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, ExpiresAtUtcAtTransition)
        VALUES (@HistoryId, @CertificationId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @ExpiresAtUtcAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'CertificationHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_CertificationHistoryEntry_GetByParentId
-- Description:   Returns all CertificationHistoryEntry records for a given CertificationId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_CertificationHistoryEntry_GetByParentId
    @CertificationId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @CertificationId IS NULL
            THROW 50001, N'CertificationId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'CertificationHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'CertificationId=', @CertificationId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.CertificationId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.ExpiresAtUtcAtTransition
        FROM [Compliance].[CertificationHistoryEntry] AS e
        WHERE e.CertificationId = @CertificationId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'CertificationId=', @CertificationId, N'; User=', SYSTEM_USER);
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
                N'CertificationHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Compliance_Create
-- Description:   Inserts a new Compliance record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Compliance_Create
    @ComplianceId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @RegulationReference nvarchar(200),
    @RuleDescription nvarchar(MAX),
    @ComplianceScore decimal(7,4),
    @NonConformanceCount int,
    @ResponsiblePartyId uniqueidentifier,
    @LastEvaluatedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ComplianceId IS NULL
            THROW 50001, N'ComplianceId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Compliance', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ComplianceId=', @ComplianceId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[Compliance] (ComplianceId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, RegulationReference, RuleDescription, ComplianceScore, NonConformanceCount, ResponsiblePartyId, LastEvaluatedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@ComplianceId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @RegulationReference, @RuleDescription, @ComplianceScore, @NonConformanceCount, @ResponsiblePartyId, @LastEvaluatedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ComplianceId=', @ComplianceId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Compliance', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Compliance_Update
-- Description:   Updates an existing Compliance record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Compliance_Update
    @ComplianceId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @RegulationReference nvarchar(200),
    @RuleDescription nvarchar(MAX),
    @ComplianceScore decimal(7,4),
    @NonConformanceCount int,
    @ResponsiblePartyId uniqueidentifier,
    @LastEvaluatedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ComplianceId IS NULL
            THROW 50001, N'ComplianceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Compliance] WHERE ComplianceId = @ComplianceId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Compliance] WHERE ComplianceId = @ComplianceId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Compliance', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ComplianceId=', @ComplianceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Compliance].[Compliance]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            RegulationReference = @RegulationReference,
            RuleDescription = @RuleDescription,
            ComplianceScore = @ComplianceScore,
            NonConformanceCount = @NonConformanceCount,
            ResponsiblePartyId = @ResponsiblePartyId,
            LastEvaluatedAtUtc = @LastEvaluatedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ComplianceId = @ComplianceId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ComplianceId=', @ComplianceId, N'; User=', SYSTEM_USER);
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
                N'Compliance', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Compliance_GetById
-- Description:   Retrieves a single Compliance record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Compliance_GetById
    @ComplianceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ComplianceId IS NULL
            THROW 50001, N'ComplianceId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Compliance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ComplianceId=', @ComplianceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ComplianceId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.RegulationReference, e.RuleDescription, e.ComplianceScore, e.NonConformanceCount, e.ResponsiblePartyId, e.LastEvaluatedAtUtc
        FROM [Compliance].[Compliance] AS e
        WHERE e.ComplianceId = @ComplianceId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ComplianceId=', @ComplianceId, N'; User=', SYSTEM_USER);
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
                N'Compliance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Compliance_List
-- Description:   Returns a paginated, soft-delete-filtered list of Compliance records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Compliance_List
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
                N'Compliance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ComplianceId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.RegulationReference, e.RuleDescription, e.ComplianceScore, e.NonConformanceCount, e.ResponsiblePartyId, e.LastEvaluatedAtUtc
        FROM [Compliance].[Compliance] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ComplianceId
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
                N'Compliance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Compliance_Delete
-- Description:   Soft-deletes a Compliance record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Compliance_Delete
    @ComplianceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ComplianceId IS NULL
            THROW 50001, N'ComplianceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Compliance] WHERE ComplianceId = @ComplianceId AND ISNULL(IsDeleted,0) = 0)
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
                N'Compliance', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ComplianceId=', @ComplianceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Compliance].[Compliance]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ComplianceId = @ComplianceId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ComplianceId=', @ComplianceId, N'; User=', SYSTEM_USER);
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
                N'Compliance', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ComplianceHistoryEntry_Create
-- Description:   Inserts a new append-only ComplianceHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ComplianceHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @ComplianceId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @ComplianceScoreAtTransition decimal(7,4),
    @NonConformanceCountAtTransition int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ComplianceHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[ComplianceHistoryEntry] (HistoryId, ComplianceId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, ComplianceScoreAtTransition, NonConformanceCountAtTransition)
        VALUES (@HistoryId, @ComplianceId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @ComplianceScoreAtTransition, @NonConformanceCountAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'ComplianceHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ComplianceHistoryEntry_GetByParentId
-- Description:   Returns all ComplianceHistoryEntry records for a given ComplianceId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ComplianceHistoryEntry_GetByParentId
    @ComplianceId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ComplianceId IS NULL
            THROW 50001, N'ComplianceId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ComplianceHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ComplianceId=', @ComplianceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.ComplianceId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.ComplianceScoreAtTransition, e.NonConformanceCountAtTransition
        FROM [Compliance].[ComplianceHistoryEntry] AS e
        WHERE e.ComplianceId = @ComplianceId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ComplianceId=', @ComplianceId, N'; User=', SYSTEM_USER);
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
                N'ComplianceHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ManagementOfChange_Create
-- Description:   Inserts a new ManagementOfChange record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ManagementOfChange_Create
    @ManagementOfChangeId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ChangeTitle nvarchar(300),
    @ChangeType nvarchar(100),
    @Justification nvarchar(MAX),
    @RiskLevel nvarchar(50),
    @RequestedByUserId uniqueidentifier,
    @ApprovedByUserId uniqueidentifier,
    @ApprovalStatus nvarchar(50),
    @ImplementationAtUtc datetime2(7),
    @ReviewDueAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagementOfChangeId IS NULL
            THROW 50001, N'ManagementOfChangeId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ManagementOfChange', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[ManagementOfChange] (ManagementOfChangeId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, ChangeTitle, ChangeType, Justification, RiskLevel, RequestedByUserId, ApprovedByUserId, ApprovalStatus, ImplementationAtUtc, ReviewDueAtUtc, IsDeleted, CreatedUtc)
        VALUES (@ManagementOfChangeId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @ChangeTitle, @ChangeType, @Justification, @RiskLevel, @RequestedByUserId, @ApprovedByUserId, @ApprovalStatus, @ImplementationAtUtc, @ReviewDueAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'ManagementOfChange', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ManagementOfChange_Update
-- Description:   Updates an existing ManagementOfChange record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ManagementOfChange_Update
    @ManagementOfChangeId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ChangeTitle nvarchar(300),
    @ChangeType nvarchar(100),
    @Justification nvarchar(MAX),
    @RiskLevel nvarchar(50),
    @RequestedByUserId uniqueidentifier,
    @ApprovedByUserId uniqueidentifier,
    @ApprovalStatus nvarchar(50),
    @ImplementationAtUtc datetime2(7),
    @ReviewDueAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagementOfChangeId IS NULL
            THROW 50001, N'ManagementOfChangeId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[ManagementOfChange] WHERE ManagementOfChangeId = @ManagementOfChangeId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Compliance].[ManagementOfChange] WHERE ManagementOfChangeId = @ManagementOfChangeId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ManagementOfChange', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Compliance].[ManagementOfChange]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            ChangeTitle = @ChangeTitle,
            ChangeType = @ChangeType,
            Justification = @Justification,
            RiskLevel = @RiskLevel,
            RequestedByUserId = @RequestedByUserId,
            ApprovedByUserId = @ApprovedByUserId,
            ApprovalStatus = @ApprovalStatus,
            ImplementationAtUtc = @ImplementationAtUtc,
            ReviewDueAtUtc = @ReviewDueAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ManagementOfChangeId = @ManagementOfChangeId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; User=', SYSTEM_USER);
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
                N'ManagementOfChange', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ManagementOfChange_GetById
-- Description:   Retrieves a single ManagementOfChange record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ManagementOfChange_GetById
    @ManagementOfChangeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagementOfChangeId IS NULL
            THROW 50001, N'ManagementOfChangeId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ManagementOfChange', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ManagementOfChangeId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ChangeTitle, e.ChangeType, e.Justification, e.RiskLevel, e.RequestedByUserId, e.ApprovedByUserId, e.ApprovalStatus, e.ImplementationAtUtc, e.ReviewDueAtUtc
        FROM [Compliance].[ManagementOfChange] AS e
        WHERE e.ManagementOfChangeId = @ManagementOfChangeId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; User=', SYSTEM_USER);
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
                N'ManagementOfChange', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ManagementOfChange_List
-- Description:   Returns a paginated, soft-delete-filtered list of ManagementOfChange records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ManagementOfChange_List
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
                N'ManagementOfChange', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ManagementOfChangeId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ChangeTitle, e.ChangeType, e.Justification, e.RiskLevel, e.RequestedByUserId, e.ApprovedByUserId, e.ApprovalStatus, e.ImplementationAtUtc, e.ReviewDueAtUtc
        FROM [Compliance].[ManagementOfChange] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ManagementOfChangeId
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
                N'ManagementOfChange', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ManagementOfChange_Delete
-- Description:   Soft-deletes a ManagementOfChange record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ManagementOfChange_Delete
    @ManagementOfChangeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagementOfChangeId IS NULL
            THROW 50001, N'ManagementOfChangeId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[ManagementOfChange] WHERE ManagementOfChangeId = @ManagementOfChangeId AND ISNULL(IsDeleted,0) = 0)
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
                N'ManagementOfChange', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Compliance].[ManagementOfChange]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ManagementOfChangeId = @ManagementOfChangeId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; User=', SYSTEM_USER);
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
                N'ManagementOfChange', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ManagementOfChangeHistoryEntry_Create
-- Description:   Inserts a new append-only ManagementOfChangeHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ManagementOfChangeHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @ManagementOfChangeId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @ApprovalStatusAtTransition nvarchar(50),
    @RiskLevelAtTransition nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ManagementOfChangeHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[ManagementOfChangeHistoryEntry] (HistoryId, ManagementOfChangeId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, ApprovalStatusAtTransition, RiskLevelAtTransition)
        VALUES (@HistoryId, @ManagementOfChangeId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @ApprovalStatusAtTransition, @RiskLevelAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'ManagementOfChangeHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ManagementOfChangeHistoryEntry_GetByParentId
-- Description:   Returns all ManagementOfChangeHistoryEntry records for a given ManagementOfChangeId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ManagementOfChangeHistoryEntry_GetByParentId
    @ManagementOfChangeId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagementOfChangeId IS NULL
            THROW 50001, N'ManagementOfChangeId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ManagementOfChangeHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.ManagementOfChangeId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.ApprovalStatusAtTransition, e.RiskLevelAtTransition
        FROM [Compliance].[ManagementOfChangeHistoryEntry] AS e
        WHERE e.ManagementOfChangeId = @ManagementOfChangeId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagementOfChangeId=', @ManagementOfChangeId, N'; User=', SYSTEM_USER);
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
                N'ManagementOfChangeHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Permits_Create
-- Description:   Inserts a new Permits record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Permits_Create
    @PermitsId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @PermitType nvarchar(100),
    @PermitNumber nvarchar(100),
    @WorkDescription nvarchar(MAX),
    @Location nvarchar(300),
    @HazardsIdentified nvarchar(MAX),
    @RequestedByUserId uniqueidentifier,
    @ApprovedByUserId uniqueidentifier,
    @IssuedAtUtc datetime2(7),
    @ExpiresAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PermitsId IS NULL
            THROW 50001, N'PermitsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Permits', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PermitsId=', @PermitsId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[Permits] (PermitsId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, PermitType, PermitNumber, WorkDescription, Location, HazardsIdentified, RequestedByUserId, ApprovedByUserId, IssuedAtUtc, ExpiresAtUtc, IsDeleted, CreatedUtc)
        VALUES (@PermitsId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @PermitType, @PermitNumber, @WorkDescription, @Location, @HazardsIdentified, @RequestedByUserId, @ApprovedByUserId, @IssuedAtUtc, @ExpiresAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitsId=', @PermitsId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Permits', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Permits_Update
-- Description:   Updates an existing Permits record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Permits_Update
    @PermitsId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @PermitType nvarchar(100),
    @PermitNumber nvarchar(100),
    @WorkDescription nvarchar(MAX),
    @Location nvarchar(300),
    @HazardsIdentified nvarchar(MAX),
    @RequestedByUserId uniqueidentifier,
    @ApprovedByUserId uniqueidentifier,
    @IssuedAtUtc datetime2(7),
    @ExpiresAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PermitsId IS NULL
            THROW 50001, N'PermitsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Permits] WHERE PermitsId = @PermitsId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Permits] WHERE PermitsId = @PermitsId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Permits', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PermitsId=', @PermitsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Compliance].[Permits]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            PermitType = @PermitType,
            PermitNumber = @PermitNumber,
            WorkDescription = @WorkDescription,
            Location = @Location,
            HazardsIdentified = @HazardsIdentified,
            RequestedByUserId = @RequestedByUserId,
            ApprovedByUserId = @ApprovedByUserId,
            IssuedAtUtc = @IssuedAtUtc,
            ExpiresAtUtc = @ExpiresAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE PermitsId = @PermitsId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitsId=', @PermitsId, N'; User=', SYSTEM_USER);
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
                N'Permits', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Permits_GetById
-- Description:   Retrieves a single Permits record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Permits_GetById
    @PermitsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PermitsId IS NULL
            THROW 50001, N'PermitsId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Permits', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PermitsId=', @PermitsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PermitsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.PermitType, e.PermitNumber, e.WorkDescription, e.Location, e.HazardsIdentified, e.RequestedByUserId, e.ApprovedByUserId, e.IssuedAtUtc, e.ExpiresAtUtc
        FROM [Compliance].[Permits] AS e
        WHERE e.PermitsId = @PermitsId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitsId=', @PermitsId, N'; User=', SYSTEM_USER);
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
                N'Permits', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Permits_List
-- Description:   Returns a paginated, soft-delete-filtered list of Permits records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Permits_List
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
                N'Permits', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PermitsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.PermitType, e.PermitNumber, e.WorkDescription, e.Location, e.HazardsIdentified, e.RequestedByUserId, e.ApprovedByUserId, e.IssuedAtUtc, e.ExpiresAtUtc
        FROM [Compliance].[Permits] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.PermitsId
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
                N'Permits', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Permits_Delete
-- Description:   Soft-deletes a Permits record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Permits_Delete
    @PermitsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PermitsId IS NULL
            THROW 50001, N'PermitsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Permits] WHERE PermitsId = @PermitsId AND ISNULL(IsDeleted,0) = 0)
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
                N'Permits', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PermitsId=', @PermitsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Compliance].[Permits]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE PermitsId = @PermitsId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitsId=', @PermitsId, N'; User=', SYSTEM_USER);
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
                N'Permits', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_PermitsHistoryEntry_Create
-- Description:   Inserts a new append-only PermitsHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_PermitsHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @PermitsId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @ExpiresAtUtcAtTransition datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PermitsHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[PermitsHistoryEntry] (HistoryId, PermitsId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, ExpiresAtUtcAtTransition)
        VALUES (@HistoryId, @PermitsId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @ExpiresAtUtcAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'PermitsHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_PermitsHistoryEntry_GetByParentId
-- Description:   Returns all PermitsHistoryEntry records for a given PermitsId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_PermitsHistoryEntry_GetByParentId
    @PermitsId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PermitsId IS NULL
            THROW 50001, N'PermitsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PermitsHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PermitsId=', @PermitsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.PermitsId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.ExpiresAtUtcAtTransition
        FROM [Compliance].[PermitsHistoryEntry] AS e
        WHERE e.PermitsId = @PermitsId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PermitsId=', @PermitsId, N'; User=', SYSTEM_USER);
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
                N'PermitsHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Policy_Create
-- Description:   Inserts a new Policy record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Policy_Create
    @PolicyId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @PolicyTitle nvarchar(300),
    @PolicyVersion int,
    @Category nvarchar(100),
    @OwnerId uniqueidentifier,
    @EffectiveAtUtc datetime2(7),
    @AcknowledgmentRequired bit,
    @AcknowledgmentCount int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PolicyId IS NULL
            THROW 50001, N'PolicyId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Policy', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PolicyId=', @PolicyId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[Policy] (PolicyId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, PolicyTitle, PolicyVersion, Category, OwnerId, EffectiveAtUtc, AcknowledgmentRequired, AcknowledgmentCount, IsDeleted, CreatedUtc)
        VALUES (@PolicyId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @PolicyTitle, @PolicyVersion, @Category, @OwnerId, @EffectiveAtUtc, @AcknowledgmentRequired, @AcknowledgmentCount, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PolicyId=', @PolicyId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Policy', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Policy_Update
-- Description:   Updates an existing Policy record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Policy_Update
    @PolicyId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @PolicyTitle nvarchar(300),
    @PolicyVersion int,
    @Category nvarchar(100),
    @OwnerId uniqueidentifier,
    @EffectiveAtUtc datetime2(7),
    @AcknowledgmentRequired bit,
    @AcknowledgmentCount int,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PolicyId IS NULL
            THROW 50001, N'PolicyId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Policy] WHERE PolicyId = @PolicyId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Policy] WHERE PolicyId = @PolicyId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Policy', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PolicyId=', @PolicyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Compliance].[Policy]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            PolicyTitle = @PolicyTitle,
            PolicyVersion = @PolicyVersion,
            Category = @Category,
            OwnerId = @OwnerId,
            EffectiveAtUtc = @EffectiveAtUtc,
            AcknowledgmentRequired = @AcknowledgmentRequired,
            AcknowledgmentCount = @AcknowledgmentCount,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE PolicyId = @PolicyId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PolicyId=', @PolicyId, N'; User=', SYSTEM_USER);
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
                N'Policy', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Policy_GetById
-- Description:   Retrieves a single Policy record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Policy_GetById
    @PolicyId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PolicyId IS NULL
            THROW 50001, N'PolicyId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Policy', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PolicyId=', @PolicyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PolicyId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.PolicyTitle, e.PolicyVersion, e.Category, e.OwnerId, e.EffectiveAtUtc, e.AcknowledgmentRequired, e.AcknowledgmentCount
        FROM [Compliance].[Policy] AS e
        WHERE e.PolicyId = @PolicyId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PolicyId=', @PolicyId, N'; User=', SYSTEM_USER);
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
                N'Policy', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Policy_List
-- Description:   Returns a paginated, soft-delete-filtered list of Policy records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Policy_List
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
                N'Policy', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PolicyId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.PolicyTitle, e.PolicyVersion, e.Category, e.OwnerId, e.EffectiveAtUtc, e.AcknowledgmentRequired, e.AcknowledgmentCount
        FROM [Compliance].[Policy] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.PolicyId
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
                N'Policy', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Policy_Delete
-- Description:   Soft-deletes a Policy record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Policy_Delete
    @PolicyId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PolicyId IS NULL
            THROW 50001, N'PolicyId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Policy] WHERE PolicyId = @PolicyId AND ISNULL(IsDeleted,0) = 0)
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
                N'Policy', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PolicyId=', @PolicyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Compliance].[Policy]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE PolicyId = @PolicyId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PolicyId=', @PolicyId, N'; User=', SYSTEM_USER);
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
                N'Policy', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_PolicyHistoryEntry_Create
-- Description:   Inserts a new append-only PolicyHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_PolicyHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @PolicyId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @PolicyVersionAtTransition int,
    @AcknowledgmentCountAtTransition int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PolicyHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[PolicyHistoryEntry] (HistoryId, PolicyId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, PolicyVersionAtTransition, AcknowledgmentCountAtTransition)
        VALUES (@HistoryId, @PolicyId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @PolicyVersionAtTransition, @AcknowledgmentCountAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'PolicyHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_PolicyHistoryEntry_GetByParentId
-- Description:   Returns all PolicyHistoryEntry records for a given PolicyId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_PolicyHistoryEntry_GetByParentId
    @PolicyId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PolicyId IS NULL
            THROW 50001, N'PolicyId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PolicyHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PolicyId=', @PolicyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.PolicyId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.PolicyVersionAtTransition, e.AcknowledgmentCountAtTransition
        FROM [Compliance].[PolicyHistoryEntry] AS e
        WHERE e.PolicyId = @PolicyId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PolicyId=', @PolicyId, N'; User=', SYSTEM_USER);
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
                N'PolicyHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Procedures_Create
-- Description:   Inserts a new Procedures record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Procedures_Create
    @ProceduresId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ProcedureTitle nvarchar(300),
    @ProcedureNumber nvarchar(100),
    @Category nvarchar(100),
    @RevisionNumber int,
    @OwnerId uniqueidentifier,
    @EffectiveAtUtc datetime2(7),
    @ReviewDueAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProceduresId IS NULL
            THROW 50001, N'ProceduresId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Procedures', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ProceduresId=', @ProceduresId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[Procedures] (ProceduresId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, ProcedureTitle, ProcedureNumber, Category, RevisionNumber, OwnerId, EffectiveAtUtc, ReviewDueAtUtc, IsDeleted, CreatedUtc)
        VALUES (@ProceduresId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @ProcedureTitle, @ProcedureNumber, @Category, @RevisionNumber, @OwnerId, @EffectiveAtUtc, @ReviewDueAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProceduresId=', @ProceduresId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Procedures', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Procedures_Update
-- Description:   Updates an existing Procedures record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Procedures_Update
    @ProceduresId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ProcedureTitle nvarchar(300),
    @ProcedureNumber nvarchar(100),
    @Category nvarchar(100),
    @RevisionNumber int,
    @OwnerId uniqueidentifier,
    @EffectiveAtUtc datetime2(7),
    @ReviewDueAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProceduresId IS NULL
            THROW 50001, N'ProceduresId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Procedures] WHERE ProceduresId = @ProceduresId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Procedures] WHERE ProceduresId = @ProceduresId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Procedures', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ProceduresId=', @ProceduresId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Compliance].[Procedures]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            ProcedureTitle = @ProcedureTitle,
            ProcedureNumber = @ProcedureNumber,
            Category = @Category,
            RevisionNumber = @RevisionNumber,
            OwnerId = @OwnerId,
            EffectiveAtUtc = @EffectiveAtUtc,
            ReviewDueAtUtc = @ReviewDueAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ProceduresId = @ProceduresId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProceduresId=', @ProceduresId, N'; User=', SYSTEM_USER);
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
                N'Procedures', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Procedures_GetById
-- Description:   Retrieves a single Procedures record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Procedures_GetById
    @ProceduresId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProceduresId IS NULL
            THROW 50001, N'ProceduresId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Procedures', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ProceduresId=', @ProceduresId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ProceduresId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ProcedureTitle, e.ProcedureNumber, e.Category, e.RevisionNumber, e.OwnerId, e.EffectiveAtUtc, e.ReviewDueAtUtc
        FROM [Compliance].[Procedures] AS e
        WHERE e.ProceduresId = @ProceduresId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProceduresId=', @ProceduresId, N'; User=', SYSTEM_USER);
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
                N'Procedures', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Procedures_List
-- Description:   Returns a paginated, soft-delete-filtered list of Procedures records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Procedures_List
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
                N'Procedures', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ProceduresId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ProcedureTitle, e.ProcedureNumber, e.Category, e.RevisionNumber, e.OwnerId, e.EffectiveAtUtc, e.ReviewDueAtUtc
        FROM [Compliance].[Procedures] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ProceduresId
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
                N'Procedures', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_Procedures_Delete
-- Description:   Soft-deletes a Procedures record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_Procedures_Delete
    @ProceduresId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProceduresId IS NULL
            THROW 50001, N'ProceduresId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[Procedures] WHERE ProceduresId = @ProceduresId AND ISNULL(IsDeleted,0) = 0)
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
                N'Procedures', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ProceduresId=', @ProceduresId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Compliance].[Procedures]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ProceduresId = @ProceduresId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProceduresId=', @ProceduresId, N'; User=', SYSTEM_USER);
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
                N'Procedures', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ProceduresHistoryEntry_Create
-- Description:   Inserts a new append-only ProceduresHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ProceduresHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @ProceduresId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @RevisionNumberAtTransition int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ProceduresHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[ProceduresHistoryEntry] (HistoryId, ProceduresId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, RevisionNumberAtTransition)
        VALUES (@HistoryId, @ProceduresId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @RevisionNumberAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'ProceduresHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_ProceduresHistoryEntry_GetByParentId
-- Description:   Returns all ProceduresHistoryEntry records for a given ProceduresId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_ProceduresHistoryEntry_GetByParentId
    @ProceduresId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProceduresId IS NULL
            THROW 50001, N'ProceduresId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ProceduresHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ProceduresId=', @ProceduresId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.ProceduresId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.RevisionNumberAtTransition
        FROM [Compliance].[ProceduresHistoryEntry] AS e
        WHERE e.ProceduresId = @ProceduresId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProceduresId=', @ProceduresId, N'; User=', SYSTEM_USER);
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
                N'ProceduresHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_RegulatoryCalendar_Create
-- Description:   Inserts a new RegulatoryCalendar record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_RegulatoryCalendar_Create
    @RegulatoryCalendarId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @EventTitle nvarchar(300),
    @RegulatoryBody nvarchar(150),
    @JurisdictionCode nvarchar(50),
    @ComplianceCategory nvarchar(100),
    @ResponsiblePartyId uniqueidentifier,
    @DueAtUtc datetime2(7),
    @ReminderLeadDays int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RegulatoryCalendarId IS NULL
            THROW 50001, N'RegulatoryCalendarId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RegulatoryCalendar', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[RegulatoryCalendar] (RegulatoryCalendarId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, EventTitle, RegulatoryBody, JurisdictionCode, ComplianceCategory, ResponsiblePartyId, DueAtUtc, ReminderLeadDays, IsDeleted, CreatedUtc)
        VALUES (@RegulatoryCalendarId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @EventTitle, @RegulatoryBody, @JurisdictionCode, @ComplianceCategory, @ResponsiblePartyId, @DueAtUtc, @ReminderLeadDays, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'RegulatoryCalendar', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_RegulatoryCalendar_Update
-- Description:   Updates an existing RegulatoryCalendar record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_RegulatoryCalendar_Update
    @RegulatoryCalendarId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @EventTitle nvarchar(300),
    @RegulatoryBody nvarchar(150),
    @JurisdictionCode nvarchar(50),
    @ComplianceCategory nvarchar(100),
    @ResponsiblePartyId uniqueidentifier,
    @DueAtUtc datetime2(7),
    @ReminderLeadDays int,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RegulatoryCalendarId IS NULL
            THROW 50001, N'RegulatoryCalendarId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[RegulatoryCalendar] WHERE RegulatoryCalendarId = @RegulatoryCalendarId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Compliance].[RegulatoryCalendar] WHERE RegulatoryCalendarId = @RegulatoryCalendarId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'RegulatoryCalendar', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Compliance].[RegulatoryCalendar]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            EventTitle = @EventTitle,
            RegulatoryBody = @RegulatoryBody,
            JurisdictionCode = @JurisdictionCode,
            ComplianceCategory = @ComplianceCategory,
            ResponsiblePartyId = @ResponsiblePartyId,
            DueAtUtc = @DueAtUtc,
            ReminderLeadDays = @ReminderLeadDays,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE RegulatoryCalendarId = @RegulatoryCalendarId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId, N'; User=', SYSTEM_USER);
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
                N'RegulatoryCalendar', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_RegulatoryCalendar_GetById
-- Description:   Retrieves a single RegulatoryCalendar record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_RegulatoryCalendar_GetById
    @RegulatoryCalendarId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RegulatoryCalendarId IS NULL
            THROW 50001, N'RegulatoryCalendarId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RegulatoryCalendar', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RegulatoryCalendarId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.EventTitle, e.RegulatoryBody, e.JurisdictionCode, e.ComplianceCategory, e.ResponsiblePartyId, e.DueAtUtc, e.ReminderLeadDays
        FROM [Compliance].[RegulatoryCalendar] AS e
        WHERE e.RegulatoryCalendarId = @RegulatoryCalendarId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId, N'; User=', SYSTEM_USER);
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
                N'RegulatoryCalendar', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_RegulatoryCalendar_List
-- Description:   Returns a paginated, soft-delete-filtered list of RegulatoryCalendar records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_RegulatoryCalendar_List
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
                N'RegulatoryCalendar', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RegulatoryCalendarId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.EventTitle, e.RegulatoryBody, e.JurisdictionCode, e.ComplianceCategory, e.ResponsiblePartyId, e.DueAtUtc, e.ReminderLeadDays
        FROM [Compliance].[RegulatoryCalendar] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.RegulatoryCalendarId
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
                N'RegulatoryCalendar', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_RegulatoryCalendar_Delete
-- Description:   Soft-deletes a RegulatoryCalendar record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_RegulatoryCalendar_Delete
    @RegulatoryCalendarId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RegulatoryCalendarId IS NULL
            THROW 50001, N'RegulatoryCalendarId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Compliance].[RegulatoryCalendar] WHERE RegulatoryCalendarId = @RegulatoryCalendarId AND ISNULL(IsDeleted,0) = 0)
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
                N'RegulatoryCalendar', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Compliance].[RegulatoryCalendar]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE RegulatoryCalendarId = @RegulatoryCalendarId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId, N'; User=', SYSTEM_USER);
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
                N'RegulatoryCalendar', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_RegulatoryCalendarHistoryEntry_Create
-- Description:   Inserts a new append-only RegulatoryCalendarHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_RegulatoryCalendarHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @RegulatoryCalendarId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @DueAtUtcAtTransition datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RegulatoryCalendarHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Compliance].[RegulatoryCalendarHistoryEntry] (HistoryId, RegulatoryCalendarId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, DueAtUtcAtTransition)
        VALUES (@HistoryId, @RegulatoryCalendarId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @DueAtUtcAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'RegulatoryCalendarHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Compliance].sp_RegulatoryCalendarHistoryEntry_GetByParentId
-- Description:   Returns all RegulatoryCalendarHistoryEntry records for a given RegulatoryCalendarId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].sp_RegulatoryCalendarHistoryEntry_GetByParentId
    @RegulatoryCalendarId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RegulatoryCalendarId IS NULL
            THROW 50001, N'RegulatoryCalendarId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RegulatoryCalendarHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.RegulatoryCalendarId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.DueAtUtcAtTransition
        FROM [Compliance].[RegulatoryCalendarHistoryEntry] AS e
        WHERE e.RegulatoryCalendarId = @RegulatoryCalendarId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RegulatoryCalendarId=', @RegulatoryCalendarId, N'; User=', SYSTEM_USER);
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
                N'RegulatoryCalendarHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ContractorVisitor].sp_ContractorVisitor_Create
-- Description:   Inserts a new ContractorVisitor record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ContractorVisitor].sp_ContractorVisitor_Create
    @ContractorVisitorId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorVisitorId IS NULL
            THROW 50001, N'ContractorVisitorId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractorVisitor', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractorVisitorId=', @ContractorVisitorId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [ContractorVisitor].[ContractorVisitor] (ContractorVisitorId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@ContractorVisitorId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorVisitorId=', @ContractorVisitorId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'ContractorVisitor', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ContractorVisitor].sp_ContractorVisitor_Update
-- Description:   Updates an existing ContractorVisitor record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ContractorVisitor].sp_ContractorVisitor_Update
    @ContractorVisitorId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorVisitorId IS NULL
            THROW 50001, N'ContractorVisitorId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [ContractorVisitor].[ContractorVisitor] WHERE ContractorVisitorId = @ContractorVisitorId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [ContractorVisitor].[ContractorVisitor] WHERE ContractorVisitorId = @ContractorVisitorId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ContractorVisitor', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractorVisitorId=', @ContractorVisitorId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [ContractorVisitor].[ContractorVisitor]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractorVisitorId = @ContractorVisitorId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorVisitorId=', @ContractorVisitorId, N'; User=', SYSTEM_USER);
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
                N'ContractorVisitor', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ContractorVisitor].sp_ContractorVisitor_GetById
-- Description:   Retrieves a single ContractorVisitor record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ContractorVisitor].sp_ContractorVisitor_GetById
    @ContractorVisitorId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorVisitorId IS NULL
            THROW 50001, N'ContractorVisitorId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractorVisitor', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ContractorVisitorId=', @ContractorVisitorId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractorVisitorId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [ContractorVisitor].[ContractorVisitor] AS e
        WHERE e.ContractorVisitorId = @ContractorVisitorId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorVisitorId=', @ContractorVisitorId, N'; User=', SYSTEM_USER);
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
                N'ContractorVisitor', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ContractorVisitor].sp_ContractorVisitor_List
-- Description:   Returns a paginated, soft-delete-filtered list of ContractorVisitor records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ContractorVisitor].sp_ContractorVisitor_List
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
                N'ContractorVisitor', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractorVisitorId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [ContractorVisitor].[ContractorVisitor] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ContractorVisitorId
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
                N'ContractorVisitor', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ContractorVisitor].sp_ContractorVisitor_Delete
-- Description:   Soft-deletes a ContractorVisitor record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ContractorVisitor].sp_ContractorVisitor_Delete
    @ContractorVisitorId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorVisitorId IS NULL
            THROW 50001, N'ContractorVisitorId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [ContractorVisitor].[ContractorVisitor] WHERE ContractorVisitorId = @ContractorVisitorId AND ISNULL(IsDeleted,0) = 0)
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
                N'ContractorVisitor', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractorVisitorId=', @ContractorVisitorId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [ContractorVisitor].[ContractorVisitor]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractorVisitorId = @ContractorVisitorId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorVisitorId=', @ContractorVisitorId, N'; User=', SYSTEM_USER);
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
                N'ContractorVisitor', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [DigitalTwin].sp_DigitalTwin_Create
-- Description:   Inserts a new DigitalTwin record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [DigitalTwin].sp_DigitalTwin_Create
    @DigitalTwinId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DigitalTwinId IS NULL
            THROW 50001, N'DigitalTwinId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'DigitalTwin', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'DigitalTwinId=', @DigitalTwinId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [DigitalTwin].[DigitalTwin] (DigitalTwinId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@DigitalTwinId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DigitalTwinId=', @DigitalTwinId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'DigitalTwin', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [DigitalTwin].sp_DigitalTwin_Update
-- Description:   Updates an existing DigitalTwin record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [DigitalTwin].sp_DigitalTwin_Update
    @DigitalTwinId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DigitalTwinId IS NULL
            THROW 50001, N'DigitalTwinId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [DigitalTwin].[DigitalTwin] WHERE DigitalTwinId = @DigitalTwinId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [DigitalTwin].[DigitalTwin] WHERE DigitalTwinId = @DigitalTwinId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'DigitalTwin', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'DigitalTwinId=', @DigitalTwinId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [DigitalTwin].[DigitalTwin]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE DigitalTwinId = @DigitalTwinId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DigitalTwinId=', @DigitalTwinId, N'; User=', SYSTEM_USER);
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
                N'DigitalTwin', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [DigitalTwin].sp_DigitalTwin_GetById
-- Description:   Retrieves a single DigitalTwin record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [DigitalTwin].sp_DigitalTwin_GetById
    @DigitalTwinId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DigitalTwinId IS NULL
            THROW 50001, N'DigitalTwinId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'DigitalTwin', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'DigitalTwinId=', @DigitalTwinId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.DigitalTwinId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [DigitalTwin].[DigitalTwin] AS e
        WHERE e.DigitalTwinId = @DigitalTwinId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DigitalTwinId=', @DigitalTwinId, N'; User=', SYSTEM_USER);
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
                N'DigitalTwin', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [DigitalTwin].sp_DigitalTwin_List
-- Description:   Returns a paginated, soft-delete-filtered list of DigitalTwin records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [DigitalTwin].sp_DigitalTwin_List
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
                N'DigitalTwin', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.DigitalTwinId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [DigitalTwin].[DigitalTwin] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.DigitalTwinId
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
                N'DigitalTwin', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [DigitalTwin].sp_DigitalTwin_Delete
-- Description:   Soft-deletes a DigitalTwin record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [DigitalTwin].sp_DigitalTwin_Delete
    @DigitalTwinId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @DigitalTwinId IS NULL
            THROW 50001, N'DigitalTwinId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [DigitalTwin].[DigitalTwin] WHERE DigitalTwinId = @DigitalTwinId AND ISNULL(IsDeleted,0) = 0)
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
                N'DigitalTwin', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'DigitalTwinId=', @DigitalTwinId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [DigitalTwin].[DigitalTwin]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE DigitalTwinId = @DigitalTwinId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'DigitalTwinId=', @DigitalTwinId, N'; User=', SYSTEM_USER);
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
                N'DigitalTwin', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnterpriseFabric].sp_EnterpriseFabric_Create
-- Description:   Inserts a new EnterpriseFabric record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnterpriseFabric].sp_EnterpriseFabric_Create
    @EnterpriseFabricId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EnterpriseFabricId IS NULL
            THROW 50001, N'EnterpriseFabricId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EnterpriseFabric', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [EnterpriseFabric].[EnterpriseFabric] (EnterpriseFabricId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@EnterpriseFabricId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'EnterpriseFabric', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnterpriseFabric].sp_EnterpriseFabric_Update
-- Description:   Updates an existing EnterpriseFabric record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnterpriseFabric].sp_EnterpriseFabric_Update
    @EnterpriseFabricId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EnterpriseFabricId IS NULL
            THROW 50001, N'EnterpriseFabricId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [EnterpriseFabric].[EnterpriseFabric] WHERE EnterpriseFabricId = @EnterpriseFabricId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [EnterpriseFabric].[EnterpriseFabric] WHERE EnterpriseFabricId = @EnterpriseFabricId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'EnterpriseFabric', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [EnterpriseFabric].[EnterpriseFabric]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE EnterpriseFabricId = @EnterpriseFabricId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId, N'; User=', SYSTEM_USER);
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
                N'EnterpriseFabric', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnterpriseFabric].sp_EnterpriseFabric_GetById
-- Description:   Retrieves a single EnterpriseFabric record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnterpriseFabric].sp_EnterpriseFabric_GetById
    @EnterpriseFabricId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EnterpriseFabricId IS NULL
            THROW 50001, N'EnterpriseFabricId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EnterpriseFabric', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.EnterpriseFabricId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [EnterpriseFabric].[EnterpriseFabric] AS e
        WHERE e.EnterpriseFabricId = @EnterpriseFabricId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId, N'; User=', SYSTEM_USER);
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
                N'EnterpriseFabric', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnterpriseFabric].sp_EnterpriseFabric_List
-- Description:   Returns a paginated, soft-delete-filtered list of EnterpriseFabric records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnterpriseFabric].sp_EnterpriseFabric_List
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
                N'EnterpriseFabric', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.EnterpriseFabricId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [EnterpriseFabric].[EnterpriseFabric] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.EnterpriseFabricId
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
                N'EnterpriseFabric', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnterpriseFabric].sp_EnterpriseFabric_Delete
-- Description:   Soft-deletes a EnterpriseFabric record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnterpriseFabric].sp_EnterpriseFabric_Delete
    @EnterpriseFabricId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EnterpriseFabricId IS NULL
            THROW 50001, N'EnterpriseFabricId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [EnterpriseFabric].[EnterpriseFabric] WHERE EnterpriseFabricId = @EnterpriseFabricId AND ISNULL(IsDeleted,0) = 0)
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
                N'EnterpriseFabric', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [EnterpriseFabric].[EnterpriseFabric]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE EnterpriseFabricId = @EnterpriseFabricId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnterpriseFabricId=', @EnterpriseFabricId, N'; User=', SYSTEM_USER);
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
                N'EnterpriseFabric', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnvironmentalHealth].sp_EnvironmentalHealth_Create
-- Description:   Inserts a new EnvironmentalHealth record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnvironmentalHealth].sp_EnvironmentalHealth_Create
    @EnvironmentalHealthId uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @MetricType nvarchar(100),
    @MeasurementValue decimal(18,4),
    @Unit nvarchar(50),
    @Threshold decimal(18,4),
    @Status nvarchar(50),
    @RecordedAtUtc datetime2(3),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EnvironmentalHealthId IS NULL
            THROW 50001, N'EnvironmentalHealthId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EnvironmentalHealth', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId, N'; ', N'TenantId=', @TenantId, N'; ', N'SiteId=', @SiteId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [EnvironmentalHealth].[EnvironmentalHealth] (EnvironmentalHealthId, TenantId, SiteId, MetricType, MeasurementValue, Unit, Threshold, Status, RecordedAtUtc, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@EnvironmentalHealthId, @TenantId, @SiteId, @MetricType, @MeasurementValue, @Unit, @Threshold, @Status, @RecordedAtUtc, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId, N'; ', N'TenantId=', @TenantId, N'; ', N'SiteId=', @SiteId, N'; User=', SYSTEM_USER);
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
                N'EnvironmentalHealth', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnvironmentalHealth].sp_EnvironmentalHealth_Update
-- Description:   Updates an existing EnvironmentalHealth record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnvironmentalHealth].sp_EnvironmentalHealth_Update
    @EnvironmentalHealthId uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @MetricType nvarchar(100),
    @MeasurementValue decimal(18,4),
    @Unit nvarchar(50),
    @Threshold decimal(18,4),
    @Status nvarchar(50),
    @RecordedAtUtc datetime2(3),
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EnvironmentalHealthId IS NULL
            THROW 50001, N'EnvironmentalHealthId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [EnvironmentalHealth].[EnvironmentalHealth] WHERE EnvironmentalHealthId = @EnvironmentalHealthId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [EnvironmentalHealth].[EnvironmentalHealth] WHERE EnvironmentalHealthId = @EnvironmentalHealthId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'EnvironmentalHealth', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [EnvironmentalHealth].[EnvironmentalHealth]
        SET
            TenantId = @TenantId,
            SiteId = @SiteId,
            MetricType = @MetricType,
            MeasurementValue = @MeasurementValue,
            Unit = @Unit,
            Threshold = @Threshold,
            Status = @Status,
            RecordedAtUtc = @RecordedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE EnvironmentalHealthId = @EnvironmentalHealthId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId, N'; User=', SYSTEM_USER);
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
                N'EnvironmentalHealth', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnvironmentalHealth].sp_EnvironmentalHealth_GetById
-- Description:   Retrieves a single EnvironmentalHealth record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnvironmentalHealth].sp_EnvironmentalHealth_GetById
    @EnvironmentalHealthId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EnvironmentalHealthId IS NULL
            THROW 50001, N'EnvironmentalHealthId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EnvironmentalHealth', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.EnvironmentalHealthId, e.TenantId, e.SiteId, e.MetricType, e.MeasurementValue, e.Unit, e.Threshold, e.Status, e.RecordedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [EnvironmentalHealth].[EnvironmentalHealth] AS e
        WHERE e.EnvironmentalHealthId = @EnvironmentalHealthId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId, N'; User=', SYSTEM_USER);
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
                N'EnvironmentalHealth', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnvironmentalHealth].sp_EnvironmentalHealth_List
-- Description:   Returns a paginated, soft-delete-filtered list of EnvironmentalHealth records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnvironmentalHealth].sp_EnvironmentalHealth_List
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
                N'EnvironmentalHealth', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.EnvironmentalHealthId, e.TenantId, e.SiteId, e.MetricType, e.MeasurementValue, e.Unit, e.Threshold, e.Status, e.RecordedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [EnvironmentalHealth].[EnvironmentalHealth] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.EnvironmentalHealthId
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
                N'EnvironmentalHealth', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [EnvironmentalHealth].sp_EnvironmentalHealth_Delete
-- Description:   Soft-deletes a EnvironmentalHealth record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [EnvironmentalHealth].sp_EnvironmentalHealth_Delete
    @EnvironmentalHealthId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EnvironmentalHealthId IS NULL
            THROW 50001, N'EnvironmentalHealthId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [EnvironmentalHealth].[EnvironmentalHealth] WHERE EnvironmentalHealthId = @EnvironmentalHealthId AND ISNULL(IsDeleted,0) = 0)
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
                N'EnvironmentalHealth', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [EnvironmentalHealth].[EnvironmentalHealth]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE EnvironmentalHealthId = @EnvironmentalHealthId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EnvironmentalHealthId=', @EnvironmentalHealthId, N'; User=', SYSTEM_USER);
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
                N'EnvironmentalHealth', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Governance].sp_Governance_Create
-- Description:   Inserts a new Governance record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Governance].sp_Governance_Create
    @GovernanceId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @GovernanceId IS NULL
            THROW 50001, N'GovernanceId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Governance', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'GovernanceId=', @GovernanceId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Governance].[Governance] (GovernanceId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@GovernanceId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'GovernanceId=', @GovernanceId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'Governance', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Governance].sp_Governance_Update
-- Description:   Updates an existing Governance record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Governance].sp_Governance_Update
    @GovernanceId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @GovernanceId IS NULL
            THROW 50001, N'GovernanceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Governance].[Governance] WHERE GovernanceId = @GovernanceId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Governance].[Governance] WHERE GovernanceId = @GovernanceId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Governance', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'GovernanceId=', @GovernanceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Governance].[Governance]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE GovernanceId = @GovernanceId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'GovernanceId=', @GovernanceId, N'; User=', SYSTEM_USER);
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
                N'Governance', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Governance].sp_Governance_GetById
-- Description:   Retrieves a single Governance record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Governance].sp_Governance_GetById
    @GovernanceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @GovernanceId IS NULL
            THROW 50001, N'GovernanceId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Governance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'GovernanceId=', @GovernanceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.GovernanceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [Governance].[Governance] AS e
        WHERE e.GovernanceId = @GovernanceId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'GovernanceId=', @GovernanceId, N'; User=', SYSTEM_USER);
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
                N'Governance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Governance].sp_Governance_List
-- Description:   Returns a paginated, soft-delete-filtered list of Governance records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Governance].sp_Governance_List
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
                N'Governance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.GovernanceId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [Governance].[Governance] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.GovernanceId
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
                N'Governance', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Governance].sp_Governance_Delete
-- Description:   Soft-deletes a Governance record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Governance].sp_Governance_Delete
    @GovernanceId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @GovernanceId IS NULL
            THROW 50001, N'GovernanceId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Governance].[Governance] WHERE GovernanceId = @GovernanceId AND ISNULL(IsDeleted,0) = 0)
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
                N'Governance', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'GovernanceId=', @GovernanceId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Governance].[Governance]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE GovernanceId = @GovernanceId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'GovernanceId=', @GovernanceId, N'; User=', SYSTEM_USER);
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
                N'Governance', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [HumanFactors].sp_HumanFactors_Create
-- Description:   Inserts a new HumanFactors record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [HumanFactors].sp_HumanFactors_Create
    @HumanFactorsId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HumanFactorsId IS NULL
            THROW 50001, N'HumanFactorsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'HumanFactors', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HumanFactorsId=', @HumanFactorsId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [HumanFactors].[HumanFactors] (HumanFactorsId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@HumanFactorsId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HumanFactorsId=', @HumanFactorsId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'HumanFactors', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [HumanFactors].sp_HumanFactors_Update
-- Description:   Updates an existing HumanFactors record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [HumanFactors].sp_HumanFactors_Update
    @HumanFactorsId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HumanFactorsId IS NULL
            THROW 50001, N'HumanFactorsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [HumanFactors].[HumanFactors] WHERE HumanFactorsId = @HumanFactorsId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [HumanFactors].[HumanFactors] WHERE HumanFactorsId = @HumanFactorsId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'HumanFactors', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HumanFactorsId=', @HumanFactorsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [HumanFactors].[HumanFactors]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE HumanFactorsId = @HumanFactorsId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HumanFactorsId=', @HumanFactorsId, N'; User=', SYSTEM_USER);
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
                N'HumanFactors', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [HumanFactors].sp_HumanFactors_GetById
-- Description:   Retrieves a single HumanFactors record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [HumanFactors].sp_HumanFactors_GetById
    @HumanFactorsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HumanFactorsId IS NULL
            THROW 50001, N'HumanFactorsId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'HumanFactors', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'HumanFactorsId=', @HumanFactorsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HumanFactorsId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [HumanFactors].[HumanFactors] AS e
        WHERE e.HumanFactorsId = @HumanFactorsId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HumanFactorsId=', @HumanFactorsId, N'; User=', SYSTEM_USER);
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
                N'HumanFactors', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [HumanFactors].sp_HumanFactors_List
-- Description:   Returns a paginated, soft-delete-filtered list of HumanFactors records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [HumanFactors].sp_HumanFactors_List
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
                N'HumanFactors', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HumanFactorsId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [HumanFactors].[HumanFactors] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HumanFactorsId
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
                N'HumanFactors', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [HumanFactors].sp_HumanFactors_Delete
-- Description:   Soft-deletes a HumanFactors record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [HumanFactors].sp_HumanFactors_Delete
    @HumanFactorsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HumanFactorsId IS NULL
            THROW 50001, N'HumanFactorsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [HumanFactors].[HumanFactors] WHERE HumanFactorsId = @HumanFactorsId AND ISNULL(IsDeleted,0) = 0)
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
                N'HumanFactors', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HumanFactorsId=', @HumanFactorsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [HumanFactors].[HumanFactors]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE HumanFactorsId = @HumanFactorsId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HumanFactorsId=', @HumanFactorsId, N'; User=', SYSTEM_USER);
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
                N'HumanFactors', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Alert_Create
-- Description:   Inserts a new Alert record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Alert_Create
    @AlertId uniqueidentifier,
    @AlertType nvarchar(100),
    @Severity nvarchar(50),
    @Status nvarchar(50),
    @Title nvarchar(500),
    @Description nvarchar(MAX),
    @SourceModule nvarchar(200),
    @AcknowledgedAtUtc datetime2(3),
    @InProgressAtUtc datetime2(3),
    @EscalatedAtUtc datetime2(3),
    @ResolvedAtUtc datetime2(3),
    @ClosedAtUtc datetime2(3),
    @ArchivedAtUtc datetime2(3),
    @AcknowledgedBy uniqueidentifier,
    @InProgressBy uniqueidentifier,
    @EscalatedBy uniqueidentifier,
    @ResolvedBy uniqueidentifier,
    @ClosedBy uniqueidentifier,
    @ArchivedBy uniqueidentifier,
    @EscalationReason nvarchar(500),
    @ResolutionSummary nvarchar(MAX),
    @Notes nvarchar(MAX),
    @SlaExpiration datetime2(3),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AlertId IS NULL
            THROW 50001, N'AlertId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Alert', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AlertId=', @AlertId, N'; ', N'AlertType=', @AlertType, N'; ', N'Severity=', @Severity), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[Alert] (AlertId, AlertType, Severity, Status, Title, Description, SourceModule, AcknowledgedAtUtc, InProgressAtUtc, EscalatedAtUtc, ResolvedAtUtc, ClosedAtUtc, ArchivedAtUtc, AcknowledgedBy, InProgressBy, EscalatedBy, ResolvedBy, ClosedBy, ArchivedBy, EscalationReason, ResolutionSummary, Notes, SlaExpiration, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@AlertId, @AlertType, @Severity, @Status, @Title, @Description, @SourceModule, @AcknowledgedAtUtc, @InProgressAtUtc, @EscalatedAtUtc, @ResolvedAtUtc, @ClosedAtUtc, @ArchivedAtUtc, @AcknowledgedBy, @InProgressBy, @EscalatedBy, @ResolvedBy, @ClosedBy, @ArchivedBy, @EscalationReason, @ResolutionSummary, @Notes, @SlaExpiration, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; ', N'AlertType=', @AlertType, N'; ', N'Severity=', @Severity, N'; User=', SYSTEM_USER);
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
                N'Alert', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Alert_Update
-- Description:   Updates an existing Alert record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Alert_Update
    @AlertId uniqueidentifier,
    @AlertType nvarchar(100),
    @Severity nvarchar(50),
    @Status nvarchar(50),
    @Title nvarchar(500),
    @Description nvarchar(MAX),
    @SourceModule nvarchar(200),
    @AcknowledgedAtUtc datetime2(3),
    @InProgressAtUtc datetime2(3),
    @EscalatedAtUtc datetime2(3),
    @ResolvedAtUtc datetime2(3),
    @ClosedAtUtc datetime2(3),
    @ArchivedAtUtc datetime2(3),
    @AcknowledgedBy uniqueidentifier,
    @InProgressBy uniqueidentifier,
    @EscalatedBy uniqueidentifier,
    @ResolvedBy uniqueidentifier,
    @ClosedBy uniqueidentifier,
    @ArchivedBy uniqueidentifier,
    @EscalationReason nvarchar(500),
    @ResolutionSummary nvarchar(MAX),
    @Notes nvarchar(MAX),
    @SlaExpiration datetime2(3),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AlertId IS NULL
            THROW 50001, N'AlertId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Alert] WHERE AlertId = @AlertId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Operational].[Alert] WHERE AlertId = @AlertId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Alert', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Operational].[Alert]
        SET
            AlertType = @AlertType,
            Severity = @Severity,
            Status = @Status,
            Title = @Title,
            Description = @Description,
            SourceModule = @SourceModule,
            AcknowledgedAtUtc = @AcknowledgedAtUtc,
            InProgressAtUtc = @InProgressAtUtc,
            EscalatedAtUtc = @EscalatedAtUtc,
            ResolvedAtUtc = @ResolvedAtUtc,
            ClosedAtUtc = @ClosedAtUtc,
            ArchivedAtUtc = @ArchivedAtUtc,
            AcknowledgedBy = @AcknowledgedBy,
            InProgressBy = @InProgressBy,
            EscalatedBy = @EscalatedBy,
            ResolvedBy = @ResolvedBy,
            ClosedBy = @ClosedBy,
            ArchivedBy = @ArchivedBy,
            EscalationReason = @EscalationReason,
            ResolutionSummary = @ResolutionSummary,
            Notes = @Notes,
            SlaExpiration = @SlaExpiration,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE AlertId = @AlertId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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
                N'Alert', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Alert_GetById
-- Description:   Retrieves a single Alert record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Alert_GetById
    @AlertId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AlertId IS NULL
            THROW 50001, N'AlertId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Alert', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AlertId, e.AlertType, e.Severity, e.Status, e.Title, e.Description, e.SourceModule, e.AcknowledgedAtUtc, e.InProgressAtUtc, e.EscalatedAtUtc, e.ResolvedAtUtc, e.ClosedAtUtc, e.ArchivedAtUtc, e.AcknowledgedBy, e.InProgressBy, e.EscalatedBy, e.ResolvedBy, e.ClosedBy, e.ArchivedBy, e.EscalationReason, e.ResolutionSummary, e.Notes, e.SlaExpiration, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [Operational].[Alert] AS e
        WHERE e.AlertId = @AlertId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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
                N'Alert', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Alert_List
-- Description:   Returns a paginated, soft-delete-filtered list of Alert records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Alert_List
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
                N'Alert', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AlertId, e.AlertType, e.Severity, e.Status, e.Title, e.Description, e.SourceModule, e.AcknowledgedAtUtc, e.InProgressAtUtc, e.EscalatedAtUtc, e.ResolvedAtUtc, e.ClosedAtUtc, e.ArchivedAtUtc, e.AcknowledgedBy, e.InProgressBy, e.EscalatedBy, e.ResolvedBy, e.ClosedBy, e.ArchivedBy, e.EscalationReason, e.ResolutionSummary, e.Notes, e.SlaExpiration, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [Operational].[Alert] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.AlertId
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
                N'Alert', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Alert_Delete
-- Description:   Soft-deletes a Alert record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Alert_Delete
    @AlertId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AlertId IS NULL
            THROW 50001, N'AlertId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Alert] WHERE AlertId = @AlertId AND ISNULL(IsDeleted,0) = 0)
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
                N'Alert', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[Alert]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE AlertId = @AlertId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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
                N'Alert', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_AlertHistoryEntry_Create
-- Description:   Inserts a new append-only AlertHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_AlertHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @AlertId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AlertHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[AlertHistoryEntry] (HistoryId, AlertId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId)
        VALUES (@HistoryId, @AlertId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'AlertHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_AlertHistoryEntry_GetByParentId
-- Description:   Returns all AlertHistoryEntry records for a given AlertId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_AlertHistoryEntry_GetByParentId
    @AlertId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AlertId IS NULL
            THROW 50001, N'AlertId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AlertHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'AlertId=', @AlertId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.AlertId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId
        FROM [Operational].[AlertHistoryEntry] AS e
        WHERE e.AlertId = @AlertId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertId=', @AlertId, N'; User=', SYSTEM_USER);
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
                N'AlertHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_AlertSummary_Create
-- Description:   Inserts a new AlertSummary record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_AlertSummary_Create
    @AlertSummaryId uniqueidentifier,
    @SummaryDateKey int,
    @TenantId uniqueidentifier,
    @AlertType nvarchar(100),
    @Severity nvarchar(50),
    @Status nvarchar(50),
    @AlertCount int,
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AlertSummaryId IS NULL
            THROW 50001, N'AlertSummaryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AlertSummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AlertSummaryId=', @AlertSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[AlertSummary] (AlertSummaryId, SummaryDateKey, TenantId, AlertType, Severity, Status, AlertCount, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@AlertSummaryId, @SummaryDateKey, @TenantId, @AlertType, @Severity, @Status, @AlertCount, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertSummaryId=', @AlertSummaryId, N'; User=', SYSTEM_USER);
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
                N'AlertSummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_AlertSummary_GetById
-- Description:   Retrieves a single AlertSummary record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_AlertSummary_GetById
    @AlertSummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AlertSummaryId IS NULL
            THROW 50001, N'AlertSummaryId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'AlertSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'AlertSummaryId=', @AlertSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AlertSummaryId, e.SummaryDateKey, e.TenantId, e.AlertType, e.Severity, e.Status, e.AlertCount, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Operational].[AlertSummary] AS e
        WHERE e.AlertSummaryId = @AlertSummaryId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertSummaryId=', @AlertSummaryId, N'; User=', SYSTEM_USER);
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
                N'AlertSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_AlertSummary_List
-- Description:   Returns a paginated, soft-delete-filtered list of AlertSummary records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_AlertSummary_List
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
                N'AlertSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.AlertSummaryId, e.SummaryDateKey, e.TenantId, e.AlertType, e.Severity, e.Status, e.AlertCount, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Operational].[AlertSummary] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.AlertSummaryId
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
                N'AlertSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_AlertSummary_Delete
-- Description:   Soft-deletes a AlertSummary record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_AlertSummary_Delete
    @AlertSummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @AlertSummaryId IS NULL
            THROW 50001, N'AlertSummaryId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[AlertSummary] WHERE AlertSummaryId = @AlertSummaryId AND ISNULL(IsDeleted,0) = 0)
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
                N'AlertSummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'AlertSummaryId=', @AlertSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[AlertSummary]
        SET IsDeleted = 1
        WHERE AlertSummaryId = @AlertSummaryId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'AlertSummaryId=', @AlertSummaryId, N'; User=', SYSTEM_USER);
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
                N'AlertSummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Checklists_Create
-- Description:   Inserts a new Checklists record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Checklists_Create
    @ChecklistsId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ChecklistTitle nvarchar(300),
    @ChecklistType nvarchar(100),
    @Location nvarchar(300),
    @TotalItems int,
    @CompletedItems int,
    @ExceptionCount int,
    @CompletedByUserId uniqueidentifier,
    @CompletedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ChecklistsId IS NULL
            THROW 50001, N'ChecklistsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Checklists', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ChecklistsId=', @ChecklistsId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[Checklists] (ChecklistsId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, ChecklistTitle, ChecklistType, Location, TotalItems, CompletedItems, ExceptionCount, CompletedByUserId, CompletedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@ChecklistsId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @ChecklistTitle, @ChecklistType, @Location, @TotalItems, @CompletedItems, @ExceptionCount, @CompletedByUserId, @CompletedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ChecklistsId=', @ChecklistsId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Checklists', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Checklists_Update
-- Description:   Updates an existing Checklists record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Checklists_Update
    @ChecklistsId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ChecklistTitle nvarchar(300),
    @ChecklistType nvarchar(100),
    @Location nvarchar(300),
    @TotalItems int,
    @CompletedItems int,
    @ExceptionCount int,
    @CompletedByUserId uniqueidentifier,
    @CompletedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ChecklistsId IS NULL
            THROW 50001, N'ChecklistsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Checklists] WHERE ChecklistsId = @ChecklistsId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Operational].[Checklists] WHERE ChecklistsId = @ChecklistsId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Checklists', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ChecklistsId=', @ChecklistsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Operational].[Checklists]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            ChecklistTitle = @ChecklistTitle,
            ChecklistType = @ChecklistType,
            Location = @Location,
            TotalItems = @TotalItems,
            CompletedItems = @CompletedItems,
            ExceptionCount = @ExceptionCount,
            CompletedByUserId = @CompletedByUserId,
            CompletedAtUtc = @CompletedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ChecklistsId = @ChecklistsId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ChecklistsId=', @ChecklistsId, N'; User=', SYSTEM_USER);
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
                N'Checklists', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Checklists_GetById
-- Description:   Retrieves a single Checklists record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Checklists_GetById
    @ChecklistsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ChecklistsId IS NULL
            THROW 50001, N'ChecklistsId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Checklists', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ChecklistsId=', @ChecklistsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ChecklistsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ChecklistTitle, e.ChecklistType, e.Location, e.TotalItems, e.CompletedItems, e.ExceptionCount, e.CompletedByUserId, e.CompletedAtUtc
        FROM [Operational].[Checklists] AS e
        WHERE e.ChecklistsId = @ChecklistsId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ChecklistsId=', @ChecklistsId, N'; User=', SYSTEM_USER);
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
                N'Checklists', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Checklists_List
-- Description:   Returns a paginated, soft-delete-filtered list of Checklists records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Checklists_List
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
                N'Checklists', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ChecklistsId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ChecklistTitle, e.ChecklistType, e.Location, e.TotalItems, e.CompletedItems, e.ExceptionCount, e.CompletedByUserId, e.CompletedAtUtc
        FROM [Operational].[Checklists] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ChecklistsId
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
                N'Checklists', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Checklists_Delete
-- Description:   Soft-deletes a Checklists record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Checklists_Delete
    @ChecklistsId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ChecklistsId IS NULL
            THROW 50001, N'ChecklistsId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Checklists] WHERE ChecklistsId = @ChecklistsId AND ISNULL(IsDeleted,0) = 0)
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
                N'Checklists', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ChecklistsId=', @ChecklistsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[Checklists]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ChecklistsId = @ChecklistsId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ChecklistsId=', @ChecklistsId, N'; User=', SYSTEM_USER);
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
                N'Checklists', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_ChecklistsHistoryEntry_Create
-- Description:   Inserts a new append-only ChecklistsHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_ChecklistsHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @ChecklistsId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @CompletedItemsAtTransition int,
    @ExceptionCountAtTransition int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ChecklistsHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[ChecklistsHistoryEntry] (HistoryId, ChecklistsId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, CompletedItemsAtTransition, ExceptionCountAtTransition)
        VALUES (@HistoryId, @ChecklistsId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @CompletedItemsAtTransition, @ExceptionCountAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'ChecklistsHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_ChecklistsHistoryEntry_GetByParentId
-- Description:   Returns all ChecklistsHistoryEntry records for a given ChecklistsId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_ChecklistsHistoryEntry_GetByParentId
    @ChecklistsId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ChecklistsId IS NULL
            THROW 50001, N'ChecklistsId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ChecklistsHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ChecklistsId=', @ChecklistsId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.ChecklistsId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.CompletedItemsAtTransition, e.ExceptionCountAtTransition
        FROM [Operational].[ChecklistsHistoryEntry] AS e
        WHERE e.ChecklistsId = @ChecklistsId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ChecklistsId=', @ChecklistsId, N'; User=', SYSTEM_USER);
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
                N'ChecklistsHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Contractor_Create
-- Description:   Inserts a new Contractor record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Contractor_Create
    @ContractorId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ContractorCompany nvarchar(300),
    @ContractorName nvarchar(300),
    @QualificationStatus nvarchar(50),
    @ReadinessScore decimal(7,4),
    @PrimaryContactId uniqueidentifier,
    @InsuranceExpiresAtUtc datetime2(7),
    @OnboardedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorId IS NULL
            THROW 50001, N'ContractorId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Contractor', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractorId=', @ContractorId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[Contractor] (ContractorId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, ContractorCompany, ContractorName, QualificationStatus, ReadinessScore, PrimaryContactId, InsuranceExpiresAtUtc, OnboardedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@ContractorId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @ContractorCompany, @ContractorName, @QualificationStatus, @ReadinessScore, @PrimaryContactId, @InsuranceExpiresAtUtc, @OnboardedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorId=', @ContractorId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Contractor', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Contractor_Update
-- Description:   Updates an existing Contractor record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Contractor_Update
    @ContractorId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ContractorCompany nvarchar(300),
    @ContractorName nvarchar(300),
    @QualificationStatus nvarchar(50),
    @ReadinessScore decimal(7,4),
    @PrimaryContactId uniqueidentifier,
    @InsuranceExpiresAtUtc datetime2(7),
    @OnboardedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorId IS NULL
            THROW 50001, N'ContractorId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Contractor] WHERE ContractorId = @ContractorId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Operational].[Contractor] WHERE ContractorId = @ContractorId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Contractor', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractorId=', @ContractorId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Operational].[Contractor]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            ContractorCompany = @ContractorCompany,
            ContractorName = @ContractorName,
            QualificationStatus = @QualificationStatus,
            ReadinessScore = @ReadinessScore,
            PrimaryContactId = @PrimaryContactId,
            InsuranceExpiresAtUtc = @InsuranceExpiresAtUtc,
            OnboardedAtUtc = @OnboardedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractorId = @ContractorId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorId=', @ContractorId, N'; User=', SYSTEM_USER);
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
                N'Contractor', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Contractor_GetById
-- Description:   Retrieves a single Contractor record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Contractor_GetById
    @ContractorId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorId IS NULL
            THROW 50001, N'ContractorId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Contractor', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ContractorId=', @ContractorId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractorId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ContractorCompany, e.ContractorName, e.QualificationStatus, e.ReadinessScore, e.PrimaryContactId, e.InsuranceExpiresAtUtc, e.OnboardedAtUtc
        FROM [Operational].[Contractor] AS e
        WHERE e.ContractorId = @ContractorId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorId=', @ContractorId, N'; User=', SYSTEM_USER);
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
                N'Contractor', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Contractor_List
-- Description:   Returns a paginated, soft-delete-filtered list of Contractor records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Contractor_List
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
                N'Contractor', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractorId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ContractorCompany, e.ContractorName, e.QualificationStatus, e.ReadinessScore, e.PrimaryContactId, e.InsuranceExpiresAtUtc, e.OnboardedAtUtc
        FROM [Operational].[Contractor] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ContractorId
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
                N'Contractor', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Contractor_Delete
-- Description:   Soft-deletes a Contractor record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Contractor_Delete
    @ContractorId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorId IS NULL
            THROW 50001, N'ContractorId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Contractor] WHERE ContractorId = @ContractorId AND ISNULL(IsDeleted,0) = 0)
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
                N'Contractor', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractorId=', @ContractorId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[Contractor]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractorId = @ContractorId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorId=', @ContractorId, N'; User=', SYSTEM_USER);
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
                N'Contractor', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_ContractorHistoryEntry_Create
-- Description:   Inserts a new append-only ContractorHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_ContractorHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @ContractorId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @ReadinessScoreAtTransition decimal(7,4),
    @QualificationStatusAtTransition nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractorHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[ContractorHistoryEntry] (HistoryId, ContractorId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, ReadinessScoreAtTransition, QualificationStatusAtTransition)
        VALUES (@HistoryId, @ContractorId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @ReadinessScoreAtTransition, @QualificationStatusAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'ContractorHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_ContractorHistoryEntry_GetByParentId
-- Description:   Returns all ContractorHistoryEntry records for a given ContractorId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_ContractorHistoryEntry_GetByParentId
    @ContractorId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractorId IS NULL
            THROW 50001, N'ContractorId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractorHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ContractorId=', @ContractorId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.ContractorId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.ReadinessScoreAtTransition, e.QualificationStatusAtTransition
        FROM [Operational].[ContractorHistoryEntry] AS e
        WHERE e.ContractorId = @ContractorId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractorId=', @ContractorId, N'; User=', SYSTEM_USER);
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
                N'ContractorHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_PPEManagement_Create
-- Description:   Inserts a new PPEManagement record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_PPEManagement_Create
    @PPEManagementId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @WorkerId uniqueidentifier,
    @PpeType nvarchar(100),
    @PpeItem nvarchar(200),
    @ComplianceStatus nvarchar(50),
    @IssuedAtUtc datetime2(7),
    @ReturnedAtUtc datetime2(7),
    @InspectionDueAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PPEManagementId IS NULL
            THROW 50001, N'PPEManagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PPEManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PPEManagementId=', @PPEManagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[PPEManagement] (PPEManagementId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, WorkerId, PpeType, PpeItem, ComplianceStatus, IssuedAtUtc, ReturnedAtUtc, InspectionDueAtUtc, IsDeleted, CreatedUtc)
        VALUES (@PPEManagementId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @WorkerId, @PpeType, @PpeItem, @ComplianceStatus, @IssuedAtUtc, @ReturnedAtUtc, @InspectionDueAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PPEManagementId=', @PPEManagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'PPEManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_PPEManagement_Update
-- Description:   Updates an existing PPEManagement record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_PPEManagement_Update
    @PPEManagementId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @WorkerId uniqueidentifier,
    @PpeType nvarchar(100),
    @PpeItem nvarchar(200),
    @ComplianceStatus nvarchar(50),
    @IssuedAtUtc datetime2(7),
    @ReturnedAtUtc datetime2(7),
    @InspectionDueAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PPEManagementId IS NULL
            THROW 50001, N'PPEManagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[PPEManagement] WHERE PPEManagementId = @PPEManagementId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Operational].[PPEManagement] WHERE PPEManagementId = @PPEManagementId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'PPEManagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PPEManagementId=', @PPEManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Operational].[PPEManagement]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            WorkerId = @WorkerId,
            PpeType = @PpeType,
            PpeItem = @PpeItem,
            ComplianceStatus = @ComplianceStatus,
            IssuedAtUtc = @IssuedAtUtc,
            ReturnedAtUtc = @ReturnedAtUtc,
            InspectionDueAtUtc = @InspectionDueAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE PPEManagementId = @PPEManagementId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PPEManagementId=', @PPEManagementId, N'; User=', SYSTEM_USER);
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
                N'PPEManagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_PPEManagement_GetById
-- Description:   Retrieves a single PPEManagement record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_PPEManagement_GetById
    @PPEManagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PPEManagementId IS NULL
            THROW 50001, N'PPEManagementId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PPEManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PPEManagementId=', @PPEManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PPEManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.WorkerId, e.PpeType, e.PpeItem, e.ComplianceStatus, e.IssuedAtUtc, e.ReturnedAtUtc, e.InspectionDueAtUtc
        FROM [Operational].[PPEManagement] AS e
        WHERE e.PPEManagementId = @PPEManagementId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PPEManagementId=', @PPEManagementId, N'; User=', SYSTEM_USER);
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
                N'PPEManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_PPEManagement_List
-- Description:   Returns a paginated, soft-delete-filtered list of PPEManagement records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_PPEManagement_List
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
                N'PPEManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.PPEManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.WorkerId, e.PpeType, e.PpeItem, e.ComplianceStatus, e.IssuedAtUtc, e.ReturnedAtUtc, e.InspectionDueAtUtc
        FROM [Operational].[PPEManagement] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.PPEManagementId
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
                N'PPEManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_PPEManagement_Delete
-- Description:   Soft-deletes a PPEManagement record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_PPEManagement_Delete
    @PPEManagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PPEManagementId IS NULL
            THROW 50001, N'PPEManagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[PPEManagement] WHERE PPEManagementId = @PPEManagementId AND ISNULL(IsDeleted,0) = 0)
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
                N'PPEManagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'PPEManagementId=', @PPEManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[PPEManagement]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE PPEManagementId = @PPEManagementId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PPEManagementId=', @PPEManagementId, N'; User=', SYSTEM_USER);
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
                N'PPEManagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_PPEManagementHistoryEntry_Create
-- Description:   Inserts a new append-only PPEManagementHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_PPEManagementHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @PPEManagementId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @ComplianceStatusAtTransition nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PPEManagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[PPEManagementHistoryEntry] (HistoryId, PPEManagementId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, ComplianceStatusAtTransition)
        VALUES (@HistoryId, @PPEManagementId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @ComplianceStatusAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'PPEManagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_PPEManagementHistoryEntry_GetByParentId
-- Description:   Returns all PPEManagementHistoryEntry records for a given PPEManagementId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_PPEManagementHistoryEntry_GetByParentId
    @PPEManagementId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @PPEManagementId IS NULL
            THROW 50001, N'PPEManagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'PPEManagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PPEManagementId=', @PPEManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.PPEManagementId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.ComplianceStatusAtTransition
        FROM [Operational].[PPEManagementHistoryEntry] AS e
        WHERE e.PPEManagementId = @PPEManagementId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'PPEManagementId=', @PPEManagementId, N'; User=', SYSTEM_USER);
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
                N'PPEManagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagement_Create
-- Description:   Inserts a new RiskManagement record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagement_Create
    @RiskManagementId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @HazardDescription nvarchar(MAX),
    @RiskCategory nvarchar(100),
    @LikelihoodRating int,
    @SeverityRating int,
    @ControlMeasures nvarchar(MAX),
    @ResponsiblePartyId uniqueidentifier,
    @ReviewDueAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RiskManagementId IS NULL
            THROW 50001, N'RiskManagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RiskManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RiskManagementId=', @RiskManagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[RiskManagement] (RiskManagementId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, HazardDescription, RiskCategory, LikelihoodRating, SeverityRating, ControlMeasures, ResponsiblePartyId, ReviewDueAtUtc, IsDeleted, CreatedUtc)
        VALUES (@RiskManagementId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @HazardDescription, @RiskCategory, @LikelihoodRating, @SeverityRating, @ControlMeasures, @ResponsiblePartyId, @ReviewDueAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskManagementId=', @RiskManagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'RiskManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagement_Update
-- Description:   Updates an existing RiskManagement record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagement_Update
    @RiskManagementId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @HazardDescription nvarchar(MAX),
    @RiskCategory nvarchar(100),
    @LikelihoodRating int,
    @SeverityRating int,
    @ControlMeasures nvarchar(MAX),
    @ResponsiblePartyId uniqueidentifier,
    @ReviewDueAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RiskManagementId IS NULL
            THROW 50001, N'RiskManagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[RiskManagement] WHERE RiskManagementId = @RiskManagementId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Operational].[RiskManagement] WHERE RiskManagementId = @RiskManagementId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'RiskManagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RiskManagementId=', @RiskManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Operational].[RiskManagement]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            HazardDescription = @HazardDescription,
            RiskCategory = @RiskCategory,
            LikelihoodRating = @LikelihoodRating,
            SeverityRating = @SeverityRating,
            ControlMeasures = @ControlMeasures,
            ResponsiblePartyId = @ResponsiblePartyId,
            ReviewDueAtUtc = @ReviewDueAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE RiskManagementId = @RiskManagementId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskManagementId=', @RiskManagementId, N'; User=', SYSTEM_USER);
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
                N'RiskManagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagement_GetById
-- Description:   Retrieves a single RiskManagement record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagement_GetById
    @RiskManagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RiskManagementId IS NULL
            THROW 50001, N'RiskManagementId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RiskManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'RiskManagementId=', @RiskManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RiskManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.HazardDescription, e.RiskCategory, e.LikelihoodRating, e.SeverityRating, e.ControlMeasures, e.ResponsiblePartyId, e.ReviewDueAtUtc
        FROM [Operational].[RiskManagement] AS e
        WHERE e.RiskManagementId = @RiskManagementId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskManagementId=', @RiskManagementId, N'; User=', SYSTEM_USER);
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
                N'RiskManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagement_List
-- Description:   Returns a paginated, soft-delete-filtered list of RiskManagement records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagement_List
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
                N'RiskManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RiskManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.HazardDescription, e.RiskCategory, e.LikelihoodRating, e.SeverityRating, e.ControlMeasures, e.ResponsiblePartyId, e.ReviewDueAtUtc
        FROM [Operational].[RiskManagement] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.RiskManagementId
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
                N'RiskManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagement_Delete
-- Description:   Soft-deletes a RiskManagement record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagement_Delete
    @RiskManagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RiskManagementId IS NULL
            THROW 50001, N'RiskManagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[RiskManagement] WHERE RiskManagementId = @RiskManagementId AND ISNULL(IsDeleted,0) = 0)
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
                N'RiskManagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RiskManagementId=', @RiskManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[RiskManagement]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE RiskManagementId = @RiskManagementId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskManagementId=', @RiskManagementId, N'; User=', SYSTEM_USER);
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
                N'RiskManagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagementHistoryEntry_Create
-- Description:   Inserts a new append-only RiskManagementHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagementHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @RiskManagementId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @RiskScoreAtTransition int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RiskManagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[RiskManagementHistoryEntry] (HistoryId, RiskManagementId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, RiskScoreAtTransition)
        VALUES (@HistoryId, @RiskManagementId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @RiskScoreAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'RiskManagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagementHistoryEntry_GetByParentId
-- Description:   Returns all RiskManagementHistoryEntry records for a given RiskManagementId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagementHistoryEntry_GetByParentId
    @RiskManagementId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RiskManagementId IS NULL
            THROW 50001, N'RiskManagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RiskManagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'RiskManagementId=', @RiskManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.RiskManagementId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.RiskScoreAtTransition
        FROM [Operational].[RiskManagementHistoryEntry] AS e
        WHERE e.RiskManagementId = @RiskManagementId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskManagementId=', @RiskManagementId, N'; User=', SYSTEM_USER);
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
                N'RiskManagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagementSummary_Create
-- Description:   Inserts a new RiskManagementSummary record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagementSummary_Create
    @RiskManagementSummaryId uniqueidentifier,
    @SummaryDateKey int,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @RiskCategory nvarchar(100),
    @RiskBand nvarchar(20),
    @RiskCount int,
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RiskManagementSummaryId IS NULL
            THROW 50001, N'RiskManagementSummaryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RiskManagementSummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RiskManagementSummaryId=', @RiskManagementSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[RiskManagementSummary] (RiskManagementSummaryId, SummaryDateKey, TenantId, SiteId, RiskCategory, RiskBand, RiskCount, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@RiskManagementSummaryId, @SummaryDateKey, @TenantId, @SiteId, @RiskCategory, @RiskBand, @RiskCount, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskManagementSummaryId=', @RiskManagementSummaryId, N'; User=', SYSTEM_USER);
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
                N'RiskManagementSummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagementSummary_GetById
-- Description:   Retrieves a single RiskManagementSummary record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagementSummary_GetById
    @RiskManagementSummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RiskManagementSummaryId IS NULL
            THROW 50001, N'RiskManagementSummaryId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RiskManagementSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'RiskManagementSummaryId=', @RiskManagementSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RiskManagementSummaryId, e.SummaryDateKey, e.TenantId, e.SiteId, e.RiskCategory, e.RiskBand, e.RiskCount, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Operational].[RiskManagementSummary] AS e
        WHERE e.RiskManagementSummaryId = @RiskManagementSummaryId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskManagementSummaryId=', @RiskManagementSummaryId, N'; User=', SYSTEM_USER);
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
                N'RiskManagementSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagementSummary_List
-- Description:   Returns a paginated, soft-delete-filtered list of RiskManagementSummary records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagementSummary_List
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
                N'RiskManagementSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RiskManagementSummaryId, e.SummaryDateKey, e.TenantId, e.SiteId, e.RiskCategory, e.RiskBand, e.RiskCount, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Operational].[RiskManagementSummary] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.RiskManagementSummaryId
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
                N'RiskManagementSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_RiskManagementSummary_Delete
-- Description:   Soft-deletes a RiskManagementSummary record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_RiskManagementSummary_Delete
    @RiskManagementSummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RiskManagementSummaryId IS NULL
            THROW 50001, N'RiskManagementSummaryId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[RiskManagementSummary] WHERE RiskManagementSummaryId = @RiskManagementSummaryId AND ISNULL(IsDeleted,0) = 0)
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
                N'RiskManagementSummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RiskManagementSummaryId=', @RiskManagementSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[RiskManagementSummary]
        SET IsDeleted = 1
        WHERE RiskManagementSummaryId = @RiskManagementSummaryId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RiskManagementSummaryId=', @RiskManagementSummaryId, N'; User=', SYSTEM_USER);
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
                N'RiskManagementSummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Schedules_Create
-- Description:   Inserts a new Schedules record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Schedules_Create
    @SchedulesId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @TaskName nvarchar(300),
    @RecurrencePattern nvarchar(50),
    @AssignedToUserId uniqueidentifier,
    @DueAtUtc datetime2(7),
    @SlaWindowMinutes int,
    @CompletedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SchedulesId IS NULL
            THROW 50001, N'SchedulesId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Schedules', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'SchedulesId=', @SchedulesId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[Schedules] (SchedulesId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, TaskName, RecurrencePattern, AssignedToUserId, DueAtUtc, SlaWindowMinutes, CompletedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@SchedulesId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @TaskName, @RecurrencePattern, @AssignedToUserId, @DueAtUtc, @SlaWindowMinutes, @CompletedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SchedulesId=', @SchedulesId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Schedules', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Schedules_Update
-- Description:   Updates an existing Schedules record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Schedules_Update
    @SchedulesId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @TaskName nvarchar(300),
    @RecurrencePattern nvarchar(50),
    @AssignedToUserId uniqueidentifier,
    @DueAtUtc datetime2(7),
    @SlaWindowMinutes int,
    @CompletedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SchedulesId IS NULL
            THROW 50001, N'SchedulesId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Schedules] WHERE SchedulesId = @SchedulesId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Operational].[Schedules] WHERE SchedulesId = @SchedulesId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Schedules', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'SchedulesId=', @SchedulesId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Operational].[Schedules]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            TaskName = @TaskName,
            RecurrencePattern = @RecurrencePattern,
            AssignedToUserId = @AssignedToUserId,
            DueAtUtc = @DueAtUtc,
            SlaWindowMinutes = @SlaWindowMinutes,
            CompletedAtUtc = @CompletedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE SchedulesId = @SchedulesId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SchedulesId=', @SchedulesId, N'; User=', SYSTEM_USER);
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
                N'Schedules', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Schedules_GetById
-- Description:   Retrieves a single Schedules record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Schedules_GetById
    @SchedulesId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SchedulesId IS NULL
            THROW 50001, N'SchedulesId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Schedules', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'SchedulesId=', @SchedulesId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.SchedulesId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.TaskName, e.RecurrencePattern, e.AssignedToUserId, e.DueAtUtc, e.SlaWindowMinutes, e.CompletedAtUtc
        FROM [Operational].[Schedules] AS e
        WHERE e.SchedulesId = @SchedulesId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SchedulesId=', @SchedulesId, N'; User=', SYSTEM_USER);
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
                N'Schedules', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Schedules_List
-- Description:   Returns a paginated, soft-delete-filtered list of Schedules records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Schedules_List
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
                N'Schedules', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.SchedulesId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.TaskName, e.RecurrencePattern, e.AssignedToUserId, e.DueAtUtc, e.SlaWindowMinutes, e.CompletedAtUtc
        FROM [Operational].[Schedules] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.SchedulesId
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
                N'Schedules', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Schedules_Delete
-- Description:   Soft-deletes a Schedules record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Schedules_Delete
    @SchedulesId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SchedulesId IS NULL
            THROW 50001, N'SchedulesId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Schedules] WHERE SchedulesId = @SchedulesId AND ISNULL(IsDeleted,0) = 0)
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
                N'Schedules', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'SchedulesId=', @SchedulesId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[Schedules]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE SchedulesId = @SchedulesId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SchedulesId=', @SchedulesId, N'; User=', SYSTEM_USER);
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
                N'Schedules', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_SchedulesHistoryEntry_Create
-- Description:   Inserts a new append-only SchedulesHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_SchedulesHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @SchedulesId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @DueAtUtcAtTransition datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'SchedulesHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[SchedulesHistoryEntry] (HistoryId, SchedulesId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, DueAtUtcAtTransition)
        VALUES (@HistoryId, @SchedulesId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @DueAtUtcAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'SchedulesHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_SchedulesHistoryEntry_GetByParentId
-- Description:   Returns all SchedulesHistoryEntry records for a given SchedulesId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_SchedulesHistoryEntry_GetByParentId
    @SchedulesId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SchedulesId IS NULL
            THROW 50001, N'SchedulesId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'SchedulesHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'SchedulesId=', @SchedulesId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.SchedulesId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.DueAtUtcAtTransition
        FROM [Operational].[SchedulesHistoryEntry] AS e
        WHERE e.SchedulesId = @SchedulesId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SchedulesId=', @SchedulesId, N'; User=', SYSTEM_USER);
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
                N'SchedulesHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Training_Create
-- Description:   Inserts a new Training record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Training_Create
    @TrainingId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @CourseName nvarchar(300),
    @CourseCode nvarchar(100),
    @WorkerId uniqueidentifier,
    @ComplianceCategory nvarchar(100),
    @AssignedAtUtc datetime2(7),
    @DueAtUtc datetime2(7),
    @CompletedAtUtc datetime2(7),
    @Score decimal(7,4)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TrainingId IS NULL
            THROW 50001, N'TrainingId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Training', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TrainingId=', @TrainingId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[Training] (TrainingId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, CourseName, CourseCode, WorkerId, ComplianceCategory, AssignedAtUtc, DueAtUtc, CompletedAtUtc, Score, IsDeleted, CreatedUtc)
        VALUES (@TrainingId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @CourseName, @CourseCode, @WorkerId, @ComplianceCategory, @AssignedAtUtc, @DueAtUtc, @CompletedAtUtc, @Score, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TrainingId=', @TrainingId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Training', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Training_Update
-- Description:   Updates an existing Training record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Training_Update
    @TrainingId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @CourseName nvarchar(300),
    @CourseCode nvarchar(100),
    @WorkerId uniqueidentifier,
    @ComplianceCategory nvarchar(100),
    @AssignedAtUtc datetime2(7),
    @DueAtUtc datetime2(7),
    @CompletedAtUtc datetime2(7),
    @Score decimal(7,4),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TrainingId IS NULL
            THROW 50001, N'TrainingId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Training] WHERE TrainingId = @TrainingId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Operational].[Training] WHERE TrainingId = @TrainingId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Training', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TrainingId=', @TrainingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Operational].[Training]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            CourseName = @CourseName,
            CourseCode = @CourseCode,
            WorkerId = @WorkerId,
            ComplianceCategory = @ComplianceCategory,
            AssignedAtUtc = @AssignedAtUtc,
            DueAtUtc = @DueAtUtc,
            CompletedAtUtc = @CompletedAtUtc,
            Score = @Score,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE TrainingId = @TrainingId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TrainingId=', @TrainingId, N'; User=', SYSTEM_USER);
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
                N'Training', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Training_GetById
-- Description:   Retrieves a single Training record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Training_GetById
    @TrainingId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TrainingId IS NULL
            THROW 50001, N'TrainingId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Training', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'TrainingId=', @TrainingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.TrainingId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.CourseName, e.CourseCode, e.WorkerId, e.ComplianceCategory, e.AssignedAtUtc, e.DueAtUtc, e.CompletedAtUtc, e.Score
        FROM [Operational].[Training] AS e
        WHERE e.TrainingId = @TrainingId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TrainingId=', @TrainingId, N'; User=', SYSTEM_USER);
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
                N'Training', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Training_List
-- Description:   Returns a paginated, soft-delete-filtered list of Training records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Training_List
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
                N'Training', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.TrainingId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.CourseName, e.CourseCode, e.WorkerId, e.ComplianceCategory, e.AssignedAtUtc, e.DueAtUtc, e.CompletedAtUtc, e.Score
        FROM [Operational].[Training] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.TrainingId
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
                N'Training', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_Training_Delete
-- Description:   Soft-deletes a Training record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_Training_Delete
    @TrainingId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TrainingId IS NULL
            THROW 50001, N'TrainingId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Operational].[Training] WHERE TrainingId = @TrainingId AND ISNULL(IsDeleted,0) = 0)
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
                N'Training', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TrainingId=', @TrainingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Operational].[Training]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE TrainingId = @TrainingId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TrainingId=', @TrainingId, N'; User=', SYSTEM_USER);
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
                N'Training', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_TrainingHistoryEntry_Create
-- Description:   Inserts a new append-only TrainingHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_TrainingHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @TrainingId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @ScoreAtTransition decimal(7,4)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'TrainingHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Operational].[TrainingHistoryEntry] (HistoryId, TrainingId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, ScoreAtTransition)
        VALUES (@HistoryId, @TrainingId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @ScoreAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'TrainingHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Operational].sp_TrainingHistoryEntry_GetByParentId
-- Description:   Returns all TrainingHistoryEntry records for a given TrainingId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Operational].sp_TrainingHistoryEntry_GetByParentId
    @TrainingId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TrainingId IS NULL
            THROW 50001, N'TrainingId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'TrainingHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'TrainingId=', @TrainingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.TrainingId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.ScoreAtTransition
        FROM [Operational].[TrainingHistoryEntry] AS e
        WHERE e.TrainingId = @TrainingId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TrainingId=', @TrainingId, N'; User=', SYSTEM_USER);
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
                N'TrainingHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [OperationalSafety].sp_OperationalSafety_Create
-- Description:   Inserts a new OperationalSafety record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [OperationalSafety].sp_OperationalSafety_Create
    @OperationalSafetyId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @OperationalSafetyId IS NULL
            THROW 50001, N'OperationalSafetyId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'OperationalSafety', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'OperationalSafetyId=', @OperationalSafetyId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [OperationalSafety].[OperationalSafety] (OperationalSafetyId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@OperationalSafetyId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'OperationalSafetyId=', @OperationalSafetyId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'OperationalSafety', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [OperationalSafety].sp_OperationalSafety_Update
-- Description:   Updates an existing OperationalSafety record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [OperationalSafety].sp_OperationalSafety_Update
    @OperationalSafetyId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @OperationalSafetyId IS NULL
            THROW 50001, N'OperationalSafetyId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [OperationalSafety].[OperationalSafety] WHERE OperationalSafetyId = @OperationalSafetyId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [OperationalSafety].[OperationalSafety] WHERE OperationalSafetyId = @OperationalSafetyId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'OperationalSafety', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'OperationalSafetyId=', @OperationalSafetyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [OperationalSafety].[OperationalSafety]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE OperationalSafetyId = @OperationalSafetyId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'OperationalSafetyId=', @OperationalSafetyId, N'; User=', SYSTEM_USER);
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
                N'OperationalSafety', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [OperationalSafety].sp_OperationalSafety_GetById
-- Description:   Retrieves a single OperationalSafety record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [OperationalSafety].sp_OperationalSafety_GetById
    @OperationalSafetyId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @OperationalSafetyId IS NULL
            THROW 50001, N'OperationalSafetyId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'OperationalSafety', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'OperationalSafetyId=', @OperationalSafetyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.OperationalSafetyId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [OperationalSafety].[OperationalSafety] AS e
        WHERE e.OperationalSafetyId = @OperationalSafetyId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'OperationalSafetyId=', @OperationalSafetyId, N'; User=', SYSTEM_USER);
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
                N'OperationalSafety', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [OperationalSafety].sp_OperationalSafety_List
-- Description:   Returns a paginated, soft-delete-filtered list of OperationalSafety records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [OperationalSafety].sp_OperationalSafety_List
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
                N'OperationalSafety', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.OperationalSafetyId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [OperationalSafety].[OperationalSafety] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.OperationalSafetyId
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
                N'OperationalSafety', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [OperationalSafety].sp_OperationalSafety_Delete
-- Description:   Soft-deletes a OperationalSafety record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [OperationalSafety].sp_OperationalSafety_Delete
    @OperationalSafetyId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @OperationalSafetyId IS NULL
            THROW 50001, N'OperationalSafetyId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [OperationalSafety].[OperationalSafety] WHERE OperationalSafetyId = @OperationalSafetyId AND ISNULL(IsDeleted,0) = 0)
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
                N'OperationalSafety', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'OperationalSafetyId=', @OperationalSafetyId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [OperationalSafety].[OperationalSafety]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE OperationalSafetyId = @OperationalSafetyId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'OperationalSafetyId=', @OperationalSafetyId, N'; User=', SYSTEM_USER);
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
                N'OperationalSafety', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ProcessManagement].sp_ProcessDefinition_Create
-- Description:   Inserts a new ProcessDefinition record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ProcessManagement].sp_ProcessDefinition_Create
    @ProcessDefinitionId uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ProcessName nvarchar(256),
    @Category nvarchar(100),
    @Status nvarchar(50),
    @Version int,
    @OwnerId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProcessDefinitionId IS NULL
            THROW 50001, N'ProcessDefinitionId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ProcessDefinition', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ProcessDefinitionId=', @ProcessDefinitionId, N'; ', N'TenantId=', @TenantId, N'; ', N'SiteId=', @SiteId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [ProcessManagement].[ProcessDefinition] (ProcessDefinitionId, TenantId, SiteId, ProcessName, Category, Status, Version, OwnerId, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@ProcessDefinitionId, @TenantId, @SiteId, @ProcessName, @Category, @Status, @Version, @OwnerId, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProcessDefinitionId=', @ProcessDefinitionId, N'; ', N'TenantId=', @TenantId, N'; ', N'SiteId=', @SiteId, N'; User=', SYSTEM_USER);
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
                N'ProcessDefinition', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ProcessManagement].sp_ProcessDefinition_Update
-- Description:   Updates an existing ProcessDefinition record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ProcessManagement].sp_ProcessDefinition_Update
    @ProcessDefinitionId uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ProcessName nvarchar(256),
    @Category nvarchar(100),
    @Status nvarchar(50),
    @Version int,
    @OwnerId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProcessDefinitionId IS NULL
            THROW 50001, N'ProcessDefinitionId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [ProcessManagement].[ProcessDefinition] WHERE ProcessDefinitionId = @ProcessDefinitionId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [ProcessManagement].[ProcessDefinition] WHERE ProcessDefinitionId = @ProcessDefinitionId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ProcessDefinition', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ProcessDefinitionId=', @ProcessDefinitionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [ProcessManagement].[ProcessDefinition]
        SET
            TenantId = @TenantId,
            SiteId = @SiteId,
            ProcessName = @ProcessName,
            Category = @Category,
            Status = @Status,
            Version = @Version,
            OwnerId = @OwnerId,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ProcessDefinitionId = @ProcessDefinitionId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProcessDefinitionId=', @ProcessDefinitionId, N'; User=', SYSTEM_USER);
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
                N'ProcessDefinition', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ProcessManagement].sp_ProcessDefinition_GetById
-- Description:   Retrieves a single ProcessDefinition record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ProcessManagement].sp_ProcessDefinition_GetById
    @ProcessDefinitionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProcessDefinitionId IS NULL
            THROW 50001, N'ProcessDefinitionId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ProcessDefinition', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ProcessDefinitionId=', @ProcessDefinitionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ProcessDefinitionId, e.TenantId, e.SiteId, e.ProcessName, e.Category, e.Status, e.Version, e.OwnerId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [ProcessManagement].[ProcessDefinition] AS e
        WHERE e.ProcessDefinitionId = @ProcessDefinitionId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProcessDefinitionId=', @ProcessDefinitionId, N'; User=', SYSTEM_USER);
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
                N'ProcessDefinition', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ProcessManagement].sp_ProcessDefinition_List
-- Description:   Returns a paginated, soft-delete-filtered list of ProcessDefinition records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ProcessManagement].sp_ProcessDefinition_List
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
                N'ProcessDefinition', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ProcessDefinitionId, e.TenantId, e.SiteId, e.ProcessName, e.Category, e.Status, e.Version, e.OwnerId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [ProcessManagement].[ProcessDefinition] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ProcessDefinitionId
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
                N'ProcessDefinition', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [ProcessManagement].sp_ProcessDefinition_Delete
-- Description:   Soft-deletes a ProcessDefinition record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [ProcessManagement].sp_ProcessDefinition_Delete
    @ProcessDefinitionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ProcessDefinitionId IS NULL
            THROW 50001, N'ProcessDefinitionId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [ProcessManagement].[ProcessDefinition] WHERE ProcessDefinitionId = @ProcessDefinitionId AND ISNULL(IsDeleted,0) = 0)
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
                N'ProcessDefinition', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ProcessDefinitionId=', @ProcessDefinitionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [ProcessManagement].[ProcessDefinition]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ProcessDefinitionId = @ProcessDefinitionId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ProcessDefinitionId=', @ProcessDefinitionId, N'; User=', SYSTEM_USER);
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
                N'ProcessDefinition', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_EmergencyManagement_Create
-- Description:   Inserts a new EmergencyManagement record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_EmergencyManagement_Create
    @EmergencyManagementId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @EmergencyType nvarchar(100),
    @Severity nvarchar(50),
    @Location nvarchar(300),
    @CasualtyCount int,
    @ResponseTeamNotified bit,
    @DeclaredAtUtc datetime2(7),
    @StabilizedAtUtc datetime2(7),
    @ResolvedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmergencyManagementId IS NULL
            THROW 50001, N'EmergencyManagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EmergencyManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EmergencyManagementId=', @EmergencyManagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [SafetyEvents].[EmergencyManagement] (EmergencyManagementId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, EmergencyType, Severity, Location, CasualtyCount, ResponseTeamNotified, DeclaredAtUtc, StabilizedAtUtc, ResolvedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@EmergencyManagementId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @EmergencyType, @Severity, @Location, @CasualtyCount, @ResponseTeamNotified, @DeclaredAtUtc, @StabilizedAtUtc, @ResolvedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EmergencyManagementId=', @EmergencyManagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'EmergencyManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_EmergencyManagement_Update
-- Description:   Updates an existing EmergencyManagement record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_EmergencyManagement_Update
    @EmergencyManagementId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @EmergencyType nvarchar(100),
    @Severity nvarchar(50),
    @Location nvarchar(300),
    @CasualtyCount int,
    @ResponseTeamNotified bit,
    @DeclaredAtUtc datetime2(7),
    @StabilizedAtUtc datetime2(7),
    @ResolvedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmergencyManagementId IS NULL
            THROW 50001, N'EmergencyManagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[EmergencyManagement] WHERE EmergencyManagementId = @EmergencyManagementId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[EmergencyManagement] WHERE EmergencyManagementId = @EmergencyManagementId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'EmergencyManagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EmergencyManagementId=', @EmergencyManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [SafetyEvents].[EmergencyManagement]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            EmergencyType = @EmergencyType,
            Severity = @Severity,
            Location = @Location,
            CasualtyCount = @CasualtyCount,
            ResponseTeamNotified = @ResponseTeamNotified,
            DeclaredAtUtc = @DeclaredAtUtc,
            StabilizedAtUtc = @StabilizedAtUtc,
            ResolvedAtUtc = @ResolvedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE EmergencyManagementId = @EmergencyManagementId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EmergencyManagementId=', @EmergencyManagementId, N'; User=', SYSTEM_USER);
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
                N'EmergencyManagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_EmergencyManagement_GetById
-- Description:   Retrieves a single EmergencyManagement record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_EmergencyManagement_GetById
    @EmergencyManagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmergencyManagementId IS NULL
            THROW 50001, N'EmergencyManagementId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EmergencyManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'EmergencyManagementId=', @EmergencyManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.EmergencyManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.EmergencyType, e.Severity, e.Location, e.CasualtyCount, e.ResponseTeamNotified, e.DeclaredAtUtc, e.StabilizedAtUtc, e.ResolvedAtUtc
        FROM [SafetyEvents].[EmergencyManagement] AS e
        WHERE e.EmergencyManagementId = @EmergencyManagementId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EmergencyManagementId=', @EmergencyManagementId, N'; User=', SYSTEM_USER);
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
                N'EmergencyManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_EmergencyManagement_List
-- Description:   Returns a paginated, soft-delete-filtered list of EmergencyManagement records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_EmergencyManagement_List
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
                N'EmergencyManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.EmergencyManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.EmergencyType, e.Severity, e.Location, e.CasualtyCount, e.ResponseTeamNotified, e.DeclaredAtUtc, e.StabilizedAtUtc, e.ResolvedAtUtc
        FROM [SafetyEvents].[EmergencyManagement] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.EmergencyManagementId
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
                N'EmergencyManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_EmergencyManagement_Delete
-- Description:   Soft-deletes a EmergencyManagement record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_EmergencyManagement_Delete
    @EmergencyManagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmergencyManagementId IS NULL
            THROW 50001, N'EmergencyManagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[EmergencyManagement] WHERE EmergencyManagementId = @EmergencyManagementId AND ISNULL(IsDeleted,0) = 0)
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
                N'EmergencyManagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'EmergencyManagementId=', @EmergencyManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [SafetyEvents].[EmergencyManagement]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE EmergencyManagementId = @EmergencyManagementId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EmergencyManagementId=', @EmergencyManagementId, N'; User=', SYSTEM_USER);
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
                N'EmergencyManagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_EmergencyManagementHistoryEntry_Create
-- Description:   Inserts a new append-only EmergencyManagementHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_EmergencyManagementHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @EmergencyManagementId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @SeverityAtTransition nvarchar(50),
    @CasualtyCountAtTransition int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EmergencyManagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [SafetyEvents].[EmergencyManagementHistoryEntry] (HistoryId, EmergencyManagementId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, SeverityAtTransition, CasualtyCountAtTransition)
        VALUES (@HistoryId, @EmergencyManagementId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @SeverityAtTransition, @CasualtyCountAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'EmergencyManagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_EmergencyManagementHistoryEntry_GetByParentId
-- Description:   Returns all EmergencyManagementHistoryEntry records for a given EmergencyManagementId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_EmergencyManagementHistoryEntry_GetByParentId
    @EmergencyManagementId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @EmergencyManagementId IS NULL
            THROW 50001, N'EmergencyManagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'EmergencyManagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'EmergencyManagementId=', @EmergencyManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.EmergencyManagementId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.SeverityAtTransition, e.CasualtyCountAtTransition
        FROM [SafetyEvents].[EmergencyManagementHistoryEntry] AS e
        WHERE e.EmergencyManagementId = @EmergencyManagementId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'EmergencyManagementId=', @EmergencyManagementId, N'; User=', SYSTEM_USER);
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
                N'EmergencyManagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagement_Create
-- Description:   Inserts a new IncidentManagement record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagement_Create
    @IncidentManagementId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @Title nvarchar(300),
    @Description nvarchar(MAX),
    @Severity nvarchar(50),
    @Location nvarchar(300),
    @IncidentOccurredAtUtc datetime2(7),
    @InjuryType nvarchar(50),
    @ReportedByWorkerId uniqueidentifier,
    @AssignedInvestigatorId uniqueidentifier,
    @RootCause nvarchar(MAX),
    @CorrectiveActions nvarchar(MAX),
    @ClosedAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IncidentManagementId IS NULL
            THROW 50001, N'IncidentManagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IncidentManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'IncidentManagementId=', @IncidentManagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [SafetyEvents].[IncidentManagement] (IncidentManagementId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, Title, Description, Severity, Location, IncidentOccurredAtUtc, InjuryType, ReportedByWorkerId, AssignedInvestigatorId, RootCause, CorrectiveActions, ClosedAtUtc, IsDeleted, CreatedUtc)
        VALUES (@IncidentManagementId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @Title, @Description, @Severity, @Location, @IncidentOccurredAtUtc, @InjuryType, @ReportedByWorkerId, @AssignedInvestigatorId, @RootCause, @CorrectiveActions, @ClosedAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentManagementId=', @IncidentManagementId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'IncidentManagement', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagement_Update
-- Description:   Updates an existing IncidentManagement record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagement_Update
    @IncidentManagementId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @Title nvarchar(300),
    @Description nvarchar(MAX),
    @Severity nvarchar(50),
    @Location nvarchar(300),
    @IncidentOccurredAtUtc datetime2(7),
    @InjuryType nvarchar(50),
    @ReportedByWorkerId uniqueidentifier,
    @AssignedInvestigatorId uniqueidentifier,
    @RootCause nvarchar(MAX),
    @CorrectiveActions nvarchar(MAX),
    @ClosedAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IncidentManagementId IS NULL
            THROW 50001, N'IncidentManagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[IncidentManagement] WHERE IncidentManagementId = @IncidentManagementId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[IncidentManagement] WHERE IncidentManagementId = @IncidentManagementId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'IncidentManagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'IncidentManagementId=', @IncidentManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [SafetyEvents].[IncidentManagement]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            Title = @Title,
            Description = @Description,
            Severity = @Severity,
            Location = @Location,
            IncidentOccurredAtUtc = @IncidentOccurredAtUtc,
            InjuryType = @InjuryType,
            ReportedByWorkerId = @ReportedByWorkerId,
            AssignedInvestigatorId = @AssignedInvestigatorId,
            RootCause = @RootCause,
            CorrectiveActions = @CorrectiveActions,
            ClosedAtUtc = @ClosedAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE IncidentManagementId = @IncidentManagementId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentManagementId=', @IncidentManagementId, N'; User=', SYSTEM_USER);
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
                N'IncidentManagement', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagement_GetById
-- Description:   Retrieves a single IncidentManagement record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagement_GetById
    @IncidentManagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IncidentManagementId IS NULL
            THROW 50001, N'IncidentManagementId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IncidentManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'IncidentManagementId=', @IncidentManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.IncidentManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.Title, e.Description, e.Severity, e.Location, e.IncidentOccurredAtUtc, e.InjuryType, e.ReportedByWorkerId, e.AssignedInvestigatorId, e.RootCause, e.CorrectiveActions, e.ClosedAtUtc
        FROM [SafetyEvents].[IncidentManagement] AS e
        WHERE e.IncidentManagementId = @IncidentManagementId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentManagementId=', @IncidentManagementId, N'; User=', SYSTEM_USER);
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
                N'IncidentManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagement_List
-- Description:   Returns a paginated, soft-delete-filtered list of IncidentManagement records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagement_List
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
                N'IncidentManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.IncidentManagementId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.Title, e.Description, e.Severity, e.Location, e.IncidentOccurredAtUtc, e.InjuryType, e.ReportedByWorkerId, e.AssignedInvestigatorId, e.RootCause, e.CorrectiveActions, e.ClosedAtUtc
        FROM [SafetyEvents].[IncidentManagement] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.IncidentManagementId
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
                N'IncidentManagement', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagement_Delete
-- Description:   Soft-deletes a IncidentManagement record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagement_Delete
    @IncidentManagementId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IncidentManagementId IS NULL
            THROW 50001, N'IncidentManagementId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[IncidentManagement] WHERE IncidentManagementId = @IncidentManagementId AND ISNULL(IsDeleted,0) = 0)
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
                N'IncidentManagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'IncidentManagementId=', @IncidentManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [SafetyEvents].[IncidentManagement]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE IncidentManagementId = @IncidentManagementId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentManagementId=', @IncidentManagementId, N'; User=', SYSTEM_USER);
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
                N'IncidentManagement', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagementHistoryEntry_Create
-- Description:   Inserts a new append-only IncidentManagementHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagementHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @IncidentManagementId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @SeverityAtTransition nvarchar(50),
    @RootCauseAtTransition nvarchar(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IncidentManagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [SafetyEvents].[IncidentManagementHistoryEntry] (HistoryId, IncidentManagementId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, SeverityAtTransition, RootCauseAtTransition)
        VALUES (@HistoryId, @IncidentManagementId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @SeverityAtTransition, @RootCauseAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'IncidentManagementHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagementHistoryEntry_GetByParentId
-- Description:   Returns all IncidentManagementHistoryEntry records for a given IncidentManagementId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagementHistoryEntry_GetByParentId
    @IncidentManagementId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IncidentManagementId IS NULL
            THROW 50001, N'IncidentManagementId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IncidentManagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'IncidentManagementId=', @IncidentManagementId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.IncidentManagementId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.SeverityAtTransition, e.RootCauseAtTransition
        FROM [SafetyEvents].[IncidentManagementHistoryEntry] AS e
        WHERE e.IncidentManagementId = @IncidentManagementId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentManagementId=', @IncidentManagementId, N'; User=', SYSTEM_USER);
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
                N'IncidentManagementHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagementSummary_Create
-- Description:   Inserts a new IncidentManagementSummary record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagementSummary_Create
    @IncidentManagementSummaryId uniqueidentifier,
    @SummaryDateKey int,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @Severity nvarchar(50),
    @Status nvarchar(50),
    @IncidentCount int,
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IncidentManagementSummaryId IS NULL
            THROW 50001, N'IncidentManagementSummaryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IncidentManagementSummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'IncidentManagementSummaryId=', @IncidentManagementSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [SafetyEvents].[IncidentManagementSummary] (IncidentManagementSummaryId, SummaryDateKey, TenantId, SiteId, Severity, Status, IncidentCount, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@IncidentManagementSummaryId, @SummaryDateKey, @TenantId, @SiteId, @Severity, @Status, @IncidentCount, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentManagementSummaryId=', @IncidentManagementSummaryId, N'; User=', SYSTEM_USER);
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
                N'IncidentManagementSummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagementSummary_GetById
-- Description:   Retrieves a single IncidentManagementSummary record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagementSummary_GetById
    @IncidentManagementSummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IncidentManagementSummaryId IS NULL
            THROW 50001, N'IncidentManagementSummaryId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IncidentManagementSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'IncidentManagementSummaryId=', @IncidentManagementSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.IncidentManagementSummaryId, e.SummaryDateKey, e.TenantId, e.SiteId, e.Severity, e.Status, e.IncidentCount, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [SafetyEvents].[IncidentManagementSummary] AS e
        WHERE e.IncidentManagementSummaryId = @IncidentManagementSummaryId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentManagementSummaryId=', @IncidentManagementSummaryId, N'; User=', SYSTEM_USER);
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
                N'IncidentManagementSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagementSummary_List
-- Description:   Returns a paginated, soft-delete-filtered list of IncidentManagementSummary records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagementSummary_List
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
                N'IncidentManagementSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.IncidentManagementSummaryId, e.SummaryDateKey, e.TenantId, e.SiteId, e.Severity, e.Status, e.IncidentCount, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [SafetyEvents].[IncidentManagementSummary] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.IncidentManagementSummaryId
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
                N'IncidentManagementSummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_IncidentManagementSummary_Delete
-- Description:   Soft-deletes a IncidentManagementSummary record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_IncidentManagementSummary_Delete
    @IncidentManagementSummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IncidentManagementSummaryId IS NULL
            THROW 50001, N'IncidentManagementSummaryId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[IncidentManagementSummary] WHERE IncidentManagementSummaryId = @IncidentManagementSummaryId AND ISNULL(IsDeleted,0) = 0)
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
                N'IncidentManagementSummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'IncidentManagementSummaryId=', @IncidentManagementSummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [SafetyEvents].[IncidentManagementSummary]
        SET IsDeleted = 1
        WHERE IncidentManagementSummaryId = @IncidentManagementSummaryId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IncidentManagementSummaryId=', @IncidentManagementSummaryId, N'; User=', SYSTEM_USER);
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
                N'IncidentManagementSummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_Observation_Create
-- Description:   Inserts a new Observation record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_Observation_Create
    @ObservationId uniqueidentifier,
    @Status nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ObserverId uniqueidentifier,
    @ObservationType nvarchar(50),
    @HazardCategory nvarchar(100),
    @Location nvarchar(300),
    @Description nvarchar(MAX),
    @CorrectiveActionRequired bit,
    @FollowUpDueAtUtc datetime2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ObservationId IS NULL
            THROW 50001, N'ObservationId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Observation', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ObservationId=', @ObservationId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [SafetyEvents].[Observation] (ObservationId, Status, CreatedBy, UpdatedBy, TenantId, SiteId, ObserverId, ObservationType, HazardCategory, Location, Description, CorrectiveActionRequired, FollowUpDueAtUtc, IsDeleted, CreatedUtc)
        VALUES (@ObservationId, @Status, @CreatedBy, @UpdatedBy, @TenantId, @SiteId, @ObserverId, @ObservationType, @HazardCategory, @Location, @Description, @CorrectiveActionRequired, @FollowUpDueAtUtc, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ObservationId=', @ObservationId, N'; ', N'Status=', @Status, N'; ', N'CreatedBy=', @CreatedBy, N'; User=', SYSTEM_USER);
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
                N'Observation', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_Observation_Update
-- Description:   Updates an existing Observation record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_Observation_Update
    @ObservationId uniqueidentifier,
    @Status nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @SiteId uniqueidentifier,
    @ObserverId uniqueidentifier,
    @ObservationType nvarchar(50),
    @HazardCategory nvarchar(100),
    @Location nvarchar(300),
    @Description nvarchar(MAX),
    @CorrectiveActionRequired bit,
    @FollowUpDueAtUtc datetime2(7),
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ObservationId IS NULL
            THROW 50001, N'ObservationId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[Observation] WHERE ObservationId = @ObservationId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[Observation] WHERE ObservationId = @ObservationId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Observation', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ObservationId=', @ObservationId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [SafetyEvents].[Observation]
        SET
            Status = @Status,
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            SiteId = @SiteId,
            ObserverId = @ObserverId,
            ObservationType = @ObservationType,
            HazardCategory = @HazardCategory,
            Location = @Location,
            Description = @Description,
            CorrectiveActionRequired = @CorrectiveActionRequired,
            FollowUpDueAtUtc = @FollowUpDueAtUtc,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ObservationId = @ObservationId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ObservationId=', @ObservationId, N'; User=', SYSTEM_USER);
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
                N'Observation', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_Observation_GetById
-- Description:   Retrieves a single Observation record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_Observation_GetById
    @ObservationId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ObservationId IS NULL
            THROW 50001, N'ObservationId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Observation', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ObservationId=', @ObservationId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ObservationId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ObserverId, e.ObservationType, e.HazardCategory, e.Location, e.Description, e.CorrectiveActionRequired, e.FollowUpDueAtUtc
        FROM [SafetyEvents].[Observation] AS e
        WHERE e.ObservationId = @ObservationId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ObservationId=', @ObservationId, N'; User=', SYSTEM_USER);
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
                N'Observation', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_Observation_List
-- Description:   Returns a paginated, soft-delete-filtered list of Observation records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_Observation_List
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
                N'Observation', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ObservationId, e.Status, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken, e.SiteId, e.ObserverId, e.ObservationType, e.HazardCategory, e.Location, e.Description, e.CorrectiveActionRequired, e.FollowUpDueAtUtc
        FROM [SafetyEvents].[Observation] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.ObservationId
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
                N'Observation', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_Observation_Delete
-- Description:   Soft-deletes a Observation record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_Observation_Delete
    @ObservationId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ObservationId IS NULL
            THROW 50001, N'ObservationId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [SafetyEvents].[Observation] WHERE ObservationId = @ObservationId AND ISNULL(IsDeleted,0) = 0)
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
                N'Observation', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ObservationId=', @ObservationId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [SafetyEvents].[Observation]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ObservationId = @ObservationId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ObservationId=', @ObservationId, N'; User=', SYSTEM_USER);
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
                N'Observation', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_ObservationHistoryEntry_Create
-- Description:   Inserts a new append-only ObservationHistoryEntry record. No Update/Delete procs: table is immutable by design.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_ObservationHistoryEntry_Create
    @HistoryId uniqueidentifier,
    @ObservationId uniqueidentifier,
    @PreviousStatus nvarchar(100),
    @NewStatus nvarchar(100),
    @ChangedAtUtc datetime2(3),
    @ChangedBy nvarchar(256),
    @Notes nvarchar(MAX),
    @TenantId uniqueidentifier,
    @CorrectiveActionRequiredAtTransition bit
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @HistoryId IS NULL
            THROW 50001, N'HistoryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ObservationHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'HistoryId=', @HistoryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [SafetyEvents].[ObservationHistoryEntry] (HistoryId, ObservationId, PreviousStatus, NewStatus, ChangedAtUtc, ChangedBy, Notes, TenantId, CorrectiveActionRequiredAtTransition)
        VALUES (@HistoryId, @ObservationId, @PreviousStatus, @NewStatus, @ChangedAtUtc, @ChangedBy, @Notes, @TenantId, @CorrectiveActionRequiredAtTransition);
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'HistoryId=', @HistoryId, N'; User=', SYSTEM_USER);
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
                N'ObservationHistoryEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [SafetyEvents].sp_ObservationHistoryEntry_GetByParentId
-- Description:   Returns all ObservationHistoryEntry records for a given ObservationId (append-only, no soft-delete filter).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [SafetyEvents].sp_ObservationHistoryEntry_GetByParentId
    @ObservationId uniqueidentifier,
    @TenantId uniqueidentifier = NULL,
    @PageNumber int = 1,
    @PageSize int = 200
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ObservationId IS NULL
            THROW 50001, N'ObservationId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ObservationHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ObservationId=', @ObservationId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.HistoryId, e.ObservationId, e.PreviousStatus, e.NewStatus, e.ChangedAtUtc, e.ChangedBy, e.Notes, e.TenantId, e.CorrectiveActionRequiredAtTransition
        FROM [SafetyEvents].[ObservationHistoryEntry] AS e
        WHERE e.ObservationId = @ObservationId
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.HistoryId
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ObservationId=', @ObservationId, N'; User=', SYSTEM_USER);
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
                N'ObservationHistoryEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Sustainability].sp_Sustainability_Create
-- Description:   Inserts a new Sustainability record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Sustainability].sp_Sustainability_Create
    @SustainabilityId uniqueidentifier,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SustainabilityId IS NULL
            THROW 50001, N'SustainabilityId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Sustainability', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'SustainabilityId=', @SustainabilityId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Sustainability].[Sustainability] (SustainabilityId, CreatedBy, UpdatedBy, TenantId, IsDeleted, CreatedUtc)
        VALUES (@SustainabilityId, @CreatedBy, @UpdatedBy, @TenantId, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SustainabilityId=', @SustainabilityId, N'; ', N'CreatedBy=', @CreatedBy, N'; ', N'UpdatedBy=', @UpdatedBy, N'; User=', SYSTEM_USER);
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
                N'Sustainability', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Sustainability].sp_Sustainability_Update
-- Description:   Updates an existing Sustainability record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Sustainability].sp_Sustainability_Update
    @SustainabilityId uniqueidentifier,
    @UpdatedBy uniqueidentifier,
    @TenantId uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SustainabilityId IS NULL
            THROW 50001, N'SustainabilityId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Sustainability].[Sustainability] WHERE SustainabilityId = @SustainabilityId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Sustainability].[Sustainability] WHERE SustainabilityId = @SustainabilityId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'Sustainability', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'SustainabilityId=', @SustainabilityId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Sustainability].[Sustainability]
        SET
            UpdatedBy = @UpdatedBy,
            TenantId = @TenantId,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE SustainabilityId = @SustainabilityId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SustainabilityId=', @SustainabilityId, N'; User=', SYSTEM_USER);
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
                N'Sustainability', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Sustainability].sp_Sustainability_GetById
-- Description:   Retrieves a single Sustainability record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Sustainability].sp_Sustainability_GetById
    @SustainabilityId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SustainabilityId IS NULL
            THROW 50001, N'SustainabilityId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'Sustainability', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'SustainabilityId=', @SustainabilityId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.SustainabilityId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [Sustainability].[Sustainability] AS e
        WHERE e.SustainabilityId = @SustainabilityId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SustainabilityId=', @SustainabilityId, N'; User=', SYSTEM_USER);
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
                N'Sustainability', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Sustainability].sp_Sustainability_List
-- Description:   Returns a paginated, soft-delete-filtered list of Sustainability records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Sustainability].sp_Sustainability_List
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
                N'Sustainability', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.SustainabilityId, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.TenantId, e.ConcurrencyToken
        FROM [Sustainability].[Sustainability] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.SustainabilityId
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
                N'Sustainability', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Sustainability].sp_Sustainability_Delete
-- Description:   Soft-deletes a Sustainability record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Sustainability].sp_Sustainability_Delete
    @SustainabilityId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @SustainabilityId IS NULL
            THROW 50001, N'SustainabilityId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Sustainability].[Sustainability] WHERE SustainabilityId = @SustainabilityId AND ISNULL(IsDeleted,0) = 0)
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
                N'Sustainability', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'SustainabilityId=', @SustainabilityId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Sustainability].[Sustainability]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE SustainabilityId = @SustainabilityId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'SustainabilityId=', @SustainabilityId, N'; User=', SYSTEM_USER);
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
                N'Sustainability', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [WorkerWellBeing].sp_WorkerWellBeing_Create
-- Description:   Inserts a new WorkerWellBeing record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [WorkerWellBeing].sp_WorkerWellBeing_Create
    @WorkerWellBeingId uniqueidentifier,
    @TenantId uniqueidentifier,
    @WorkerId uniqueidentifier,
    @WellBeingRiskLevel nvarchar(50),
    @Status nvarchar(50),
    @RecordedAtUtc datetime2(3),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @WorkerWellBeingId IS NULL
            THROW 50001, N'WorkerWellBeingId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'WorkerWellBeing', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId, N'; ', N'TenantId=', @TenantId, N'; ', N'WorkerId=', @WorkerId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [WorkerWellBeing].[WorkerWellBeing] (WorkerWellBeingId, TenantId, WorkerId, WellBeingRiskLevel, Status, RecordedAtUtc, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@WorkerWellBeingId, @TenantId, @WorkerId, @WellBeingRiskLevel, @Status, @RecordedAtUtc, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId, N'; ', N'TenantId=', @TenantId, N'; ', N'WorkerId=', @WorkerId, N'; User=', SYSTEM_USER);
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
                N'WorkerWellBeing', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [WorkerWellBeing].sp_WorkerWellBeing_Update
-- Description:   Updates an existing WorkerWellBeing record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [WorkerWellBeing].sp_WorkerWellBeing_Update
    @WorkerWellBeingId uniqueidentifier,
    @TenantId uniqueidentifier,
    @WorkerId uniqueidentifier,
    @WellBeingRiskLevel nvarchar(50),
    @Status nvarchar(50),
    @RecordedAtUtc datetime2(3),
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @WorkerWellBeingId IS NULL
            THROW 50001, N'WorkerWellBeingId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [WorkerWellBeing].[WorkerWellBeing] WHERE WorkerWellBeingId = @WorkerWellBeingId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [WorkerWellBeing].[WorkerWellBeing] WHERE WorkerWellBeingId = @WorkerWellBeingId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'WorkerWellBeing', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [WorkerWellBeing].[WorkerWellBeing]
        SET
            TenantId = @TenantId,
            WorkerId = @WorkerId,
            WellBeingRiskLevel = @WellBeingRiskLevel,
            Status = @Status,
            RecordedAtUtc = @RecordedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE WorkerWellBeingId = @WorkerWellBeingId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId, N'; User=', SYSTEM_USER);
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
                N'WorkerWellBeing', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [WorkerWellBeing].sp_WorkerWellBeing_GetById
-- Description:   Retrieves a single WorkerWellBeing record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [WorkerWellBeing].sp_WorkerWellBeing_GetById
    @WorkerWellBeingId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @WorkerWellBeingId IS NULL
            THROW 50001, N'WorkerWellBeingId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'WorkerWellBeing', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.WorkerWellBeingId, e.TenantId, e.WorkerId, e.WellBeingRiskLevel, e.Status, e.RecordedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [WorkerWellBeing].[WorkerWellBeing] AS e
        WHERE e.WorkerWellBeingId = @WorkerWellBeingId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId, N'; User=', SYSTEM_USER);
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
                N'WorkerWellBeing', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [WorkerWellBeing].sp_WorkerWellBeing_List
-- Description:   Returns a paginated, soft-delete-filtered list of WorkerWellBeing records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [WorkerWellBeing].sp_WorkerWellBeing_List
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
                N'WorkerWellBeing', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.WorkerWellBeingId, e.TenantId, e.WorkerId, e.WellBeingRiskLevel, e.Status, e.RecordedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [WorkerWellBeing].[WorkerWellBeing] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
          AND (@TenantId IS NULL OR e.TenantId = @TenantId)
        ORDER BY e.WorkerWellBeingId
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
                N'WorkerWellBeing', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [WorkerWellBeing].sp_WorkerWellBeing_Delete
-- Description:   Soft-deletes a WorkerWellBeing record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [WorkerWellBeing].sp_WorkerWellBeing_Delete
    @WorkerWellBeingId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @WorkerWellBeingId IS NULL
            THROW 50001, N'WorkerWellBeingId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [WorkerWellBeing].[WorkerWellBeing] WHERE WorkerWellBeingId = @WorkerWellBeingId AND ISNULL(IsDeleted,0) = 0)
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
                N'WorkerWellBeing', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [WorkerWellBeing].[WorkerWellBeing]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE WorkerWellBeingId = @WorkerWellBeingId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'WorkerWellBeingId=', @WorkerWellBeingId, N'; User=', SYSTEM_USER);
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
                N'WorkerWellBeing', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
