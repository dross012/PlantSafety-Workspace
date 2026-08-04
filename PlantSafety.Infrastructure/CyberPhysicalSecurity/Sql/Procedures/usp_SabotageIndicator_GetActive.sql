-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SabotageIndicator_GetActive.sql | Created: 2026-08-04
--  Description: Backs ListActiveSabotageIndicatorsQuery / the Active Sabotage Indicator Board.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_GetActive]
    @TenantId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN
    SELECT SabotageIndicatorId, TenantId, SiteId, Source, SourceReferenceId, Description, Severity, Status,
           CorrelatedIndicatorCount, RaisedAtUtc, RiskManagementPriority, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[SabotageIndicator]
    WHERE TenantId = @TenantId AND Status IN (N'Open', N'UnderInvestigation')
    ORDER BY RaisedAtUtc DESC;
END
GO
