-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.IncidentManagement (Features-tree Incident aggregate)
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IncidentRead_SearchCount.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team (Claude Code procedure-architect-agent)
--  SchemaVersion:  2026.1
--  Description:    Deterministic total-count companion to usp_IncidentRead_Search, used by
--                  IncidentReadDapperRepository.SearchAsync to populate the paginated-result
--                  total. Filter predicate is identical to usp_IncidentRead_Search (minus the
--                  PageNumber/PageSize paging parameters). TRY/CATCH + THROW only (no
--                  AuditLog/ErrorLog writes) per the established read-procedure convention.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyEvents].[usp_IncidentRead_SearchCount]
(
    @TenantId   UNIQUEIDENTIFIER,
    @SearchTerm NVARCHAR(200)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        DECLARE @LikeTerm NVARCHAR(202) = N'%' + @SearchTerm + N'%';

        SELECT COUNT(1)
        FROM [SafetyEvents].[Incident]
        WHERE TenantId = @TenantId
          AND IsDeleted = 0
          AND (
                Title LIKE @LikeTerm
             OR Description LIKE @LikeTerm
             OR Location LIKE @LikeTerm
          );

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        THROW;
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
