-- =============================================
-- Phase 13 - Monitoring, Diagnostics, Governance Tooling
-- Script:        132-server-ddl-trigger.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Purpose:       Server-scoped DDL trigger for schema-protection logging into a
--                queryable, structured table (complements, does not replace, the
--                file-based SQL Server Audit from script 130).
-- Judgment call (per operating rule 8 - stricter compliance, documented assumption):
--   The Server Audit (130) already captures SCHEMA_OBJECT_CHANGE_GROUP for the 8
--   primary databases in a durable, tamper-evident file target -- that is the
--   authoritative security-audit record. This trigger is added ANYWAY, deliberately,
--   because it provides a second, independent, T-SQL-queryable value: a live table
--   (no need to parse .sqlaudit files) capturing EVENTDATA() including the exact
--   command text, restricted to PlantSafety.* databases only, for fast DBA/governance
--   queries and future dashboarding. It is LOGGING-ONLY -- it never blocks, denies, or
--   rolls back a DDL statement. A blocking/preventive trigger was NOT built: the task
--   only asked for "schema-protection LOGGING," and a blocking trigger risks locking
--   developers out of legitimate schema changes with 0 rows of production traffic to
--   justify that risk yet -- the stricter-but-safer choice here is log-only.
-- Notes:
--   - ON ALL SERVER FOR DDL_DATABASE_LEVEL_EVENTS fires for DDL in every database on
--     the instance; the trigger body filters to PlantSafety.% databases only so it
--     does not pollute the log with tempdb/master noise from unrelated tooling.
--   - Wrapped in TRY/CATCH: a logging failure must never fail the underlying DDL
--     operation (same "audit is best-effort, never blocks primary operation"
--     principle used throughout the canonical stored-procedure template).
--   - Idempotent.
-- =============================================
USE [PlantSafety.AuditDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = N'dbo' AND t.name = N'ServerDdlEventLog')
BEGIN
    CREATE TABLE dbo.ServerDdlEventLog
    (
        ServerDdlEventLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ServerDdlEventLog PRIMARY KEY CLUSTERED,
        EventTimeUtc         datetime2(7)   NOT NULL CONSTRAINT DF_ServerDdlEventLog_EventTimeUtc DEFAULT (SYSUTCDATETIME()),
        EventType            nvarchar(200)  NOT NULL,
        DatabaseName         nvarchar(400)  NULL,
        SchemaName           nvarchar(400)  NULL,
        ObjectName           nvarchar(400)  NULL,
        ObjectType           nvarchar(200)  NULL,
        LoginName            nvarchar(400)  NULL,
        HostName             nvarchar(400)  NULL,
        ApplicationName      nvarchar(400)  NULL,
        TsqlCommand          nvarchar(max)  NULL,
        EventXml             xml            NULL
    );
END
GO

------------------------------------------------------------
-- Server trigger (drop/recreate for idempotency)
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.server_triggers WHERE name = N'trg_Server_DDL_SchemaProtection')
    DROP TRIGGER trg_Server_DDL_SchemaProtection ON ALL SERVER;
GO

CREATE TRIGGER trg_Server_DDL_SchemaProtection
ON ALL SERVER
FOR DDL_DATABASE_LEVEL_EVENTS
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @data xml = EVENTDATA();
        DECLARE @dbname nvarchar(400) = @data.value('(/EVENT_INSTANCE/DatabaseName)[1]', 'nvarchar(400)');

        -- Only log DDL for PlantSafety.* databases -- avoids noise from tempdb/master/
        -- unrelated local tooling and keeps this table scoped to in-scope governance surface.
        IF @dbname LIKE N'PlantSafety.%'
        BEGIN
            INSERT INTO [PlantSafety.AuditDB].dbo.ServerDdlEventLog
            (
                EventType, DatabaseName, SchemaName, ObjectName, ObjectType,
                LoginName, HostName, ApplicationName, TsqlCommand, EventXml
            )
            VALUES
            (
                @data.value('(/EVENT_INSTANCE/EventType)[1]', 'nvarchar(200)'),
                @dbname,
                @data.value('(/EVENT_INSTANCE/SchemaName)[1]', 'nvarchar(400)'),
                @data.value('(/EVENT_INSTANCE/ObjectName)[1]', 'nvarchar(400)'),
                @data.value('(/EVENT_INSTANCE/ObjectType)[1]', 'nvarchar(200)'),
                ORIGINAL_LOGIN(),
                HOST_NAME(),
                APP_NAME(),
                @data.value('(/EVENT_INSTANCE/TSQLCommand/CommandText)[1]', 'nvarchar(max)'),
                @data
            );
        END
    END TRY
    BEGIN CATCH
        -- Logging must never block or fail the underlying DDL operation.
    END CATCH
END
GO
