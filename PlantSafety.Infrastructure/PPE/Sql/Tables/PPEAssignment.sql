-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PPE
--  Layer:          Infrastructure -- SQL Scripts
--  File:           PPEAssignment.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Table backing the PPEAssignment aggregate root (IPPEAssignmentRepository).
--                  Tenant-scoped (TenantId NOT NULL uniqueidentifier).
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PPE')
    EXEC('CREATE SCHEMA PPE AUTHORIZATION dbo;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PPEAssignment' AND schema_id = SCHEMA_ID('PPE'))
BEGIN
    CREATE TABLE [PPE].[PPEAssignment]
    (
        PPEAssignmentId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PPEAssignment PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        SiteId UNIQUEIDENTIFIER NULL,
        Status NVARCHAR(50) NOT NULL,
        WorkerId UNIQUEIDENTIFIER NOT NULL,
        JobRole NVARCHAR(400) NOT NULL,
        HazardZone NVARCHAR(400) NOT NULL,
        AssignedPpeItems NVARCHAR(400) NOT NULL,
        FitTestExpiresAtUtc DATETIME2(3) NULL,
        ReplacementReason NVARCHAR(400) NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        UpdatedAtUtc DATETIME2(3) NULL,
        UpdatedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_PPEAssignment_IsDeleted DEFAULT (0),
        ConcurrencyToken UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PPEAssignment_Concurrency DEFAULT (NEWID())
    );

    CREATE NONCLUSTERED INDEX IX_PPEAssignment_TenantId_Site ON [PPE].[PPEAssignment] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPEAssignment_Status ON [PPE].[PPEAssignment] (Status, IsDeleted);
    CREATE NONCLUSTERED INDEX IX_PPEAssignment_CreatedAtUtc ON [PPE].[PPEAssignment] (CreatedAtUtc);
END
GO
