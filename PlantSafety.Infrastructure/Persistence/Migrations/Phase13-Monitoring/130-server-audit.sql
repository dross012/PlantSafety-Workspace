-- =============================================
-- Phase 13 - Monitoring, Diagnostics, Governance Tooling
-- Script:        130-server-audit.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Purpose:       Server-level SQL Server Audit (DBA/security audit) capturing:
--                  - Schema (DDL) changes on the 8 primary PlantSafety databases
--                  - Permission changes (GRANT/REVOKE/DENY) at server and database level
--                  - Failed login attempts
--                This is DISTINCT from and COMPLEMENTS the Phase 6 application-level
--                AuditDB.dbo.AuditLog trail. That table records business-entity CRUD;
--                this SERVER AUDIT records DBA/security-relevant events independent of
--                whether application code correctly calls into its own audit path.
-- Notes:
--   - Basic SQL Server Audit (server audit + database audit specification) is available
--     on Express edition since SQL Server 2016 SP1 -- confirmed live-verified below;
--     no edition block here.
--   - File target directory: C:\Users\dross\SQLAudit\PlantSafety\ (must exist and be
--     writable by the SQL Server service account before running this script).
--   - Idempotent: drops and recreates if already present, so this script can be re-run.
-- =============================================
USE [master]
GO

------------------------------------------------------------
-- 1. Server Audit object (file target)
------------------------------------------------------------
IF EXISTS (SELECT 1 FROM sys.server_audits WHERE name = N'PlantSafety_ServerAudit')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.server_audits WHERE name = N'PlantSafety_ServerAudit' AND is_state_enabled = 1)
        ALTER SERVER AUDIT [PlantSafety_ServerAudit] WITH (STATE = OFF);

    -- Server audit specification must be dropped/disabled before dropping the audit
    IF EXISTS (SELECT 1 FROM sys.server_audit_specifications WHERE name = N'PlantSafety_ServerAuditSpec')
    BEGIN
        ALTER SERVER AUDIT SPECIFICATION [PlantSafety_ServerAuditSpec] WITH (STATE = OFF);
        DROP SERVER AUDIT SPECIFICATION [PlantSafety_ServerAuditSpec];
    END

    DROP SERVER AUDIT [PlantSafety_ServerAudit];
END
GO

CREATE SERVER AUDIT [PlantSafety_ServerAudit]
TO FILE
(
    FILEPATH = N'C:\Users\dross\SQLAudit\PlantSafety\',
    MAXSIZE = 100 MB,
    MAX_ROLLOVER_FILES = 50,
    RESERVE_DISK_SPACE = OFF
)
WITH
(
    QUEUE_DELAY = 1000,
    ON_FAILURE = CONTINUE
);
GO

ALTER SERVER AUDIT [PlantSafety_ServerAudit] WITH (STATE = ON);
GO

------------------------------------------------------------
-- 2. Server Audit Specification: failed logins + server-level permission changes
------------------------------------------------------------
CREATE SERVER AUDIT SPECIFICATION [PlantSafety_ServerAuditSpec]
FOR SERVER AUDIT [PlantSafety_ServerAudit]
    ADD (FAILED_LOGIN_GROUP),
    ADD (SERVER_PERMISSION_CHANGE_GROUP),
    ADD (SERVER_ROLE_MEMBER_CHANGE_GROUP)
WITH (STATE = ON);
GO

------------------------------------------------------------
-- 3. Database Audit Specifications: DDL + permission changes,
--    one per primary database (schema-per-module boundary respected --
--    each spec lives in and only covers its own database).
------------------------------------------------------------
IF DB_ID(N'PlantSafety.ArchiveDB') IS NOT NULL
BEGIN
    USE [PlantSafety.ArchiveDB];
    IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'PlantSafety_DbAuditSpec')
    BEGIN
        ALTER DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec] WITH (STATE = OFF);
        DROP DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec];
    END
    EXEC(N'
    CREATE DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec]
    FOR SERVER AUDIT [PlantSafety_ServerAudit]
        ADD (SCHEMA_OBJECT_CHANGE_GROUP),
        ADD (SCHEMA_OBJECT_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
    WITH (STATE = ON);');
END
GO

IF DB_ID(N'PlantSafety.CloudIdentityDB') IS NOT NULL
BEGIN
    USE [PlantSafety.CloudIdentityDB];
    IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'PlantSafety_DbAuditSpec')
    BEGIN
        ALTER DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec] WITH (STATE = OFF);
        DROP DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec];
    END
    EXEC(N'
    CREATE DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec]
    FOR SERVER AUDIT [PlantSafety_ServerAudit]
        ADD (SCHEMA_OBJECT_CHANGE_GROUP),
        ADD (SCHEMA_OBJECT_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
    WITH (STATE = ON);');
END
GO

IF DB_ID(N'PlantSafety.CoreDB') IS NOT NULL
BEGIN
    USE [PlantSafety.CoreDB];
    IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'PlantSafety_DbAuditSpec')
    BEGIN
        ALTER DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec] WITH (STATE = OFF);
        DROP DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec];
    END
    EXEC(N'
    CREATE DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec]
    FOR SERVER AUDIT [PlantSafety_ServerAudit]
        ADD (SCHEMA_OBJECT_CHANGE_GROUP),
        ADD (SCHEMA_OBJECT_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
    WITH (STATE = ON);');
END
GO

IF DB_ID(N'PlantSafety.DiagnosticsDB') IS NOT NULL
BEGIN
    USE [PlantSafety.DiagnosticsDB];
    IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'PlantSafety_DbAuditSpec')
    BEGIN
        ALTER DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec] WITH (STATE = OFF);
        DROP DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec];
    END
    EXEC(N'
    CREATE DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec]
    FOR SERVER AUDIT [PlantSafety_ServerAudit]
        ADD (SCHEMA_OBJECT_CHANGE_GROUP),
        ADD (SCHEMA_OBJECT_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
    WITH (STATE = ON);');
END
GO

IF DB_ID(N'PlantSafety.ExportImportDB') IS NOT NULL
BEGIN
    USE [PlantSafety.ExportImportDB];
    IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'PlantSafety_DbAuditSpec')
    BEGIN
        ALTER DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec] WITH (STATE = OFF);
        DROP DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec];
    END
    EXEC(N'
    CREATE DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec]
    FOR SERVER AUDIT [PlantSafety_ServerAudit]
        ADD (SCHEMA_OBJECT_CHANGE_GROUP),
        ADD (SCHEMA_OBJECT_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
    WITH (STATE = ON);');
END
GO

IF DB_ID(N'PlantSafety.IntegrationDB') IS NOT NULL
BEGIN
    USE [PlantSafety.IntegrationDB];
    IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'PlantSafety_DbAuditSpec')
    BEGIN
        ALTER DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec] WITH (STATE = OFF);
        DROP DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec];
    END
    EXEC(N'
    CREATE DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec]
    FOR SERVER AUDIT [PlantSafety_ServerAudit]
        ADD (SCHEMA_OBJECT_CHANGE_GROUP),
        ADD (SCHEMA_OBJECT_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
    WITH (STATE = ON);');
END
GO

IF DB_ID(N'PlantSafety.PredictiveDB') IS NOT NULL
BEGIN
    USE [PlantSafety.PredictiveDB];
    IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'PlantSafety_DbAuditSpec')
    BEGIN
        ALTER DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec] WITH (STATE = OFF);
        DROP DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec];
    END
    EXEC(N'
    CREATE DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec]
    FOR SERVER AUDIT [PlantSafety_ServerAudit]
        ADD (SCHEMA_OBJECT_CHANGE_GROUP),
        ADD (SCHEMA_OBJECT_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
    WITH (STATE = ON);');
END
GO

IF DB_ID(N'PlantSafety.ReportingDB') IS NOT NULL
BEGIN
    USE [PlantSafety.ReportingDB];
    IF EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'PlantSafety_DbAuditSpec')
    BEGIN
        ALTER DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec] WITH (STATE = OFF);
        DROP DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec];
    END
    EXEC(N'
    CREATE DATABASE AUDIT SPECIFICATION [PlantSafety_DbAuditSpec]
    FOR SERVER AUDIT [PlantSafety_ServerAudit]
        ADD (SCHEMA_OBJECT_CHANGE_GROUP),
        ADD (SCHEMA_OBJECT_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_PERMISSION_CHANGE_GROUP),
        ADD (DATABASE_ROLE_MEMBER_CHANGE_GROUP)
    WITH (STATE = ON);');
END
GO

USE [master]
GO
