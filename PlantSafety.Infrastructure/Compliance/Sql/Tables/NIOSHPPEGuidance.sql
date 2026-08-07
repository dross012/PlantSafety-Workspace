USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Dave Ross
-- Create date:   2026-08-07
-- Table:         Compliance.NIOSHPPEGuidance
-- Description:   NIOSH personal protective equipment guidance rows for a
--                given hazard (PPE type, performance criteria, limitations).
-- Notes:
--   - Reference data: no TenantId, no soft-delete columns.
--   - One hazard can map to multiple PPE guidance rows (gloves, respirator,
--     eye protection, etc.).
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name='Compliance') EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;')
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NIOSHPPEGuidance' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NIOSHPPEGuidance]
    (
        [GuidanceId]           UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_NIOSHPPEGuidance_GuidanceId DEFAULT NEWID(),
        [HazardId]             UNIQUEIDENTIFIER    NOT NULL,
        [PPEType]              NVARCHAR(100)       NOT NULL,
        [PerformanceCriteria]  NVARCHAR(MAX)       NULL,
        [Limitations]          NVARCHAR(MAX)       NULL,
        [CreatedAtUtc]         DATETIME2(3)        NOT NULL CONSTRAINT DF_NIOSHPPEGuidance_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        [RowVersion]           ROWVERSION,

        CONSTRAINT PK_NIOSHPPEGuidance PRIMARY KEY CLUSTERED ([GuidanceId]),
        CONSTRAINT FK_NIOSHPPEGuidance_NIOSHHazardProfile FOREIGN KEY ([HazardId])
            REFERENCES [Compliance].[NIOSHHazardProfile] ([HazardId])
    );
END
GO

------------------------------------------------------------
-- FK-supporting index
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NIOSHPPEGuidance_HazardId' AND object_id = OBJECT_ID('Compliance.NIOSHPPEGuidance'))
    CREATE NONCLUSTERED INDEX IX_NIOSHPPEGuidance_HazardId
        ON [Compliance].[NIOSHPPEGuidance] ([HazardId]);
GO
