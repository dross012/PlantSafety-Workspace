-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.InspectionsOSHA
--  Layer:          Infrastructure - SQL Scripts
--  File:           InspectionsOSHA.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the InspectionsOSHA read-model aggregate backing
--                  IOSHARepository.GetInspectionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsOSHA' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InspectionsOSHA] (
        [InspectionId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [InspectionScope]         NVARCHAR(300)       NOT NULL,
        [ScheduledAtUtc]          DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsOSHA_TenantId_ScheduledAtUtc]
        ON [Compliance].[InspectionsOSHA] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO