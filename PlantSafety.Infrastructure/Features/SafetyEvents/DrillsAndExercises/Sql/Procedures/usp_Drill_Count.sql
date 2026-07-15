-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.DrillsAndExercises
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_Drill_Count.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Deterministic total-count companion to usp_Drill_List, used to
--                  populate the canonical PaginatedResult<T> envelope (§XVIII).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_Drill_Count]
(
    @TenantId  UNIQUEIDENTIFIER,
    @SiteId    UNIQUEIDENTIFIER = NULL,
    @Status    NVARCHAR(64)     = NULL,
    @DrillType NVARCHAR(64)     = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT COUNT(1)
        FROM [SafetyEvents].[Drill]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@Status IS NULL OR Status = @Status)
          AND (@DrillType IS NULL OR DrillType = @DrillType);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
