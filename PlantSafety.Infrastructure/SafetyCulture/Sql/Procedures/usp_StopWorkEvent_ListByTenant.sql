-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.StopWorkEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_StopWorkEvent_ListByTenant.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IStopWorkEventRepository.ListAsync. Returns
--                  candidate header rows for the tenant/site; StopWorkEventSpecification
--                  predicates are applied in-memory in the repository after rehydration.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_StopWorkEvent_ListByTenant]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
SELECT
    [StopWorkEventId], [TenantId], [SiteId], [Status], [WorkArea], [HazardDescription],
    [InitiatedByActorKind], [InitiatedByUserId], [ResolutionSummary], [WasCelebrated],
    [RelatedPermitId], [RelatedChecklistCompletionId], [InvestigationFindings],
    [InvestigatedAtUtc], [ResolvedAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [SafetyCulture].[StopWorkEvent]
WHERE [TenantId] = @TenantId
  AND (@SiteId IS NULL OR [SiteId] = @SiteId)
  AND ISNULL([IsDeleted], 0) = 0
ORDER BY [CreatedAtUtc] DESC;
GO
