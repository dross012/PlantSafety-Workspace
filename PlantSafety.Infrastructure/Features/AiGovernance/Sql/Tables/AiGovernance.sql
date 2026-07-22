-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.AiGovernance
--  Layer:          Infrastructure – SQL Scripts
--  File:           AiGovernance.sql
--  Created:        2026-07-22
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the governed AI/ML model registry, backing
--                  IAiGovernanceRepository (usp_Model_Create/GetById/List).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AiGovernance')
BEGIN
    EXEC('CREATE SCHEMA [AiGovernance]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Model' AND schema_id = SCHEMA_ID('AiGovernance'))
BEGIN
    CREATE TABLE [AiGovernance].[Model] (
        ModelId           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId            UNIQUEIDENTIFIER NULL,
        ModelName         NVARCHAR(200)    NOT NULL,
        ModelVersion      NVARCHAR(50)     NOT NULL,
        GovernanceStatus  NVARCHAR(50)     NOT NULL,
        RiskTier          NVARCHAR(20)     NOT NULL,
        DriftStatus       NVARCHAR(50)     NOT NULL,
        LastReviewedAtUtc DATETIME2(3)     NOT NULL,
        ReviewerName      NVARCHAR(200)    NULL,
        IsDeleted         BIT              NOT NULL CONSTRAINT DF_AiGovernance_Model_IsDeleted DEFAULT (0),
        CorrelationId     UNIQUEIDENTIFIER NULL,
        CreatedAtUtc      DATETIME2(3)     NOT NULL CONSTRAINT DF_AiGovernance_Model_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CreatedBy         UNIQUEIDENTIFIER NULL,
        ModifiedAtUtc     DATETIME2(3)     NULL,
        ModifiedBy        UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_AiGovernance_Model_TenantId_SiteId] ON [AiGovernance].[Model] (TenantId, SiteId);
    CREATE UNIQUE NONCLUSTERED INDEX [UX_AiGovernance_Model_TenantId_Name_Version] ON [AiGovernance].[Model] (TenantId, ModelName, ModelVersion);
END
GO
