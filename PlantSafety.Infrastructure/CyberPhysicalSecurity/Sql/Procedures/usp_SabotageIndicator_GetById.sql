-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SabotageIndicator_GetById.sql | Created: 2026-08-04
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_GetById]
    @SabotageIndicatorId UNIQUEIDENTIFIER,
    @TenantId            UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT SabotageIndicatorId, TenantId, SiteId, Source, SourceReferenceId, Description, Severity, Status,
           CorrelatedIndicatorCount, RaisedAtUtc, RiskManagementPriority, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[SabotageIndicator]
    WHERE SabotageIndicatorId = @SabotageIndicatorId AND TenantId = @TenantId;
END
GO
