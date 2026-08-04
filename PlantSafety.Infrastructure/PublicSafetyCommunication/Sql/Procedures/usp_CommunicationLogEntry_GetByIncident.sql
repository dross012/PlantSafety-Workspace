-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_CommunicationLogEntry_GetByIncident.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only stored procedure backing ICommunicationLogEntryRepository
--                  (Application layer read model). Supports GetCommunicationLogByIncidentQuery
--                  and the After-Action Communication Timeline dashboard. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PublicSafetyCommunication].[usp_CommunicationLogEntry_GetByIncident]
    @TenantId   UNIQUEIDENTIFIER,
    @IncidentId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT CommunicationLogEntryId, TenantId, SiteId, IncidentId, RelatedIncidentNotificationId, RelatedPublicAlertBroadcastId,
        Direction, Medium, CounterpartyName, Summary, OccurredAtUtc, CreatedAtUtc
    FROM [PublicSafetyCommunication].[CommunicationLogEntry]
    WHERE TenantId = @TenantId AND IncidentId = @IncidentId
    ORDER BY OccurredAtUtc ASC;
END
GO
