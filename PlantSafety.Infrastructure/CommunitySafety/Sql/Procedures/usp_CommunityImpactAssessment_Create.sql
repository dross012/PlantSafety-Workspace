-- ============================================================================
--  Module:         CommunitySafety.CommunityImpactAssessment
--  File:           usp_CommunityImpactAssessment_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [CommunitySafety].[CommunityImpactAssessment], backing
--                  ICommunityImpactAssessmentRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityImpactAssessment_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @CommunityImpactAssessmentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @IncidentId UNIQUEIDENTIFIER,
    @Severity NVARCHAR(30),
    @AffectedArea NVARCHAR(200),
    @EstimatedPersonsAffected INT = NULL,
    @Description NVARCHAR(4000),
    @RequiresRightToKnowReporting BIT,
    @AssessedByUserId UNIQUEIDENTIFIER,
    @AssessedAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [CommunitySafety].[CommunityImpactAssessment]
            ([CommunityImpactAssessmentId], [TenantId], [SiteId], [Status], [IncidentId], [Severity], [AffectedArea], [EstimatedPersonsAffected], [Description], [RequiresRightToKnowReporting], [AssessedByUserId], [AssessedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@CommunityImpactAssessmentId, @TenantId, @SiteId, @Status, @IncidentId, @Severity, @AffectedArea, @EstimatedPersonsAffected, @Description, @RequiresRightToKnowReporting, @AssessedByUserId, @AssessedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
