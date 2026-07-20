-- =============================================
-- Phase 13 - Monitoring, Diagnostics, Governance Tooling
-- Script:        131-extended-events-sessions.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Purpose:       Two Extended Events sessions for DBA-level visibility independent of
--                application-level logging:
--                  (a) PlantSafety_ErrorCapture     - errors with severity >= 16
--                      (mirrors what Phase 7 ErrorDB.dbo.ErrorLog captures at the app
--                      layer, but works even if app error-handling has a bug/gap)
--                  (b) PlantSafety_LongRunningQueries - RPC/batch completions with
--                      duration > 1000ms, for once real traffic exists
-- Notes:
--   - Targets are event_file targets written to the same governance directory as the
--     Server Audit (C:\Users\dross\SQLAudit\PlantSafety\) -- the SQL Server service
--     account already has write access there from script 130.
--   - Idempotent: drops session if it already exists before recreating.
--   - STARTUP_STATE = ON so both sessions survive a service restart.
-- =============================================

------------------------------------------------------------
-- (a) Error capture session
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.server_event_sessions WHERE name = N'PlantSafety_ErrorCapture')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.dm_xe_sessions WHERE name = N'PlantSafety_ErrorCapture')
        ALTER EVENT SESSION [PlantSafety_ErrorCapture] ON SERVER STATE = STOP;
    DROP EVENT SESSION [PlantSafety_ErrorCapture] ON SERVER;
END
GO

CREATE EVENT SESSION [PlantSafety_ErrorCapture]
ON SERVER
ADD EVENT sqlserver.error_reported
(
    ACTION (sqlserver.sql_text, sqlserver.session_id, sqlserver.database_name,
            sqlserver.client_hostname, sqlserver.username, sqlserver.client_app_name)
    WHERE ([severity] >= (16))
)
ADD TARGET package0.event_file
(
    SET filename = N'C:\Users\dross\SQLAudit\PlantSafety\XE_ErrorCapture.xel',
        max_file_size = (50),
        max_rollover_files = (10)
)
WITH
(
    MAX_MEMORY = 4096 KB,
    EVENT_RETENTION_MODE = ALLOW_SINGLE_EVENT_LOSS,
    MAX_DISPATCH_LATENCY = 5 SECONDS,
    STARTUP_STATE = ON
);
GO

ALTER EVENT SESSION [PlantSafety_ErrorCapture] ON SERVER STATE = START;
GO

------------------------------------------------------------
-- (b) Long-running query capture session (duration > 1000 ms = 1,000,000 microseconds)
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.server_event_sessions WHERE name = N'PlantSafety_LongRunningQueries')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.dm_xe_sessions WHERE name = N'PlantSafety_LongRunningQueries')
        ALTER EVENT SESSION [PlantSafety_LongRunningQueries] ON SERVER STATE = STOP;
    DROP EVENT SESSION [PlantSafety_LongRunningQueries] ON SERVER;
END
GO

CREATE EVENT SESSION [PlantSafety_LongRunningQueries]
ON SERVER
ADD EVENT sqlserver.rpc_completed
(
    ACTION (sqlserver.sql_text, sqlserver.session_id, sqlserver.database_name,
            sqlserver.client_hostname, sqlserver.username, sqlserver.client_app_name)
    WHERE ([duration] > (1000000))
),
ADD EVENT sqlserver.sql_batch_completed
(
    ACTION (sqlserver.sql_text, sqlserver.session_id, sqlserver.database_name,
            sqlserver.client_hostname, sqlserver.username, sqlserver.client_app_name)
    WHERE ([duration] > (1000000))
)
ADD TARGET package0.event_file
(
    SET filename = N'C:\Users\dross\SQLAudit\PlantSafety\XE_LongRunningQueries.xel',
        max_file_size = (50),
        max_rollover_files = (10)
)
WITH
(
    MAX_MEMORY = 4096 KB,
    EVENT_RETENTION_MODE = ALLOW_SINGLE_EVENT_LOSS,
    MAX_DISPATCH_LATENCY = 5 SECONDS,
    STARTUP_STATE = ON
);
GO

ALTER EVENT SESSION [PlantSafety_LongRunningQueries] ON SERVER STATE = START;
GO
