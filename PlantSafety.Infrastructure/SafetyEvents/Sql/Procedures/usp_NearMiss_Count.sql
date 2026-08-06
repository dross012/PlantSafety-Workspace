-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NearMiss_Count.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only row count of [SafetyEvents].[NearMiss] matching the same filter set
--                  as usp_NearMiss_List (minus paging/ordering). Backs INearMissRepository.ListAsync
--                  / NearMissDapperRepository.ListAsync's separate countParameters call -- returns
--                  a single scalar int via QuerySingleOrDefaultAsync<int?>, matching
--                  NearMissDapperRepository.ListAsync's consumption exactly. TRY/CATCH + THROW only
--                  (no AuditLog/ErrorLog writes) per the established read-procedure convention.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_NearMiss_Count]
(
    @TenantId          UNIQUEIDENTIFIER,
    @SiteId            UNIQUEIDENTIFIER = NULL,
    @PotentialSeverity NVARCHAR(50)     = NULL,
    @HazardCategory    NVARCHAR(100)    = NULL,
    @OccurredFromUtc   DATETIME2(3)     = NULL,
    @OccurredToUtc     DATETIME2(3)     = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT COUNT(*)
        FROM [SafetyEvents].[NearMiss]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@PotentialSeverity IS NULL OR PotentialSeverity = @PotentialSeverity)
          AND (@HazardCategory IS NULL OR HazardCategory = @HazardCategory)
          AND (@OccurredFromUtc IS NULL OR OccurredAtUtc >= @OccurredFromUtc)
          AND (@OccurredToUtc IS NULL OR OccurredAtUtc <= @OccurredToUtc);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
