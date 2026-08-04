-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicAlertBroadcast_GetAllActive.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all non-deleted PublicAlertBroadcast rows. Backs
--                  IPublicAlertBroadcastRepository.ListAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT PublicAlertBroadcastId, TenantId, SiteId, Status, IncidentId, SeverityCode, ChannelCode, Headline, Message,
        TargetArea, IssuedAtUtc, IssuedBy, DeliveryConfirmedAtUtc, FailureReason, CancelledAtUtc,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PublicSafetyCommunication].[PublicAlertBroadcast]
    WHERE IsDeleted = 0;
END
GO
