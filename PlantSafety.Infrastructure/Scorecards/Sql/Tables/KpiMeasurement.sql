-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Scorecards
--  Layer:          Infrastructure -- SQL Scripts
--  File:           KpiMeasurement.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Child table backing the Scorecard.Measurements collection (KpiMeasurement
--                  entity). TenantId is denormalized from the parent Scorecard for
--                  Row-Level-Security discovery/enforcement. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'KpiMeasurement' AND schema_id = SCHEMA_ID('Scorecards'))
BEGIN
    CREATE TABLE [Scorecards].[KpiMeasurement]
    (
        KpiMeasurementId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_KpiMeasurement PRIMARY KEY,
        ScorecardId      UNIQUEIDENTIFIER NOT NULL CONSTRAINT FK_KpiMeasurement_Scorecard REFERENCES [Scorecards].[Scorecard] (ScorecardId),
        TenantId         UNIQUEIDENTIFIER NOT NULL,
        Metric           NVARCHAR(50)     NOT NULL,
        Numerator        DECIMAL(18,4)    NOT NULL,
        Denominator      DECIMAL(18,4)    NOT NULL,
        ComputedValue    DECIMAL(18,4)    NOT NULL,
        DataSourceModule NVARCHAR(200)    NOT NULL,
        RecordedAtUtc    DATETIME2(3)     NOT NULL,
        IsDeleted        BIT              NOT NULL CONSTRAINT DF_KpiMeasurement_IsDeleted DEFAULT (0)
    );

    CREATE NONCLUSTERED INDEX IX_KpiMeasurement_ScorecardId ON [Scorecards].[KpiMeasurement] (ScorecardId, IsDeleted);
END
GO
