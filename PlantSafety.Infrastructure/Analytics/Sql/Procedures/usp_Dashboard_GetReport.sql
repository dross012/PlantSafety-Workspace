-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Analytics.Dashboard
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Dashboard_GetReport.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Cross-module date-ranged report backing IDashboardRepository.GetReportAsync /
--                  DashboardDapperRepository.GetReportAsync. See usp_Dashboard_GetSnapshot.sql for
--                  the shared source-table notes (Incident/NearMiss/Observation). Additional notes
--                  specific to this proc:
--                    - Trir/Dart: NOT COMPUTED. Both are standard OSHA rate formulas requiring
--                      total hours worked in the period (TRIR = incidents x 200,000 / hours worked;
--                      DART is the same formula restricted to days-away/restricted/transfer cases).
--                      No hours-worked/exposure-hours table or column was found tracked anywhere in
--                      this codebase during this pass. SafetyReportReadModel.Trir/Dart are
--                      non-nullable decimal (not decimal?), so SQL NULL cannot be returned here --
--                      Dapper would throw mapping a NULL into a non-nullable value type. Returned as
--                      a literal 0 as the only type-safe option; THIS IS A SENTINEL, NOT A REAL
--                      MEASUREMENT -- do not surface it to users as an actual TRIR/DART of zero
--                      without also surfacing that it is uncomputed.
--                    - OpenCAPAs: [ContinuousImprovement].[CorrectiveAction], Status <> 'Closed'
--                      (the table's own confirmed lifecycle: Open/InProgress/AwaitingVerification/
--                      Closed/Overdue -- verified earlier in this session while building the
--                      CorrectiveAction Start/Complete/Close/MarkOverdue command stack).
--                    - TrainingComplianceRate: NOT COMPUTED. Requires a denominator (total required
--                      trainings per worker/site) not established anywhere found in this pass. Also
--                      non-nullable decimal on the read model -- same 0-as-sentinel caveat as
--                      Trir/Dart above.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Dashboard_GetReport]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            @TenantId AS TenantId,
            @SiteId AS SiteId,
            @FromUtc AS FromUtc,
            @ToUtc AS ToUtc,
            SYSUTCDATETIME() AS GeneratedAtUtc,
            CAST(0 AS DECIMAL(10,2)) AS Trir,
            CAST(0 AS DECIMAL(10,2)) AS Dart,
            (SELECT COUNT(*) FROM [SafetyEvents].[Incident] WHERE TenantId = @TenantId AND IsDeleted = 0
                AND IncidentOccurredAtUtc BETWEEN @FromUtc AND @ToUtc
                AND (@SiteId IS NULL OR SiteId = @SiteId)) AS TotalIncidents,
            (SELECT COUNT(*) FROM [SafetyEvents].[NearMiss] WHERE TenantId = @TenantId
                AND OccurredAtUtc BETWEEN @FromUtc AND @ToUtc
                AND (@SiteId IS NULL OR SiteId = @SiteId)) AS TotalNearMisses,
            (SELECT COUNT(*) FROM [SafetyEvents].[Observation] WHERE TenantId = @TenantId
                AND ObservedAtUtc BETWEEN @FromUtc AND @ToUtc
                AND (@SiteId IS NULL OR SiteId = @SiteId)) AS TotalObservations,
            (SELECT COUNT(*) FROM [ContinuousImprovement].[CorrectiveAction] WHERE TenantId = @TenantId
                AND IsDeleted = 0 AND Status <> N'Closed') AS OpenCAPAs,
            CAST(0 AS DECIMAL(5,2)) AS TrainingComplianceRate,
            (SELECT TOP 1 CultureIndexScore FROM [Analytics].[Culture] WHERE TenantId = @TenantId
                ORDER BY CreatedAtUtc DESC) AS CultureScore;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
