-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.ContinuousImprovement.CAPA
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_CorrectiveAction_List.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of [ContinuousImprovement].
--                  [CorrectiveAction] rows. Backs ICorrectiveActionRepository.ListAsync /
--                  CorrectiveActionDapperRepository.ListAsync. Parameter names/types mirror the
--                  StoredProcedureBuilder Add* calls in CorrectiveActionDapperRepository.ListAsync
--                  exactly (TenantId, SourceEventId, ActionType, Priority, Status, OrderBy,
--                  SortDirection, PageNumber, PageSize). Windowed COUNT(*) OVER() AS TotalCount
--                  avoids a second round trip, matching the usp_Process_List precedent and the
--                  CorrectiveActionDapperRepository.ListAsync consumption pattern
--                  (rows[0].TotalCount). Deterministic ORDER BY + OFFSET/FETCH paging, following
--                  the usp_IncidentRead_List / usp_Process_List CASE-based sortable-column switch
--                  with a fixed tiebreaker. Soft-delete filtering (IsDeleted = 0) applied per
--                  canonical pattern. TRY/CATCH + THROW only (no AuditLog/ErrorLog writes) per the
--                  established read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContinuousImprovement].[usp_CorrectiveAction_List]
(
    @TenantId       UNIQUEIDENTIFIER,
    @SourceEventId  UNIQUEIDENTIFIER = NULL,
    @ActionType     NVARCHAR(50)     = NULL,
    @Priority       NVARCHAR(50)     = NULL,
    @Status         NVARCHAR(50)     = NULL,
    @OrderBy        NVARCHAR(100)    = 'DueAtUtc',
    @SortDirection  NVARCHAR(4)      = 'ASC',
    @PageNumber     INT              = 1,
    @PageSize       INT              = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            CaId,
            TenantId,
            SourceEventId,
            SourceEventType,
            Title,
            Description,
            ActionType,
            Priority,
            AssignedToWorkerId,
            DueAtUtc,
            Status,
            CompletedAtUtc,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
            COUNT(*) OVER() AS TotalCount
        FROM [ContinuousImprovement].[CorrectiveAction]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SourceEventId IS NULL OR SourceEventId = @SourceEventId)
          AND (@ActionType IS NULL OR ActionType = @ActionType)
          AND (@Priority IS NULL OR Priority = @Priority)
          AND (@Status IS NULL OR Status = @Status)
        ORDER BY
            CASE WHEN @OrderBy = 'DueAtUtc' AND @SortDirection = 'ASC'  THEN DueAtUtc END ASC,
            CASE WHEN @OrderBy = 'DueAtUtc' AND @SortDirection = 'DESC' THEN DueAtUtc END DESC,
            CASE WHEN @OrderBy = 'Priority' AND @SortDirection = 'ASC'  THEN Priority END ASC,
            CASE WHEN @OrderBy = 'Priority' AND @SortDirection = 'DESC' THEN Priority END DESC,
            CASE WHEN @OrderBy = 'Status'   AND @SortDirection = 'ASC'  THEN Status END ASC,
            CASE WHEN @OrderBy = 'Status'   AND @SortDirection = 'DESC' THEN Status END DESC,
            CASE WHEN @OrderBy = 'Title'    AND @SortDirection = 'ASC'  THEN Title END ASC,
            CASE WHEN @OrderBy = 'Title'    AND @SortDirection = 'DESC' THEN Title END DESC,
            DueAtUtc ASC
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
