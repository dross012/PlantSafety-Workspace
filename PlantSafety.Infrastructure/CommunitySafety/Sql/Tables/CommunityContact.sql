-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CommunitySafety.CommunityContact
--  Layer:          Infrastructure - SQL Scripts
--  File:           CommunityContact.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CommunityContact aggregate root backing
--                  ICommunityContactRepository. TenantId is NOT NULL so this table is
--                  automatically picked up by the live-discovery Row-Level-Security policy in
--                  90-row-level-security.sql (Security.fn_TenantAccessPredicate). PII-minimal
--                  per the CommunitySafety data-governance mandate: no home address, no date of
--                  birth. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CommunitySafety')
BEGIN
    EXEC('CREATE SCHEMA [CommunitySafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityContact' AND schema_id = SCHEMA_ID('CommunitySafety'))
BEGIN
    CREATE TABLE [CommunitySafety].[CommunityContact] (
        [CommunityContactId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [DisplayName] NVARCHAR(200) NOT NULL,
        [ContactType] NVARCHAR(50) NOT NULL,
        [Organization] NVARCHAR(200) NULL,
        [CommunityArea] NVARCHAR(200) NOT NULL,
        [Email] NVARCHAR(320) NOT NULL,
        [Phone] NVARCHAR(32) NULL,
        [OptedInForNotifications] BIT NOT NULL CONSTRAINT DF_CommunityContact_OptedIn DEFAULT (1),
        [ArchivedAtUtc] DATETIME2(3) NULL,
        [ArchivalReason] NVARCHAR(1000) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityContact_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CommunityContact_TenantId_SiteId]
        ON [CommunitySafety].[CommunityContact] ([TenantId], [SiteId])
        INCLUDE ([Status], [DisplayName], [ContactType]);
END
GO
