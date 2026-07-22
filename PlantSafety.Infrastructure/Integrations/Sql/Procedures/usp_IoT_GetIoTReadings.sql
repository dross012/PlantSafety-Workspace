-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.IoT
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_IoT_GetIoTReadings.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IIoTRepository.GetIoTReadingsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_IoT_GetIoTReadings]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        ReadingId,
        TenantId,
        SiteId,
        DeviceId,
        SensorType,
        ReadingValue,
        Status,
        RecordedAtUtc,
        IngestedBy
    FROM [Integrations].[IoTItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND RecordedAtUtc >= @FromUtc
      AND RecordedAtUtc <= @ToUtc
    ORDER BY RecordedAtUtc DESC;
END
GO
