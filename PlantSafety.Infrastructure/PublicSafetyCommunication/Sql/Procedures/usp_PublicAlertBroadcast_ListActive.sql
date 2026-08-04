-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicAlertBroadcast_ListActive.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IPublicAlertBroadcastRepository
--                  (Application layer read model). Supports ListActiveBroadcastsQuery and the
--                  Broadcast History dashboard / broadcast delivery success rate KPI.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_ListActive]
    @TenantId UNIQUEIDENTIFIER,
    @SiteId   UNIQUEIDENTIFIER = NULL
AS
SET NOCOUNT ON;
BEGIN
    SELECT PublicAlertBroadcastId, TenantId, SiteId, Status, IncidentId, SeverityCode, ChannelCode, Headline, Message,
        TargetArea, IssuedAtUtc, DeliveryConfirmedAtUtc, FailureReason, CreatedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PublicSafetyCommunication].[PublicAlertBroadcast]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND Status IN (N'Sent', N'Delivered')
      AND IsDeleted = 0
    ORDER BY IssuedAtUtc DESC;
END
GO
