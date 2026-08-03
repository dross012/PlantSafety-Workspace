-- ============================================================================
--  Project:        PlantSafety.Platform
--  Layer:          Infrastructure - SQL Scripts
--  File:           PermissionUsageLog.sql
--  Created:        2026-08-03
--  Author:         PlantSafety Platform Team (Claude Code performance-optimization-agent)
--  SchemaVersion:  2026.1
--  Description:    Append-only authorization-decision (permission usage / access) log, written
--                  to by AuthorizationBehavior for every IAuthorizedRequest evaluated, allow or
--                  deny (Handbook Phase 6 §5.4 Compliance Monitoring). Deliberately a separate
--                  table from the cross-database, read-only [PlantSafety.AuditDB].dbo.AuditLog
--                  (no PermissionName/Decision columns exist there, and it has no write
--                  contract in this codebase -- see IPermissionUsageLogRepository remarks).
--                  Uses the existing [Auditing] schema (already created by
--                  usp_OutboxMessages_GetByTenant's AuditTimeline module). Additive-only
--                  evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Auditing')
BEGIN
    EXEC('CREATE SCHEMA [Auditing]');
END
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES
               WHERE TABLE_SCHEMA = 'Auditing' AND TABLE_NAME = 'PermissionUsageLog')
BEGIN
    CREATE TABLE [Auditing].[PermissionUsageLog] (
        [PermissionUsageLogId] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TenantId]             UNIQUEIDENTIFIER NOT NULL,
        [UserId]               UNIQUEIDENTIFIER NOT NULL,
        [Permission]           NVARCHAR(256)    NOT NULL,
        [RequestType]          NVARCHAR(256)    NOT NULL,
        [Decision]             NVARCHAR(16)     NOT NULL,
        [CorrelationId]        UNIQUEIDENTIFIER NOT NULL,
        [OccurredAtUtc]        DATETIME2(3)     NOT NULL CONSTRAINT DF_PermissionUsageLog_OccurredAtUtc DEFAULT (SYSUTCDATETIME())
    );

    CREATE INDEX IX_PermissionUsageLog_TenantId_UserId_OccurredAtUtc
        ON [Auditing].[PermissionUsageLog] ([TenantId], [UserId], [OccurredAtUtc]);
END
GO
