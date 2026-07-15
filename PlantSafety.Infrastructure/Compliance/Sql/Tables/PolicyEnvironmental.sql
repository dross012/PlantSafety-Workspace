-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyEnvironmental
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyEnvironmental.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyEnvironmental read-model aggregate backing
--                  IEnvironmentalRepository.GetEnvironmentalPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyEnvironmental' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyEnvironmental] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EnvironmentalAspect]     NVARCHAR(300)       NOT NULL,
        [RegulatoryReference]     NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyEnvironmental_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyEnvironmental] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO