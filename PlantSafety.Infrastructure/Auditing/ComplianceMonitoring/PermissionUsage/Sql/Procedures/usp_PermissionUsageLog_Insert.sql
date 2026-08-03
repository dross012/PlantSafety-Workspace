-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_PermissionUsageLog_Insert.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
--  SchemaVersion:  2026.1
--  Description:    Inserts a single authorization-decision row. Called by
--                  PermissionUsageLogDapperRepository. Additive-only evolution.
-- ============================================================================

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'usp_PermissionUsageLog_Insert' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    DROP PROCEDURE [Auditing].[usp_PermissionUsageLog_Insert];
END
GO

CREATE PROCEDURE [Auditing].[usp_PermissionUsageLog_Insert]
    @TenantId      UNIQUEIDENTIFIER,
    @UserId        UNIQUEIDENTIFIER,
    @Permission    NVARCHAR(256),
    @RequestType   NVARCHAR(256),
    @Decision      NVARCHAR(16),
    @CorrelationId UNIQUEIDENTIFIER,
    @OccurredAtUtc DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [Auditing].[PermissionUsageLog]
        ([TenantId], [UserId], [Permission], [RequestType], [Decision], [CorrelationId], [OccurredAtUtc])
    VALUES
        (@TenantId, @UserId, @Permission, @RequestType, @Decision, @CorrelationId, @OccurredAtUtc);
END
GO
