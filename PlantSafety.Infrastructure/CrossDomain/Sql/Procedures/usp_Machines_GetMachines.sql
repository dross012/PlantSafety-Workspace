-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Machines
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Machines_GetMachines.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Machines],
--                  backing IMachinesRepository.GetMachinesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Machines_GetMachines]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [MachineId],
        [TenantId],
        [SiteId],
        [MachineName],
        [MachineType],
        [SerialNumber],
        [Status],
        [LastServicedAtUtc],
        [LastServicedBy]
FROM [CrossDomain].[Machines]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [LastServicedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [LastServicedAtUtc] DESC;
GO
