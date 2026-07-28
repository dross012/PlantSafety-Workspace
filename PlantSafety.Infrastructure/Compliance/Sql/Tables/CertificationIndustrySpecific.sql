-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.IndustrySpecific
--  Layer:          Infrastructure – SQL Scripts
--  File:           CertificationIndustrySpecific.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Certification.IndustrySpecific read-model, backing
--                  IIndustrySpecificRepository.GetIndustrySpecificAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationIndustrySpecific' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationIndustrySpecific] (
        CertificationId  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        HolderName       NVARCHAR(200)    NOT NULL,
        IndustryStandard NVARCHAR(150)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        IssuedAtUtc      DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        VerifiedBy       UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationIndustrySpecific_TenantId_SiteId] ON [Compliance].[CertificationIndustrySpecific] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationIndustrySpecific_IssuedAtUtc] ON [Compliance].[CertificationIndustrySpecific] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[CertificationIndustrySpecific]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[CertificationIndustrySpecific] ADD RowVersion ROWVERSION NOT NULL;
END
GO
