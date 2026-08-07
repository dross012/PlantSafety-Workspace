USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Dave Ross
-- Create date:   2026-08-07
-- Procedure:     Compliance.usp_NIOSHHazardModuleMapping_GetByModule
-- Description:   Retrieves NIOSH hazards mapped to a given PlantSafety
--                module, joined to the parent hazard profile.
-- Notes:
--   - Validates all input parameters
--   - [Module] is free text (see NIOSHHazardModuleMapping.sql for the
--     current known-valid values); not DB-enforced
--   - Best-effort audit logging for viewer access
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].[usp_NIOSHHazardModuleMapping_GetByModule]
    @Module NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        ------------------------------------------------------------
        -- Input validation
        ------------------------------------------------------------
        IF @Module IS NULL OR LTRIM(RTRIM(@Module)) = N''
            THROW 60006, 'Module is required.', 1;

        ------------------------------------------------------------
        -- Audit: viewer access (best-effort, non-fatal)
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'NIOSHHazardModuleMapping', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                CONCAT(N'Module=', @Module), N'Viewer access via usp_NIOSHHazardModuleMapping_GetByModule',
                N'INFO', N'usp_NIOSHHazardModuleMapping_GetByModule'
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;

        ------------------------------------------------------------
        -- Main query
        ------------------------------------------------------------
        SELECT
            mm.MappingId,
            mm.HazardId,
            hp.ChemicalName,
            hp.CASNumber,
            mm.Module,
            mm.IsPrimary,
            mm.Severity,
            mm.MappingRuleCode,
            mm.Rationale,
            mm.CreatedAtUtc
        FROM [Compliance].[NIOSHHazardModuleMapping] AS mm
        INNER JOIN [Compliance].[NIOSHHazardProfile] AS hp
            ON hp.HazardId = mm.HazardId
        WHERE mm.Module = @Module
        ORDER BY mm.IsPrimary DESC, hp.ChemicalName;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext NVARCHAR(600) = CONCAT(N'Module=', ISNULL(@Module, N'NULL'), N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails NVARCHAR(MAX) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @SourceDatabase NVARCHAR(128) = DB_NAME();
        DECLARE @Module128 NVARCHAR(128) = OBJECT_NAME(@@PROCID);

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, @SourceDatabase, @Module128
            );
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                BeforeSnapshot, AfterSnapshot, ErrorContext, Severity, Module, ViewerAccessed
            )
            VALUES (
                N'NIOSHHazardModuleMapping', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext, ERROR_SEVERITY(), @Module128, 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO
