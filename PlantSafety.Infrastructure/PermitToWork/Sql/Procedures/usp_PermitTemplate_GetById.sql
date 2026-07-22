-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitTemplate_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PermitTemplate row by identity. Backs IPermitTemplateRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitTemplate_GetById]
    @PermitTemplateId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitTemplateId, TenantId, SiteId, Status, PermitTypeCode, RequiredControls, StandardValidityHours, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PermitToWork].[PermitTemplate]
    WHERE PermitTemplateId = @PermitTemplateId AND IsDeleted = 0;
END
GO
