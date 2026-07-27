-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange
--  Layer:          Infrastructure – SQL Scripts
--  File:           Moc.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ManagementOfChange (MOC) aggregate root, backing
--                  IMocRepository.CreateAsync/ApproveAsync/GetByIdAsync. This is one of only
--                  three real write paths in the Compliance module (see
--                  docs/architecture/scheduling-maintenance-compliance-extraction-blueprint.md
--                  Sec 1.2) -- previously the table and its stored procedures did not exist at
--                  all even though MocDapperRepository referenced them, so CreateMocCommand /
--                  the (currently-unwired) MOC-approve path could never have succeeded against a
--                  real database.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Moc' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Moc] (
        MocId              UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Moc PRIMARY KEY,
        TenantId            UNIQUEIDENTIFIER NOT NULL,
        SiteId              UNIQUEIDENTIFIER NOT NULL,
        Title               NVARCHAR(200)    NOT NULL,
        Description         NVARCHAR(4000)   NOT NULL,
        ChangeType          NVARCHAR(50)     NOT NULL,
        RiskLevel           NVARCHAR(50)     NOT NULL,
        InitiatorId         UNIQUEIDENTIFIER NOT NULL,
        ProposedStartUtc    DATETIME2(3)     NOT NULL,
        ProposedEndUtc      DATETIME2(3)     NULL,
        Status              NVARCHAR(30)     NOT NULL CONSTRAINT DF_Moc_Status DEFAULT (N'Draft'),
        ApprovalNotes       NVARCHAR(2000)   NULL,
        ApprovedByActorId   UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc       DATETIME2(3)     NULL,
        LastModifiedAtUtc   DATETIME2(3)     NOT NULL,
        CreatedAtUtc        DATETIME2(3)     NOT NULL,
        CreatedBy           UNIQUEIDENTIFIER NOT NULL,
        IsDeleted           BIT              NOT NULL CONSTRAINT DF_Moc_IsDeleted DEFAULT (0),
        RowVersion          ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Moc_TenantId_SiteId] ON [Compliance].[Moc] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX [IX_Moc_TenantId_Status] ON [Compliance].[Moc] (TenantId, Status, IsDeleted);
END
GO
