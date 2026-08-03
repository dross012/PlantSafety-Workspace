# ============================================================================
# Dockerfile — PlantSafety.Api
# Phase 6.6 (Enterprise Deployment & Release Architecture), §6.2 Blue/Green Deployments.
#
# GENERIC / CLOUD-AGNOSTIC BY DESIGN: the actual target deploy platform for
# PlantSafety.Api is not determinable from this repository as it stands (see
# docs\Phase6\deployment-release-architecture.md's §6.2 finding — the sibling `Azure`
# solution models Azure resource types domain-side and is NOT an App Service/AKS
# deployment configuration for this API). This Dockerfile therefore targets plain
# Docker/OCI semantics only — no Kubernetes manifests, no Azure App Service
# site-extensions/deployment-slot config, no azd/azure.yaml — so it can be adapted to
# whichever platform is chosen without rework. That platform decision is flagged as a
# genuine open ambiguity, not guessed here.
#
# Standard multi-stage .NET 8 build: build/test in an SDK image, publish, then copy
# only the published output into a slim ASP.NET runtime image.
# ============================================================================

# ---- Build stage -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first so `dotnet restore` is Docker-layer-cached
# independently of source-code changes.
COPY PlantSafety.sln ./
COPY PlantSafety.Api/PlantSafety.Api.csproj PlantSafety.Api/
COPY PlantSafety.Application/PlantSafety.Application.csproj PlantSafety.Application/
COPY PlantSafety.Domain/PlantSafety.Domain.csproj PlantSafety.Domain/
COPY PlantSafety.Infrastructure/PlantSafety.Infrastructure.csproj PlantSafety.Infrastructure/
COPY PlantSafety.SharedKernel/PlantSafety.SharedKernel.csproj PlantSafety.SharedKernel/

# NOTE: PlantSafety.Infrastructure also references two ../../Shared/* projects
# (Shared.AuditLogging.Infrastructure, Shared.EventBus.Infrastructure,
# Shared.DomainEvents.Abstractions) that live OUTSIDE this solution's own directory tree
# (D:\PlantSafety\source\Shared\...). A real CI/build context for this image therefore
# needs its Docker build context rooted one level above PlantSafety\ (or those Shared
# projects vendored/packaged as NuGet packages) so those relative ProjectReference paths
# resolve. This is flagged here rather than silently worked around with a fabricated
# path, since the correct resolution depends on a build-topology decision outside this
# Dockerfile's scope.
COPY Shared/ ../Shared/

RUN dotnet restore PlantSafety.Api/PlantSafety.Api.csproj

# Now copy the rest of the source and publish.
COPY . .
RUN dotnet publish PlantSafety.Api/PlantSafety.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Run as a non-root user inside the container (standard container-hardening practice).
RUN useradd --uid 5678 --create-home appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Phase 7 §4.1/§4.2 (Global Scalability & Multi-Region Deployment): identifies which region this
# specific container instance is running in, once real region-scoped deployments exist. Defaults
# to "UsEast" (Region.Default). This is a real, safe, additive build-arg -- it does NOT imply any
# actual multi-region deployment exists; it only lets a future real deployment pipeline stamp
# each region's container with its own identity (e.g. for structured logging / metrics
# dimension, and as the value an ops team would bind to configuration key
# "MultiRegion:CurrentRegion" so ITenantRegionRouter-adjacent code can know its own region).
ARG DEPLOY_REGION=UsEast
ENV MultiRegion__CurrentRegion=$DEPLOY_REGION

ENTRYPOINT ["dotnet", "PlantSafety.Api.dll"]
