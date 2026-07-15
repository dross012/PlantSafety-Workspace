-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RegulatoryRequirements
--  Layer:          Infrastructure - SQL Scripts
--  File:           RegulatoryRequirements.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RegulatoryRequirements read-model aggregate backing
--                  IRegulatoryRequirementsRepository.GetRegulatoryRequirementsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RegulatoryRequirements' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RegulatoryRequirements] (
        [RequirementId]           UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [RequirementTitle]        NVARCHAR(300)       NOT NULL,
        [Jurisdiction]            NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RegulatoryRequirements_TenantId_EffectiveAtUtc]
        ON [Compliance].[RegulatoryRequirements] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO