-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Analytics.Dashboard
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Dashboard_GetSnapshot.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Cross-module aggregation backing IDashboardRepository.GetDashboardAsync /
--                  DashboardDapperRepository.GetDashboardAsync. Unlike the single-table gaps found
--                  elsewhere in this pass (NearMiss/Observation/Rca/Budget), this proc joins
--                  several independently-owned tables. Sources used, each verified to exist with a
--                  real TenantId/Status column before being wired in:
--                    - TotalIncidents/OpenIncidents/CriticalIncidents: [SafetyEvents].[Incident]
--                      (Status <> 'Closed' = open; Severity = 'Critical' = critical -- both taken
--                      verbatim from the table's own columns).
--                    - TotalNearMisses: [SafetyEvents].[NearMiss] (the Features-namespaced table
--                      added earlier in this pass; see NearMiss.sql for why this one and not
--                      [SafetyEvents].[NearMisses]).
--                    - TotalObservations/PositiveObservations/NegativeObservations:
--                      [SafetyEvents].[Observation] (added earlier in this pass). ObservationType
--                      is documented as Positive/AtRisk/Unsafe on ObservationReadModel; Positive
--                      maps to PositiveObservations, {AtRisk, Unsafe} to NegativeObservations.
--                    - OpenPermits: [Compliance].[Permits], Status NOT IN ('Closed','Expired',
--                      'Cancelled','Rejected') -- ASSUMPTION: no PermitStatus enum was found in
--                      source to confirm the exact terminal-state vocabulary; this is a defensible
--                      "not obviously closed" heuristic, not a confirmed exact match. Flag for
--                      correction if the real enum differs.
--                    - OverdueObligations: [Compliance].[Obligations], DueAtUtc < SYSUTCDATETIME()
--                      AND Status NOT IN ('Completed','Closed') -- same caveat as OpenPermits.
--                    - CultureScore: [Analytics].[Culture], most recent CultureIndexScore
--                      (TenantId-scoped, most recently created row; NULL if none exist yet).
--                    - LostTimeInjuries: NOT COMPUTED. No lost-time-injury flag/column was found on
--                      [SafetyEvents].[Incident] or anywhere else searched. Hardcoded to 0 rather
--                      than fabricating a source. Needs a real column (e.g. an
--                      IsLostTimeInjury BIT) before this can be accurate.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Analytics].[usp_Dashboard_GetSnapshot]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            @TenantId AS TenantId,
            @SiteId AS SiteId,
            (SELECT COUNT(*) FROM [SafetyEvents].[Incident] WHERE TenantId = @TenantId AND IsDeleted = 0
                AND (@SiteId IS NULL OR SiteId = @SiteId)) AS TotalIncidents,
            (SELECT COUNT(*) FROM [SafetyEvents].[Incident] WHERE TenantId = @TenantId AND IsDeleted = 0
                AND Status <> N'Closed' AND (@SiteId IS NULL OR SiteId = @SiteId)) AS OpenIncidents,
            (SELECT COUNT(*) FROM [SafetyEvents].[Incident] WHERE TenantId = @TenantId AND IsDeleted = 0
                AND Severity = N'Critical' AND (@SiteId IS NULL OR SiteId = @SiteId)) AS CriticalIncidents,
            0 AS LostTimeInjuries,
            (SELECT COUNT(*) FROM [SafetyEvents].[NearMiss] WHERE TenantId = @TenantId
                AND (@SiteId IS NULL OR SiteId = @SiteId)) AS TotalNearMisses,
            (SELECT COUNT(*) FROM [SafetyEvents].[Observation] WHERE TenantId = @TenantId
                AND (@SiteId IS NULL OR SiteId = @SiteId)) AS TotalObservations,
            (SELECT COUNT(*) FROM [SafetyEvents].[Observation] WHERE TenantId = @TenantId
                AND ObservationType = N'Positive' AND (@SiteId IS NULL OR SiteId = @SiteId)) AS PositiveObservations,
            (SELECT COUNT(*) FROM [SafetyEvents].[Observation] WHERE TenantId = @TenantId
                AND ObservationType IN (N'AtRisk', N'Unsafe') AND (@SiteId IS NULL OR SiteId = @SiteId)) AS NegativeObservations,
            (SELECT COUNT(*) FROM [Compliance].[Permits] WHERE TenantId = @TenantId AND IsDeleted = 0
                AND Status NOT IN (N'Closed', N'Expired', N'Cancelled', N'Rejected')) AS OpenPermits,
            (SELECT COUNT(*) FROM [Compliance].[Obligations] WHERE TenantId = @TenantId
                AND DueAtUtc < SYSUTCDATETIME() AND Status NOT IN (N'Completed', N'Closed')) AS OverdueObligations,
            (SELECT TOP 1 CultureIndexScore FROM [Analytics].[Culture] WHERE TenantId = @TenantId
                ORDER BY CreatedAtUtc DESC) AS CultureScore,
            SYSUTCDATETIME() AS CalculatedAtUtc;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
