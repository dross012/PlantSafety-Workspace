-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Notifications.Internal
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NotificationsInternal_GetInternalNotifications.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  IInternalRepository.GetInternalNotificationsAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsInternal_GetInternalNotifications]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        NotificationId,
        TenantId,
        SiteId,
        RecipientUserId,
        Message,
        Category,
        Status,
        DeliveredAtUtc,
        TriggeredBy
    FROM [Integrations].[NotificationsInternalItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND DeliveredAtUtc >= @FromUtc
      AND DeliveredAtUtc <= @ToUtc
    ORDER BY DeliveredAtUtc DESC;
END
GO
