-- =============================================
-- Shared.AuditLogging
-- Script:        01-recommended-indexes.sql
-- Purpose:       OPTIONAL, additive, idempotent indexes recommended for
--                [PlantSafety.AuditDB].dbo.AuditLog to support the query
--                shape used by dbo.usp_SearchAuditLog (see
--                ..\Phase1-StoredProcedures\01-usp-search-auditlog.sql):
--                filter on TableName/Operation/ChangedBy/Module/Severity/
--                ChangeTime range/ErrorContext LIKE, then
--                ORDER BY ChangeTime DESC with OFFSET/FETCH paging.
--
-- NOT YET VERIFIED AGAINST THE LIVE TABLE: this script has not been run
-- against the live [PlantSafety.AuditDB], and it is unknown whether
-- equivalent indexes already exist there today. It is guarded by
-- sys.indexes existence checks and is purely additive (CREATE INDEX only,
-- never DROP/ALTER), so it is safe to run, but a human/DBA should confirm
-- current index state and expected write volume on the live table first --
-- this is a large, high-write-throughput, append-only table shared by 8
-- databases, and even a purely additive nonclustered index has a real
-- storage and write-cost impact that should be sized and reviewed before
-- being applied to production.
--
-- No FOREIGN KEY columns exist on this table (TableName/Module are
-- free-text descriptive labels populated from OBJECT_NAME()/literals, not
-- real foreign keys -- cross-database foreign keys are not supported by SQL
-- Server, and TableName does not reference one single table/database), so
-- enforce-foreign-keys' "every FK must be indexed" rule does not apply
-- here; the indexes below are query-shape driven instead.
--
-- No clustered index / no PK: consistent with 00-create-auditlog-table.sql
-- not inventing a primary key, this script also does not add a clustered
-- index. dbo.AuditLog remains a heap; adding a clustering key to an
-- existing live table is a much higher-risk, rebuild-the-table operation
-- and is explicitly out of scope here.
-- =============================================

USE [PlantSafety.AuditDB]
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_ChangeTime' AND object_id = OBJECT_ID(N'dbo.AuditLog'))
    CREATE NONCLUSTERED INDEX IX_AuditLog_ChangeTime
        ON dbo.AuditLog (ChangeTime DESC)
        INCLUDE (TableName, SourceDatabase, Operation, ChangedBy, Module, Severity);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_TableName_Module_ChangeTime' AND object_id = OBJECT_ID(N'dbo.AuditLog'))
    CREATE NONCLUSTERED INDEX IX_AuditLog_TableName_Module_ChangeTime
        ON dbo.AuditLog (TableName, Module, ChangeTime DESC);
GO
