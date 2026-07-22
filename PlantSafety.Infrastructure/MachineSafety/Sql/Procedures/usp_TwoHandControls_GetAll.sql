-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         MachineSafety.TwoHandControls
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_TwoHandControls_GetAll.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted TwoHandControls rows, backing
--                  ITwoHandControlsRepository.ListAsync (the Domain specification predicate carries
--                  no server-side-translatable filter, so it is applied in-memory in the
--                  Infrastructure repository against this full result set).
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [MachineSafety].[usp_TwoHandControls_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [TwoHandControlsId], [TenantId], [SiteId], [Status], [ControlTitle], [EquipmentType], [EquipmentLocation], [TestedByUserId], [TestedAtUtc], [NextTestDueAtUtc], [BypassAttemptCount], [SynchronizationScore], [CreatedAtUtc], [CreatedBy]
    FROM [MachineSafety].[TwoHandControls]
    WHERE ISNULL([IsDeleted], 0) = 0;
END;
GO
