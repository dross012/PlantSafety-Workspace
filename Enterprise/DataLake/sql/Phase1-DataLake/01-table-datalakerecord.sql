-- ============================================================================
--  Project:        Enterprise.DataLake -- Phase1-DataLake
--  File:           01-table-datalakerecord.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [DataLake].[DataLakeRecord] -- the durable
--                  mirror of DataLakeRecord.cs (RecordId/RecordType/
--                  PartitionKey/SourceId/Payload/RecordedAtUtc). Genuinely
--                  append-only: no UPDATE path is defined below (only
--                  INSERT and read), matching the C# record's immutability
--                  and this codebase's validate-append-only rule. Payload
--                  stays an opaque NVARCHAR(MAX) string -- per
--                  DataLakeRecord.cs's own header, the caller serializes its
--                  own domain event/DTO before writing, and this project is
--                  deliberately decoupled from every upstream domain
--                  project's real type. PartitionKey (DataLakePartitionKey,
--                  presumably a date/tenant-shaped value in the C# type) is
--                  persisted as a plain NVARCHAR partition-key string;
--                  RecordedAtUtc is the primary time-range query column.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DataLakeRecord' AND schema_id = SCHEMA_ID('DataLake'))
BEGIN
    CREATE TABLE [DataLake].[DataLakeRecord]
    (
        [RecordId]       UNIQUEIDENTIFIER  NOT NULL,
        [RecordType]     NVARCHAR(20)      NOT NULL,
        [PartitionKey]   NVARCHAR(200)     NOT NULL,
        [SourceId]       UNIQUEIDENTIFIER  NOT NULL,
        [Payload]        NVARCHAR(MAX)     NOT NULL,
        [RecordedAtUtc]  DATETIME2(3)      NOT NULL,

        [CreatedUtc]     DATETIME2(3)      NOT NULL CONSTRAINT DF_DataLakeRecord_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]      NVARCHAR(128)     NOT NULL CONSTRAINT DF_DataLakeRecord_CreatedBy DEFAULT SYSTEM_USER,

        CONSTRAINT PK_DataLakeRecord PRIMARY KEY NONCLUSTERED ([RecordId]),
        CONSTRAINT CK_DataLakeRecord_RecordType CHECK ([RecordType] IN
            (N'Facility', N'Employee', N'Machine', N'Hazard', N'Event', N'Workflow', N'Prediction'))
    );

    -- Clustered on RecordedAtUtc, not RecordId: append-only, time-ordered
    -- write pattern (matches the C# ConcurrentQueue's insertion order) and
    -- every expected query pattern is time-range-scoped, per
    -- validate-partitioning/create-clustered-pk guidance for event-history
    -- tables. PK above stays NONCLUSTERED on RecordId (StrongId uniqueness),
    -- clustered index carries the real physical/query-pattern ordering.
    CREATE CLUSTERED INDEX CIX_DataLakeRecord_RecordedAtUtc ON [DataLake].[DataLakeRecord] ([RecordedAtUtc]);
END
GO
