-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Operational.RiskManagement
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RiskManagement_List.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [Operational].[RiskManagementReadModel],
--                  backing IRiskManagementRepository.GetRiskManagementAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Operational].[usp_RiskManagement_List]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [RiskRecordId],
        [TenantId],
        [SiteId],
        [RiskDescription],
        [RiskLevel],
        [Status],
        [IdentifiedAtUtc],
        [IdentifiedBy]
FROM [Operational].[RiskManagementReadModel]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [IdentifiedAtUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [IdentifiedAtUtc] DESC;
GO
