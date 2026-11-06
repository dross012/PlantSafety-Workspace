-- =============================================
-- Enterprise.Integration -- EmergencyMgt-Integration
-- Script:  40-seed-routingkeys.sql
-- Purpose: Registers the 20 routing keys EmergencyMgt publishes across its base module and its
--          14 Extensions subsystems (base lifecycle/notification/evacuation/response/drill/
--          after-action/corrective-action keys, plus one key per Extensions subsystem that has
--          an obvious single dominant event).
-- =============================================

USE [PlantSafety.CoreDB]
GO
SET NOCOUNT ON;
GO

EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.event.created',                 @Description = N'A new EmergencyEvent was declared.';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.event.updated',                 @Description = N'An EmergencyEvent record was updated.';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.event.lifecycle',               @Description = N'An EmergencyEvent transitioned lifecycle status (Declared/Active Response/Stabilizing/Resolved/Closed/Archived).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.notification.sent',             @Description = N'An EmergencyNotification was sent.';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.evacuation.updated',            @Description = N'An EmergencyEvacuationRoute was updated.';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.response.logged',               @Description = N'An EmergencyResponseLog entry was recorded.';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.drill.executed',                @Description = N'An EmergencyDrill was executed.';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.afteraction.completed',         @Description = N'An EmergencyAfterActionReport was completed.';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.correctiveaction.created',      @Description = N'An EmergencyCorrectiveAction was created.';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.readiness.scored',              @Description = N'An EmergencyReadinessScore was computed (Extensions: Readiness Score).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.compliance.artifact.created',   @Description = N'An EmergencyComplianceArtifact was generated (Extensions: Compliance Engine).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.site.event.created',            @Description = N'An EmergencySiteEvent was recorded (Extensions: Multi-Site Orchestration).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.resource.assigned',             @Description = N'An EmergencyResource was assigned (Extensions: Resource Management).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.zone.updated',                  @Description = N'An EmergencyZone was updated (Extensions: Zones & Geofencing).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.command.assigned',              @Description = N'An EmergencyCommandAssignment was made (Extensions: ICS-Lite Command Structure).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.simulation.run',                @Description = N'An EmergencySimulationRun completed (Extensions: Simulation Engine).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.ai.recommendation.created',     @Description = N'An EmergencyAIRecommendation was generated (Extensions: AI Assistant).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.video.event.detected',          @Description = N'An EmergencyVideoEvent was detected (Extensions: Video Analytics).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.drone.telemetry.received',      @Description = N'EmergencyDroneTelemetry was received (Extensions: Drone Integration).';
EXEC [EmergencyMgtIntegration].[usp_RoutingKeyRegistry_Register] @RoutingKey = N'emergency.responder.relationship.updated',@Description = N'An EmergencyResponderRelationship was updated (Extensions: Social Graph).';
GO
