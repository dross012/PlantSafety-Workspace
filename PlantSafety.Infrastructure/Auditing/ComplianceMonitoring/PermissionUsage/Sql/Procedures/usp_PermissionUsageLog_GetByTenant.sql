-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PermissionUsageLog_GetByTenant.sql
--  Created:        2026-08-03
--  Author:         Security & Compliance Agent (Phase 7 §5.4)
--  SchemaVersion:  2026.1
--  Description:    Read-side query over [Auditing].[PermissionUsageLog], mandatorily scoped by
--                  @TenantId (not optional) -- backs IPermissionUsageLogReadRepository /
--                  GetTenantAuditStreamQuery. Additive-only evolution.
-- ============================================================================

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'usp_PermissionUsageLog_GetByTenant' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    DROP PROCEDURE [Auditing].[usp_PermissionUsageLog_GetByTenant];
END
GO

CREATE PROCEDURE [Auditing].[usp_PermissionUsageLog_GetByTenant]
    @TenantId  UNIQUEIDENTIFIER,
    @FromUtc   DATETIME2(3),
    @ToUtc     DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        [TenantId],
        [UserId],
        [Permission],
        [RequestType],
        [Decision],
        [CorrelationId],
        [OccurredAtUtc]
    FROM [Auditing].[PermissionUsageLog]
    WHERE [TenantId] = @TenantId
      AND [OccurredAtUtc] BETWEEN @FromUtc AND @ToUtc
    ORDER BY [OccurredAtUtc] DESC;
END
GO
