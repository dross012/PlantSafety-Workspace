-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditTimeline
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_OutboxMessages_GetByTenant.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Read-only, tenant-scoped, deterministic (no dynamic SQL) query over the
--                  cross-module [System].[OutboxMessages] event log. Backs both
--                  IAuditTimelineEventSourceRepository (event-timeline reconstruction) and
--                  ISchedulingDecisionHistoryProvider (scheduling-decision history reconstructed
--                  purely from durable cross-module integration/domain events, per Handbook Part
--                  IX Canonical Dependency Rules -- no hard Scheduling module dependency).
--                  Entity-id/event-name filtering happens in the Infrastructure C# layer against
--                  the JSON Payload column, since OutboxMessages carries no EntityId column of
--                  its own (payload shape varies per module). Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [Auditing].[usp_OutboxMessages_GetByTenant]
    @TenantId NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN
    BEGIN TRY
        SELECT MessageId, EventType, SchemaVersion, TenantId, ActorId, CorrelationId, OccurredAt, Payload
        FROM [System].[OutboxMessages]
        WHERE TenantId = @TenantId
        ORDER BY OccurredAt ASC;
    END TRY
    BEGIN CATCH
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'TenantId=', @TenantId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

        THROW;
    END CATCH
END
GO
