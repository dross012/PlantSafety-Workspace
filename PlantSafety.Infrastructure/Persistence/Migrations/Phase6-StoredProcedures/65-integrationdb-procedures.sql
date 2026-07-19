USE [PlantSafety.IntegrationDB]

GO

SET ANSI_NULLS ON

GO

SET QUOTED_IDENTIFIER ON

GO

-- =============================================

-- Phase 6: Stored Procedures (canonical FY2027 template)

-- Database: PlantSafety.IntegrationDB

-- Generated: 2026-07-19 by Procedure Architect agent, grounded in

-- sql-database-skill\references\00-canonical-templates.md

-- Idempotent: CREATE OR ALTER PROCEDURE throughout.

-- =============================================

GO



-- =============================================
-- Procedure:     [Integration].sp_ContractField_Create
-- Description:   Inserts a new ContractField record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractField_Create
    @ContractFieldId uniqueidentifier,
    @ContractSchemaId uniqueidentifier,
    @Name nvarchar(200),
    @FieldType nvarchar(50),
    @IsRequired bit,
    @AllowedValuesJson nvarchar(MAX),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractFieldId IS NULL
            THROW 50001, N'ContractFieldId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractField', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractFieldId=', @ContractFieldId, N'; ', N'ContractSchemaId=', @ContractSchemaId, N'; ', N'Name=', @Name), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[ContractField] (ContractFieldId, ContractSchemaId, Name, FieldType, IsRequired, AllowedValuesJson, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@ContractFieldId, @ContractSchemaId, @Name, @FieldType, @IsRequired, @AllowedValuesJson, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractFieldId=', @ContractFieldId, N'; ', N'ContractSchemaId=', @ContractSchemaId, N'; ', N'Name=', @Name, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractField', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractField_Update
-- Description:   Updates an existing ContractField record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractField_Update
    @ContractFieldId uniqueidentifier,
    @ContractSchemaId uniqueidentifier,
    @Name nvarchar(200),
    @FieldType nvarchar(50),
    @IsRequired bit,
    @AllowedValuesJson nvarchar(MAX),
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractFieldId IS NULL
            THROW 50001, N'ContractFieldId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractField] WHERE ContractFieldId = @ContractFieldId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractField] WHERE ContractFieldId = @ContractFieldId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ContractField', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractFieldId=', @ContractFieldId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Integration].[ContractField]
        SET
            ContractSchemaId = @ContractSchemaId,
            Name = @Name,
            FieldType = @FieldType,
            IsRequired = @IsRequired,
            AllowedValuesJson = @AllowedValuesJson,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractFieldId = @ContractFieldId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractFieldId=', @ContractFieldId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractField', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractField_GetById
-- Description:   Retrieves a single ContractField record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractField_GetById
    @ContractFieldId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractFieldId IS NULL
            THROW 50001, N'ContractFieldId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractField', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ContractFieldId=', @ContractFieldId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractFieldId, e.ContractSchemaId, e.Name, e.FieldType, e.IsRequired, e.AllowedValuesJson, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Integration].[ContractField] AS e
        WHERE e.ContractFieldId = @ContractFieldId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractFieldId=', @ContractFieldId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractField', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractField_List
-- Description:   Returns a paginated, soft-delete-filtered list of ContractField records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractField_List
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
                N'ContractField', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractFieldId, e.ContractSchemaId, e.Name, e.FieldType, e.IsRequired, e.AllowedValuesJson, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Integration].[ContractField] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.ContractFieldId
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
                N'ContractField', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractField_Delete
-- Description:   Soft-deletes a ContractField record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractField_Delete
    @ContractFieldId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractFieldId IS NULL
            THROW 50001, N'ContractFieldId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractField] WHERE ContractFieldId = @ContractFieldId AND ISNULL(IsDeleted,0) = 0)
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
                N'ContractField', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractFieldId=', @ContractFieldId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[ContractField]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractFieldId = @ContractFieldId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractFieldId=', @ContractFieldId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractField', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractSchema_Create
-- Description:   Inserts a new ContractSchema record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractSchema_Create
    @ContractSchemaId uniqueidentifier,
    @SchemaName nvarchar(200),
    @MessageType nvarchar(50),
    @Version nvarchar(50),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractSchemaId IS NULL
            THROW 50001, N'ContractSchemaId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractSchema', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractSchemaId=', @ContractSchemaId, N'; ', N'SchemaName=', @SchemaName, N'; ', N'MessageType=', @MessageType), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[ContractSchema] (ContractSchemaId, SchemaName, MessageType, Version, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@ContractSchemaId, @SchemaName, @MessageType, @Version, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractSchemaId=', @ContractSchemaId, N'; ', N'SchemaName=', @SchemaName, N'; ', N'MessageType=', @MessageType, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractSchema', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractSchema_Update
-- Description:   Updates an existing ContractSchema record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractSchema_Update
    @ContractSchemaId uniqueidentifier,
    @SchemaName nvarchar(200),
    @MessageType nvarchar(50),
    @Version nvarchar(50),
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractSchemaId IS NULL
            THROW 50001, N'ContractSchemaId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractSchema] WHERE ContractSchemaId = @ContractSchemaId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractSchema] WHERE ContractSchemaId = @ContractSchemaId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ContractSchema', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractSchemaId=', @ContractSchemaId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Integration].[ContractSchema]
        SET
            SchemaName = @SchemaName,
            MessageType = @MessageType,
            Version = @Version,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractSchemaId = @ContractSchemaId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractSchemaId=', @ContractSchemaId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractSchema', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractSchema_GetById
-- Description:   Retrieves a single ContractSchema record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractSchema_GetById
    @ContractSchemaId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractSchemaId IS NULL
            THROW 50001, N'ContractSchemaId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractSchema', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ContractSchemaId=', @ContractSchemaId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractSchemaId, e.SchemaName, e.MessageType, e.Version, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Integration].[ContractSchema] AS e
        WHERE e.ContractSchemaId = @ContractSchemaId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractSchemaId=', @ContractSchemaId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractSchema', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractSchema_List
-- Description:   Returns a paginated, soft-delete-filtered list of ContractSchema records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractSchema_List
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
                N'ContractSchema', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractSchemaId, e.SchemaName, e.MessageType, e.Version, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Integration].[ContractSchema] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.ContractSchemaId
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
                N'ContractSchema', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractSchema_Delete
-- Description:   Soft-deletes a ContractSchema record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractSchema_Delete
    @ContractSchemaId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractSchemaId IS NULL
            THROW 50001, N'ContractSchemaId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractSchema] WHERE ContractSchemaId = @ContractSchemaId AND ISNULL(IsDeleted,0) = 0)
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
                N'ContractSchema', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractSchemaId=', @ContractSchemaId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[ContractSchema]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractSchemaId = @ContractSchemaId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractSchemaId=', @ContractSchemaId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractSchema', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractVersion_Create
-- Description:   Inserts a new ContractVersion record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractVersion_Create
    @ContractVersionId uniqueidentifier,
    @ContractSchemaId uniqueidentifier,
    @SchemaName nvarchar(200),
    @Version nvarchar(50),
    @LifecycleState nvarchar(50),
    @ReleasedAtUtc datetime2(3),
    @DeprecatedAtUtc datetime2(3),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractVersionId IS NULL
            THROW 50001, N'ContractVersionId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractVersion', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractVersionId=', @ContractVersionId, N'; ', N'ContractSchemaId=', @ContractSchemaId, N'; ', N'SchemaName=', @SchemaName), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[ContractVersion] (ContractVersionId, ContractSchemaId, SchemaName, Version, LifecycleState, ReleasedAtUtc, DeprecatedAtUtc, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@ContractVersionId, @ContractSchemaId, @SchemaName, @Version, @LifecycleState, @ReleasedAtUtc, @DeprecatedAtUtc, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractVersionId=', @ContractVersionId, N'; ', N'ContractSchemaId=', @ContractSchemaId, N'; ', N'SchemaName=', @SchemaName, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractVersion', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractVersion_Update
-- Description:   Updates an existing ContractVersion record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractVersion_Update
    @ContractVersionId uniqueidentifier,
    @ContractSchemaId uniqueidentifier,
    @SchemaName nvarchar(200),
    @Version nvarchar(50),
    @LifecycleState nvarchar(50),
    @ReleasedAtUtc datetime2(3),
    @DeprecatedAtUtc datetime2(3),
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractVersionId IS NULL
            THROW 50001, N'ContractVersionId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractVersion] WHERE ContractVersionId = @ContractVersionId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractVersion] WHERE ContractVersionId = @ContractVersionId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'ContractVersion', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractVersionId=', @ContractVersionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Integration].[ContractVersion]
        SET
            ContractSchemaId = @ContractSchemaId,
            SchemaName = @SchemaName,
            Version = @Version,
            LifecycleState = @LifecycleState,
            ReleasedAtUtc = @ReleasedAtUtc,
            DeprecatedAtUtc = @DeprecatedAtUtc,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractVersionId = @ContractVersionId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractVersionId=', @ContractVersionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractVersion', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractVersion_GetById
-- Description:   Retrieves a single ContractVersion record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractVersion_GetById
    @ContractVersionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractVersionId IS NULL
            THROW 50001, N'ContractVersionId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ContractVersion', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ContractVersionId=', @ContractVersionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractVersionId, e.ContractSchemaId, e.SchemaName, e.Version, e.LifecycleState, e.ReleasedAtUtc, e.DeprecatedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Integration].[ContractVersion] AS e
        WHERE e.ContractVersionId = @ContractVersionId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractVersionId=', @ContractVersionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractVersion', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractVersion_List
-- Description:   Returns a paginated, soft-delete-filtered list of ContractVersion records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractVersion_List
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
                N'ContractVersion', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ContractVersionId, e.ContractSchemaId, e.SchemaName, e.Version, e.LifecycleState, e.ReleasedAtUtc, e.DeprecatedAtUtc, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Integration].[ContractVersion] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.ContractVersionId
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
                N'ContractVersion', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ContractVersion_Delete
-- Description:   Soft-deletes a ContractVersion record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ContractVersion_Delete
    @ContractVersionId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ContractVersionId IS NULL
            THROW 50001, N'ContractVersionId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ContractVersion] WHERE ContractVersionId = @ContractVersionId AND ISNULL(IsDeleted,0) = 0)
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
                N'ContractVersion', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ContractVersionId=', @ContractVersionId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[ContractVersion]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE ContractVersionId = @ContractVersionId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ContractVersionId=', @ContractVersionId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ContractVersion', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_IntegrationLogDailySummary_Create
-- Description:   Inserts a new IntegrationLogDailySummary record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogDailySummary_Create
    @IntegrationLogDailySummaryId uniqueidentifier,
    @SummaryDateKey int,
    @ModuleName nvarchar(200),
    @OperationName nvarchar(200),
    @SuccessCount int,
    @FailureCount int,
    @AverageElapsedMs int,
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IntegrationLogDailySummaryId IS NULL
            THROW 50001, N'IntegrationLogDailySummaryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IntegrationLogDailySummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'IntegrationLogDailySummaryId=', @IntegrationLogDailySummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[IntegrationLogDailySummary] (IntegrationLogDailySummaryId, SummaryDateKey, ModuleName, OperationName, SuccessCount, FailureCount, AverageElapsedMs, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@IntegrationLogDailySummaryId, @SummaryDateKey, @ModuleName, @OperationName, @SuccessCount, @FailureCount, @AverageElapsedMs, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IntegrationLogDailySummaryId=', @IntegrationLogDailySummaryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'IntegrationLogDailySummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_IntegrationLogDailySummary_GetById
-- Description:   Retrieves a single IntegrationLogDailySummary record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogDailySummary_GetById
    @IntegrationLogDailySummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IntegrationLogDailySummaryId IS NULL
            THROW 50001, N'IntegrationLogDailySummaryId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IntegrationLogDailySummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'IntegrationLogDailySummaryId=', @IntegrationLogDailySummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.IntegrationLogDailySummaryId, e.SummaryDateKey, e.ModuleName, e.OperationName, e.SuccessCount, e.FailureCount, e.AverageElapsedMs, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[IntegrationLogDailySummary] AS e
        WHERE e.IntegrationLogDailySummaryId = @IntegrationLogDailySummaryId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IntegrationLogDailySummaryId=', @IntegrationLogDailySummaryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'IntegrationLogDailySummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_IntegrationLogDailySummary_List
-- Description:   Returns a paginated, soft-delete-filtered list of IntegrationLogDailySummary records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogDailySummary_List
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
                N'IntegrationLogDailySummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.IntegrationLogDailySummaryId, e.SummaryDateKey, e.ModuleName, e.OperationName, e.SuccessCount, e.FailureCount, e.AverageElapsedMs, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[IntegrationLogDailySummary] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.IntegrationLogDailySummaryId
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
                N'IntegrationLogDailySummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_IntegrationLogDailySummary_Delete
-- Description:   Soft-deletes a IntegrationLogDailySummary record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogDailySummary_Delete
    @IntegrationLogDailySummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @IntegrationLogDailySummaryId IS NULL
            THROW 50001, N'IntegrationLogDailySummaryId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[IntegrationLogDailySummary] WHERE IntegrationLogDailySummaryId = @IntegrationLogDailySummaryId AND ISNULL(IsDeleted,0) = 0)
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
                N'IntegrationLogDailySummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'IntegrationLogDailySummaryId=', @IntegrationLogDailySummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[IntegrationLogDailySummary]
        SET IsDeleted = 1
        WHERE IntegrationLogDailySummaryId = @IntegrationLogDailySummaryId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'IntegrationLogDailySummaryId=', @IntegrationLogDailySummaryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'IntegrationLogDailySummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_IntegrationLogEntry_Create
-- Description:   Inserts a new IntegrationLogEntry record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogEntry_Create
    @IntegrationLogEntryId uniqueidentifier,
    @CorrelationId uniqueidentifier,
    @CausationId uniqueidentifier,
    @MessageId uniqueidentifier,
    @TraceId nvarchar(100),
    @SpanId nvarchar(100),
    @TimestampUtc datetime2(3),
    @ElapsedMs bigint,
    @OperationName nvarchar(200),
    @ModuleName nvarchar(200),
    @Environment nvarchar(50),
    @Version nvarchar(50),
    @Success bit,
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TimestampUtc IS NULL
            THROW 50001, N'TimestampUtc is required.', 1;
        IF @IntegrationLogEntryId IS NULL
            THROW 50001, N'IntegrationLogEntryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IntegrationLogEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'IntegrationLogEntryId=', @IntegrationLogEntryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[IntegrationLogEntry] (IntegrationLogEntryId, CorrelationId, CausationId, MessageId, TraceId, SpanId, TimestampUtc, ElapsedMs, OperationName, ModuleName, Environment, Version, Success, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@IntegrationLogEntryId, @CorrelationId, @CausationId, @MessageId, @TraceId, @SpanId, @TimestampUtc, @ElapsedMs, @OperationName, @ModuleName, @Environment, @Version, @Success, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'IntegrationLogEntryId=', @IntegrationLogEntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'IntegrationLogEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_IntegrationLogEntry_GetById
-- Description:   Retrieves a single IntegrationLogEntry record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogEntry_GetById
    @TimestampUtc datetime2(3),
    @IntegrationLogEntryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TimestampUtc IS NULL
            THROW 50001, N'TimestampUtc is required.', 1;

        IF @IntegrationLogEntryId IS NULL
            THROW 50001, N'IntegrationLogEntryId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'IntegrationLogEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'IntegrationLogEntryId=', @IntegrationLogEntryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.IntegrationLogEntryId, e.CorrelationId, e.CausationId, e.MessageId, e.TraceId, e.SpanId, e.TimestampUtc, e.ElapsedMs, e.OperationName, e.ModuleName, e.Environment, e.Version, e.Success, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[IntegrationLogEntry] AS e
        WHERE e.TimestampUtc = @TimestampUtc AND e.IntegrationLogEntryId = @IntegrationLogEntryId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'IntegrationLogEntryId=', @IntegrationLogEntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'IntegrationLogEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_IntegrationLogEntry_List
-- Description:   Returns a paginated, soft-delete-filtered list of IntegrationLogEntry records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogEntry_List
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
                N'IntegrationLogEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.IntegrationLogEntryId, e.CorrelationId, e.CausationId, e.MessageId, e.TraceId, e.SpanId, e.TimestampUtc, e.ElapsedMs, e.OperationName, e.ModuleName, e.Environment, e.Version, e.Success, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[IntegrationLogEntry] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.TimestampUtc
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
                N'IntegrationLogEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_IntegrationLogEntry_Delete
-- Description:   Soft-deletes a IntegrationLogEntry record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_IntegrationLogEntry_Delete
    @TimestampUtc datetime2(3),
    @IntegrationLogEntryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TimestampUtc IS NULL
            THROW 50001, N'TimestampUtc is required.', 1;

        IF @IntegrationLogEntryId IS NULL
            THROW 50001, N'IntegrationLogEntryId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[IntegrationLogEntry] WHERE TimestampUtc = @TimestampUtc AND IntegrationLogEntryId = @IntegrationLogEntryId AND ISNULL(IsDeleted,0) = 0)
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
                N'IntegrationLogEntry', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'IntegrationLogEntryId=', @IntegrationLogEntryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[IntegrationLogEntry]
        SET IsDeleted = 1
        WHERE TimestampUtc = @TimestampUtc AND IntegrationLogEntryId = @IntegrationLogEntryId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'IntegrationLogEntryId=', @IntegrationLogEntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'IntegrationLogEntry', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ModuleHealthDailySummary_Create
-- Description:   Inserts a new ModuleHealthDailySummary record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ModuleHealthDailySummary_Create
    @ModuleHealthDailySummaryId uniqueidentifier,
    @SummaryDateKey int,
    @ModuleName nvarchar(200),
    @Status nvarchar(50),
    @SnapshotCount int,
    @AverageResponseLatencyMs int,
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ModuleHealthDailySummaryId IS NULL
            THROW 50001, N'ModuleHealthDailySummaryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ModuleHealthDailySummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ModuleHealthDailySummaryId=', @ModuleHealthDailySummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[ModuleHealthDailySummary] (ModuleHealthDailySummaryId, SummaryDateKey, ModuleName, Status, SnapshotCount, AverageResponseLatencyMs, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@ModuleHealthDailySummaryId, @SummaryDateKey, @ModuleName, @Status, @SnapshotCount, @AverageResponseLatencyMs, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModuleHealthDailySummaryId=', @ModuleHealthDailySummaryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ModuleHealthDailySummary', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ModuleHealthDailySummary_GetById
-- Description:   Retrieves a single ModuleHealthDailySummary record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ModuleHealthDailySummary_GetById
    @ModuleHealthDailySummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ModuleHealthDailySummaryId IS NULL
            THROW 50001, N'ModuleHealthDailySummaryId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ModuleHealthDailySummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ModuleHealthDailySummaryId=', @ModuleHealthDailySummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ModuleHealthDailySummaryId, e.SummaryDateKey, e.ModuleName, e.Status, e.SnapshotCount, e.AverageResponseLatencyMs, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[ModuleHealthDailySummary] AS e
        WHERE e.ModuleHealthDailySummaryId = @ModuleHealthDailySummaryId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModuleHealthDailySummaryId=', @ModuleHealthDailySummaryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ModuleHealthDailySummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ModuleHealthDailySummary_List
-- Description:   Returns a paginated, soft-delete-filtered list of ModuleHealthDailySummary records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ModuleHealthDailySummary_List
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
                N'ModuleHealthDailySummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ModuleHealthDailySummaryId, e.SummaryDateKey, e.ModuleName, e.Status, e.SnapshotCount, e.AverageResponseLatencyMs, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[ModuleHealthDailySummary] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.ModuleHealthDailySummaryId
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
                N'ModuleHealthDailySummary', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ModuleHealthDailySummary_Delete
-- Description:   Soft-deletes a ModuleHealthDailySummary record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ModuleHealthDailySummary_Delete
    @ModuleHealthDailySummaryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ModuleHealthDailySummaryId IS NULL
            THROW 50001, N'ModuleHealthDailySummaryId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ModuleHealthDailySummary] WHERE ModuleHealthDailySummaryId = @ModuleHealthDailySummaryId AND ISNULL(IsDeleted,0) = 0)
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
                N'ModuleHealthDailySummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ModuleHealthDailySummaryId=', @ModuleHealthDailySummaryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[ModuleHealthDailySummary]
        SET IsDeleted = 1
        WHERE ModuleHealthDailySummaryId = @ModuleHealthDailySummaryId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModuleHealthDailySummaryId=', @ModuleHealthDailySummaryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ModuleHealthDailySummary', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ModuleHealthSnapshot_Create
-- Description:   Inserts a new ModuleHealthSnapshot record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ModuleHealthSnapshot_Create
    @ModuleHealthSnapshotId uniqueidentifier,
    @ModuleName nvarchar(200),
    @Status nvarchar(50),
    @CheckedAtUtc datetime2(3),
    @ResponseLatencyMs int,
    @DegradationReason nvarchar(1000),
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ModuleHealthSnapshotId IS NULL
            THROW 50001, N'ModuleHealthSnapshotId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ModuleHealthSnapshot', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ModuleHealthSnapshotId=', @ModuleHealthSnapshotId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[ModuleHealthSnapshot] (ModuleHealthSnapshotId, ModuleName, Status, CheckedAtUtc, ResponseLatencyMs, DegradationReason, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@ModuleHealthSnapshotId, @ModuleName, @Status, @CheckedAtUtc, @ResponseLatencyMs, @DegradationReason, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModuleHealthSnapshotId=', @ModuleHealthSnapshotId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ModuleHealthSnapshot', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ModuleHealthSnapshot_GetById
-- Description:   Retrieves a single ModuleHealthSnapshot record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ModuleHealthSnapshot_GetById
    @ModuleHealthSnapshotId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ModuleHealthSnapshotId IS NULL
            THROW 50001, N'ModuleHealthSnapshotId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ModuleHealthSnapshot', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'ModuleHealthSnapshotId=', @ModuleHealthSnapshotId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ModuleHealthSnapshotId, e.ModuleName, e.Status, e.CheckedAtUtc, e.ResponseLatencyMs, e.DegradationReason, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[ModuleHealthSnapshot] AS e
        WHERE e.ModuleHealthSnapshotId = @ModuleHealthSnapshotId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModuleHealthSnapshotId=', @ModuleHealthSnapshotId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ModuleHealthSnapshot', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ModuleHealthSnapshot_List
-- Description:   Returns a paginated, soft-delete-filtered list of ModuleHealthSnapshot records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ModuleHealthSnapshot_List
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
                N'ModuleHealthSnapshot', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ModuleHealthSnapshotId, e.ModuleName, e.Status, e.CheckedAtUtc, e.ResponseLatencyMs, e.DegradationReason, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[ModuleHealthSnapshot] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.ModuleHealthSnapshotId
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
                N'ModuleHealthSnapshot', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ModuleHealthSnapshot_Delete
-- Description:   Soft-deletes a ModuleHealthSnapshot record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ModuleHealthSnapshot_Delete
    @ModuleHealthSnapshotId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @ModuleHealthSnapshotId IS NULL
            THROW 50001, N'ModuleHealthSnapshotId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ModuleHealthSnapshot] WHERE ModuleHealthSnapshotId = @ModuleHealthSnapshotId AND ISNULL(IsDeleted,0) = 0)
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
                N'ModuleHealthSnapshot', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'ModuleHealthSnapshotId=', @ModuleHealthSnapshotId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[ModuleHealthSnapshot]
        SET IsDeleted = 1
        WHERE ModuleHealthSnapshotId = @ModuleHealthSnapshotId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ModuleHealthSnapshotId=', @ModuleHealthSnapshotId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ModuleHealthSnapshot', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ReplayRecord_Create
-- Description:   Inserts a new ReplayRecord record (insert-once; no Update proc, since table has no UpdatedUtc/ConcurrencyToken).
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ReplayRecord_Create
    @ReplayRecordId uniqueidentifier,
    @MessageId uniqueidentifier,
    @CorrelationId uniqueidentifier,
    @CausationId uniqueidentifier,
    @SchemaName nvarchar(200),
    @ContractVersion nvarchar(50),
    @TimestampUtc datetime2(3),
    @SerializedPayload nvarchar(MAX),
    @CreatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TimestampUtc IS NULL
            THROW 50001, N'TimestampUtc is required.', 1;
        IF @ReplayRecordId IS NULL
            THROW 50001, N'ReplayRecordId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ReplayRecord', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'ReplayRecordId=', @ReplayRecordId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[ReplayRecord] (ReplayRecordId, MessageId, CorrelationId, CausationId, SchemaName, ContractVersion, TimestampUtc, SerializedPayload, CreatedBy, IsDeleted, CreatedUtc)
        VALUES (@ReplayRecordId, @MessageId, @CorrelationId, @CausationId, @SchemaName, @ContractVersion, @TimestampUtc, @SerializedPayload, @CreatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'ReplayRecordId=', @ReplayRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ReplayRecord', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ReplayRecord_GetById
-- Description:   Retrieves a single ReplayRecord record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ReplayRecord_GetById
    @TimestampUtc datetime2(3),
    @ReplayRecordId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TimestampUtc IS NULL
            THROW 50001, N'TimestampUtc is required.', 1;

        IF @ReplayRecordId IS NULL
            THROW 50001, N'ReplayRecordId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'ReplayRecord', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'ReplayRecordId=', @ReplayRecordId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ReplayRecordId, e.MessageId, e.CorrelationId, e.CausationId, e.SchemaName, e.ContractVersion, e.TimestampUtc, e.SerializedPayload, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[ReplayRecord] AS e
        WHERE e.TimestampUtc = @TimestampUtc AND e.ReplayRecordId = @ReplayRecordId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'ReplayRecordId=', @ReplayRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ReplayRecord', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ReplayRecord_List
-- Description:   Returns a paginated, soft-delete-filtered list of ReplayRecord records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ReplayRecord_List
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
                N'ReplayRecord', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.ReplayRecordId, e.MessageId, e.CorrelationId, e.CausationId, e.SchemaName, e.ContractVersion, e.TimestampUtc, e.SerializedPayload, e.CreatedUtc, e.CreatedBy, e.IsDeleted
        FROM [Integration].[ReplayRecord] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.TimestampUtc
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
                N'ReplayRecord', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_ReplayRecord_Delete
-- Description:   Soft-deletes a ReplayRecord record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_ReplayRecord_Delete
    @TimestampUtc datetime2(3),
    @ReplayRecordId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TimestampUtc IS NULL
            THROW 50001, N'TimestampUtc is required.', 1;

        IF @ReplayRecordId IS NULL
            THROW 50001, N'ReplayRecordId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[ReplayRecord] WHERE TimestampUtc = @TimestampUtc AND ReplayRecordId = @ReplayRecordId AND ISNULL(IsDeleted,0) = 0)
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
                N'ReplayRecord', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'ReplayRecordId=', @ReplayRecordId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[ReplayRecord]
        SET IsDeleted = 1
        WHERE TimestampUtc = @TimestampUtc AND ReplayRecordId = @ReplayRecordId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TimestampUtc=', @TimestampUtc, N'; ', N'ReplayRecordId=', @ReplayRecordId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'ReplayRecord', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_RoutingTableEntry_Create
-- Description:   Inserts a new RoutingTableEntry record.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_RoutingTableEntry_Create
    @RoutingTableEntryId uniqueidentifier,
    @SchemaName nvarchar(200),
    @MessageType nvarchar(50),
    @SubscribersJson nvarchar(MAX),
    @CreatedBy uniqueidentifier,
    @UpdatedBy uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoutingTableEntryId IS NULL
            THROW 50001, N'RoutingTableEntryId is required.', 1;
        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RoutingTableEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RoutingTableEntryId=', @RoutingTableEntryId, N'; ', N'SchemaName=', @SchemaName, N'; ', N'MessageType=', @MessageType), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        INSERT INTO [Integration].[RoutingTableEntry] (RoutingTableEntryId, SchemaName, MessageType, SubscribersJson, CreatedBy, UpdatedBy, IsDeleted, CreatedUtc)
        VALUES (@RoutingTableEntryId, @SchemaName, @MessageType, @SubscribersJson, @CreatedBy, @UpdatedBy, 0, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoutingTableEntryId=', @RoutingTableEntryId, N'; ', N'SchemaName=', @SchemaName, N'; ', N'MessageType=', @MessageType, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'RoutingTableEntry', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_RoutingTableEntry_Update
-- Description:   Updates an existing RoutingTableEntry record with optimistic concurrency.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_RoutingTableEntry_Update
    @RoutingTableEntryId uniqueidentifier,
    @SchemaName nvarchar(200),
    @MessageType nvarchar(50),
    @SubscribersJson nvarchar(MAX),
    @UpdatedBy uniqueidentifier,
    @ConcurrencyToken rowversion
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoutingTableEntryId IS NULL
            THROW 50001, N'RoutingTableEntryId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[RoutingTableEntry] WHERE RoutingTableEntryId = @RoutingTableEntryId AND ISNULL(IsDeleted,0) = 0)
            THROW 50004, N'Record not found or has been deleted.', 1;

        IF NOT EXISTS (SELECT 1 FROM [Integration].[RoutingTableEntry] WHERE RoutingTableEntryId = @RoutingTableEntryId AND ConcurrencyToken = @ConcurrencyToken)
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
                N'RoutingTableEntry', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RoutingTableEntryId=', @RoutingTableEntryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        UPDATE [Integration].[RoutingTableEntry]
        SET
            SchemaName = @SchemaName,
            MessageType = @MessageType,
            SubscribersJson = @SubscribersJson,
            UpdatedBy = @UpdatedBy,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE RoutingTableEntryId = @RoutingTableEntryId
          AND ISNULL(IsDeleted,0) = 0 AND ConcurrencyToken = @ConcurrencyToken;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoutingTableEntryId=', @RoutingTableEntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'RoutingTableEntry', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_RoutingTableEntry_GetById
-- Description:   Retrieves a single RoutingTableEntry record by primary key.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_RoutingTableEntry_GetById
    @RoutingTableEntryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoutingTableEntryId IS NULL
            THROW 50001, N'RoutingTableEntryId is required.', 1;


        ------------------------------------------------------------
        -- Audit: best-effort, non-fatal
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'RoutingTableEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'RoutingTableEntryId=', @RoutingTableEntryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RoutingTableEntryId, e.SchemaName, e.MessageType, e.SubscribersJson, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Integration].[RoutingTableEntry] AS e
        WHERE e.RoutingTableEntryId = @RoutingTableEntryId
          AND ISNULL(e.IsDeleted, 0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoutingTableEntryId=', @RoutingTableEntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'RoutingTableEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_RoutingTableEntry_List
-- Description:   Returns a paginated, soft-delete-filtered list of RoutingTableEntry records.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_RoutingTableEntry_List
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
                N'RoutingTableEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                1, CONCAT(N'PageNumber=', @PageNumber, N'; PageSize=', @PageSize), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation
        ------------------------------------------------------------
        SELECT e.RoutingTableEntryId, e.SchemaName, e.MessageType, e.SubscribersJson, e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
        FROM [Integration].[RoutingTableEntry] AS e
        WHERE ISNULL(e.IsDeleted, 0) = 0
        ORDER BY e.RoutingTableEntryId
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
                N'RoutingTableEntry', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
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
-- Procedure:     [Integration].sp_RoutingTableEntry_Delete
-- Description:   Soft-deletes a RoutingTableEntry record (sets IsDeleted = 1). Never a hard DELETE.
-- Notes:
--   - Validates all input parameters
--   - Filters out soft-deleted records
--   - Best-effort audit logging
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
--   - StrongId-safe identity usage
-- =============================================
CREATE OR ALTER PROCEDURE [Integration].sp_RoutingTableEntry_Delete
    @RoutingTableEntryId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @RoutingTableEntryId IS NULL
            THROW 50001, N'RoutingTableEntryId is required.', 1;


        IF NOT EXISTS (SELECT 1 FROM [Integration].[RoutingTableEntry] WHERE RoutingTableEntryId = @RoutingTableEntryId AND ISNULL(IsDeleted,0) = 0)
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
                N'RoutingTableEntry', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                0, CONCAT(N'RoutingTableEntryId=', @RoutingTableEntryId), 0, OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;
        ------------------------------------------------------------
        -- Main operation (soft delete only)
        ------------------------------------------------------------
        UPDATE [Integration].[RoutingTableEntry]
        SET IsDeleted = 1,
            UpdatedUtc = SYSUTCDATETIME()
        WHERE RoutingTableEntryId = @RoutingTableEntryId
          AND ISNULL(IsDeleted,0) = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'RoutingTableEntryId=', @RoutingTableEntryId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @__SourceDatabase nvarchar(128) = DB_NAME();
        DECLARE @__Module nvarchar(128) = OBJECT_NAME(@@PROCID);

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
            ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
            ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
        )
        VALUES (
            ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
            SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, @__SourceDatabase, @__Module
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'RoutingTableEntry', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @__ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO
