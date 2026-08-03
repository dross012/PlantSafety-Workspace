-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_RequestMetricEvent_InsertBatch.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
--  SchemaVersion:  2026.1
--  Description:    Single-row insert executed once per drained sample by
--                  RequestMetricsCaptureService. Dapper's ExecuteAsync natively batches an
--                  IEnumerable<TParam> into one round-trip-efficient multi-exec call, so a
--                  single-row proc is sufficient here (mirrors usp_DeadLetterEvents_Insert's
--                  single-row shape). Additive-only evolution.
-- ============================================================================

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'usp_RequestMetricEvent_InsertBatch' AND schema_id = SCHEMA_ID('PlatformHealth'))
BEGIN
    DROP PROCEDURE [PlatformHealth].[usp_RequestMetricEvent_InsertBatch];
END
GO

CREATE PROCEDURE [PlatformHealth].[usp_RequestMetricEvent_InsertBatch]
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @Method        NVARCHAR(16),
    @Path          NVARCHAR(512),
    @StatusCode    INT,
    @ElapsedMs     BIGINT,
    @OccurredAtUtc DATETIME2(3)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [PlatformHealth].[RequestMetricEvent]
        ([TenantId], [CorrelationId], [Method], [Path], [StatusCode], [ElapsedMs], [OccurredAtUtc])
    VALUES
        (@TenantId, @CorrelationId, @Method, @Path, @StatusCode, @ElapsedMs, @OccurredAtUtc);
END
GO
