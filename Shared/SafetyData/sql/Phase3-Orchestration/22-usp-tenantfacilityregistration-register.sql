-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        22-usp-tenantfacilityregistration-register.sql
-- Purpose:       Creates Orchestration.usp_TenantFacilityRegistration_Register
--                against [PlantSafety.CoreDB] -- mirrors
--                ITenantFacilityRegistry.Register(TenantFacilityRegistration
--                registration): "Registers (or replaces) the facility binding
--                for the given tenant" -- true upsert keyed by TenantId
--                (unlike the EventId-idempotent Record procedures in this
--                folder, this one DOES replace an existing row, per the
--                interface's own doc comment).
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Orchestration].[usp_TenantFacilityRegistration_Register]
    @TenantId     nvarchar(100),
    @FacilityId   uniqueidentifier,
    @Name         nvarchar(200),
    @IndustryName nvarchar(200),
    @Location     nvarchar(400)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TenantId IS NULL OR LTRIM(RTRIM(@TenantId)) = N''
            THROW 50001, N'TenantId is required.', 1;
        IF @FacilityId IS NULL OR @FacilityId = '00000000-0000-0000-0000-000000000000'
            THROW 50001, N'FacilityId is required and must not be empty.', 1;
        IF @Name IS NULL OR LTRIM(RTRIM(@Name)) = N''
            THROW 50001, N'Name is required.', 1;
        IF @IndustryName IS NULL OR LTRIM(RTRIM(@IndustryName)) = N''
            THROW 50001, N'IndustryName is required.', 1;

        MERGE [Orchestration].[TenantFacilityRegistration] AS target
        USING (SELECT @TenantId AS TenantId) AS source
            ON target.[TenantId] = source.TenantId
        WHEN MATCHED THEN
            UPDATE SET
                [FacilityId] = @FacilityId,
                [Name] = @Name,
                [IndustryName] = @IndustryName,
                [Location] = @Location,
                [UpdatedUtc] = SYSUTCDATETIME(),
                [UpdatedBy] = SYSTEM_USER
        WHEN NOT MATCHED THEN
            INSERT ([TenantId], [FacilityId], [Name], [IndustryName], [Location])
            VALUES (@TenantId, @FacilityId, @Name, @IndustryName, @Location);

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'Orchestration.TenantFacilityRegistration', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                CONCAT(N'TenantId=', @TenantId, N'; FacilityId=', @FacilityId, N'; IndustryName=', @IndustryName),
                NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext22 nvarchar(200) = CONCAT(N'Params=TenantId=', @TenantId, N'; FacilityId=', @FacilityId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails22 nvarchar(max) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext22, @ErrorDetails22, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'Orchestration.TenantFacilityRegistration', DB_NAME(), N'UPSERT', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext22, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
