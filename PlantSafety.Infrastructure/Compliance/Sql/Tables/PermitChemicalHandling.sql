-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits.ChemicalHandling
--  Layer:          Infrastructure – SQL Scripts
--  File:           PermitChemicalHandling.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permits.ChemicalHandling read-model, backing
--                  IChemicalHandlingRepository.GetChemicalHandlingAsync. Named with the
--                  Permit prefix to avoid collision with the unrelated
--                  Compliance.Policy.ChemicalHandling aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PermitChemicalHandling' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PermitChemicalHandling] (
        PermitId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId      UNIQUEIDENTIFIER NOT NULL,
        SiteId        UNIQUEIDENTIFIER NULL,
        ChemicalName  NVARCHAR(200)    NOT NULL,
        PermitHolder  NVARCHAR(200)    NOT NULL,
        Status        NVARCHAR(50)     NOT NULL,
        IssuedAtUtc   DATETIME2(3)     NOT NULL,
        ExpiresAtUtc  DATETIME2(3)     NOT NULL,
        ApprovedBy    UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PermitChemicalHandling_TenantId_SiteId] ON [Compliance].[PermitChemicalHandling] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PermitChemicalHandling_IssuedAtUtc] ON [Compliance].[PermitChemicalHandling] (IssuedAtUtc);
END
GO
