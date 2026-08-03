-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           DimShift.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Shift reference-lookup dimension table. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DimShift' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[DimShift]
    (
        ShiftKey         UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DimShift PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteKey            UNIQUEIDENTIFIER NOT NULL,
        ShiftName           NVARCHAR(60)     NOT NULL,
        StartTimeLocal        TIME(0)          NOT NULL,
        EndTimeLocal            TIME(0)          NOT NULL,
        DurationHours             FLOAT            NOT NULL,
        CrossesMidnight             BIT              NOT NULL CONSTRAINT DF_DimShift_CrossesMidnight DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_DimShift_Tenant_Site ON [AnalyticsUnifiedModel].[DimShift] (TenantId, SiteKey);
END
GO
