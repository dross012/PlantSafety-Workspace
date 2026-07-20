-- =============================================
-- Migration:     91-dynamic-data-masking.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Description:   Dynamic Data Masking (DDM) on the exact columns flagged
--                sensitive by Phase 7's data classification pass
--                (72-data-classification.sql, PlantSafety.CoreDB only --
--                all 9 classified columns live in CoreDB).
--
-- Masking function choice:
--   * Free-text narrative fields (nvarchar(max)) -> partial() showing the
--     first 8 characters then masking the rest with a fixed literal, so an
--     ad-hoc/reporting viewer can still see enough to identify the record
--     type/context without exposing the sensitive narrative content:
--       partial(8, "...[MASKED]...", 0)
--   * Short, enum-like/classification fields (InjuryType nvarchar(100),
--     WorkerWellBeing.WellBeingRiskLevel nvarchar(100)) -> default(),
--     which fully replaces the value with 'xxxx' for string types. These
--     are categorical/classification values, not narrative -- partial
--     reveal of a short enum-like string materially increases guessability
--     (e.g. distinguishing "Fatality" vs "First Aid" from 3-4 leading
--     chars), so default() (full mask) is the more appropriate/stricter
--     choice per the task's own function-choice guidance.
--
-- Grant model:
--   * app_executor: GRANT UNMASK -- the application's own SP layer must see
--     real data (matches Phase 7 RBAC role, 75-access-control-rbac.sql).
--   * readonly_reporting: NO UNMASK grant -- ad-hoc/reporting queries via
--     this role see masked values only.
--   * dbo (current dev login, per Phase 7 RBAC notes: dbo already has
--     owner-level access and bypasses masking implicitly as db_owner /
--     UNMASK is unnecessary for dbo) -- no explicit grant needed.
--
-- Idempotency: ALTER COLUMN ... ADD MASKED WITH (...) is naturally
-- idempotent-safe if preceded by a DROP MASKED guard; GRANT/REVOKE are
-- idempotent. Safe to re-run.
-- =============================================
SET NOCOUNT ON;
GO
USE [PlantSafety.CoreDB];
GO

------------------------------------------------------------
-- Free-text narrative fields -> partial() masking
------------------------------------------------------------
DECLARE @PartialCols TABLE (SchemaName sysname, TableName sysname, ColumnName sysname);
INSERT INTO @PartialCols (SchemaName, TableName, ColumnName) VALUES
    (N'SafetyEvents', N'IncidentManagement', N'Description'),
    (N'SafetyEvents', N'IncidentManagement', N'RootCause'),
    (N'SafetyEvents', N'IncidentManagement', N'CorrectiveActions'),
    (N'SafetyEvents', N'Observation', N'Description'),
    (N'SafetyEvents', N'ObservationHistoryEntry', N'Notes'),
    (N'SafetyEvents', N'EmergencyManagementHistoryEntry', N'Notes'),
    (N'SafetyEvents', N'IncidentManagementHistoryEntry', N'Notes'),
    (N'SafetyEvents', N'IncidentManagementHistoryEntry', N'RootCauseAtTransition');

DECLARE @s sysname, @t sysname, @c sysname, @sql nvarchar(max);
DECLARE pcur CURSOR LOCAL FAST_FORWARD FOR SELECT SchemaName, TableName, ColumnName FROM @PartialCols;
OPEN pcur;
FETCH NEXT FROM pcur INTO @s, @t, @c;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.masked_columns mc
        JOIN sys.tables tb ON mc.object_id = tb.object_id
        JOIN sys.schemas sc ON tb.schema_id = sc.schema_id
        WHERE sc.name = @s AND tb.name = @t AND mc.name = @c
    )
        SET @sql = N'ALTER TABLE ' + QUOTENAME(@s) + N'.' + QUOTENAME(@t)
                 + N' ALTER COLUMN ' + QUOTENAME(@c) + N' DROP MASKED;';
    ELSE
        SET @sql = NULL;
    IF @sql IS NOT NULL EXEC sp_executesql @sql;

    SET @sql = N'ALTER TABLE ' + QUOTENAME(@s) + N'.' + QUOTENAME(@t)
             + N' ALTER COLUMN ' + QUOTENAME(@c)
             + N' ADD MASKED WITH (FUNCTION = ''partial(8, "...[MASKED]...", 0)'');';
    EXEC sp_executesql @sql;

    FETCH NEXT FROM pcur INTO @s, @t, @c;
END
CLOSE pcur;
DEALLOCATE pcur;
GO

------------------------------------------------------------
-- Short enum-like fields -> default() masking
------------------------------------------------------------
-- Idempotency note: DROP MASKED is attempted first inside TRY/CATCH (SQL
-- Server error 16007 if no mask currently exists is swallowed as
-- non-fatal); ADD MASKED then (re)applies the mask unconditionally. This
-- two-step TRY/DROP-then-ADD pattern was verified live to be reliable for
-- these single-column ALTERs (a combined single-statement IF EXISTS/DROP
-- guard was found to misbehave for these two columns during live testing
-- on this SQL Server 2025 Express build and was replaced with this
-- explicit TRY/CATCH form instead).
BEGIN TRY
    ALTER TABLE SafetyEvents.IncidentManagement ALTER COLUMN InjuryType DROP MASKED;
END TRY
BEGIN CATCH
    -- Error 16007: column does not currently have a masking function -- expected on first run.
END CATCH;
ALTER TABLE SafetyEvents.IncidentManagement ALTER COLUMN InjuryType ADD MASKED WITH (FUNCTION = 'default()');
GO

BEGIN TRY
    ALTER TABLE WorkerWellBeing.WorkerWellBeing ALTER COLUMN WellBeingRiskLevel DROP MASKED;
END TRY
BEGIN CATCH
    -- Error 16007: column does not currently have a masking function -- expected on first run.
END CATCH;
ALTER TABLE WorkerWellBeing.WorkerWellBeing ALTER COLUMN WellBeingRiskLevel ADD MASKED WITH (FUNCTION = 'default()');
GO

------------------------------------------------------------
-- Grants: app_executor sees real data; readonly_reporting does not.
------------------------------------------------------------
GRANT UNMASK TO app_executor;
REVOKE UNMASK FROM readonly_reporting;
GO

------------------------------------------------------------
-- Verification
------------------------------------------------------------
SELECT
    OBJECT_SCHEMA_NAME(mc.object_id) + '.' + OBJECT_NAME(mc.object_id) AS TableName,
    mc.name AS ColumnName,
    mc.masking_function
FROM sys.masked_columns mc
ORDER BY TableName, ColumnName;

SELECT dp.name AS PrincipalName, perm.permission_name, perm.state_desc
FROM sys.database_permissions perm
JOIN sys.database_principals dp ON perm.grantee_principal_id = dp.principal_id
WHERE perm.permission_name = 'UNMASK';
GO
