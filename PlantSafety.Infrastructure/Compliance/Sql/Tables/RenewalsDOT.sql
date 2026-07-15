-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.RenewalsDOT
--  Layer:          Infrastructure - SQL Scripts
--  File:           RenewalsDOT.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RenewalsDOT read-model aggregate backing
--                  IDOTRepository.GetRenewalsAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RenewalsDOT' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[RenewalsDOT] (
        [RenewalId]               UNIQUEIDENTIFIER    NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER    NOT NULL,
        [SiteId]                  UNIQUEIDENTIFIER    NULL,
        [CredentialType]          NVARCHAR(300)       NOT NULL,
        [ExpiresAtUtc]            DATETIME2(3)        NOT NULL,
        [Status]                  NVARCHAR(50)        NOT NULL,
        [ResponsibleParty]        UNIQUEIDENTIFIER    NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RenewalsDOT_TenantId_ExpiresAtUtc]
        ON [Compliance].[RenewalsDOT] ([TenantId], [ExpiresAtUtc])
        INCLUDE ([SiteId]);
END
GO