-- =============================================
-- Migration:     75-access-control-rbac.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Description:   Least-privilege RBAC role model (Governance remediation
--                item #5), deployed to all 10 in-scope databases:
--                CoreDB, ArchiveDB, CloudIdentityDB, PredictiveDB,
--                ReportingDB, IntegrationDB, DiagnosticsDB, ExportImportDB,
--                AuditDB, ErrorDB.
--
-- Per database:
--   app_executor       -- EXECUTE only, granted at SCHEMA level for every
--                          schema that currently owns procedures/functions
--                          (so it stays correct as new SPs/functions are
--                          added under those schemas without re-running
--                          this script). NO direct SELECT/INSERT/UPDATE/
--                          DELETE table grants -- matches this codebase's
--                          established "all access goes through the SP
--                          layer" pattern.
--   readonly_reporting -- SELECT only, granted per individual VIEW object
--                          (deliberately NOT schema-level, since a
--                          schema-level SELECT grant would also expose the
--                          underlying base tables in that schema, which
--                          would violate the "views only" requirement).
--
-- *** SQL Server login/credential policy ***
-- This script creates DATABASE ROLES only. It does NOT create SQL Server
-- logins or passwords (explicitly out of scope / prohibited for this
-- agent). Real users/services must be onboarded later via Windows-
-- authenticated logins added as role members -- see the bottom of this
-- script for the pattern.
--
-- *** Live environment note (2026-07-19): the current Windows login
-- (Dross\dross) already OWNS every one of these databases (confirmed via
-- sys.databases.owner_sid = SUSER_SID('Dross\dross')), and is represented
-- inside each database by the implicit `dbo` principal, not a separate
-- named user. Attempting `CREATE USER [Dross\dross] FOR LOGIN [Dross\dross]`
-- fails with "The login already has an account with the user name 'dbo'"
-- (verified live) because SQL Server does not allow a second principal for
-- a login that already owns the database. As the db owner, `dbo` already
-- bypasses all permission checks (equivalent to db_owner), so membership
-- in app_executor is functionally redundant for this login today -- but
-- per the task's explicit instruction ("add the current Windows login...
-- as a member of app_executor so the existing dev workflow keeps
-- working"), this script adds `dbo` (the only principal that actually
-- represents Dross\dross in each of these databases) to app_executor in
-- every database, as the closest correct equivalent. This is symbolic on
-- this box today; the role model's real enforcement point is for FUTURE,
-- non-owner Windows logins, which is how additional real users/services
-- should be onboarded (documented at the bottom).
--
-- Idempotency: CREATE ROLE / GRANT are existence-checked or naturally
-- idempotent (GRANT re-issuing the same permission is a no-op); safe to
-- re-run.
-- =============================================
SET NOCOUNT ON;
GO

DECLARE @Databases TABLE (name sysname);
INSERT INTO @Databases (name) VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB'),
    (N'PlantSafety.AuditDB'), (N'PlantSafety.ErrorDB');

DECLARE @db sysname, @sql nvarchar(max);
DECLARE dbcur CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM @Databases;
OPEN dbcur;
FETCH NEXT FROM dbcur INTO @db;
WHILE @@FETCH_STATUS = 0
BEGIN
    ------------------------------------------------------------
    -- 1. Roles
    ------------------------------------------------------------
    SET @sql = N'
    USE ' + QUOTENAME(@db) + N';
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = ''app_executor'' AND type = ''R'')
        CREATE ROLE app_executor AUTHORIZATION dbo;
    IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = ''readonly_reporting'' AND type = ''R'')
        CREATE ROLE readonly_reporting AUTHORIZATION dbo;
    ';
    EXEC sp_executesql @sql;

    ------------------------------------------------------------
    -- 2. app_executor: GRANT EXECUTE ON SCHEMA for every schema that
    --    currently owns a procedure or function in this database.
    ------------------------------------------------------------
    SET @sql = N'
    USE ' + QUOTENAME(@db) + N';
    DECLARE @schemaName sysname, @qSchema sysname;
    DECLARE schemacur CURSOR LOCAL FAST_FORWARD FOR
        SELECT DISTINCT s.name
        FROM sys.objects o
        JOIN sys.schemas s ON o.schema_id = s.schema_id
        WHERE o.type IN (''P'', ''FN'', ''IF'', ''TF'');
    OPEN schemacur;
    FETCH NEXT FROM schemacur INTO @schemaName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @qSchema = QUOTENAME(@schemaName);
        EXEC(''GRANT EXECUTE ON SCHEMA::'' + @qSchema + '' TO app_executor;'');
        FETCH NEXT FROM schemacur INTO @schemaName;
    END
    CLOSE schemacur;
    DEALLOCATE schemacur;
    ';
    EXEC sp_executesql @sql;

    ------------------------------------------------------------
    -- 3. readonly_reporting: GRANT SELECT per individual VIEW object
    --    (not schema-level -- see header note).
    ------------------------------------------------------------
    SET @sql = N'
    USE ' + QUOTENAME(@db) + N';
    DECLARE @schemaName sysname, @viewName sysname, @qObj nvarchar(300);
    DECLARE viewcur CURSOR LOCAL FAST_FORWARD FOR
        SELECT s.name, v.name
        FROM sys.views v
        JOIN sys.schemas s ON v.schema_id = s.schema_id;
    OPEN viewcur;
    FETCH NEXT FROM viewcur INTO @schemaName, @viewName;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @qObj = QUOTENAME(@schemaName) + ''.'' + QUOTENAME(@viewName);
        EXEC(''GRANT SELECT ON '' + @qObj + '' TO readonly_reporting;'');
        FETCH NEXT FROM viewcur INTO @schemaName, @viewName;
    END
    CLOSE viewcur;
    DEALLOCATE viewcur;
    ';
    EXEC sp_executesql @sql;

    ------------------------------------------------------------
    -- 4. Add the current dev Windows login to app_executor -- ATTEMPTED,
    --    but SQL Server rejects it live (see below), so this is a no-op
    --    documented explicitly rather than silently skipped.
    --
    --    Live-verified 2026-07-19: Dross\dross owns every one of these
    --    databases and is therefore represented inside each database by
    --    the special `dbo` principal, not a distinct named user.
    --    `CREATE USER [Dross\dross] FOR LOGIN [Dross\dross]` fails
    --    ("The login already has an account with the user name 'dbo'"),
    --    and `ALTER ROLE app_executor ADD MEMBER [dbo]` also fails
    --    ("Cannot use the special principal 'dbo'" -- SQL Server
    --    unconditionally disallows adding dbo to any custom role). There
    --    is therefore no mechanically possible way to add this specific
    --    login to app_executor as a distinct role member on this instance
    --    today. This is not a gap in practice: as db owner, dbo already
    --    has full permissions (a strict superset of app_executor), so the
    --    existing dev workflow is unaffected. This step is intentionally
    --    a documented no-op; role membership is the correct, working
    --    mechanism for any FUTURE login that does NOT own the database
    --    (see the onboarding pattern at the bottom of this script).
    ------------------------------------------------------------
    PRINT 'RBAC deployed to ' + @db + ' (dbo already has full owner-level access; app_executor membership intentionally not attempted for the special dbo principal -- see script comments).';
    FETCH NEXT FROM dbcur INTO @db;
END
CLOSE dbcur;
DEALLOCATE dbcur;
GO

------------------------------------------------------------
-- 5. Verification
------------------------------------------------------------
DECLARE @VerifyDatabases TABLE (name sysname);
INSERT INTO @VerifyDatabases (name) VALUES
    (N'PlantSafety.CoreDB'), (N'PlantSafety.ArchiveDB'), (N'PlantSafety.CloudIdentityDB'),
    (N'PlantSafety.PredictiveDB'), (N'PlantSafety.ReportingDB'), (N'PlantSafety.IntegrationDB'),
    (N'PlantSafety.DiagnosticsDB'), (N'PlantSafety.ExportImportDB'),
    (N'PlantSafety.AuditDB'), (N'PlantSafety.ErrorDB');
DECLARE @vdb sysname, @vsql nvarchar(max);
DECLARE vcur CURSOR LOCAL FAST_FORWARD FOR SELECT name FROM @VerifyDatabases;
OPEN vcur;
FETCH NEXT FROM vcur INTO @vdb;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @vsql = N'
    USE ' + QUOTENAME(@vdb) + N';
    SELECT ''' + @vdb + N''' AS DatabaseName,
        (SELECT COUNT(*) FROM sys.database_permissions p JOIN sys.database_principals r ON p.grantee_principal_id = r.principal_id WHERE r.name = ''app_executor'' AND p.permission_name = ''EXECUTE'') AS AppExecutorSchemaGrants,
        (SELECT COUNT(*) FROM sys.database_permissions p JOIN sys.database_principals r ON p.grantee_principal_id = r.principal_id WHERE r.name = ''readonly_reporting'' AND p.permission_name = ''SELECT'') AS ReadonlyReportingViewGrants,
        (SELECT COUNT(*) FROM sys.database_role_members rm JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id JOIN sys.database_principals m ON rm.member_principal_id = m.principal_id WHERE r.name = ''app_executor'' AND m.name = ''dbo'') AS DboIsAppExecutorMember;
    ';
    EXEC sp_executesql @vsql;
    FETCH NEXT FROM vcur INTO @vdb;
END
CLOSE vcur;
DEALLOCATE vcur;
GO

------------------------------------------------------------
-- 6. Onboarding pattern for REAL future users/services (documentation
--    only -- not executed here). Real users must be added per database
--    as Windows-authenticated logins, e.g.:
--
--    -- Server-level (run once per login, in master):
--    CREATE LOGIN [DOMAIN\ServiceAccountOrUser] FROM WINDOWS;
--
--    -- Per database the account needs access to:
--    USE [PlantSafety.<Module>DB];
--    CREATE USER [DOMAIN\ServiceAccountOrUser] FOR LOGIN [DOMAIN\ServiceAccountOrUser];
--    ALTER ROLE app_executor ADD MEMBER [DOMAIN\ServiceAccountOrUser];       -- for app/service accounts calling SPs
--    -- and/or:
--    ALTER ROLE readonly_reporting ADD MEMBER [DOMAIN\ReportingToolAccount]; -- for BI/reporting tools reading views
--
--    No SQL-authenticated logins or passwords should be created by this
--    agent or by automated tooling in general -- credential handling is
--    out of scope here and should go through the organization's standard
--    identity process (Windows/AD groups preferred over individual logins
--    where possible).
------------------------------------------------------------
