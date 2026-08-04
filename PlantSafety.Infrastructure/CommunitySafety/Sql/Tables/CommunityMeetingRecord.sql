-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CommunitySafety.CommunityMeetingRecord
--  Layer:          Infrastructure - SQL Scripts
--  File:           CommunityMeetingRecord.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the CommunityMeetingRecord aggregate root backing
--                  ICommunityMeetingRecordRepository. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CommunitySafety')
BEGIN
    EXEC('CREATE SCHEMA [CommunitySafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CommunityMeetingRecord' AND schema_id = SCHEMA_ID('CommunitySafety'))
BEGIN
    CREATE TABLE [CommunitySafety].[CommunityMeetingRecord] (
        [CommunityMeetingRecordId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [MeetingTitle] NVARCHAR(300) NOT NULL,
        [MeetingDateUtc] DATETIME2(3) NOT NULL,
        [Location] NVARCHAR(300) NOT NULL,
        [AttendeeCount] INT NOT NULL CONSTRAINT DF_CommunityMeetingRecord_AttendeeCount DEFAULT (0),
        [TopicsSummary] NVARCHAR(4000) NOT NULL,
        [ActionItems] NVARCHAR(4000) NULL,
        [RecordedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_CommunityMeetingRecord_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CommunityMeetingRecord_TenantId_SiteId_MeetingDateUtc]
        ON [CommunitySafety].[CommunityMeetingRecord] ([TenantId], [SiteId], [MeetingDateUtc])
        INCLUDE ([Status], [MeetingTitle]);
END
GO
