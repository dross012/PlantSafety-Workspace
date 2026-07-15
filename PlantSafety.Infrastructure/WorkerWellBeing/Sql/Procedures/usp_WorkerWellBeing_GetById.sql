-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         WorkerWellBeing
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_WorkerWellBeing_GetById.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row procedure for [WorkerWellBeing].[WorkerWellBeingRecords],
--                  backing IWorkerWellBeingRecordRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [WorkerWellBeing].[usp_WorkerWellBeing_GetById]
    @TenantId           UNIQUEIDENTIFIER,
    @WorkerWellBeingId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
        [WorkerWellBeingId],
        [TenantId],
        [WorkerId],
        [WellBeingRiskLevel],
        [Status],
        [RecordedAtUtc]
FROM [WorkerWellBeing].[WorkerWellBeingRecords]
WHERE [TenantId] = @TenantId
  AND [WorkerWellBeingId] = @WorkerWellBeingId;
GO
