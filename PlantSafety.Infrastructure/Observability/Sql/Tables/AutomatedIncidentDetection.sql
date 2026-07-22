-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Observability
--  Layer:          Infrastructure -- SQL Scripts
--  File:           AutomatedIncidentDetection.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the AutomatedIncidentDetection aggregate root (IAutomatedIncidentDetectionRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Observability')
    EXEC('CREATE SCHEMA Observability AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AutomatedIncidentDetection' AND schema_id = SCHEMA_ID('Observability'))
BEGIN
    CREATE TABLE [Observability].[AutomatedIncidentDetection]
    (
        AutomatedIncidentDetectionId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_AutomatedIncidentDetection PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        DetectionType NVARCHAR(400) NOT NULL,
        LocationDescription NVARCHAR(400) NOT NULL,
        NotifiedSupervisorUserId UNIQUEIDENTIFIER NULL,
        AcknowledgedBy UNIQUEIDENTIFIER NULL,
        IncidentReferenceId UNIQUEIDENTIFIER NULL,
        DismissalReason NVARCHAR(400) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_AutomatedIncidentDetection_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AutomatedIncidentDetection_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_AutomatedIncidentDetection_TenantId_Site ON [Observability].[AutomatedIncidentDetection] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AutomatedIncidentDetection_Status ON [Observability].[AutomatedIncidentDetection] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_AutomatedIncidentDetection_CreatedAtUtc ON [Observability].[AutomatedIncidentDetection] (CreatedAtUtc);
END
GO
