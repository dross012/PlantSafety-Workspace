-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EnvironmentalHealth
--  Layer:          Infrastructure -- SQL Scripts
--  File:           EnvironmentalHealthAggregate.sql
--  Created:        2026-08-02
--  SchemaVersion:  2026.1
--  Description:    Table backing the EnvironmentalHealth aggregate root
--                  (IEnvironmentalHealthRepository). Tenant-scoped (TenantId NOT NULL
--                  uniqueidentifier). Named "EnvironmentalHealthAggregate" (not bare
--                  "EnvironmentalHealth") to avoid any ambiguity with the pre-existing, dead
--                  EnvironmentalHealthReadings table/usp_EnvironmentalHealth_GetById/_List
--                  procs that live under this same module folder, and with the entirely
--                  separate, LIVE Features.EnvironmentalHealth "reading" vertical's own table.
--                  Phase 4 full build-out: this aggregate previously had zero persistence at
--                  all. Mirrors [Compliance].[AuditingAggregate]'s native ROWVERSION concurrency
--                  convention from day one. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EnvironmentalHealth')
    EXEC('CREATE SCHEMA EnvironmentalHealth AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EnvironmentalHealthAggregate' AND schema_id = SCHEMA_ID('EnvironmentalHealth'))
BEGIN
    CREATE TABLE [EnvironmentalHealth].[EnvironmentalHealthAggregate]
    (
        EnvironmentalHealthId  UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EnvironmentalHealthAggregate PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        SiteId                  UNIQUEIDENTIFIER NULL,
        Status                  NVARCHAR(50)     NOT NULL,
        MonitoringType          NVARCHAR(100)    NOT NULL,
        MeasuredValue           DECIMAL(18,4)    NULL,
        Unit                    NVARCHAR(50)     NOT NULL,
        Threshold               DECIMAL(18,4)    NOT NULL CONSTRAINT DF_EnvironmentalHealthAggregate_Threshold DEFAULT (0),
        ThresholdExceeded       BIT              NOT NULL CONSTRAINT DF_EnvironmentalHealthAggregate_ThresholdExceeded DEFAULT (0),
        EvaluatedStatus         NVARCHAR(50)     NOT NULL CONSTRAINT DF_EnvironmentalHealthAggregate_EvaluatedStatus DEFAULT (N'Normal'),
        RecordedAtUtc           DATETIME2(3)     NULL,
        CreatedAtUtc            DATETIME2(3)     NOT NULL,
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc            DATETIME2(3)     NULL,
        UpdatedBy               UNIQUEIDENTIFIER NULL,
        IsDeleted               BIT              NOT NULL CONSTRAINT DF_EnvironmentalHealthAggregate_IsDeleted DEFAULT (0),
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_EnvironmentalHealthAggregate_TenantId_Site ON [EnvironmentalHealth].[EnvironmentalHealthAggregate] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EnvironmentalHealthAggregate_Status ON [EnvironmentalHealth].[EnvironmentalHealthAggregate] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_EnvironmentalHealthAggregate_CreatedAtUtc ON [EnvironmentalHealth].[EnvironmentalHealthAggregate] (CreatedAtUtc);
END
GO
