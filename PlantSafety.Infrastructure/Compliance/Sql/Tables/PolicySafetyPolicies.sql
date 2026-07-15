-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicySafetyPolicies
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicySafetyPolicies.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicySafetyPolicies read-model aggregate backing
--                  ISafetyPoliciesRepository.GetSafetyPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicySafetyPolicies' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicySafetyPolicies] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [PolicyName]              NVARCHAR(300)       NOT NULL,
        [Category]                NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicySafetyPolicies_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicySafetyPolicies] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO