-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.InspectionsDOT
--  Layer:          Infrastructure - SQL Scripts
--  File:           InspectionsDOT.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the InspectionsDOT read-model aggregate backing
--                  IDOTRepository.GetInspectionsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InspectionsDOT' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[InspectionsDOT] (
        [InspectionId]            UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [InspectionScope]         NVARCHAR(300)       NOT NULL,
        [ScheduledAtUtc]          DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [Inspector]               UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_InspectionsDOT_TenantId_ScheduledAtUtc]
        ON [Compliance].[InspectionsDOT] ([TenantId], [ScheduledAtUtc])
        INCLUDE ([SiteId]);
END
GO