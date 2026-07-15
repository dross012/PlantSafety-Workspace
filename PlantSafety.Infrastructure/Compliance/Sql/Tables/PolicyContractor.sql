-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyContractor
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyContractor.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyContractor read-model aggregate backing
--                  IContractorRepository.GetContractorPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyContractor' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyContractor] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [ContractorRequirement]   NVARCHAR(300)       NOT NULL,
        [ApplicableTrade]         NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyContractor_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyContractor] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO