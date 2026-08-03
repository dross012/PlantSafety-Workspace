-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Billing.Metering
--  Layer:          Infrastructure -- SQL Scripts
--  File:           UsageMeteringSnapshot.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
--  SchemaVersion:  2026.1
--  Description:    Table backing UsageMeteringSnapshot persistence. New [Billing] schema in
--                  PlantSafety.CoreDB, alongside the pre-existing [Tenancy] schema. Additive-only
--                  evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Billing')
BEGIN
    EXEC('CREATE SCHEMA [Billing]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UsageMeteringSnapshot' AND schema_id = SCHEMA_ID('Billing'))
BEGIN
    CREATE TABLE [Billing].[UsageMeteringSnapshot] (
        [UsageMeteringSnapshotId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]                UNIQUEIDENTIFIER NOT NULL,
        [PeriodYear]              INT              NOT NULL,
        [PeriodMonth]             INT              NOT NULL,
        [CapturedAtUtc]           DATETIME2(3)     NOT NULL,
        [RecordedValuesJson]      NVARCHAR(MAX)    NOT NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_UsageMeteringSnapshot_TenantPeriod
        ON [Billing].[UsageMeteringSnapshot] (TenantId, PeriodYear, PeriodMonth);
END
GO
