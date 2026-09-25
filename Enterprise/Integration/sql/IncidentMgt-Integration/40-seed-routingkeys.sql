-- =============================================
-- Enterprise.Integration -- IncidentMgt-Integration
-- Script:  40-seed-routingkeys.sql
-- Purpose: Registers the 8 routing keys/topics IncidentMgt publishes (see IncidentMgt repo,
--          sql/IncidentMgt/... installer server spec Section 6 "Define routing keys/topics",
--          and this removal/integration doc Section 3.3).
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET NOCOUNT ON;
GO

EXEC [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'incident.reported',            @Description = N'A new incident was reported in IncidentMgt.';
EXEC [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'incident.updated',             @Description = N'An incident record was updated (supersedes the removed incident.escalated/closed-only pair with a general update signal; see incident.closed below for the terminal case).';
EXEC [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'incident.closed',              @Description = N'An incident reached a terminal status (Closed/Archived).';
EXEC [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'investigation.started',        @Description = N'An investigation was opened for an incident.';
EXEC [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'investigation.completed',      @Description = N'An investigation reached a conclusion.';
EXEC [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'rootcause.identified',         @Description = N'A root cause was identified for an incident.';
EXEC [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'correctiveaction.assigned',    @Description = N'A corrective action was assigned to an owner.';
EXEC [IncidentMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'correctiveaction.closed',      @Description = N'A corrective action was verified and closed.';
GO
