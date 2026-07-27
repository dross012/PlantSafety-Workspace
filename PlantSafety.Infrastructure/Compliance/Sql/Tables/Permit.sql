-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Permits
--  Layer:          Infrastructure – SQL Scripts
--  File:           Permit.sql
--  Created:        2026-07-27
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Permit-to-Work (PTW) aggregate root, backing
--                  IPermitRepository.CreateAsync/ApproveAsync/GetByIdAsync/ListAsync. This is one
--                  of only three real write paths in the Compliance module (see
--                  docs/architecture/scheduling-maintenance-compliance-extraction-blueprint.md
--                  Sec 1.2) -- previously the table and its stored procedures did not exist at
--                  all even though PermitDapperRepository referenced them.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Permit' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[Permit] (
        PermitId            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Permit PRIMARY KEY,
        TenantId             UNIQUEIDENTIFIER NOT NULL,
        SiteId               UNIQUEIDENTIFIER NOT NULL,
        PermitType           NVARCHAR(100)    NOT NULL,
        PermitNumber         NVARCHAR(50)     NOT NULL,
        WorkDescription      NVARCHAR(2000)   NOT NULL,
        WorkLocation         NVARCHAR(500)    NOT NULL,
        WorkLeadId           UNIQUEIDENTIFIER NOT NULL,
        ValidFromUtc         DATETIME2(3)     NOT NULL,
        ValidToUtc           DATETIME2(3)     NOT NULL,
        Status               NVARCHAR(30)     NOT NULL CONSTRAINT DF_Permit_Status DEFAULT (N'Draft'),
        ApprovalNotes        NVARCHAR(2000)   NULL,
        ApprovedByActorId    UNIQUEIDENTIFIER NULL,
        ApprovedAtUtc        DATETIME2(3)     NULL,
        LastModifiedAtUtc    DATETIME2(3)     NOT NULL,
        CreatedAtUtc         DATETIME2(3)     NOT NULL,
        CreatedBy            UNIQUEIDENTIFIER NOT NULL,
        IsDeleted            BIT              NOT NULL CONSTRAINT DF_Permit_IsDeleted DEFAULT (0),
        RowVersion           ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_Permit_TenantId_SiteId] ON [Compliance].[Permit] (TenantId, SiteId, IsDeleted);
    CREATE NONCLUSTERED INDEX [IX_Permit_TenantId_Status] ON [Compliance].[Permit] (TenantId, Status, IsDeleted);
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Permit_PermitNumber] ON [Compliance].[Permit] (PermitNumber);
END
GO
