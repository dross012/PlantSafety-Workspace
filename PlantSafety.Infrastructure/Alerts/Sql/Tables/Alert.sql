-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Alerts.Alert
--  Layer:          Infrastructure (SQL)
--  File:           Alert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Canonical tables for the Alerts.Alert aggregate (distinct from, and NOT to be
--                  confused with, the pre-existing [OperationalSafety].[Alerts] table owned by the
--                  OperationalSafety.Alerts "canonical reference module"). Schema [Alerts].
--                  TenantId is UNIQUEIDENTIFIER NOT NULL on every table so the platform's
--                  Security.fn_TenantAccessPredicate Row-Level-Security policy auto-discovers and
--                  protects these tables the next time 90-row-level-security.sql is (re-)run.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Alerts')
    EXEC('CREATE SCHEMA [Alerts] AUTHORIZATION dbo;');
GO

-- ----------------------------------------------------------------------------
-- [Alerts].[Alert] -- aggregate root table
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Alerts' AND t.name = 'Alert')
BEGIN
    CREATE TABLE [Alerts].[Alert]
    (
        [AlertId]                          UNIQUEIDENTIFIER   NOT NULL,
        [TenantId]                         UNIQUEIDENTIFIER   NOT NULL,
        [SiteId]                           UNIQUEIDENTIFIER   NULL,
        [Status]                           NVARCHAR(20)       NOT NULL,
        [CategoryCode]                     NVARCHAR(50)       NOT NULL,
        [SeverityCode]                     NVARCHAR(20)       NOT NULL,
        [SourceType]                       NVARCHAR(30)       NOT NULL,
        [Title]                            NVARCHAR(200)      NOT NULL,
        [LocationZoneName]                 NVARCHAR(200)      NOT NULL CONSTRAINT DF_Alert_LocationZoneName DEFAULT (N'Unspecified'),
        [LocationLatitude]                 FLOAT              NULL,
        [LocationLongitude]                FLOAT              NULL,
        [LocationIndoorPositionReference]  NVARCHAR(200)      NULL,
        [TriggeredAtUtc]                   DATETIME2(3)       NOT NULL,
        [AcknowledgedAtUtc]                DATETIME2(3)       NULL,
        [ResolvedAtUtc]                    DATETIME2(3)       NULL,
        [ClosedAtUtc]                      DATETIME2(3)       NULL,
        [CloseOutVerification]             NVARCHAR(4000)     NULL,
        [CreatedAtUtc]                     DATETIME2(3)       NOT NULL,
        [CreatedBy]                        UNIQUEIDENTIFIER   NULL,
        [UpdatedAtUtc]                     DATETIME2(3)       NULL,
        [IsDeleted]                        BIT                NOT NULL CONSTRAINT DF_Alert_IsDeleted DEFAULT (0),
        [RowVersion]                       ROWVERSION,

        CONSTRAINT PK_Alert PRIMARY KEY CLUSTERED ([AlertId]),
        CONSTRAINT CK_Alert_Status CHECK ([Status] IN (N'Triggered', N'Acknowledged', N'Dispatched', N'Escalated', N'Resolved', N'Closed'))
    );

    CREATE NONCLUSTERED INDEX IX_Alert_TenantId_Status ON [Alerts].[Alert] ([TenantId], [Status]) WHERE [IsDeleted] = 0;
    CREATE NONCLUSTERED INDEX IX_Alert_TenantId_CategoryCode ON [Alerts].[Alert] ([TenantId], [CategoryCode]) WHERE [IsDeleted] = 0;
    CREATE NONCLUSTERED INDEX IX_Alert_TenantId_TriggeredAtUtc ON [Alerts].[Alert] ([TenantId], [TriggeredAtUtc]) WHERE [IsDeleted] = 0;
END
GO

-- ----------------------------------------------------------------------------
-- [Alerts].[AlertDeliveryRecord] -- child entity: multi-channel delivery attempts
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Alerts' AND t.name = 'AlertDeliveryRecord')
BEGIN
    CREATE TABLE [Alerts].[AlertDeliveryRecord]
    (
        [DeliveryRecordId] UNIQUEIDENTIFIER  NOT NULL,
        [AlertId]          UNIQUEIDENTIFIER  NOT NULL,
        [TenantId]         UNIQUEIDENTIFIER  NOT NULL,
        [Channel]          NVARCHAR(30)      NOT NULL,
        [Succeeded]        BIT               NOT NULL,
        [AttemptedAtUtc]   DATETIME2(3)      NOT NULL,
        [FailureReason]    NVARCHAR(500)     NULL,
        [CreatedAtUtc]     DATETIME2(3)      NOT NULL CONSTRAINT DF_AlertDeliveryRecord_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]        BIT               NOT NULL CONSTRAINT DF_AlertDeliveryRecord_IsDeleted DEFAULT (0),

        CONSTRAINT PK_AlertDeliveryRecord PRIMARY KEY CLUSTERED ([DeliveryRecordId]),
        CONSTRAINT FK_AlertDeliveryRecord_Alert FOREIGN KEY ([AlertId]) REFERENCES [Alerts].[Alert] ([AlertId])
    );

    CREATE NONCLUSTERED INDEX IX_AlertDeliveryRecord_AlertId ON [Alerts].[AlertDeliveryRecord] ([AlertId]);
    CREATE NONCLUSTERED INDEX IX_AlertDeliveryRecord_TenantId ON [Alerts].[AlertDeliveryRecord] ([TenantId]) WHERE [IsDeleted] = 0;
END
GO

-- ----------------------------------------------------------------------------
-- [Alerts].[AlertResponseAction] -- child entity: response actions taken
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'Alerts' AND t.name = 'AlertResponseAction')
BEGIN
    CREATE TABLE [Alerts].[AlertResponseAction]
    (
        [ResponseActionId]   UNIQUEIDENTIFIER NOT NULL,
        [AlertId]            UNIQUEIDENTIFIER NOT NULL,
        [TenantId]           UNIQUEIDENTIFIER NOT NULL,
        [ActionDescription]  NVARCHAR(500)    NOT NULL,
        [TakenAtUtc]         DATETIME2(3)     NOT NULL,
        [TakenByActorKind]   NVARCHAR(20)     NOT NULL,
        [TakenByUserId]      UNIQUEIDENTIFIER NULL,
        [Notes]              NVARCHAR(2000)   NULL,
        [CreatedAtUtc]       DATETIME2(3)     NOT NULL CONSTRAINT DF_AlertResponseAction_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        [IsDeleted]          BIT              NOT NULL CONSTRAINT DF_AlertResponseAction_IsDeleted DEFAULT (0),

        CONSTRAINT PK_AlertResponseAction PRIMARY KEY CLUSTERED ([ResponseActionId]),
        CONSTRAINT FK_AlertResponseAction_Alert FOREIGN KEY ([AlertId]) REFERENCES [Alerts].[Alert] ([AlertId])
    );

    CREATE NONCLUSTERED INDEX IX_AlertResponseAction_AlertId ON [Alerts].[AlertResponseAction] ([AlertId]);
    CREATE NONCLUSTERED INDEX IX_AlertResponseAction_TenantId ON [Alerts].[AlertResponseAction] ([TenantId]) WHERE [IsDeleted] = 0;
END
GO
