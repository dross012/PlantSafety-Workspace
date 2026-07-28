-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.DOT
--  Layer:          Infrastructure – SQL Scripts
--  File:           CertificationDOT.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Certification.DOT read-model, backing
--                  IDOTRepository.GetDOTAsync. Named with the Certification
--                  prefix to avoid collision with the unrelated
--                  Compliance.RegulatoryCalendar DOT inspection/renewal aggregates.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationDOT' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationDOT] (
        CertificationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        DriverName      NVARCHAR(200)    NOT NULL,
        LicenseClass    NVARCHAR(50)     NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationDOT_TenantId_SiteId] ON [Compliance].[CertificationDOT] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationDOT_IssuedAtUtc] ON [Compliance].[CertificationDOT] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[CertificationDOT]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[CertificationDOT] ADD RowVersion ROWVERSION NOT NULL;
END
GO
