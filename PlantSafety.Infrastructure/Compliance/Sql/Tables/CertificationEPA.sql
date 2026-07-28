-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.EPA
--  Layer:          Infrastructure – SQL Scripts
--  File:           CertificationEPA.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Certification.EPA read-model, backing
--                  IEPARepository.GetEPAAsync. Named with the Certification
--                  prefix to avoid collision with the unrelated
--                  Compliance.RegulatoryCalendar.Deadlines.EPA aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationEPA' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationEPA] (
        CertificationId  UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        SiteId           UNIQUEIDENTIFIER NULL,
        PermitHolderName NVARCHAR(200)    NOT NULL,
        EpaProgram       NVARCHAR(100)    NOT NULL,
        Status           NVARCHAR(50)     NOT NULL,
        IssuedAtUtc      DATETIME2(3)     NOT NULL,
        ExpiresAtUtc     DATETIME2(3)     NOT NULL,
        VerifiedBy       UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationEPA_TenantId_SiteId] ON [Compliance].[CertificationEPA] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationEPA_IssuedAtUtc] ON [Compliance].[CertificationEPA] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[CertificationEPA]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[CertificationEPA] ADD RowVersion ROWVERSION NOT NULL;
END
GO
