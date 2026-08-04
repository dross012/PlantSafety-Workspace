-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CommunitySafety.CommunityNotification
--  Layer:          Infrastructure - SQL Scripts
--  File:           CommunityNotification.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CommunityNotification aggregate root backing
--                  ICommunityNotificationRepository. RecipientContactId is a weak reference to
--                  [CommunitySafety].[CommunityContact] (no FK, to allow contact archival without
--                  cascading history loss). Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CommunitySafety')
BEGIN
    EXEC('CREATE SCHEMA [CommunitySafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityNotification' AND schema_id = SCHEMA_ID('CommunitySafety'))
BEGIN
    CREATE TABLE [CommunitySafety].[CommunityNotification] (
        [CommunityNotificationId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [Channel] NVARCHAR(50) NOT NULL,
        [RecipientContactId] UNIQUEIDENTIFIER NOT NULL,
        [Subject] NVARCHAR(300) NOT NULL,
        [MessageBody] NVARCHAR(MAX) NOT NULL,
        [RelatedConcernId] UNIQUEIDENTIFIER NULL,
        [RelatedIncidentId] UNIQUEIDENTIFIER NULL,
        [SentAtUtc] DATETIME2(3) NOT NULL,
        [AcknowledgedAtUtc] DATETIME2(3) NULL,
        [FailureReason] NVARCHAR(1000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityNotification_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CommunityNotification_TenantId_SiteId_SentAtUtc]
        ON [CommunitySafety].[CommunityNotification] ([TenantId], [SiteId], [SentAtUtc])
        INCLUDE ([Status], [Channel], [RecipientContactId]);
END
GO
