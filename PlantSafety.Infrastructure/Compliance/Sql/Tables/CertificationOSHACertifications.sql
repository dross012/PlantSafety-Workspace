-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.OSHACertifications
--  Layer:          Infrastructure – SQL Scripts
--  File:           CertificationOSHACertifications.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Certification.OSHACertifications read-model, backing
--                  IOSHACertificationsRepository.GetOSHACertificationsAsync. Named with the
--                  Certification prefix to avoid collision with the unrelated
--                  Compliance.RegulatoryCalendar.Inspections.OSHA aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationOSHACertifications' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationOSHACertifications] (
        CertificationId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        HolderName      NVARCHAR(200)    NOT NULL,
        OshaStandard    NVARCHAR(100)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        IssuedAtUtc     DATETIME2(3)     NOT NULL,
        ExpiresAtUtc    DATETIME2(3)     NOT NULL,
        VerifiedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationOSHACertifications_TenantId_SiteId] ON [Compliance].[CertificationOSHACertifications] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationOSHACertifications_IssuedAtUtc] ON [Compliance].[CertificationOSHACertifications] (IssuedAtUtc);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[CertificationOSHACertifications]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[CertificationOSHACertifications] ADD RowVersion ROWVERSION NOT NULL;
END
GO
