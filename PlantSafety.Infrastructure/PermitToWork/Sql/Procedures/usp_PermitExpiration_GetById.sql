-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         PermitToWork
--  Layer:          Infrastructure -- SQL Scripts
--  File:           usp_PermitExpiration_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
--  SchemaVersion:  2026.1
--  Description:    Returns a single PermitExpiration row by identity. Backs IPermitExpirationRepository.GetByIdAsync.
--                  Additive-only evolution.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [PermitToWork].[usp_PermitExpiration_GetById]
    @PermitExpirationId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT PermitExpirationId, TenantId, SiteId, Status, PermitNumber, IssuedAtUtc, ExpiresAtUtc, ExtensionRequested, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy
    FROM [PermitToWork].[PermitExpiration]
    WHERE PermitExpirationId = @PermitExpirationId AND IsDeleted = 0;
END
GO
