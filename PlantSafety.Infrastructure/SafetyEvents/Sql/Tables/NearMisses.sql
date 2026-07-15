-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyEvents.NearMisses
--  Layer:          Infrastructure – SQL Scripts
--  File:           NearMisses.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the NearMisses read-model, backing
--                  INearMissesRepository.GetNearMissesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NearMisses' AND schema_id = SCHEMA_ID('SafetyEvents'))
BEGIN
    CREATE TABLE [SafetyEvents].[NearMisses] (
        NearMissId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        Category         NVARCHAR(100)    NOT NULL,
        PotentialSeverity NVARCHAR(50)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        ReportedAtUtc    DATETIME2(3)     NOT NULL,
        ReportedBy       UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_NearMisses_TenantId_SiteId] ON [SafetyEvents].[NearMisses] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_NearMisses_ReportedAtUtc] ON [SafetyEvents].[NearMisses] (ReportedAtUtc);
END
GO
