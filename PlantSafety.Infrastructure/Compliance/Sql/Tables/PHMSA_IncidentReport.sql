-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.PHMSA.Incidents
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PHMSA_IncidentReport.sql
--  Created:        2026-08-07
--  SchemaVersion:  2026.1
--  Description:    Table backing the IncidentReport aggregate root (Incidents module,
--                  PlantSafety.Domain.Compliance.PHMSA.Incidents) -- rich aggregate persistence,
--                  tenant-scoped, mirroring [Compliance].[Permits]. No list-valued fields on this
--                  aggregate, so no JSON columns are required here. MaterialInvolvedId is an
--                  optional FK to [Compliance].[PHMSA_HazmatMaterial] (an incident may involve a
--                  material that has not been formally classified in this system, hence the
--                  free-text MaterialInvolvedDescription fallback) and is indexed per FK-indexing
--                  policy.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Compliance')
    EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PHMSA_IncidentReport' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PHMSA_IncidentReport]
    (
        IncidentReportId          UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PHMSA_IncidentReport PRIMARY KEY,
        TenantId                   UNIQUEIDENTIFIER NOT NULL,
        SiteId                     UNIQUEIDENTIFIER NULL,
        Status                     NVARCHAR(20)     NOT NULL,
        IncidentType                 NVARCHAR(30)     NOT NULL,
        MaterialInvolvedId            UNIQUEIDENTIFIER NULL CONSTRAINT FK_PHMSA_IncidentReport_HazmatMaterial REFERENCES [Compliance].[PHMSA_HazmatMaterial] (HazmatMaterialId),
        MaterialInvolvedDescription    NVARCHAR(MAX)    NULL,
        Injuries                        INT              NOT NULL CONSTRAINT DF_PHMSA_IncidentReport_Injuries DEFAULT (0),
        FatalitiesInvolved                BIT              NOT NULL CONSTRAINT DF_PHMSA_IncidentReport_FatalitiesInvolved DEFAULT (0),
        PropertyDamageEstimateUsd           DECIMAL(18,2)    NULL,
        Narrative                             NVARCHAR(MAX)    NOT NULL,
        IncidentDateUtc                        DATETIME2(3)     NOT NULL,
        Location                                 NVARCHAR(300)    NOT NULL,
        IsReportableToPhmsa                       BIT              NOT NULL CONSTRAINT DF_PHMSA_IncidentReport_IsReportableToPhmsa DEFAULT (0),
        CreatedAtUtc                               DATETIME2(3)     NOT NULL,
        CreatedBy                                   UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc                                 DATETIME2(3)     NULL,
        UpdatedBy                                     UNIQUEIDENTIFIER NULL,
        IsDeleted                                     BIT              NOT NULL CONSTRAINT DF_PHMSA_IncidentReport_IsDeleted DEFAULT (0),
        RowVersion                                     ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_PHMSA_IncidentReport_TenantId_Site ON [Compliance].[PHMSA_IncidentReport] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_IncidentReport_Status ON [Compliance].[PHMSA_IncidentReport] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PHMSA_IncidentReport_CreatedAtUtc ON [Compliance].[PHMSA_IncidentReport] (CreatedAtUtc);
    CREATE NONCLUSTERED INDEX IX_PHMSA_IncidentReport_TenantId ON [Compliance].[PHMSA_IncidentReport] (TenantId);
    CREATE NONCLUSTERED INDEX IX_PHMSA_IncidentReport_IncidentDateUtc ON [Compliance].[PHMSA_IncidentReport] (IncidentDateUtc);
    CREATE NONCLUSTERED INDEX IX_PHMSA_IncidentReport_IsReportableToPhmsa ON [Compliance].[PHMSA_IncidentReport] (IsReportableToPhmsa);
    CREATE NONCLUSTERED INDEX IX_PHMSA_IncidentReport_MaterialInvolvedId ON [Compliance].[PHMSA_IncidentReport] (MaterialInvolvedId);
END
GO
