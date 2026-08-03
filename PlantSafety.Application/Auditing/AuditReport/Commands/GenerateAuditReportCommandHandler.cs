// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GenerateAuditReportCommand. Auto-populates the Timeline section
//   via AuditTimeline's IReportSectionProvider<AuditTimelineSectionRequest, AuditTimelineDto>
//   (Handbook Part IX §9.3.2 -- resolved through DI as a public read-model contract, never
//   dispatched as a cross-module Query through ISender), composes the five canonical sections via
//   the Domain-layer AuditReportContentBuilder, drafts the AuditReport aggregate, selects the
//   matching IAuditReportRenderer for the requested format, renders and persists the byte-stream
//   content, then marks the aggregate Generated or Failed accordingly.
//   2026-08-03: replaced the direct GetAuditTimelineForEntityQuery/ISender dispatch into the
//   sibling AuditTimeline submodule with the SharedKernel cross-module read-model composition
//   contract (structural refactor only -- output shape/data unchanged).
// SchemaVersion: 1

using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.Interfaces;
using PlantSafety.Domain.Auditing.AuditReport.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditReport.Abstractions;
using PlantSafety.Application.Auditing.AuditReport.Mapping;
using PlantSafety.Application.Auditing.AuditReport.Models;
using PlantSafety.Application.Auditing.AuditReport.Repositories;
using PlantSafety.Application.Auditing.AuditTimeline.Contracts;
using PlantSafety.Application.Auditing.AuditTimeline.Models;

namespace PlantSafety.Application.Auditing.AuditReport.Commands;

/// <summary>Handles <see cref="GenerateAuditReportCommand"/>.</summary>
public sealed class GenerateAuditReportCommandHandler : IRequestHandler<GenerateAuditReportCommand, Result<AuditReportDto>>
{
    private readonly IReportSectionProvider<AuditTimelineSectionRequest, AuditTimelineDto> _timelineSectionProvider;
    private readonly IAuditReportFactory _factory;
    private readonly AuditReportContentBuilder _contentBuilder;
    private readonly IAuditReportRepository _repository;
    private readonly IReadOnlyList<IAuditReportRenderer> _renderers;
    private readonly IAuditReportContentStore _contentStore;
    private readonly ILogger<GenerateAuditReportCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GenerateAuditReportCommandHandler(
        IReportSectionProvider<AuditTimelineSectionRequest, AuditTimelineDto> timelineSectionProvider,
        IAuditReportFactory factory,
        AuditReportContentBuilder contentBuilder,
        IAuditReportRepository repository,
        IEnumerable<IAuditReportRenderer> renderers,
        IAuditReportContentStore contentStore,
        ILogger<GenerateAuditReportCommandHandler> logger)
    {
        _timelineSectionProvider = timelineSectionProvider;
        _factory = factory;
        _contentBuilder = contentBuilder;
        _repository = repository;
        _renderers = renderers.ToList();
        _contentStore = contentStore;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditReportDto>> Handle(GenerateAuditReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!AuditReportFormat.TryParse(request.Format, out var format))
            {
                return Result<AuditReportDto>.Failure(new ApplicationError(
                    "Auditing.AuditReport.UnsupportedFormat",
                    $"'{request.Format}' is not a supported audit report format.",
                    Guid.NewGuid()));
            }

            var renderer = _renderers.FirstOrDefault(r => r.Format == format);
            if (renderer is null)
            {
                return Result<AuditReportDto>.Failure(new ApplicationError(
                    "Auditing.AuditReport.RendererNotRegistered",
                    $"No IAuditReportRenderer is registered for format '{format.Code}'.",
                    Guid.NewGuid()));
            }

            var timelineResult = await _timelineSectionProvider.GetSectionAsync(
                new AuditTimelineSectionRequest(request.TenantId, request.TargetEntityType, request.TargetEntityId), cancellationToken);

            var timelineJson = timelineResult.IsSuccess
                ? JsonSerializer.Serialize(timelineResult.Value)
                : JsonSerializer.Serialize(new { error = timelineResult.Error.Message });

            var sectionContent = new Dictionary<AuditReportSectionType, string>
            {
                [AuditReportSectionType.EntityIdentity] = request.EntityIdentityJson,
                [AuditReportSectionType.Timeline] = timelineJson,
                [AuditReportSectionType.Approvals] = request.ApprovalsJson,
                [AuditReportSectionType.Exceptions] = request.ExceptionsJson,
                [AuditReportSectionType.Signatures] = request.SignaturesJson,
            };

            var requestedBy = UserId.FromGuid(request.RequestedBy);
            var actor = PerformedByActor.User(requestedBy);
            var correlationId = new CorrelationId(request.CorrelationId);
            var causationId = new CausationId(request.CausationId);
            var nowUtc = UtcTimestamp.Now();

            var report = _factory.Draft(
                request.TenantId, request.SiteId, request.TargetEntityType, request.TargetEntityId,
                format, requestedBy, nowUtc,
                reportId => _contentBuilder.BuildSections(reportId, sectionContent));

            await _repository.AddAsync(report, cancellationToken);

            var renderModel = new AuditReportRenderModel(
                report.AuditReportId.Value, report.TargetEntityType, report.TargetEntityId, nowUtc.Value,
                report.Sections.Select(s => new AuditReportRenderSection(s.SectionType.Code, s.Heading, s.ContentJson)).ToList());

            try
            {
                var rendered = await renderer.RenderAsync(renderModel, cancellationToken);
                var reference = await _contentStore.SaveAsync(request.TenantId, report.AuditReportId.Value, format, rendered, cancellationToken);

                report.MarkGenerated(reference, rendered.LongLength, UtcTimestamp.Now(), actor, correlationId, causationId);
            }
            catch (Exception renderEx)
            {
                _logger.LogError(renderEx, "AuditReport rendering failed for {AuditReportId}", report.AuditReportId.Value);
                report.MarkFailed(renderEx.Message, UtcTimestamp.Now(), actor, correlationId, causationId);
            }

            await _repository.UpdateAsync(report, cancellationToken);

            return Result<AuditReportDto>.Success(AuditReportMapper.ToDto(report));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AuditReport for {TargetEntityType} {TargetEntityId} Tenant {TenantId}",
                request.TargetEntityType, request.TargetEntityId, request.TenantId);

            return Result<AuditReportDto>.Failure(new ApplicationError(
                "Auditing.AuditReport.GenerationFailed",
                "Unable to generate the audit report at this time.",
                Guid.NewGuid()));
        }
    }
}
