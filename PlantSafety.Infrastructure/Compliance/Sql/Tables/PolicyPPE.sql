-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyPPE
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyPPE.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyPPE read-model aggregate backing
--                  IPPERepository.GetPPEPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyPPE' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyPPE] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [PPECategory]             NVARCHAR(300)       NOT NULL,
        [RequiredEquipment]       NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyPPE_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyPPE] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PolicyPPE]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PolicyPPE] ADD RowVersion ROWVERSION NOT NULL;
END
GO