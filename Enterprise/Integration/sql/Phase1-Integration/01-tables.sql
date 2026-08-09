-- ============================================================================
--  Project:        Enterprise.Integration -- Phase1-Integration
--  File:           01-tables.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [EnterpriseIntegration].[FieldMapping]
--                  (EnterpriseIntegrationMapping.cs: SourceSystem +
--                  ExternalFieldName -> InternalFieldName, one row per
--                  explicit mapping) and [EnterpriseIntegration].
--                  [IntegrationEventLog] (append-only mirror of every real
--                  EnterpriseIntegrationEvent.cs: EventId/SourceSystem/
--                  EventType/MappedPayload/OccurredAtUtc -- MappedPayload is
--                  a real IReadOnlyDictionary<string,string>, serialized as
--                  a JSON object per this task's instruction, since it's
--                  caller-defined key/value pairs, not a fixed relational
--                  shape).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FieldMapping' AND schema_id = SCHEMA_ID('EnterpriseIntegration'))
BEGIN
    CREATE TABLE [EnterpriseIntegration].[FieldMapping]
    (
        [FieldMappingId]     UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_FieldMapping_Id DEFAULT NEWSEQUENTIALID(),
        [SourceSystem]        NVARCHAR(10)      NOT NULL,
        [ExternalFieldName]   NVARCHAR(200)     NOT NULL,
        [InternalFieldName]   NVARCHAR(200)     NOT NULL,

        [CreatedUtc]          DATETIME2(3)      NOT NULL CONSTRAINT DF_FieldMapping_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]           NVARCHAR(128)     NOT NULL CONSTRAINT DF_FieldMapping_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]          DATETIME2(3)      NULL,
        [UpdatedBy]           NVARCHAR(128)     NULL,
        [IsDeleted]           BIT               NOT NULL CONSTRAINT DF_FieldMapping_IsDeleted DEFAULT (0),

        CONSTRAINT PK_FieldMapping PRIMARY KEY CLUSTERED ([FieldMappingId]),
        CONSTRAINT UQ_FieldMapping_SourceSystem_ExternalFieldName UNIQUE ([SourceSystem], [ExternalFieldName]),
        CONSTRAINT CK_FieldMapping_SourceSystem CHECK ([SourceSystem] IN (N'HR', N'ERP', N'MES', N'CMMS', N'EHS'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IntegrationEventLog' AND schema_id = SCHEMA_ID('EnterpriseIntegration'))
BEGIN
    CREATE TABLE [EnterpriseIntegration].[IntegrationEventLog]
    (
        [EventId]           UNIQUEIDENTIFIER  NOT NULL,
        [SourceSystem]      NVARCHAR(10)      NOT NULL,
        [EventType]         NVARCHAR(200)     NOT NULL,
        [MappedPayloadJson] NVARCHAR(MAX)     NOT NULL,
        [OccurredAtUtc]     DATETIME2(3)      NOT NULL,

        [CreatedUtc]        DATETIME2(3)      NOT NULL CONSTRAINT DF_IntegrationEventLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]         NVARCHAR(128)     NOT NULL CONSTRAINT DF_IntegrationEventLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]         BIT               NOT NULL CONSTRAINT DF_IntegrationEventLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_IntegrationEventLog PRIMARY KEY CLUSTERED ([EventId]),
        CONSTRAINT CK_IntegrationEventLog_SourceSystem CHECK ([SourceSystem] IN (N'HR', N'ERP', N'MES', N'CMMS', N'EHS')),
        CONSTRAINT CK_IntegrationEventLog_MappedPayloadJson CHECK (ISJSON([MappedPayloadJson]) = 1)
    );
END
GO
