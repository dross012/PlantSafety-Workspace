USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Dave Ross
-- Create date:   2026-08-07
-- Table:         Compliance.NIOSHRespiratorMatrix
-- Description:   NIOSH respirator-selection guidance rows for a given hazard
--                (hazard type, concentration range, recommended respirator).
-- Notes:
--   - Reference data: no TenantId, no soft-delete columns.
--   - One hazard can map to multiple respirator recommendations across
--     concentration ranges/hazard types.
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name='Compliance') EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;')
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NIOSHRespiratorMatrix' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NIOSHRespiratorMatrix]
    (
        [MatrixId]              UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_NIOSHRespiratorMatrix_MatrixId DEFAULT NEWID(),
        [HazardId]               UNIQUEIDENTIFIER    NOT NULL,
        [HazardType]             NVARCHAR(100)       NOT NULL,
        [ConcentrationRange]     NVARCHAR(100)       NULL,
        [RecommendedRespirator]  NVARCHAR(200)       NOT NULL,
        [Notes]                  NVARCHAR(MAX)       NULL,
        [CreatedAtUtc]           DATETIME2(3)        NOT NULL CONSTRAINT DF_NIOSHRespiratorMatrix_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        [RowVersion]             ROWVERSION,

        CONSTRAINT PK_NIOSHRespiratorMatrix PRIMARY KEY CLUSTERED ([MatrixId]),
        CONSTRAINT FK_NIOSHRespiratorMatrix_NIOSHHazardProfile FOREIGN KEY ([HazardId])
            REFERENCES [Compliance].[NIOSHHazardProfile] ([HazardId])
    );
END
GO

------------------------------------------------------------
-- FK-supporting index
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NIOSHRespiratorMatrix_HazardId' AND object_id = OBJECT_ID('Compliance.NIOSHRespiratorMatrix'))
    CREATE NONCLUSTERED INDEX IX_NIOSHRespiratorMatrix_HazardId
        ON [Compliance].[NIOSHRespiratorMatrix] ([HazardId]);
GO
