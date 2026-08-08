-- =============================================
-- Shared.SafetyData -- Phase3-Orchestration
-- Script:        23-usp-tenantfacilityregistration-unregister.sql
-- Purpose:       Creates Orchestration.usp_TenantFacilityRegistration_Unregister
--                against [PlantSafety.CoreDB] -- mirrors
--                ITenantFacilityRegistry.Unregister(string tenantId): removes
--                the binding for the given tenant, returning (via
--                @RemovedCount OUTPUT / result set) whether a binding was
--                removed.
--
-- Idempotent: CREATE OR ALTER. Safe to re-run. Additive-only evolution.
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [Orchestration].[usp_TenantFacilityRegistration_Unregister]
    @TenantId     nvarchar(100),
    @Removed      bit OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TenantId IS NULL OR LTRIM(RTRIM(@TenantId)) = N''
            THROW 50001, N'TenantId is required.', 1;

        DELETE FROM [Orchestration].[TenantFacilityRegistration]
        WHERE [TenantId] = @TenantId;

        SET @Removed = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;

        IF @Removed = 1
        BEGIN
            BEGIN TRY
                INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                    TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                    BeforeSnapshot, ErrorContext, Severity, Module, ViewerAccessed
                )
                VALUES (
                    N'Orchestration.TenantFacilityRegistration', DB_NAME(), N'DELETE', SYSTEM_USER, SYSUTCDATETIME(),
                    CONCAT(N'TenantId=', @TenantId),
                    NULL, N'INFO', OBJECT_NAME(@@PROCID), 0
                );
            END TRY
            BEGIN CATCH
            END CATCH;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext23 nvarchar(200) = CONCAT(N'Params=TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails23 nvarchar(max) = CONCAT(
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
                SYSUTCDATETIME(), @ErrorContext23, @ErrorDetails23, DB_NAME(), OBJECT_NAME(@@PROCID)
            );
        END TRY
        BEGIN CATCH
        END CATCH;

        THROW;
    END CATCH
END
GO
