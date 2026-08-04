-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PublicAlertBroadcast_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PublicAlertBroadcast row by identity, projecting RowVersion
--                  as a string ETag. Backs IPublicAlertBroadcastRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_PublicAlertBroadcast_GetById]
    @PublicAlertBroadcastId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PublicAlertBroadcastId, TenantId, SiteId, Status, IncidentId, SeverityCode, ChannelCode, Headline, Message,
        TargetArea, IssuedAtUtc, IssuedBy, DeliveryConfirmedAtUtc, FailureReason, CancelledAtUtc,
        CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1) AS ETag
    FROM [PublicSafetyCommunication].[PublicAlertBroadcast]
    WHERE PublicAlertBroadcastId = @PublicAlertBroadcastId AND IsDeleted = 0;
END
GO
