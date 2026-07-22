-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Auditing.AuditFinding
--  Layer:          Infrastructure - SQL Scripts
--  File:           AuditFinding.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the AuditFinding aggregate root backing
--                  IAuditFindingRepository. Lives in the [Auditing] schema -- distinct from
--                  the pre-existing [Compliance].[usp_AuditFindings_GetFindings] read-model
--                  (different module: Infrastructure.Compliance.Auditing.AuditFindings) so
--                  the two never collide on schema, table, or procedure names.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditFinding' AND schema_id = SCHEMA_ID('Auditing'))
BEGIN
    CREATE TABLE [Auditing].[AuditFinding] (
        [AuditFindingId]           UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId]                 UNIQUEIDENTIFIER NOT NULL,
        [SiteId]                   UNIQUEIDENTIFIER NULL,
        [Status]                   NVARCHAR(20)     NOT NULL CONSTRAINT DF_AuditFinding_Status DEFAULT ('Created'),
        [AuditEngagementId]        UNIQUEIDENTIFIER NOT NULL,
        [FindingTitle]             NVARCHAR(200)    NOT NULL,
        [Severity]                 NVARCHAR(20)     NOT NULL CONSTRAINT DF_AuditFinding_Severity DEFAULT ('Medium'),
        [CorrectiveActionRequired] BIT              NOT NULL CONSTRAINT DF_AuditFinding_CorrectiveActionRequired DEFAULT (0),
        [AssignedToUserId]         UNIQUEIDENTIFIER NULL,
        [DueDateUtc]               DATETIME2(3)     NULL,
        [VerifiedAtUtc]            DATETIME2(3)     NULL,
        [RemediationNotes]         NVARCHAR(2000)   NULL,
        [RemediatedAtUtc]          DATETIME2(3)     NULL,
        [CreatedAtUtc]             DATETIME2(3)     NOT NULL,
        [CreatedBy]                UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted]                BIT              NOT NULL CONSTRAINT DF_AuditFinding_IsDeleted DEFAULT (0),
        [LastModifiedAtUtc]        DATETIME2(3)     NOT NULL CONSTRAINT DF_AuditFinding_LastModified DEFAULT (SYSUTCDATETIME()),
        [RowVersion]               ROWVERSION       NOT NULL,
        CONSTRAINT FK_AuditFinding_AuditEngagement
            FOREIGN KEY ([AuditEngagementId]) REFERENCES [Auditing].[AuditEngagement] ([AuditEngagementId])
    );

    CREATE NONCLUSTERED INDEX [IX_AuditFinding_TenantId_AuditEngagementId]
        ON [Auditing].[AuditFinding] ([TenantId], [AuditEngagementId])
        INCLUDE ([Status], [Severity], [FindingTitle]);
END
GO
