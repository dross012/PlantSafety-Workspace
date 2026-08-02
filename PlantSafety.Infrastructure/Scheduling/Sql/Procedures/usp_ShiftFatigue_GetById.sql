-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scheduling
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_ShiftFatigue_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single ShiftFatigue row by identity, tenant-scoped from Phase 4 full
--                  build-out (2026-07-30) onward. Backs IShiftFatigueRepository.GetByIdAsync
--                  (aggregate hydration) and the Application-layer GetShiftFatigueQuery read path.
--                  Projects RowVersion as a string ETag (CONVERT(NVARCHAR(34), CAST(RowVersion AS
--                  BINARY(8)), 1)) so callers can capture the current concurrency token for
--                  read-before-write flows. Mirrors usp_Permits_GetById.sql.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Scheduling].[usp_ShiftFatigue_GetById]
    @ShiftFatigueId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            ShiftFatigueId, TenantId, SiteId, Status, EmployeeId, ShiftPattern, ConsecutiveNightShifts, OvertimeHours,
            FatigueRiskScore, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
        FROM [Scheduling].[ShiftFatigue]
        WHERE ShiftFatigueId = @ShiftFatigueId AND TenantId = @TenantId AND IsDeleted = 0;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ShiftFatigueId=', @ShiftFatigueId, N'; TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
