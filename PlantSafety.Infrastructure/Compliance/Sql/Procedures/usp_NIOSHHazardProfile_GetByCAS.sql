USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Dave Ross
-- Create date:   2026-08-07
-- Procedure:     Compliance.usp_NIOSHHazardProfile_GetByCAS
-- Description:   Retrieves a single NIOSH hazard profile by CAS Number.
-- Notes:
--   - Validates all input parameters
--   - Reference data table has no soft-delete column to filter
--   - Best-effort audit logging for viewer access
--   - Mandatory error logging with contextual parameters
--   - XACT_ABORT ensures rollback on error
-- =============================================
CREATE OR ALTER PROCEDURE [Compliance].[usp_NIOSHHazardProfile_GetByCAS]
    @CASNumber NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        ------------------------------------------------------------
        -- Input validation
        ------------------------------------------------------------
        IF @CASNumber IS NULL OR LTRIM(RTRIM(@CASNumber)) = N''
            THROW 60001, 'CASNumber is required.', 1;

        ------------------------------------------------------------
        -- Audit: viewer access (best-effort, non-fatal)
        ------------------------------------------------------------
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (
                TableName, SourceDatabase, Operation, ChangedBy, ChangeTime,
                ViewerAccessed, ErrorContext, Severity, Module
            )
            VALUES (
                N'NIOSHHazardProfile', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                CONCAT(N'CASNumber=', @CASNumber), N'Viewer access via usp_NIOSHHazardProfile_GetByCAS',
                N'INFO', N'usp_NIOSHHazardProfile_GetByCAS'
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary operation
        END CATCH;

        ------------------------------------------------------------
        -- Main query
        ------------------------------------------------------------
        SELECT
            hp.HazardId,
            hp.ChemicalName,
            hp.CASNumber,
            hp.REL_TWA,
            hp.REL_STEL,
            hp.HealthEffects,
            hp.RecommendedControls,
            hp.PPERecommendations,
            hp.AtmosphericMonitoring,
            hp.ErgonomicFactors,
            hp.SourceUrl,
            hp.ProfileHash,
            hp.CreatedAtUtc,
            hp.UpdatedAtUtc
        FROM [Compliance].[NIOSHHazardProfile] AS hp
        WHERE hp.CASNumber = @CASNumber;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorContext NVARCHAR(600) = CONCAT(N'CASNumber=', ISNULL(@CASNumber, N'NULL'), N'; User=', SYSTEM_USER);
        DECLARE @ErrorDetails NVARCHAR(MAX) = CONCAT(
            N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorState=', ERROR_STATE(), N'; ',
            N'ErrorSeverity=', ERROR_SEVERITY(), N'; ErrorProcedure=', ISNULL(ERROR_PROCEDURE(), N'NULL'), N'; ',
            N'ErrorLine=', ERROR_LINE(), N'; ErrorMessage=', ERROR_MESSAGE());
        DECLARE @SourceDatabase NVARCHAR(128) = DB_NAME();
        DECLARE @Module NVARCHAR(128) = OBJECT_NAME(@@PROCID);

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (
                ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
                ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module
            )
            VALUES (
                ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
                SYSUTCDATETIME(), @ErrorContext, @ErrorDetails, @SourceDatabase, @Module
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
                N'NIOSHHazardProfile', DB_NAME(), N'VIEW', SYSTEM_USER, SYSUTCDATETIME(),
                NULL, NULL, @ErrorContext, ERROR_SEVERITY(), OBJECT_NAME(@@PROCID), 0
            );
        END TRY
        BEGIN CATCH
            -- Audit failures must never block primary error propagation
        END CATCH;

        THROW;
    END CATCH
END
GO
