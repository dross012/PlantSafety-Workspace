-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RenewalsISO
--  Layer:          Infrastructure - SQL Scripts
--  File:           RenewalsISO.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RenewalsISO read-model aggregate backing
--                  IISORepository.GetRenewalsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RenewalsISO' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RenewalsISO] (
        [RenewalId]               UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [CertificationStandard]   NVARCHAR(300)       NOT NULL,
        [ExpiresAtUtc]            DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RenewalsISO_TenantId_ExpiresAtUtc]
        ON [Compliance].[RenewalsISO] ([TenantId], [ExpiresAtUtc])
        INCLUDE ([SiteId]);
END
GO