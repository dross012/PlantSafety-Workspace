USE [PlantSafety.CloudIdentityDB]

GO

SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

-- =============================================

-- Phase 6: Stored Procedures (canonical FY2027 template)

-- Database: PlantSafety.CloudIdentityDB

-- Generated: 2026-07-19 by Procedure Architect agent, grounded in

-- sql-database-skill\references\00-canonical-templates.md

-- Idempotent: CREATE OR ALTER PROCEDURE throughout.

-- =============================================

GO



-- =============================================
-- Procedure:     [Identity].sp_ManagedIdentity_Create
-- Description:   Inserts a new ManagedIdentity record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ManagedIdentity_Create
    @ManagedIdentityId uniqueidentifier,
    @ArmResourceId nvarchar(500),
    @Name nvarchar(200),
    @Location nvarchar(100),
    @ProvisioningState nvarchar(50),
    @AzureTenantId uniqueidentifier,
    @PrincipalId uniqueidentifier,
    @ClientId nvarchar(100),
    @TagsJson nvarchar(MAX),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagedIdentityId IS NULL
            THROW 50001, N'ManagedIdentityId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ManagedIdentity', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ManagedIdentityId=', @ManagedIdentityId, N'; ', N'ArmResourceId=', @ArmResourceId, N'; ', N'Name=', @Name), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Identity].[ManagedIdentity] (ManagedIdentityId, ArmResourceId, Name, Location, ProvisioningState, AzureTenantId, PrincipalId, ClientId, TagsJson, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@ManagedIdentityId, @ArmResourceId, @Name, @Location, @ProvisioningState, @AzureTenantId, @PrincipalId, @ClientId, @TagsJson, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagedIdentityId=', @ManagedIdentityId, N'; ', N'ArmResourceId=', @ArmResourceId, N'; ', N'Name=', @Name, N'; User=', SYSTEM_USER);
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
                N'ManagedIdentity', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ManagedIdentity_Update
-- Description:   Updates an existing ManagedIdentity record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ManagedIdentity_Update
    @ManagedIdentityId uniqueidentifier,
    @ArmResourceId nvarchar(500),
    @Name nvarchar(200),
    @Location nvarchar(100),
    @ProvisioningState nvarchar(50),
    @AzureTenantId uniqueidentifier,
    @PrincipalId uniqueidentifier,
    @ClientId nvarchar(100),
    @TagsJson nvarchar(MAX),
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagedIdentityId IS NULL
            THROW 50001, N'ManagedIdentityId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Identity].[ManagedIdentity] WHERE ManagedIdentityId = @ManagedIdentityId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Identity].[ManagedIdentity] WHERE ManagedIdentityId = @ManagedIdentityId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ManagedIdentity', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ManagedIdentityId=', @ManagedIdentityId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Identity].[ManagedIdentity]
        SET
            ArmResourceId = @ArmResourceId,
            Name = @Name,
            Location = @Location,
            ProvisioningState = @ProvisioningState,
            AzureTenantId = @AzureTenantId,
            PrincipalId = @PrincipalId,
            ClientId = @ClientId,
            TagsJson = @TagsJson,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ManagedIdentityId = @ManagedIdentityId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagedIdentityId=', @ManagedIdentityId, N'; User=', SYSTEM_USER);
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
                N'ManagedIdentity', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ManagedIdentity_GetById
-- Description:   Retrieves a single ManagedIdentity record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ManagedIdentity_GetById
    @ManagedIdentityId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagedIdentityId IS NULL
            THROW 50001, N'ManagedIdentityId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ManagedIdentity', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ManagedIdentityId=', @ManagedIdentityId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ManagedIdentityId, e.ArmResourceId, e.Name, e.Location, e.ProvisioningState, e.AzureTenantId, e.PrincipalId, e.ClientId, e.TagsJson, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Identity].[ManagedIdentity] AS e
        WHERE e.ManagedIdentityId = @ManagedIdentityId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagedIdentityId=', @ManagedIdentityId, N'; User=', SYSTEM_USER);
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
                N'ManagedIdentity', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ManagedIdentity_List
-- Description:   Returns a paginated, soft-delete-filtered list of ManagedIdentity records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ManagedIdentity_List
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
                N'ManagedIdentity', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ManagedIdentityId, e.ArmResourceId, e.Name, e.Location, e.ProvisioningState, e.AzureTenantId, e.PrincipalId, e.ClientId, e.TagsJson, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Identity].[ManagedIdentity] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.ManagedIdentityId
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
                N'ManagedIdentity', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ManagedIdentity_Delete
-- Description:   Soft-deletes a ManagedIdentity record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ManagedIdentity_Delete
    @ManagedIdentityId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ManagedIdentityId IS NULL
            THROW 50001, N'ManagedIdentityId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Identity].[ManagedIdentity] WHERE ManagedIdentityId = @ManagedIdentityId AND ISNULL(IsDeleted,0) = 0)
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
                N'ManagedIdentity', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ManagedIdentityId=', @ManagedIdentityId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Identity].[ManagedIdentity]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ManagedIdentityId = @ManagedIdentityId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ManagedIdentityId=', @ManagedIdentityId, N'; User=', SYSTEM_USER);
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
                N'ManagedIdentity', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_RoleAssignment_Create
-- Description:   Inserts a new RoleAssignment record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_RoleAssignment_Create
    @RoleAssignmentId uniqueidentifier,
    @ArmResourceId nvarchar(500),
    @RoleDefinitionName nvarchar(200),
    @PrincipalId uniqueidentifier,
    @PrincipalType nvarchar(50),
    @RoleDefinitionId nvarchar(500),
    @Scope nvarchar(500),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoleAssignmentId IS NULL
            THROW 50001, N'RoleAssignmentId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RoleAssignment', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RoleAssignmentId=', @RoleAssignmentId, N'; ', N'ArmResourceId=', @ArmResourceId, N'; ', N'RoleDefinitionName=', @RoleDefinitionName), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Identity].[RoleAssignment] (RoleAssignmentId, ArmResourceId, RoleDefinitionName, PrincipalId, PrincipalType, RoleDefinitionId, Scope, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@RoleAssignmentId, @ArmResourceId, @RoleDefinitionName, @PrincipalId, @PrincipalType, @RoleDefinitionId, @Scope, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoleAssignmentId=', @RoleAssignmentId, N'; ', N'ArmResourceId=', @ArmResourceId, N'; ', N'RoleDefinitionName=', @RoleDefinitionName, N'; User=', SYSTEM_USER);
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
                N'RoleAssignment', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_RoleAssignment_Update
-- Description:   Updates an existing RoleAssignment record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_RoleAssignment_Update
    @RoleAssignmentId uniqueidentifier,
    @ArmResourceId nvarchar(500),
    @RoleDefinitionName nvarchar(200),
    @PrincipalId uniqueidentifier,
    @PrincipalType nvarchar(50),
    @RoleDefinitionId nvarchar(500),
    @Scope nvarchar(500),
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoleAssignmentId IS NULL
            THROW 50001, N'RoleAssignmentId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Identity].[RoleAssignment] WHERE RoleAssignmentId = @RoleAssignmentId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Identity].[RoleAssignment] WHERE RoleAssignmentId = @RoleAssignmentId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'RoleAssignment', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RoleAssignmentId=', @RoleAssignmentId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Identity].[RoleAssignment]
        SET
            ArmResourceId = @ArmResourceId,
            RoleDefinitionName = @RoleDefinitionName,
            PrincipalId = @PrincipalId,
            PrincipalType = @PrincipalType,
            RoleDefinitionId = @RoleDefinitionId,
            Scope = @Scope,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE RoleAssignmentId = @RoleAssignmentId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoleAssignmentId=', @RoleAssignmentId, N'; User=', SYSTEM_USER);
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
                N'RoleAssignment', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_RoleAssignment_GetById
-- Description:   Retrieves a single RoleAssignment record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_RoleAssignment_GetById
    @RoleAssignmentId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoleAssignmentId IS NULL
            THROW 50001, N'RoleAssignmentId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RoleAssignment', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'RoleAssignmentId=', @RoleAssignmentId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RoleAssignmentId, e.ArmResourceId, e.RoleDefinitionName, e.PrincipalId, e.PrincipalType, e.RoleDefinitionId, e.Scope, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Identity].[RoleAssignment] AS e
        WHERE e.RoleAssignmentId = @RoleAssignmentId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoleAssignmentId=', @RoleAssignmentId, N'; User=', SYSTEM_USER);
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
                N'RoleAssignment', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_RoleAssignment_List
-- Description:   Returns a paginated, soft-delete-filtered list of RoleAssignment records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_RoleAssignment_List
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
                N'RoleAssignment', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RoleAssignmentId, e.ArmResourceId, e.RoleDefinitionName, e.PrincipalId, e.PrincipalType, e.RoleDefinitionId, e.Scope, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Identity].[RoleAssignment] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.RoleAssignmentId
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
                N'RoleAssignment', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_RoleAssignment_Delete
-- Description:   Soft-deletes a RoleAssignment record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_RoleAssignment_Delete
    @RoleAssignmentId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoleAssignmentId IS NULL
            THROW 50001, N'RoleAssignmentId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Identity].[RoleAssignment] WHERE RoleAssignmentId = @RoleAssignmentId AND ISNULL(IsDeleted,0) = 0)
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
                N'RoleAssignment', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RoleAssignmentId=', @RoleAssignmentId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Identity].[RoleAssignment]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE RoleAssignmentId = @RoleAssignmentId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoleAssignmentId=', @RoleAssignmentId, N'; User=', SYSTEM_USER);
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
                N'RoleAssignment', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ServicePrincipal_Create
-- Description:   Inserts a new ServicePrincipal record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ServicePrincipal_Create
    @ServicePrincipalId uniqueidentifier,
    @EntraObjectId uniqueidentifier,
    @DisplayName nvarchar(200),
    @AppId nvarchar(100),
    @ServicePrincipalType nvarchar(50),
    @AccountEnabled bit,
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ServicePrincipalId IS NULL
            THROW 50001, N'ServicePrincipalId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ServicePrincipal', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ServicePrincipalId=', @ServicePrincipalId, N'; ', N'EntraObjectId=', @EntraObjectId, N'; ', N'DisplayName=', @DisplayName), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Identity].[ServicePrincipal] (ServicePrincipalId, EntraObjectId, DisplayName, AppId, ServicePrincipalType, AccountEnabled, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@ServicePrincipalId, @EntraObjectId, @DisplayName, @AppId, @ServicePrincipalType, @AccountEnabled, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ServicePrincipalId=', @ServicePrincipalId, N'; ', N'EntraObjectId=', @EntraObjectId, N'; ', N'DisplayName=', @DisplayName, N'; User=', SYSTEM_USER);
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
                N'ServicePrincipal', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ServicePrincipal_Update
-- Description:   Updates an existing ServicePrincipal record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ServicePrincipal_Update
    @ServicePrincipalId uniqueidentifier,
    @EntraObjectId uniqueidentifier,
    @DisplayName nvarchar(200),
    @AppId nvarchar(100),
    @ServicePrincipalType nvarchar(50),
    @AccountEnabled bit,
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ServicePrincipalId IS NULL
            THROW 50001, N'ServicePrincipalId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Identity].[ServicePrincipal] WHERE ServicePrincipalId = @ServicePrincipalId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Identity].[ServicePrincipal] WHERE ServicePrincipalId = @ServicePrincipalId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ServicePrincipal', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ServicePrincipalId=', @ServicePrincipalId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Identity].[ServicePrincipal]
        SET
            EntraObjectId = @EntraObjectId,
            DisplayName = @DisplayName,
            AppId = @AppId,
            ServicePrincipalType = @ServicePrincipalType,
            AccountEnabled = @AccountEnabled,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ServicePrincipalId = @ServicePrincipalId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ServicePrincipalId=', @ServicePrincipalId, N'; User=', SYSTEM_USER);
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
                N'ServicePrincipal', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ServicePrincipal_GetById
-- Description:   Retrieves a single ServicePrincipal record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ServicePrincipal_GetById
    @ServicePrincipalId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ServicePrincipalId IS NULL
            THROW 50001, N'ServicePrincipalId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ServicePrincipal', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ServicePrincipalId=', @ServicePrincipalId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ServicePrincipalId, e.EntraObjectId, e.DisplayName, e.AppId, e.ServicePrincipalType, e.AccountEnabled, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Identity].[ServicePrincipal] AS e
        WHERE e.ServicePrincipalId = @ServicePrincipalId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ServicePrincipalId=', @ServicePrincipalId, N'; User=', SYSTEM_USER);
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
                N'ServicePrincipal', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ServicePrincipal_List
-- Description:   Returns a paginated, soft-delete-filtered list of ServicePrincipal records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ServicePrincipal_List
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
                N'ServicePrincipal', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ServicePrincipalId, e.EntraObjectId, e.DisplayName, e.AppId, e.ServicePrincipalType, e.AccountEnabled, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Identity].[ServicePrincipal] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.ServicePrincipalId
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
                N'ServicePrincipal', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
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
-- Procedure:     [Identity].sp_ServicePrincipal_Delete
-- Description:   Soft-deletes a ServicePrincipal record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Identity].sp_ServicePrincipal_Delete
    @ServicePrincipalId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ServicePrincipalId IS NULL
            THROW 50001, N'ServicePrincipalId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Identity].[ServicePrincipal] WHERE ServicePrincipalId = @ServicePrincipalId AND ISNULL(IsDeleted,0) = 0)
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
                N'ServicePrincipal', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ServicePrincipalId=', @ServicePrincipalId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Identity].[ServicePrincipal]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ServicePrincipalId = @ServicePrincipalId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ServicePrincipalId=', @ServicePrincipalId, N'; User=', SYSTEM_USER);
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
                N'ServicePrincipal', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
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
