-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.Observation
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Observation_Count.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only row count of [SafetyEvents].[Observation] matching the same filter
--                  set as usp_Observation_List (minus paging/ordering). Backs
--                  IObservationRepository.ListAsync / ObservationDapperRepository.ListAsync's
--                  separate countParameters call. TRY/CATCH + THROW only (no AuditLog/ErrorLog
--                  writes) per the established read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Observation_Count]
(
    @TenantId        UNIQUEIDENTIFIER,
    @SiteId          UNIQUEIDENTIFIER = NULL,
    @ObservationType NVARCHAR(30)     = NULL,
    @FromUtc         DATETIME2(3)     = NULL,
    @ToUtc           DATETIME2(3)     = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT COUNT(*)
        FROM [SafetyEvents].[Observation]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@ObservationType IS NULL OR ObservationType = @ObservationType)
          AND (@FromUtc IS NULL OR ObservedAtUtc >= @FromUtc)
          AND (@ToUtc IS NULL OR ObservedAtUtc <= @ToUtc);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
