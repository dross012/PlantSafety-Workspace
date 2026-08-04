-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           CommunicationLogEntry.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the CommunicationLogEntry aggregate root
--                  (ICommunicationLogEntryRepository). Append-only: no update path exists in the
--                  application, consistent with the aggregate's immutability-once-recorded
--                  data-governance policy. Tenant-scoped. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunicationLogEntry' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
BEGIN
    CREATE TABLE [PublicSafetyCommunication].[CommunicationLogEntry]
    (
        CommunicationLogEntryId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CommunicationLogEntry PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        IncidentId UNIQUEIDENTIFIER NULL,
        RelatedIncidentNotificationId UNIQUEIDENTIFIER NULL,
        RelatedPublicAlertBroadcastId UNIQUEIDENTIFIER NULL,
        Direction NVARCHAR(32) NOT NULL,
        Medium NVARCHAR(32) NOT NULL,
        CounterpartyName NVARCHAR(400) NOT NULL,
        Summary NVARCHAR(MAX) NOT NULL,
        OccurredAtUtc DATETIME2(3) NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_CommunicationLogEntry_TenantId_Site ON [PublicSafetyCommunication].[CommunicationLogEntry] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_CommunicationLogEntry_IncidentId ON [PublicSafetyCommunication].[CommunicationLogEntry] (IncidentId, OccurredAtUtc);
END
GO
