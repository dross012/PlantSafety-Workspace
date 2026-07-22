-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyCulture.StopWorkEvent
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_StopWorkEvent_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read procedure backing IStopWorkEventRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyCulture].[usp_StopWorkEvent_GetById]
    @StopWorkEventId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
SELECT
    [StopWorkEventId], [TenantId], [SiteId], [Status], [WorkArea], [HazardDescription],
    [InitiatedByActorKind], [InitiatedByUserId], [ResolutionSummary], [WasCelebrated],
    [RelatedPermitId], [RelatedChecklistCompletionId], [InvestigationFindings],
    [InvestigatedAtUtc], [ResolvedAtUtc], [CreatedAtUtc], [CreatedBy]
FROM [SafetyCulture].[StopWorkEvent]
WHERE [StopWorkEventId] = @StopWorkEventId
  AND ISNULL([IsDeleted], 0) = 0;
GO
