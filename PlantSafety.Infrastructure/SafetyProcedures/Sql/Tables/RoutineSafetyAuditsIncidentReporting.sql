-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
--  Layer:          Infrastructure - SQL Scripts
--  File:           RoutineSafetyAuditsIncidentReporting.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the RoutineSafetyAuditsIncidentReporting aggregate root backing IRoutineSafetyAuditsIncidentReportingRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'SafetyProcedures')
BEGIN
    EXEC('CREATE SCHEMA [SafetyProcedures]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RoutineSafetyAuditsIncidentReporting' AND schema_id = SCHEMA_ID('SafetyProcedures'))
BEGIN
    CREATE TABLE [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting] (
        [RoutineSafetyAuditsIncidentReportingId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [AuditTitle] NVARCHAR(400) NOT NULL,
        [ReportingCategory] NVARCHAR(100) NOT NULL,
        [FacilityArea] NVARCHAR(400) NOT NULL,
        [ConductedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [ConductedAtUtc] DATETIME2(3) NULL,
        [NextAuditDueAtUtc] DATETIME2(3) NULL,
        [FindingCount] INT NOT NULL CONSTRAINT DF_RoutineSafetyAuditsIncidentReporting_FindingCount DEFAULT (0),
        [ComplianceScore] DECIMAL(9,2) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_RoutineSafetyAuditsIncidentReporting_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_RoutineSafetyAuditsIncidentReporting_TenantId_SiteId]
        ON [SafetyProcedures].[RoutineSafetyAuditsIncidentReporting] ([TenantId], [SiteId])
        INCLUDE ([Status], [AuditTitle]);
END
GO
