-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Integrations.Notifications.Texting
--  Layer:          Infrastructure – SQL Scripts
--  File:           usp_NotificationsTexting_GetTextingDeliveries.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing
--                  Integrations.Notifications.Texting.ITextingRepository.GetTextingDeliveriesAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Integrations].[usp_NotificationsTexting_GetTextingDeliveries]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL,
    @FromUtc  DATETIME2(3),
    @ToUtc    DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT
        SmsDeliveryId,
        TenantId,
        SiteId,
        RecipientPhoneNumber,
        MessageBody,
        TriggerEvent,
        Status,
        SentAtUtc,
        SentBy
    FROM [Integrations].[NotificationsTextingItems]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND IsDeleted = 0
      AND SentAtUtc >= @FromUtc
      AND SentAtUtc <= @ToUtc
    ORDER BY SentAtUtc DESC;
END
GO
