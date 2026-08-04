-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CyberPhysicalSecurity
--  Layer:          Infrastructure -- SQL Scripts
--  File:           SensorIntegrityCheck.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the SensorIntegrityCheck aggregate root (Sensor Integrity
--                  Monitoring subdomain). Append-only (integrity-check history is immutable).
--                  NextCheckDueUtc backs the recurring sensor integrity check scheduling hook.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CyberPhysicalSecurity')
    EXEC('CREATE SCHEMA CyberPhysicalSecurity AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SensorIntegrityCheck' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
BEGIN
    CREATE TABLE [CyberPhysicalSecurity].[SensorIntegrityCheck]
    (
        SensorIntegrityCheckId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_SensorIntegrityCheck PRIMARY KEY,
        TenantId                 UNIQUEIDENTIFIER NOT NULL,
        SiteId                   UNIQUEIDENTIFIER NOT NULL,
        OtAssetId                UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_SensorIntegrityCheck_OtAsset REFERENCES [CyberPhysicalSecurity].[OtAsset] (OtAssetId),
        CheckType                NVARCHAR(20)     NOT NULL,
        Result                   NVARCHAR(20)     NOT NULL,
        Details                  NVARCHAR(2000)   NOT NULL CONSTRAINT DF_SensorIntegrityCheck_Details DEFAULT (N''),
        CheckedAtUtc             DATETIME2(3)     NOT NULL,
        NextCheckDueUtc          DATETIME2(3)     NULL,
        CreatedAtUtc             DATETIME2(3)     NOT NULL,
        CreatedBy                UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_SensorIntegrityCheck_Tenant_Asset_CheckedAt ON [CyberPhysicalSecurity].[SensorIntegrityCheck] (TenantId, OtAssetId, CheckedAtUtc DESC);
    CREATE NONCLUSTERED INDEX IX_SensorIntegrityCheck_Tenant_NextDue ON [CyberPhysicalSecurity].[SensorIntegrityCheck] (TenantId, NextCheckDueUtc);
END
GO
