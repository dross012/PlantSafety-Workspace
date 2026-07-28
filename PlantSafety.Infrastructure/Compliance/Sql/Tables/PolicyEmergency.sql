-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PolicyEmergency
--  Layer:          Infrastructure - SQL Scripts
--  File:           PolicyEmergency.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the PolicyEmergency read-model aggregate backing
--                  IEmergencyRepository.GetEmergencyPoliciesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyEmergency' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyEmergency] (
        [PolicyId]                UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [EmergencyType]           NVARCHAR(300)       NOT NULL,
        [ResponseProcedureRef]    NVARCHAR(300)       NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [EffectiveAtUtc]          DATETIME2(3)        NOT NULL,
        [OwnerId]                 UNIQUEIDENTIFIER    NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyEmergency_TenantId_EffectiveAtUtc]
        ON [Compliance].[PolicyEmergency] ([TenantId], [EffectiveAtUtc])
        INCLUDE ([SiteId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[PolicyEmergency]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[PolicyEmergency] ADD RowVersion ROWVERSION NOT NULL;
END
GO