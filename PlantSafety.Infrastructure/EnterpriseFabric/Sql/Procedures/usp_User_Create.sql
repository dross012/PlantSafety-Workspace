-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.EnterpriseFabric.Users
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_User_Create.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Inserts a new [EnterpriseFabric].[User] row. Backs IUserRepository.CreateAsync /
--                  UserDapperRepository.CreateAsync. Parameter set matches
--                  StoredProcedureBuilder.WithCanonicalHeaders (@TenantId/@CorrelationId/@ActorId/
--                  @NowUtc) plus the explicit Add* calls in UserDapperRepository.CreateAsync
--                  exactly (@UserId/@Email/@FirstName/@LastName/@Roles/@SiteIds -- the latter two
--                  already comma-joined by the repository before the call). New rows start
--                  IsActive = 1 (no Deactivate/Reactivate endpoint exists yet per
--                  UsersV1Controller's own header comment). TRY/CATCH with AuditDB/ErrorDB logging
--                  per the established pattern (mirrors usp_CorrectiveAction_Create). Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [EnterpriseFabric].[usp_User_Create]
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2(3),
    @UserId        UNIQUEIDENTIFIER,
    @Email         NVARCHAR(320),
    @FirstName     NVARCHAR(100),
    @LastName      NVARCHAR(100),
    @Roles         NVARCHAR(4000) = NULL,
    @SiteIds       NVARCHAR(4000) = NULL
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        INSERT INTO [EnterpriseFabric].[User]
        (
            UserId, TenantId, Email, FirstName, LastName, RolesCsv, SiteIdsCsv,
            IsActive, CreatedAtUtc, CreatedBy, LastModifiedAtUtc
        )
        VALUES
        (
            @UserId, @TenantId, @Email, @FirstName, @LastName, @Roles, @SiteIds,
            1, @NowUtc, @ActorId, @NowUtc
        );

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'User', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'UserId=', @UserId, N'; CorrelationId=', @CorrelationId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'UserId=', @UserId, N'; CorrelationId=', @CorrelationId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
