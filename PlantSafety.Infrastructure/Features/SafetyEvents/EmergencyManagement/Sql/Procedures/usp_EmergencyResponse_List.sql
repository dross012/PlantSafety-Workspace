-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.SafetyEvents.EmergencyManagement
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_EmergencyResponse_List.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of EmergencyResponse records
--                  backing IEmergencyResponseRepository.ListAsync. Deterministic
--                  ORDER BY + OFFSET/FETCH paging per Enterprise Architectural Rule
--                  Handbook §1.5/§2.10. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_EmergencyResponse_List]
(
    @TenantId      UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @Status        NVARCHAR(64)     = NULL,
    @OrderBy       NVARCHAR(64)     = 'ActivatedAtUtc',
    @SortDirection NVARCHAR(4)      = 'DESC',
    @PageNumber    INT              = 1,
    @PageSize      INT              = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            ResponseId,
            TenantId,
            SiteId,
            LinkedCrisisEventId,
            ResponsePlanName,
            Description,
            ActivatedAtUtc,
            StandDownAtUtc,
            Status,
            CoordinatorId,
            LastModifiedAtUtc,
            ETag
        FROM [SafetyEvents].[EmergencyResponse]
        WHERE TenantId = @TenantId
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@Status IS NULL OR Status = @Status)
        ORDER BY
            CASE WHEN @OrderBy = 'ActivatedAtUtc' AND @SortDirection = 'ASC' THEN ActivatedAtUtc END ASC,
            CASE WHEN @OrderBy = 'ActivatedAtUtc' AND @SortDirection = 'DESC' THEN ActivatedAtUtc END DESC,
            CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'ASC' THEN Status END ASC,
            CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'DESC' THEN Status END DESC,
            ActivatedAtUtc DESC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

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
