-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditReport
--  Layer:          Infrastructure -- SQL Scripts
--  File:           AuditReport.sql
--  Created:        2026-08-02
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Tables backing the AuditReport aggregate root (IAuditReportRepository) and
--                  its AuditReportSection child entity (Handbook Phase 5 §6.2 printable audit
--                  reports). Lives in the [Auditing] schema. The rendered byte-stream content
--                  itself is NOT stored here -- see IAuditReportContentStore / AuditReportBlobContentStore,
--                  which persists to blob storage and is referenced via RenderedContentReference.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Auditing')
BEGIN
    EXEC('CREATE SCHEMA [Auditing]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditReport' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[AuditReport] (
        [AuditReportId]             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]                  UNIQUEIDENTIFIER NOT NULL,
        [SiteId]                    UNIQUEIDENTIFIER NULL,
        [TargetEntityType]          NVARCHAR(200)    NOT NULL,
        [TargetEntityId]            UNIQUEIDENTIFIER NOT NULL,
        [Format]                    NVARCHAR(10)     NOT NULL,
        [Status]                    NVARCHAR(20)     NOT NULL,
        [ContentSizeBytes]          BIGINT           NULL,
        [RenderedContentReference]  NVARCHAR(1000)   NULL,
        [FailureReason]             NVARCHAR(2000)   NULL,
        [RequestedBy]               UNIQUEIDENTIFIER NOT NULL,
        [RequestedAtUtc]            DATETIME2(3)     NOT NULL,
        [CompletedAtUtc]            DATETIME2(3)     NULL
    );

    CREATE NONCLUSTERED INDEX IX_AuditReport_Tenant_Target
        ON [Auditing].[AuditReport] (TenantId, TargetEntityType, TargetEntityId, RequestedAtUtc DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditReportSection' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[AuditReportSection] (
        [AuditReportSectionId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [AuditReportId]        UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT FK_AuditReportSection_Report REFERENCES [Auditing].[AuditReport] (AuditReportId),
        [SectionType]          NVARCHAR(30)     NOT NULL,
        [PrintOrder]           INT              NOT NULL,
        [Heading]              NVARCHAR(200)    NOT NULL,
        [ContentJson]          NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_AuditReportSection_Content DEFAULT (N'{}')
    );

    CREATE NONCLUSTERED INDEX IX_AuditReportSection_Report
        ON [Auditing].[AuditReportSection] (AuditReportId, PrintOrder);
END
GO
