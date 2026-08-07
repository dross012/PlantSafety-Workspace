USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Dave Ross
-- Create date:   2026-08-07
-- Table:         Compliance.NIOSHHazardModuleMapping
-- Description:   Many-to-many mapping of a NIOSH hazard profile to the
--                PlantSafety module(s) it is relevant to, with severity and
--                the rule code that produced the mapping (mirrors
--                OSHARequirementModuleMapping).
-- Notes:
--   - Reference data: no TenantId, no soft-delete columns.
--   - [Module] is intentionally free text, not a DB-enforced enum (mirrors
--     OSHARequirement.AppliesToModule). Valid values as of 2026-08-07: PPE,
--     OperationalSafety, EnvironmentalHealth, HazardousWasteManagement,
--     Ergonomics, PermitToWork, EmployeeSafety.
-- =============================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name='Compliance') EXEC('CREATE SCHEMA Compliance AUTHORIZATION dbo;')
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'NIOSHHazardModuleMapping' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[NIOSHHazardModuleMapping]
    (
        [MappingId]         UNIQUEIDENTIFIER    NOT NULL CONSTRAINT DF_NIOSHHazardModuleMapping_MappingId DEFAULT NEWID(),
        [HazardId]          UNIQUEIDENTIFIER    NOT NULL,
        [Module]            NVARCHAR(100)       NOT NULL,
        [IsPrimary]         BIT                 NOT NULL CONSTRAINT DF_NIOSHHazardModuleMapping_IsPrimary DEFAULT (0),
        [Severity]          NVARCHAR(50)        NOT NULL,
        [MappingRuleCode]   NVARCHAR(100)       NOT NULL,
        [Rationale]         NVARCHAR(500)       NULL,
        [CreatedAtUtc]      DATETIME2(3)        NOT NULL CONSTRAINT DF_NIOSHHazardModuleMapping_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT PK_NIOSHHazardModuleMapping PRIMARY KEY CLUSTERED ([MappingId]),
        CONSTRAINT FK_NIOSHHazardModuleMapping_NIOSHHazardProfile FOREIGN KEY ([HazardId])
            REFERENCES [Compliance].[NIOSHHazardProfile] ([HazardId]),
        CONSTRAINT UQ_NIOSHHazardModuleMapping_HazardId_Module UNIQUE ([HazardId], [Module])
    );
END
GO

------------------------------------------------------------
-- Indexes
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NIOSHHazardModuleMapping_Module' AND object_id = OBJECT_ID('Compliance.NIOSHHazardModuleMapping'))
    CREATE NONCLUSTERED INDEX IX_NIOSHHazardModuleMapping_Module
        ON [Compliance].[NIOSHHazardModuleMapping] ([Module]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_NIOSHHazardModuleMapping_HazardId' AND object_id = OBJECT_ID('Compliance.NIOSHHazardModuleMapping'))
    CREATE NONCLUSTERED INDEX IX_NIOSHHazardModuleMapping_HazardId
        ON [Compliance].[NIOSHHazardModuleMapping] ([HazardId]);
GO
