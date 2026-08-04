-- ============================================================================
--  Module: CyberPhysicalSecurity | Layer: Infrastructure -- SQL Scripts
--  File: usp_SabotageIndicator_GetForCorrelationWindow.sql | Created: 2026-08-04
--  Description: Returns active indicators raised within [@WindowStartUtc, @WindowEndUtc], used by
--                SabotageCorrelationService to determine multi-source auto-escalation.
-- ============================================================================
USE [PlantSafety.CoreDB];
GO

CREATE OR ALTER PROCEDURE [CyberPhysicalSecurity].[usp_SabotageIndicator_GetForCorrelationWindow]
    @TenantId       UNIQUEIDENTIFIER,
    @WindowStartUtc DATETIME2(3),
    @WindowEndUtc   DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN
    SELECT SabotageIndicatorId, TenantId, SiteId, Source, SourceReferenceId, Description, Severity, Status,
           CorrelatedIndicatorCount, RaisedAtUtc, RiskManagementPriority, CreatedAtUtc, CreatedBy
    FROM [CyberPhysicalSecurity].[SabotageIndicator]
    WHERE TenantId = @TenantId
      AND Status IN (N'Open', N'UnderInvestigation')
      AND RaisedAtUtc BETWEEN @WindowStartUtc AND @WindowEndUtc
    ORDER BY RaisedAtUtc DESC;
END
GO
