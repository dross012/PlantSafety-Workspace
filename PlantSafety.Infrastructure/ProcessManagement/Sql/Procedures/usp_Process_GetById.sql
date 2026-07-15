-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ProcessManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Process_GetById.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only single-row procedure for [ProcessManagement].[Processes],
--                  backing IProcessRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ProcessManagement].[usp_Process_GetById]
    @TenantId   UNIQUEIDENTIFIER,
    @ProcessId  UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;

SELECT
        [ProcessId],
        [TenantId],
        [SiteId],
        [ProcessName],
        [Category],
        [Status],
        [Version],
        [OwnerId],
        [LastModifiedAtUtc],
        CONVERT(NVARCHAR(34), [RowVersion], 1) AS [ETag]
FROM [ProcessManagement].[Processes]
WHERE [TenantId] = @TenantId
  AND [ProcessId] = @ProcessId;
GO
