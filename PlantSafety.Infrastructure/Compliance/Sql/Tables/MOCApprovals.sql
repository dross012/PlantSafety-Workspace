-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.ManagementOfChange.Approvals
--  Layer:          Infrastructure – SQL Scripts
--  File:           MOCApprovals.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the ManagementOfChange.Approvals read-model, backing
--                  IApprovalsRepository.GetApprovalsAsync (ManagementOfChange variant).
--                  Named with the MOC prefix to avoid collision with the unrelated
--                  Compliance.Policy.Approvals aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MOCApprovals' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[MOCApprovals] (
        ApprovalId      UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        ChangeRequestId UNIQUEIDENTIFIER NOT NULL,
        TenantId        UNIQUEIDENTIFIER NOT NULL,
        SiteId          UNIQUEIDENTIFIER NULL,
        ApproverName    NVARCHAR(200)    NOT NULL,
        Status          NVARCHAR(50)     NOT NULL,
        DecidedAtUtc    DATETIME2(3)     NOT NULL,
        ApprovedBy      UNIQUEIDENTIFIER NOT NULL,
        RowVersion       ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_MOCApprovals_TenantId_SiteId] ON [Compliance].[MOCApprovals] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_MOCApprovals_DecidedAtUtc] ON [Compliance].[MOCApprovals] (DecidedAtUtc);
    CREATE NONCLUSTERED INDEX [IX_MOCApprovals_ChangeRequestId] ON [Compliance].[MOCApprovals] (ChangeRequestId);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[Compliance].[MOCApprovals]') AND name = 'RowVersion')
BEGIN
    ALTER TABLE [Compliance].[MOCApprovals] ADD RowVersion ROWVERSION NOT NULL;
END
GO
