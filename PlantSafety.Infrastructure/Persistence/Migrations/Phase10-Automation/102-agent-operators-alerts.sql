-- =============================================
-- Phase 10 Automation & Integration
-- Script:        102-agent-operators-alerts.sql
-- Author:        Automation & Integration Agent
-- Create date:   2026-07-19
-- Purpose:       Governance item #3 (SQL Agent alerts & operators).
--
-- *** SQL Server Agent status (live-verified 2026-07-19): STOPPED, startup
-- type DISABLED *** (same as when Phase 7 registered the 5 purge jobs).
-- All objects below are msdb metadata-only operations and do not require
-- the Agent service to be running to create; they simply will not fire
-- until a human starts "SQL Server Agent (SQLEXPRESS)" and sets its
-- startup type to at least Manual.
--
-- (a) Operator: DBA_Alerts.
--     *** PLACEHOLDER EMAIL *** dba-alerts@plantsafety.local is NOT a real,
--     configured mailbox -- it is a placeholder so the operator object has
--     a syntactically valid email_address. A human must replace this with
--     a real, monitored address (and configure Database Mail / a mail
--     profile, which does not exist on this instance yet -- msdb.dbo.
--     sysmail_profile is empty, verified live below) before any of these
--     alerts can actually deliver email. Until then, alerts will still
--     fire and be recorded in the Windows Application Event Log / SQL
--     Server Agent error log, just without email delivery.
--
-- (b) Per-job failure notification on the 5 existing Phase 7 purge jobs.
--     NOTE: SQL Server Agent does not have a first-class "job failure"
--     Alert object type (sp_add_alert alerts fire on raised
--     errors/severities or performance conditions, not directly on "job X
--     failed"). The correct, standard mechanism for job-failure
--     notification is configuring each job's own notification settings
--     (sp_update_job @notify_level_email, @notify_email_operator_name) --
--     that is what this script does for all 5 Phase 7 jobs, rather than
--     inventing a nonstandard alert-based workaround.
--
-- (c) ErrorLog severity>=16 spike detection.
--     Buildable, bounded approach chosen: a new scheduled Agent job
--     ("PlantSafety - ErrorLog Spike Monitor") runs a stored procedure
--     every 15 minutes that counts ErrorDB.dbo.ErrorLog rows with
--     ErrorSeverity >= 16 logged in the last 15 minutes; if the count
--     exceeds a placeholder threshold (10 -- NOT sourced from any
--     confirmed SLA, flagged for human confirmation), it RAISERROR's a
--     dedicated user message WITH LOG, which is exactly the mechanism a
--     SQL Server Agent Alert (sp_add_alert on that message_id) is designed
--     to catch and route to the DBA_Alerts operator. This is the standard,
--     supported pattern for "alert on a business condition, not just a
--     raw system error" and avoids over-engineering (no WMI/performance-
--     condition alert, which would be unbuildable/unverifiable in this
--     environment with 0 rows and no sustained load).
--
-- Idempotency: operator/job/alert/message creation is existence-checked
-- throughout. Safe to rerun.
-- =============================================
SET NOCOUNT ON;
GO

USE [msdb];
GO

------------------------------------------------------------
-- 0. Live capability check: is Database Mail configured at all?
--    (Informational only -- flagged in output, does not block object
--    creation below.)
------------------------------------------------------------
SELECT COUNT(*) AS DatabaseMailProfileCount FROM msdb.dbo.sysmail_profile;
GO

------------------------------------------------------------
-- (a) Operator.
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysoperators WHERE name = N'DBA_Alerts')
BEGIN
    EXEC msdb.dbo.sp_add_operator
        @name = N'DBA_Alerts',
        @enabled = 1,
        @email_address = N'dba-alerts@plantsafety.local'; -- PLACEHOLDER, see header
    PRINT 'Created operator: DBA_Alerts (PLACEHOLDER email address -- needs a real address + Database Mail profile before delivery works).';
END
ELSE
    PRINT 'Operator DBA_Alerts already exists -- skipped.';
GO

------------------------------------------------------------
-- (b) Per-job failure notification on the 5 Phase 7 purge jobs.
------------------------------------------------------------
DECLARE @JobNames TABLE (JobName sysname);
INSERT INTO @JobNames VALUES
    (N'PlantSafety - Purge AuditLog'),
    (N'PlantSafety - Purge ErrorLog'),
    (N'PlantSafety - Purge IntegrationLogEntry'),
    (N'PlantSafety - Purge ReplayRecord'),
    (N'PlantSafety - Purge DiagnosticEvent');

DECLARE @JobName sysname;
DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT JobName FROM @JobNames;
OPEN cur;
FETCH NEXT FROM cur INTO @JobName;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = @JobName)
    BEGIN
        EXEC msdb.dbo.sp_update_job
            @job_name = @JobName,
            @notify_level_email = 2,                 -- 2 = notify on failure
            @notify_email_operator_name = N'DBA_Alerts';
        PRINT 'Configured failure-email notification for job: ' + @JobName;
    END
    ELSE
        PRINT 'WARNING: expected Phase 7 job not found, skipped: ' + @JobName;

    FETCH NEXT FROM cur INTO @JobName;
END
CLOSE cur;
DEALLOCATE cur;
GO

------------------------------------------------------------
-- (c) ErrorLog severity>=16 spike detection.
------------------------------------------------------------

-- (c.1) Dedicated user message, WITH LOG required for Agent alerts to
--       observe it in the Windows Application Event Log.
IF NOT EXISTS (SELECT 1 FROM sys.messages WHERE message_id = 50100 AND language_id = 1033)
BEGIN
    EXEC sp_addmessage
        @msgnum = 50100,
        @severity = 16,
        @msgtext = N'PlantSafety ErrorLog spike detected: %d severity>=16 row(s) logged in the last %d minute(s) (threshold %d). Investigate PlantSafety.ErrorDB.dbo.ErrorLog.',
        @lang = N'us_english',
        @with_log = 'TRUE';
    PRINT 'Registered user message 50100 (ErrorLog spike).';
END
ELSE
    PRINT 'User message 50100 already exists -- skipped.';
GO

-- (c.2) Monitoring stored procedure, created in PlantSafety.ErrorDB
--       (co-located with the table it queries).
USE [PlantSafety.ErrorDB];
GO
CREATE OR ALTER PROCEDURE dbo.sp_ErrorLog_SpikeCheck
    @WindowMinutes int = 15,
    @Threshold int = 10 -- PLACEHOLDER threshold, not sourced from a confirmed SLA -- confirm with the user before relying on this in production
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        DECLARE @CutoffUtc datetime2 = DATEADD(MINUTE, -@WindowMinutes, SYSUTCDATETIME());
        DECLARE @SpikeCount int;

        SELECT @SpikeCount = COUNT(*)
        FROM dbo.ErrorLog
        WHERE ErrorSeverity >= 16 AND ErrorDate >= @CutoffUtc;

        IF @SpikeCount > @Threshold
        BEGIN
            RAISERROR(50100, 16, 1, @SpikeCount, @WindowMinutes, @Threshold) WITH LOG;
        END
    END TRY
    BEGIN CATCH
        -- Do not let a monitoring failure masquerade as a spike, and do not
        -- swallow it silently either -- log to the same ErrorLog table via
        -- a direct INSERT (cannot call the standard cross-DB error-logging
        -- pattern here without risking recursive spike-triggering noise).
        INSERT INTO dbo.ErrorLog
            (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage,
             ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
        VALUES
            (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(),
             SYSUTCDATETIME(), N'sp_ErrorLog_SpikeCheck internal failure', NULL, DB_NAME(), OBJECT_NAME(@@PROCID));
    END CATCH
END
GO

-- (c.3) Agent job to run the check every 15 minutes.
USE [msdb];
GO
IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysschedules WHERE name = N'PlantSafety - Every 15 Minutes')
BEGIN
    EXEC msdb.dbo.sp_add_schedule
        @schedule_name = N'PlantSafety - Every 15 Minutes',
        @freq_type = 4,            -- daily
        @freq_subday_type = 4,     -- minutes
        @freq_subday_interval = 15,
        @active_start_time = 000000;
END
GO

IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysjobs WHERE name = N'PlantSafety - ErrorLog Spike Monitor')
BEGIN
    DECLARE @JobId uniqueidentifier;
    EXEC msdb.dbo.sp_add_job
        @job_name = N'PlantSafety - ErrorLog Spike Monitor',
        @enabled = 1,
        @description = N'PlantSafety Phase 10 automation: checks PlantSafety.ErrorDB.dbo.ErrorLog every 15 minutes for a severity>=16 spike (placeholder threshold=10, see 102-agent-operators-alerts.sql) and raises message 50100 WITH LOG when exceeded, which the PlantSafety - ErrorLog Severity16 Spike alert routes to the DBA_Alerts operator.',
        @owner_login_name = N'sa',
        @job_id = @JobId OUTPUT;

    EXEC msdb.dbo.sp_add_jobstep
        @job_id = @JobId,
        @step_name = N'Run spike check',
        @subsystem = N'TSQL',
        @database_name = N'PlantSafety.ErrorDB',
        @command = N'EXEC dbo.sp_ErrorLog_SpikeCheck;',
        @on_success_action = 1,
        @on_fail_action = 2;

    EXEC msdb.dbo.sp_attach_schedule
        @job_id = @JobId,
        @schedule_name = N'PlantSafety - Every 15 Minutes';

    EXEC msdb.dbo.sp_add_jobserver
        @job_id = @JobId,
        @server_name = N'(local)';

    -- Notify on failure of the monitor job itself too, same as the 5 purge jobs.
    EXEC msdb.dbo.sp_update_job
        @job_name = N'PlantSafety - ErrorLog Spike Monitor',
        @notify_level_email = 2,
        @notify_email_operator_name = N'DBA_Alerts';

    PRINT 'Registered SQL Agent job: PlantSafety - ErrorLog Spike Monitor';
END
ELSE
    PRINT 'SQL Agent job PlantSafety - ErrorLog Spike Monitor already exists -- skipped.';
GO

-- (c.4) The Alert itself: fires on message 50100, notifies DBA_Alerts.
IF NOT EXISTS (SELECT 1 FROM msdb.dbo.sysalerts WHERE name = N'PlantSafety - ErrorLog Severity16 Spike')
BEGIN
    EXEC msdb.dbo.sp_add_alert
        @name = N'PlantSafety - ErrorLog Severity16 Spike',
        @message_id = 50100,
        @severity = 0,             -- 0 because @message_id is specified
        @enabled = 1,
        @delay_between_responses = 900, -- 15 min, matches the monitor job cadence
        @include_event_description_in = 1,
        @notification_message = N'PlantSafety ErrorDB severity>=16 spike detected -- investigate PlantSafety.ErrorDB.dbo.ErrorLog immediately.';

    EXEC msdb.dbo.sp_add_notification
        @alert_name = N'PlantSafety - ErrorLog Severity16 Spike',
        @operator_name = N'DBA_Alerts',
        @notification_method = 1;  -- email

    PRINT 'Registered Agent alert: PlantSafety - ErrorLog Severity16 Spike';
END
ELSE
    PRINT 'Agent alert PlantSafety - ErrorLog Severity16 Spike already exists -- skipped.';
GO

------------------------------------------------------------
-- Verification.
------------------------------------------------------------
SELECT name, enabled, email_address FROM msdb.dbo.sysoperators WHERE name = N'DBA_Alerts';
GO
SELECT name, notify_level_email, notify_email_operator_id FROM msdb.dbo.sysjobs WHERE name LIKE N'PlantSafety%' ORDER BY name;
GO
SELECT name, message_id, severity, enabled FROM msdb.dbo.sysalerts WHERE name LIKE N'PlantSafety%';
GO
SELECT a.name AS AlertName, o.name AS OperatorName, n.notification_method
FROM msdb.dbo.sysnotifications n
JOIN msdb.dbo.sysalerts a ON a.id = n.alert_id
JOIN msdb.dbo.sysoperators o ON o.id = n.operator_id
WHERE a.name LIKE N'PlantSafety%';
GO
