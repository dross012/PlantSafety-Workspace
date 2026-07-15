-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.ISO45001
--  Layer:          Infrastructure – SQL Scripts
--  File:           CertificationISO45001.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Certification.ISO45001 read-model, backing
--                  IISO45001Repository.GetISO45001Async. Named with the
--                  Certification prefix to avoid collision with the unrelated
--                  Compliance.RegulatoryCalendar.Renewals.ISO aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationISO45001' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationISO45001] (
        CertificationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        CertifyingBody  NVARCHAR(200)    NOT NULL,
        Scope           NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationISO45001_TenantId_SiteId] ON [Compliance].[CertificationISO45001] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationISO45001_IssuedAtUtc] ON [Compliance].[CertificationISO45001] (IssuedAtUtc);
END
GO
