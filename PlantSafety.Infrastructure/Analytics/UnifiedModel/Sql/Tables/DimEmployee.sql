-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           DimEmployee.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Employee dimension table (SCD Type 2). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DimEmployee' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[DimEmployee]
    (
        SurrogateKey       UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DimEmployee PRIMARY KEY,
        BusinessKey         UNIQUEIDENTIFIER NOT NULL,
        EffectiveFromUtc     DATETIME2(3)     NOT NULL,
        EffectiveToUtc       DATETIME2(3)     NULL,
        IsCurrent             BIT              NOT NULL CONSTRAINT DF_DimEmployee_IsCurrent DEFAULT (1),
        RevisionNumber         INT              NOT NULL CONSTRAINT DF_DimEmployee_Revision DEFAULT (1),
        TenantId               UNIQUEIDENTIFIER NOT NULL,
        DisplayName             NVARCHAR(200)    NOT NULL,
        JobTitle                 NVARCHAR(150)    NOT NULL,
        Department                NVARCHAR(150)    NOT NULL,
        AssignedSiteKey            UNIQUEIDENTIFIER NULL,
        EmploymentStatus            NVARCHAR(30)     NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_DimEmployee_BusinessKey_Current ON [AnalyticsUnifiedModel].[DimEmployee] (BusinessKey, IsCurrent);
    CREATE NONCLUSTERED INDEX IX_DimEmployee_Tenant ON [AnalyticsUnifiedModel].[DimEmployee] (TenantId, IsCurrent);
    CREATE UNIQUE NONCLUSTERED INDEX UX_DimEmployee_BusinessKey_CurrentOnly
        ON [AnalyticsUnifiedModel].[DimEmployee] (BusinessKey) WHERE IsCurrent = 1;
END
GO
