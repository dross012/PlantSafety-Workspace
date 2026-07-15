-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.AiGovernance
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Model_Create.sql
--  Created:        2026-07-15
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Registers a new governed AI/ML model. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [AiGovernance].[usp_Model_Create]
(
    @TenantId      UNIQUEIDENTIFIER,
    @CorrelationId UNIQUEIDENTIFIER,
    @ActorId       UNIQUEIDENTIFIER,
    @NowUtc        DATETIME2(3),
    @ModelId       UNIQUEIDENTIFIER,
    @SiteId        UNIQUEIDENTIFIER = NULL,
    @ModelName     NVARCHAR(200),
    @ModelVersion  NVARCHAR(50),
    @RiskTier      NVARCHAR(20)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        INSERT INTO [AiGovernance].[GovernedModel]
            ([ModelId], [TenantId], [SiteId], [ModelName], [ModelVersion], [GovernanceStatus],
             [RiskTier], [DriftStatus], [LastReviewedAtUtc], [ReviewerName], [CreatedAtUtc], [CorrelationId], [ActorId])
        VALUES
            (@ModelId, @TenantId, @SiteId, @ModelName, @ModelVersion, 'PendingReview',
             @RiskTier, 'Stable', @NowUtc, NULL, @NowUtc, @CorrelationId, @ActorId);

        RETURN 0;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorNumber INT = ERROR_NUMBER();
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RETURN @ErrorNumber;
    END CATCH;
END;
GO
