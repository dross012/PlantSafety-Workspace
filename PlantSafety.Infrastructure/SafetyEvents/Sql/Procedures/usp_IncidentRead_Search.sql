-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentRead_Search.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Free-text, paginated search of [SafetyEvents].[Incident] rows across
--                  Title/Description/Location. Backs IIncidentReadRepository.SearchAsync /
--                  IncidentReadDapperRepository.SearchAsync. Parameter names/types mirror the
--                  StoredProcedureBuilder Add* calls in IncidentReadDapperRepository.
--                  SearchAsync exactly (TenantId, SearchTerm, PageNumber, PageSize). Column
--                  list mirrors IncidentReadModel. Deterministic ORDER BY (IncidentOccurredAtUtc
--                  DESC, IncidentId ASC tiebreaker) + OFFSET/FETCH paging. TRY/CATCH + THROW
--                  only (no AuditLog/ErrorLog writes) per the established read-procedure
--                  convention.
--                  UPDATED 2026-07-27 (Claude Code refactoring-agent): added CreatedAtUtc/CreatedBy
--                  to the result set to match the now-required IncidentReadModel columns (see
--                  usp_IncidentRead_GetById.sql for the Rehydrate-wiring rationale).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_Search]
(
    @TenantId   UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200),
    @PageNumber INT = 1,
    @PageSize   INT = 25
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @LikeTerm NVARCHAR(202) = N'%' + @SearchTerm + N'%';

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
            CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag,
            CreatedAtUtc,
            CreatedBy
        FROM [SafetyEvents].[Incident]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (
                Title LIKE @LikeTerm
             OR Description LIKE @LikeTerm
             OR Location LIKE @LikeTerm
          )
        ORDER BY
            IncidentOccurredAtUtc DESC,
            IncidentId ASC
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
