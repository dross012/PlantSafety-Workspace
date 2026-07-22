-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         SafetyProcedures.HazardCommunicationRightToKnow
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_HazardCommunicationRightToKnow_GetById.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Returns a single HazardCommunicationRightToKnow row by identifier, backing
--                  IHazardCommunicationRightToKnowRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [SafetyProcedures].[usp_HazardCommunicationRightToKnow_GetById]
(
    @HazardCommunicationRightToKnowId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [HazardCommunicationRightToKnowId], [TenantId], [SiteId], [Status], [ProcedureTitle], [HazardClass], [SdsReference], [ApprovedByUserId], [PublishedAtUtc], [NextReviewDueAtUtc], [LabeledItemCount], [ComplianceScore], [CreatedAtUtc], [CreatedBy]
    FROM [SafetyProcedures].[HazardCommunicationRightToKnow]
    WHERE [HazardCommunicationRightToKnowId] = @HazardCommunicationRightToKnowId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO
