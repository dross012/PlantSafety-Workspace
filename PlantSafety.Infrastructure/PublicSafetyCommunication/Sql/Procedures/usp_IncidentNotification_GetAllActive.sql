-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_IncidentNotification_GetAllActive.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted IncidentNotification rows. Backs
--                  IIncidentNotificationRepository.ListAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_IncidentNotification_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT IncidentNotificationId, TenantId, SiteId, Status, IncidentId, PublicSafetyAgencyId, ChannelCode, Message,
        DispatchedAtUtc, DispatchedBy, AcknowledgedAtUtc, AcknowledgedByContactName, ClosedAtUtc, FailureReason,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PublicSafetyCommunication].[IncidentNotification]
    WHERE IsDeleted = 0;
END
GO
