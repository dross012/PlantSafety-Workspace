-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.TwoHandControls
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_TwoHandControls_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single TwoHandControls row by identifier, backing
--                  ITwoHandControlsRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_TwoHandControls_GetById]
(
    @TwoHandControlsId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [TwoHandControlsId], [TenantId], [SiteId], [Status], [ControlTitle], [EquipmentType], [EquipmentLocation], [TestedByUserId], [TestedAtUtc], [NextTestDueAtUtc], [BypassAttemptCount], [SynchronizationScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[TwoHandControls]
    WHERE [TwoHandControlsId] = @TwoHandControlsId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
