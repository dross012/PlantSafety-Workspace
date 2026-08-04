-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_IncidentNotification_GetStatus.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing IIncidentNotificationRepository
--                  (Application layer read model). Supports GetIncidentNotificationStatusQuery
--                  and the notification dispatch latency / agency acknowledgement rate KPIs.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_IncidentNotification_GetStatus]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @IncidentId UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT IncidentNotificationId, TenantId, SiteId, Status, IncidentId, PublicSafetyAgencyId, ChannelCode, Message,
        DispatchedAtUtc, AcknowledgedAtUtc, AcknowledgedByContactName, ClosedAtUtc, FailureReason, CreatedAtUtc,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PublicSafetyCommunication].[IncidentNotification]
    WHERE TenantId = @TenantId
      AND (@SiteId IS NULL OR SiteId = @SiteId)
      AND (@IncidentId IS NULL OR IncidentId = @IncidentId)
      AND CreatedAtUtc >= @FromUtc AND CreatedAtUtc <= @ToUtc
      AND IsDeleted = 0
    ORDER BY CreatedAtUtc DESC;
END
GO
