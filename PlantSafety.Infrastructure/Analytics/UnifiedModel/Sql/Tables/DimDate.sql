-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Analytics.UnifiedModel
--  Layer:          Infrastructure -- SQL Scripts
--  File:           DimDate.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Static calendar dimension table. Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'AnalyticsUnifiedModel')
    EXEC('CREATE SCHEMA AnalyticsUnifiedModel AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DimDate' AND schema_id = SCHEMA_ID('AnalyticsUnifiedModel'))
BEGIN
    CREATE TABLE [AnalyticsUnifiedModel].[DimDate]
    (
        DateKey        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DimDate PRIMARY KEY,
        CalendarDate     DATE             NOT NULL,
        Year              INT              NOT NULL,
        Quarter            INT              NOT NULL,
        Month               INT              NOT NULL,
        MonthName            NVARCHAR(20)     NOT NULL,
        IsoWeek                INT              NOT NULL,
        DayOfWeekIso            INT              NOT NULL,
        IsWeekend                BIT              NOT NULL CONSTRAINT DF_DimDate_Weekend DEFAULT (0),
        IsHoliday                  BIT              NOT NULL CONSTRAINT DF_DimDate_Holiday DEFAULT (0),
        FiscalYear                   INT              NULL,
        FiscalPeriod                   INT              NULL
    );

    CREATE UNIQUE NONCLUSTERED INDEX UX_DimDate_CalendarDate ON [AnalyticsUnifiedModel].[DimDate] (CalendarDate);
END
GO
