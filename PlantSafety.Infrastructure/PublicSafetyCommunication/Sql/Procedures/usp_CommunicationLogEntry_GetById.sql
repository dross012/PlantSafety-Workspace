-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CommunicationLogEntry_GetById.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single CommunicationLogEntry row by identity. Backs
--                  ICommunicationLogEntryRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_CommunicationLogEntry_GetById]
    @CommunicationLogEntryId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT CommunicationLogEntryId, TenantId, SiteId, IncidentId, RelatedIncidentNotificationId, RelatedPublicAlertBroadcastId,
        Direction, Medium, CounterpartyName, Summary, OccurredAtUtc, CreatedAtUtc, CreatedBy
    FROM [PublicSafetyCommunication].[CommunicationLogEntry]
    WHERE CommunicationLogEntryId = @CommunicationLogEntryId;
END
GO
