-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Compliance.Policy.Approvals
--  Layer:          Infrastructure – SQL Scripts
--  File:           PolicyApprovals.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Policy.Approvals read-model, backing
--                  IApprovalsRepository.GetApprovalsAsync (Policy variant).
--                  Named with the Policy prefix to avoid collision with the
--                  unrelated Compliance.ManagementOfChange.Approvals aggregate.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PolicyApprovals' AND schema_id = SCHEMA_ID('Compliance'))
BEGIN
    CREATE TABLE [Compliance].[PolicyApprovals] (
        ApprovalId     UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId       UNIQUEIDENTIFIER NOT NULL,
        SiteId         UNIQUEIDENTIFIER NULL,
        PolicyId       UNIQUEIDENTIFIER NOT NULL,
        PolicyTitle    NVARCHAR(300)    NOT NULL,
        Status         NVARCHAR(50)     NOT NULL,
        DecisionAtUtc  DATETIME2(3)     NOT NULL,
        ApproverId     UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX [IX_PolicyApprovals_TenantId_SiteId] ON [Compliance].[PolicyApprovals] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX [IX_PolicyApprovals_DecisionAtUtc] ON [Compliance].[PolicyApprovals] (DecisionAtUtc);
    CREATE NONCLUSTERED INDEX [IX_PolicyApprovals_PolicyId] ON [Compliance].[PolicyApprovals] (PolicyId);
END
GO
