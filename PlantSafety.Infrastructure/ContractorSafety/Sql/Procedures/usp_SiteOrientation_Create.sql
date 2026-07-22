-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         ContractorSafety.SiteOrientation
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_SiteOrientation_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [ContractorSafety].[SiteOrientation], backing
--                  ISiteOrientationRepository.AddAsync. Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [ContractorSafety].[usp_SiteOrientation_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @SiteOrientationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @OrientationTitle NVARCHAR(400),
    @ContractorName NVARCHAR(400),
    @ConductedByUserId UNIQUEIDENTIFIER,
    @DeliveredAtUtc DATETIME2(3) = NULL,
    @AttendeeCount INT,
    @ComprehensionScore DECIMAL(9,2) = NULL,
    @PassingScoreThreshold DECIMAL(9,2),
    @ExpiresAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [ContractorSafety].[SiteOrientation]
            ([SiteOrientationId], [TenantId], [SiteId], [Status], [OrientationTitle], [ContractorName], [ConductedByUserId], [DeliveredAtUtc], [AttendeeCount], [ComprehensionScore], [PassingScoreThreshold], [ExpiresAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@SiteOrientationId, @TenantId, @SiteId, @Status, @OrientationTitle, @ContractorName, @ConductedByUserId, @DeliveredAtUtc, @AttendeeCount, @ComprehensionScore, @PassingScoreThreshold, @ExpiresAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
