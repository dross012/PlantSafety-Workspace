-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure - SQL Scripts
--  File:           RequestMetricEvent.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
--  SchemaVersion:  2026.1
--  Description:    Per-HTTP-request latency/outcome sample table backing the
--                  PlatformHealthDashboard BI dashboard (Handbook Phase 6 §5.3 Metrics &
--                  Dashboards). Written to by RequestMetricsCaptureService (a bounded-channel
--                  hosted service draining samples recorded by RequestLoggingMiddleware via
--                  IRequestMetricsRecorder -- never a synchronous per-request insert). Read by
--                  usp_PlatformHealthDashboard_GetSummary for throughput/latency-percentile/
--                  error-rate aggregation. Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PlatformHealth')
BEGIN
    EXEC('CREATE SCHEMA [PlatformHealth]');
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'PlatformHealth' AND TABLE_NAME = 'RequestMetricEvent')
BEGIN
    CREATE TABLE [PlatformHealth].[RequestMetricEvent] (
        [RequestMetricEventId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TenantId]             UNIQUEIDENTIFIER NOT NULL,
        [CorrelationId]        UNIQUEIDENTIFIER NOT NULL,
        [Method]               NVARCHAR(16)     NOT NULL,
        [Path]                 NVARCHAR(512)    NOT NULL,
        [StatusCode]           INT              NOT NULL,
        [ElapsedMs]            BIGINT           NOT NULL,
        [OccurredAtUtc]        DATETIME2(3)     NOT NULL CONSTRAINT DF_RequestMetricEvent_OccurredAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_RequestMetricEvent_TenantId_OccurredAtUtc
        ON [PlatformHealth].[RequestMetricEvent] ([TenantId], [OccurredAtUtc]);
END
GO
