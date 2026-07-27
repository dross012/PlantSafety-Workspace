-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentRead_List.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only, paginated, filterable list of [SafetyEvents].[Incident] rows.
--                  Backs IIncidentReadRepository.ListAsync / IncidentReadDapperRepository.
--                  ListAsync. Parameter names/types mirror the StoredProcedureBuilder Add*
--                  calls in IncidentReadDapperRepository.ListAsync exactly (TenantId, SiteId,
--                  Status, Severity, OccurredFromUtc, OccurredToUtc, OrderBy, SortDirection,
--                  PageNumber, PageSize). Deterministic ORDER BY + OFFSET/FETCH paging,
--                  following the usp_CrisisEvent_List precedent (CASE-based sortable-column
--                  switch with a fixed tiebreaker). Column list mirrors IncidentReadModel.
--                  TRY/CATCH + THROW only (no AuditLog/ErrorLog writes) per the established
--                  read-procedure convention. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_List]
(
    @TenantId        UNIQUEIDENTIFIER,
    @SiteId          UNIQUEIDENTIFIER = NULL,
    @Status          NVARCHAR(64)     = NULL,
    @Severity        NVARCHAR(64)     = NULL,
    @OccurredFromUtc DATETIME2(3)     = NULL,
    @OccurredToUtc   DATETIME2(3)     = NULL,
    @OrderBy         NVARCHAR(64)     = 'IncidentOccurredAtUtc',
    @SortDirection   NVARCHAR(4)      = 'DESC',
    @PageNumber      INT              = 1,
    @PageSize        INT              = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT
            IncidentId,
            TenantId,
            SiteId,
            Title,
            Description,
            IncidentOccurredAtUtc,
            ReportedAtUtc,
            Severity,
            Status,
            Location,
            ReportedByWorkerId,
            AssignedInvestigatorId,
            ClosedAtUtc,
            LastModifiedByActorId,
            LastModifiedAtUtc,
            CONVERT(NVARCHAR(34), RowVersion, 1) AS ETag
        FROM [SafetyEvents].[Incident]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (@SiteId IS NULL OR SiteId = @SiteId)
          AND (@Status IS NULL OR Status = @Status)
          AND (@Severity IS NULL OR Severity = @Severity)
          AND (@OccurredFromUtc IS NULL OR IncidentOccurredAtUtc >= @OccurredFromUtc)
          AND (@OccurredToUtc IS NULL OR IncidentOccurredAtUtc <= @OccurredToUtc)
        ORDER BY
            CASE WHEN @OrderBy = 'IncidentOccurredAtUtc' AND @SortDirection = 'ASC' THEN IncidentOccurredAtUtc END ASC,
            CASE WHEN @OrderBy = 'IncidentOccurredAtUtc' AND @SortDirection = 'DESC' THEN IncidentOccurredAtUtc END DESC,
            CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'ASC' THEN Status END ASC,
            CASE WHEN @OrderBy = 'Status' AND @SortDirection = 'DESC' THEN Status END DESC,
            CASE WHEN @OrderBy = 'Severity' AND @SortDirection = 'ASC' THEN Severity END ASC,
            CASE WHEN @OrderBy = 'Severity' AND @SortDirection = 'DESC' THEN Severity END DESC,
            CASE WHEN @OrderBy = 'Title' AND @SortDirection = 'ASC' THEN Title END ASC,
            CASE WHEN @OrderBy = 'Title' AND @SortDirection = 'DESC' THEN Title END DESC,
            IncidentOccurredAtUtc DESC
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
