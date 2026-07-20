-- =============================================
-- Phase 13 - Monitoring, Diagnostics, Governance Tooling
-- Script:        133-server-logon-trigger.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Purpose:       Server-scoped LOGON trigger for login auditing (successful logons --
--                failed logons are already captured by the SQL Server Audit in 130).
--
-- *** DEPLOYED IN A DISABLED STATE. DO NOT ENABLE WITHOUT READING THIS NOTE. ***
--
-- Safety rationale (operating rule 8 - stricter compliance, documented assumption):
--   LOGON triggers can lock out ALL connections, including sysadmin, if the trigger
--   body ever errors outside of what its own TRY/CATCH can absorb, or if the engine
--   itself cannot execute the trigger for any reason. The standard recovery path is a
--   Dedicated Admin Connection (DAC). This environment was verified LIVE on 2026-07-19
--   to have NO usable DAC:
--     - SQL Server Browser service is present but STOPPED, and the current shell does
--       not have rights to start it (Start-Service failed: access denied / not
--       elevated -- BUILTIN\Administrators membership is present but marked "deny
--       only" i.e. a non-elevated UAC token).
--     - `sqlcmd -S "(local)\SQLEXPRESS" -A`, `ADMIN:(local)\SQLEXPRESS`, and explicit
--       `lpc:`/`np:` protocol prefixes were all tried live and all failed to reach DAC.
--     - No confirmed, safe, non-disruptive way to restart the SQL Server service from
--       this session either (would drop all other live connections and was not
--       attempted, consistent with "never touch git" grade caution around anything
--       irreversible/disruptive without explicit instruction).
--   Given no verified escape hatch exists in this session, the trigger below is
--   created (so the logic is fully written, reviewed, and idempotent-deployable) but
--   is explicitly DISABLED immediately after creation. A disabled server trigger is
--   inert -- it exists in sys.server_triggers but never fires, so it carries zero
--   lockout risk. Enabling it (`ENABLE TRIGGER trg_Server_Logon_Audit ON ALL SERVER;`)
--   should only be done by a human operator who has first confirmed a working DAC path
--   (e.g. started SQL Server Browser with admin rights, or otherwise has physical/RDP
--   console access to recover via `sqlnet -f`/single-user mode if needed).
--
-- Logic notes (defensive design, dry-run validated live before being wired into the
-- trigger -- see session transcript: `SELECT ORIGINAL_LOGIN(), HOST_NAME(), APP_NAME(),
-- SYSUTCDATETIME()` executed cleanly against this instance):
--   - The entire body is wrapped in TRY/CATCH with no re-throw: any logging failure
--     (e.g. AuditDB briefly offline) is swallowed silently and NEVER denies the logon.
--   - There is no explicit ROLLBACK, no explicit denial logic of any kind -- this
--     trigger can only ever log-and-allow, never log-and-block.
--   - Idempotent creation.
-- =============================================
USE [PlantSafety.AuditDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id
                WHERE s.name = N'dbo' AND t.name = N'ServerLogonLog')
BEGIN
    CREATE TABLE dbo.ServerLogonLog
    (
        ServerLogonLogId bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_ServerLogonLog PRIMARY KEY CLUSTERED,
        EventTimeUtc      datetime2(7)  NOT NULL CONSTRAINT DF_ServerLogonLog_EventTimeUtc DEFAULT (SYSUTCDATETIME()),
        LoginName         nvarchar(400) NULL,
        HostName          nvarchar(400) NULL,
        ApplicationName   nvarchar(400) NULL,
        ClientNetAddress  nvarchar(100) NULL
    );
END
GO

------------------------------------------------------------
-- Server LOGON trigger (drop/recreate for idempotency)
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.server_triggers WHERE name = N'trg_Server_Logon_Audit')
    DROP TRIGGER trg_Server_Logon_Audit ON ALL SERVER;
GO

CREATE TRIGGER trg_Server_Logon_Audit
ON ALL SERVER
FOR LOGON
AS
BEGIN
    -- Log-only, best-effort, never denies a logon. No THROW/ROLLBACK anywhere in this body.
    BEGIN TRY
        INSERT INTO [PlantSafety.AuditDB].dbo.ServerLogonLog
        (
            LoginName, HostName, ApplicationName, ClientNetAddress
        )
        SELECT
            ORIGINAL_LOGIN(),
            HOST_NAME(),
            APP_NAME(),
            client_net_address
        FROM sys.dm_exec_connections
        WHERE session_id = @@SPID;
    END TRY
    BEGIN CATCH
        -- Swallow all errors -- logging must never block a legitimate logon.
    END CATCH
END
GO

-- Deployed disabled -- see safety rationale above. Do not enable without a verified DAC path.
DISABLE TRIGGER trg_Server_Logon_Audit ON ALL SERVER;
GO
