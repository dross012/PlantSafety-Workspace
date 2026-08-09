-- ============================================================================
--  Project:        Enterprise.DataLake -- Phase1-DataLake
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Description:    Covering indexes for the lake's two real query patterns:
--                  by RecordType + time range, and by PartitionKey.
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DataLakeRecord_RecordType_RecordedAtUtc' AND object_id = OBJECT_ID('DataLake.DataLakeRecord'))
    CREATE NONCLUSTERED INDEX IX_DataLakeRecord_RecordType_RecordedAtUtc
        ON [DataLake].[DataLakeRecord] ([RecordType], [RecordedAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DataLakeRecord_PartitionKey' AND object_id = OBJECT_ID('DataLake.DataLakeRecord'))
    CREATE NONCLUSTERED INDEX IX_DataLakeRecord_PartitionKey
        ON [DataLake].[DataLakeRecord] ([PartitionKey]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DataLakeRecord_SourceId' AND object_id = OBJECT_ID('DataLake.DataLakeRecord'))
    CREATE NONCLUSTERED INDEX IX_DataLakeRecord_SourceId
        ON [DataLake].[DataLakeRecord] ([SourceId]);
GO
