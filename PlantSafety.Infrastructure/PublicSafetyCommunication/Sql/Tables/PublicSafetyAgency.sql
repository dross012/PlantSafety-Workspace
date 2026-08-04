-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PublicSafetyCommunication
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PublicSafetyAgency.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PublicSafetyAgency aggregate root (IPublicSafetyAgencyRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PublicSafetyCommunication')
    EXEC('CREATE SCHEMA PublicSafetyCommunication AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PublicSafetyAgency' AND schema_id = SCHEMA_ID('PublicSafetyCommunication'))
BEGIN
    CREATE TABLE [PublicSafetyCommunication].[PublicSafetyAgency]
    (
        PublicSafetyAgencyId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PublicSafetyAgency PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        AgencyName NVARCHAR(400) NOT NULL,
        AgencyType NVARCHAR(64) NOT NULL,
        JurisdictionDescription NVARCHAR(400) NOT NULL,
        PrimaryPhoneNumber NVARCHAR(64) NOT NULL,
        PrimaryEmail NVARCHAR(320) NULL,
        VerificationStatus NVARCHAR(50) NOT NULL,
        LastVerifiedAtUtc DATETIME2(3) NULL,
        NextVerificationDueAtUtc DATETIME2(3) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PublicSafetyAgency_IsDeleted DEFAULT (0),
        RowVersion ROWVERSION NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_PublicSafetyAgency_TenantId_Site ON [PublicSafetyCommunication].[PublicSafetyAgency] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PublicSafetyAgency_Status ON [PublicSafetyCommunication].[PublicSafetyAgency] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PublicSafetyAgency_AgencyType ON [PublicSafetyCommunication].[PublicSafetyAgency] (AgencyType, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PublicSafetyAgency_NextVerificationDueAtUtc ON [PublicSafetyCommunication].[PublicSafetyAgency] (NextVerificationDueAtUtc);
END
GO
