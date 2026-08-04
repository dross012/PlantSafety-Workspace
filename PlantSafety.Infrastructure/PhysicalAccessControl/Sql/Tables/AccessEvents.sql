-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PhysicalAccessControl.AccessEvents
--  Layer:          Infrastructure - SQL Scripts
--  File:           AccessEvents.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AccessEvent aggregate root backing IAccessEventRepository.
--                  Raw access events are retained ~1 year then aggregated/anonymized per
--                  data-governance policy (see AccessEventArchivalPolicy).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AccessEvents' AND schema_id = SCHEMA_ID('PhysicalAccessControl'))
BEGIN
    CREATE TABLE [PhysicalAccessControl].[AccessEvents] (
        [AccessEventId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AccessPointId] UNIQUEIDENTIFIER NOT NULL,
        [CredentialId] UNIQUEIDENTIFIER NULL,
        [SafetyZoneId] UNIQUEIDENTIFIER NULL,
        [EventType] NVARCHAR(100) NOT NULL,
        [Direction] NVARCHAR(20) NOT NULL,
        [OccurredAtUtc] DATETIME2(3) NOT NULL,
        [FlaggedAnomalyType] NVARCHAR(100) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_AccessEvents_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AccessEvents_TenantId_SafetyZoneId_OccurredAtUtc]
        ON [PhysicalAccessControl].[AccessEvents] ([TenantId], [SafetyZoneId], [OccurredAtUtc])
        INCLUDE ([Status], [EventType], [Direction], [AccessPointId], [CredentialId]);
END
GO
