-- ============================================================================
--  Module:         CommunitySafety.CommunityConcern
--  File:           usp_CommunityConcern_GetOpen.sql
--  Created:        2026-08-04
--  Description:    Read-only stored procedure backing
--                  ICommunityConcernRepository.GetOpenCommunityConcernItemsAsync (Application
--                  read-model for the ListOpenCommunityConcerns query / Community Concern Tracker
--                  dashboard). "Open" excludes Resolved and Archived. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_GetOpen]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        [CommunityConcernId], [TenantId], [SiteId], [Category], [Severity], [FacilityArea], [Description],
        [ReportedByContactId], [RelatedIncidentId], [Status] AS [ConcernStatus], [LoggedAtUtc], [ResolvedAtUtc],
        [CreatedAtUtc], [CreatedBy]
    FROM [CommunitySafety].[CommunityConcern]
    WHERE [TenantId] = @TenantId
      AND (@SiteId IS NULL OR [SiteId] = @SiteId)
      AND [Status] NOT IN (N'Resolved', N'Archived')
      AND ISNULL([IsDeleted], 0) = 0
    ORDER BY [LoggedAtUtc] DESC;
END
GO
