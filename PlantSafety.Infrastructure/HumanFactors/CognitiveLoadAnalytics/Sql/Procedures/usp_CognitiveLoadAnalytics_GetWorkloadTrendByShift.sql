-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         HumanFactors.CognitiveLoadAnalytics
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CognitiveLoadAnalytics_GetWorkloadTrendByShift.sql
--  Created:        2026-08-04
--  SchemaVersion:  2026.1
--  Description:    Retrieves workload-trend snapshots across all ledgers matching a
--                  tenant/site/shift within a period. Backs GetWorkloadTrendByShiftQuery / the
--                  Cognitive Load by Shift/Role dashboard.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [HumanFactorsCognitiveLoadAnalytics].[usp_CognitiveLoadAnalytics_GetWorkloadTrendByShift]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @ShiftType NVARCHAR(20),
    @PeriodStartUtc DATETIME2(3),
    @PeriodEndUtc DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT
            t.WorkloadTrendRecordId, t.CognitiveLoadLedgerId, l.ShiftType,
            t.PeriodStartUtc, t.PeriodEndUtc, t.MeanCognitiveLoadScore, t.OverloadEventCount, t.ReadingCount, t.RecalculatedAtUtc
        FROM [HumanFactorsCognitiveLoadAnalytics].[WorkloadTrendRecord] AS t
        INNER JOIN [HumanFactorsCognitiveLoadAnalytics].[CognitiveLoadLedger] AS l ON l.CognitiveLoadLedgerId = t.CognitiveLoadLedgerId
        WHERE l.TenantId = @TenantId
          AND ((@SiteId IS NULL AND l.SiteId IS NULL) OR l.SiteId = @SiteId)
          AND l.ShiftType = @ShiftType
          AND t.PeriodStartUtc >= @PeriodStartUtc
          AND t.PeriodEndUtc <= @PeriodEndUtc
          AND l.IsDeleted = 0
        ORDER BY t.RecalculatedAtUtc ASC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

        THROW;
    END CATCH
END
GO
