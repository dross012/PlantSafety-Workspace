-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyChemicalHandling
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyChemicalHandling.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyChemicalHandling read-model aggregate backing
--                  IChemicalHandlingRepository.GetChemicalHandlingPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyChemicalHandling' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyChemicalHandling] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ChemicalName]            NVARCHAR(300)       NOT NULL,
        [HazardClass]             NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyChemicalHandling_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyChemicalHandling] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO