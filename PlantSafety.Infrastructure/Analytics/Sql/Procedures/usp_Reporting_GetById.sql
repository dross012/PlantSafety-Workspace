-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.Reporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Reporting_GetById.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-report detail procedure for [Analytics].[Reporting],
--                  backing IReportingRepository.GetReportByIdAsync. Backs the PlantSafety.UI
--                  Reporting module's Report Detail page. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Reporting_GetById]
    @TenantId       UNIQUEIDENTIFIER,
    @ReportItemId   UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
        [ReportItemId],
        [ReportName],
        [Description],
        [ReportCategory],
        [Status],
        [PeriodStartUtc],
        [PeriodEndUtc],
        [GeneratedAtUtc],
        [OwnerName],
        [AvailableFormats],
        [SourceModule]
FROM [Analytics].[Reporting]
WHERE [TenantId] = @TenantId
  AND [ReportItemId] = @ReportItemId;
GO
