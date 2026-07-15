-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyHR
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyHR.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyHR read-model aggregate backing
--                  IHRRepository.GetHRPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyHR' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyHR] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [PolicyArea]              NVARCHAR(300)       NOT NULL,
        [ApplicableRole]          NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyHR_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyHR] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO