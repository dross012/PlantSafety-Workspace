-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_IncidentNotification_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single IncidentNotification row by identity, projecting RowVersion
--                  as a string ETag. Backs IIncidentNotificationRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_IncidentNotification_GetById]
    @IncidentNotificationId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT IncidentNotificationId, TenantId, SiteId, Status, IncidentId, PublicSafetyAgencyId, ChannelCode, Message,
        DispatchedAtUtc, DispatchedBy, AcknowledgedAtUtc, AcknowledgedByContactName, ClosedAtUtc, FailureReason,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PublicSafetyCommunication].[IncidentNotification]
    WHERE IncidentNotificationId = @IncidentNotificationId AND IsDeleted = 0;
END
GO
