-- ============================================================================
--  Module:         CommunitySafety.CommunityConcern
--  File:           usp_CommunityConcern_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single CommunityConcern row by identifier, scoped to a tenant.
--                  Backs both ICommunityConcernRepository.GetByIdAsync (Domain, no tenant filter
--                  required since the id is globally unique) and the Application read-model
--                  by-id lookup (tenant-scoped for defense in depth). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_GetById]
(
    @CommunityConcernId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [CommunityConcernId], [TenantId], [SiteId], [Category], [Severity], [FacilityArea], [Description],
        [ReportedByContactId], [RelatedIncidentId], [Status] AS [ConcernStatus], [LoggedAtUtc], [ResolvedAtUtc],
        [CreatedAtUtc], [CreatedBy]
    FROM [CommunitySafety].[CommunityConcern]
    WHERE [CommunityConcernId] = @CommunityConcernId
      AND (@TenantId IS NULL OR [TenantId] = @TenantId)
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
