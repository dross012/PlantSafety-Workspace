USE [PlantSafety.CloudIdentityDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Phase 5 Views & Summaries
-- Script:        52-cloudidentitydb-views.sql
-- Author:        View & Summary Builder (unattended agent run)
-- Purpose:       Active-record views for PlantSafety.CloudIdentityDB's 3 tables.
--                Note: the schema is named "Identity", a reserved T-SQL keyword --
--                every reference below uses [Identity] bracket-quoting (confirmed live:
--                unbracketed Identity.<Table> raises Msg 156 "Incorrect syntax near the
--                keyword 'Identity'"; matches the [Identity] bracketing already used in
--                Phase 4's 42-cloudidentitydb-triggers-functions.sql).
--
-- Deliberately NOT created:
--   - No cross-table reporting view: sys.foreign_keys confirms ZERO foreign keys exist
--     across Identity.ManagedIdentity, Identity.RoleAssignment, Identity.ServicePrincipal
--     (verified live). RoleAssignment.PrincipalId is a plain Azure AD object id column
--     (PrincipalType free-text), not an FK to either ManagedIdentity or ServicePrincipal,
--     so joining them would be an invented relationship, not a grounded one.
--   - No tenant-scoped view: this module has no TenantId column at all (Azure-identity
--     resources are scoped by ArmResourceId/AzureTenantId, not the platform TenantId),
--     so the CoreDB TVF pattern does not apply here either.
--   - No materialized summary table: no time-series/aggregation access pattern is
--     documented for this module (role-assignment/identity inventories are queried by
--     resource id, not aggregated by day). No summary recommended.
-- =============================================

CREATE OR ALTER VIEW [Identity].vw_ManagedIdentity_Active
AS
SELECT
    e.ManagedIdentityId, e.ArmResourceId, e.Name, e.Location, e.ProvisioningState,
    e.AzureTenantId, e.PrincipalId, e.ClientId, e.TagsJson,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM [Identity].ManagedIdentity AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW [Identity].vw_RoleAssignment_Active
AS
SELECT
    e.RoleAssignmentId, e.ArmResourceId, e.RoleDefinitionName, e.PrincipalId, e.PrincipalType,
    e.RoleDefinitionId, e.Scope,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM [Identity].RoleAssignment AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO

CREATE OR ALTER VIEW [Identity].vw_ServicePrincipal_Active
AS
SELECT
    e.ServicePrincipalId, e.EntraObjectId, e.DisplayName, e.AppId, e.ServicePrincipalType,
    e.AccountEnabled,
    e.CreatedUtc, e.CreatedBy, e.UpdatedUtc, e.UpdatedBy, e.IsDeleted, e.ConcurrencyToken
FROM [Identity].ServicePrincipal AS e
WHERE ISNULL(e.IsDeleted, 0) = 0;
GO
