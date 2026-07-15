-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Certification.Contractor
--  Layer:          Infrastructure – SQL Scripts
--  File:           CertificationContractor.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Certification.Contractor read-model, backing
--                  IContractorRepository.GetContractorAsync. Named with the
--                  Certification prefix to avoid collision with the unrelated
--                  Compliance.Policy.Contractor aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CertificationContractor' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[CertificationContractor] (
        CertificationId   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        ContractorName    NVARCHAR(200)    NOT NULL,
        CertificationType NVARCHAR(100)    NOT NULL,
        Status            NVARCHAR(50)     NOT NULL,
        IssuedAtUtc       DATETIME2(3)     NOT NULL,
        ExpiresAtUtc      DATETIME2(3)     NOT NULL,
        VerifiedBy        UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_CertificationContractor_TenantId_SiteId] ON [Compliance].[CertificationContractor] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_CertificationContractor_IssuedAtUtc] ON [Compliance].[CertificationContractor] (IssuedAtUtc);
END
GO
