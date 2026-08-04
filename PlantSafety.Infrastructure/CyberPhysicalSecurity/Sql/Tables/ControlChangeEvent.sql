-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CyberPhysicalSecurity
--  Layer:          Infrastructure -- SQL Scripts
--  File:           ControlChangeEvent.sql
--  Created:        2026-08-04
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the ControlChangeEvent aggregate root (Anomalous Control-Change
--                  Detection subdomain). Append-only, tamper-evident: no UPDATE trigger/proc ever
--                  modifies PriorRecordHash/RecordHash/DetectedAtUtc/ChangedParameter/OldValue/
--                  NewValue once inserted -- only AuthorizationStatus may be reclassified via
--                  usp_ControlChangeEvent_ReclassifyAuthorization. RecordHash is a SHA-256 hex
--                  digest chained from PriorRecordHash (per-asset chain), computed in the Domain
--                  layer (ControlChangeEvent.ComputeRecordHash) so any retroactive tampering with a
--                  historical row is detectable by re-walking the chain.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'CyberPhysicalSecurity')
    EXEC('CREATE SCHEMA CyberPhysicalSecurity AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ControlChangeEvent' AND schema_id = SCHEMA_ID('CyberPhysicalSecurity'))
BEGIN
    CREATE TABLE [CyberPhysicalSecurity].[ControlChangeEvent]
    (
        ControlChangeEventId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_ControlChangeEvent PRIMARY KEY,
        TenantId              UNIQUEIDENTIFIER NOT NULL,
        SiteId                UNIQUEIDENTIFIER NOT NULL,
        OtAssetId             UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_ControlChangeEvent_OtAsset REFERENCES [CyberPhysicalSecurity].[OtAsset] (OtAssetId),
        ChangeDescription     NVARCHAR(2000)   NOT NULL CONSTRAINT DF_ControlChangeEvent_Desc DEFAULT (N''),
        ChangedParameter      NVARCHAR(256)    NOT NULL,
        OldValue              NVARCHAR(512)    NOT NULL CONSTRAINT DF_ControlChangeEvent_Old DEFAULT (N''),
        NewValue              NVARCHAR(512)    NOT NULL CONSTRAINT DF_ControlChangeEvent_New DEFAULT (N''),
        ChangeSourceUserId    UNIQUEIDENTIFIER NULL,
        AuthorizationStatus   NVARCHAR(20)     NOT NULL,
        DetectedAtUtc         DATETIME2(3)     NOT NULL,
        PriorRecordHash       CHAR(64)         NULL,
        RecordHash            CHAR(64)         NOT NULL,
        CreatedAtUtc          DATETIME2(3)     NOT NULL,
        CreatedBy             UNIQUEIDENTIFIER NOT NULL,
        RowVersion            ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_ControlChangeEvent_Tenant_Asset_DetectedAt ON [CyberPhysicalSecurity].[ControlChangeEvent] (TenantId, OtAssetId, DetectedAtUtc DESC);
    CREATE NONCLUSTERED INDEX IX_ControlChangeEvent_Tenant_AuthStatus ON [CyberPhysicalSecurity].[ControlChangeEvent] (TenantId, AuthorizationStatus);
END
GO
