-- ============================================================================
--  Module:         CommunitySafety.CommunityConcern
--  File:           usp_CommunityConcern_GetAll.sql
--  Created:        2026-08-04
--  Description:    Returns all non-deleted CommunityConcern rows, backing
--                  ICommunityConcernRepository.ListAsync (the Domain specification predicate
--                  carries no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set). Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [CommunityConcernId], [TenantId], [SiteId], [Status], [Category], [Severity], [FacilityArea], [Description],
        [ReportedByContactId], [RelatedIncidentId], [LoggedAtUtc], [CreatedAtUtc], [CreatedBy],
        [EscalationCount], [EscalationReason], [EscalatedAtUtc], [ResolutionNotes], [ResolvedAtUtc]
    FROM [CommunitySafety].[CommunityConcern]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
