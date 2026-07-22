-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PPETrainingFit.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PPETrainingFit aggregate root (IPPETrainingFitRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PPE')
    EXEC('CREATE SCHEMA PPE AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PPETrainingFit' AND schema_id = SCHEMA_ID('PPE'))
BEGIN
    CREATE TABLE [PPE].[PPETrainingFit]
    (
        PPETrainingFitId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PPETrainingFit PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        WorkerId UNIQUEIDENTIFIER NOT NULL,
        TrainingType NVARCHAR(400) NOT NULL,
        FitTestExpiresAtUtc DATETIME2(3) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PPETrainingFit_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PPETrainingFit_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PPETrainingFit_TenantId_Site ON [PPE].[PPETrainingFit] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPETrainingFit_Status ON [PPE].[PPETrainingFit] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPETrainingFit_CreatedAtUtc ON [PPE].[PPETrainingFit] (CreatedAtUtc);
END
GO
