-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentRead_Count.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Deterministic total-count companion to usp_IncidentRead_List, used by
--                  IncidentReadDapperRepository.ListAsync to populate the paginated-result
--                  total. Filter predicate is identical to usp_IncidentRead_List (minus the
--                  OrderBy/SortDirection/PageNumber/PageSize paging parameters), following the
--                  usp_CrisisEvent_Count precedent. TRY/CATCH + THROW only (no AuditLog/
--                  ErrorLog writes) per the established read-procedure convention.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_Count]
(
    @TenantId        UNIQUEIDENTIFIER,
    @SiteId          UNIQUEIDENTIFIER = NULL,
    @Status          NVARCHAR(64)     = NULL,
    @Severity        NVARCHAR(64)     = NULL,
    @OccurredFromUtc DATETIME2(3)     = NULL,
    @OccurredToUtc   DATETIME2(3)     = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT COUNT(1)
        FROM [SafetyEvents].[Incident]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@Status IS NULL OR Status = @Status)
          AND (@Severity IS NULL OR Severity = @Severity)
          AND (@OccurredFromUtc IS NULL OR IncidentOccurredAtUtc >= @OccurredFromUtc)
          AND (@OccurredToUtc IS NULL OR IncidentOccurredAtUtc <= @OccurredToUtc);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
