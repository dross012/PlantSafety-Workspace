-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CommunicationLogEntry_GetAllActive.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns all CommunicationLogEntry rows. Backs
--                  ICommunicationLogEntryRepository.ListAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_CommunicationLogEntry_GetAllActive]
AS
SET NOCOUNT ON;
BEGIN
    SELECT CommunicationLogEntryId, TenantId, SiteId, IncidentId, RelatedIncidentNotificationId, RelatedPublicAlertBroadcastId,
        Direction, Medium, CounterpartyName, Summary, OccurredAtUtc, CreatedAtUtc, CreatedBy
    FROM [PublicSafetyCommunication].[CommunicationLogEntry];
END
GO
