-- =============================================
-- Shared.SafetyData -- Phase6-MultiFacilityTenant
-- Script:        20-usp-tenantfacilitymap.sql
-- Purpose:       usp_TenantFacilityMap_Bind / Unbind / GetByTenantId.
-- Idempotent: CREATE OR ALTER. Safe to re-run.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [TenantMap].[usp_TenantFacilityMap_Bind]
    @TenantId            nvarchar(100),
    @FacilityId           uniqueidentifier,
    @IsPrimaryFacility    bit = 0,
    @BoundAtUtc           datetime2(3)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TenantId IS NULL OR LTRIM(RTRIM(@TenantId)) = N''
            THROW 50001, N'TenantId is required.', 1;
        IF @FacilityId IS NULL
            THROW 50001, N'FacilityId is required.', 1;

        MERGE [TenantMap].[TenantFacilityMap] AS target
        USING (SELECT @TenantId AS TenantId, @FacilityId AS FacilityId) AS source
            ON target.[TenantId] = source.TenantId AND target.[FacilityId] = source.FacilityId
        WHEN MATCHED THEN
            UPDATE SET [IsPrimaryFacility] = @IsPrimaryFacility, [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([TenantId], [FacilityId], [IsPrimaryFacility], [BoundAtUtc])
            VALUES (@TenantId, @FacilityId, @IsPrimaryFacility, @BoundAtUtc);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (N'TenantMap.TenantFacilityMap', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'TenantId=', @TenantId, N'; FacilityId=', @FacilityId), NULL, N'INFO', OBJECT_NAME(@@PROCID), 0);
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext80 nvarchar(200) = CONCAT(N'Params=TenantId=', @TenantId, N'; FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails80 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext80, @ErrorDetails80, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [TenantMap].[usp_TenantFacilityMap_Unbind]
    @TenantId    nvarchar(100),
    @FacilityId  uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TenantId IS NULL OR @FacilityId IS NULL
            THROW 50001, N'TenantId and FacilityId are required.', 1;

        UPDATE [TenantMap].[TenantFacilityMap]
        SET [IsDeleted] = 1, [UpdatedUtc] = SYSUTCDATETIME(), [UpdatedBy] = SYSTEM_USER
        WHERE [TenantId] = @TenantId AND [FacilityId] = @FacilityId;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext81 nvarchar(200) = CONCAT(N'Params=TenantId=', @TenantId, N'; FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails81 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext81, @ErrorDetails81, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE [TenantMap].[usp_TenantFacilityMap_GetByTenantId]
    @TenantId nvarchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TenantId IS NULL
            THROW 50001, N'TenantId is required.', 1;

        SELECT [TenantId], [FacilityId], [IsPrimaryFacility], [BoundAtUtc]
        FROM [TenantMap].[TenantFacilityMap]
        WHERE [TenantId] = @TenantId AND ISNULL([IsDeleted], 0) = 0
        ORDER BY [IsPrimaryFacility] DESC, [BoundAtUtc] ASC;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext82 nvarchar(200) = CONCAT(N'Params=TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails82 nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());
        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                    SYSUTCDATETIME(), @ErrorContext82, @ErrorDetails82, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
        THROW;
    END CATCH
END
GO
