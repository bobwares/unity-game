# Unity Addressables Domain Paper

## Ubiquitous Language
- **Unity Addressable Asset System (Unity Addressables)**: The official Unity package that manages dynamic loading, cataloging, and delivery of asset content through address keys.
- **Content Delivery Network (CDN)**: A geographically distributed cache that serves binary and text content to clients with low latency.
- **Amazon Web Services (AWS)**: The cloud platform that hosts the CDN origin, storage, and supporting services.
- **Amazon Simple Storage Service (Amazon S3)**: The object storage service used as the authoritative origin for remote Addressable assets.
- **Domain Name System (DNS)**: The naming system that maps human-readable hostnames to IP addresses for CDN routing.
- **Internet Protocol (IP)**: The networking protocol that assigns numerical addresses used by clients when resolving CDN endpoints.
- **AWS Web Application Firewall (AWS WAF)**: The managed firewall service that enforces security rules on CDN requests.
- **Digital Content Creation Studio (Design Studio)**: The location where artists and designers produce assets inside Unity or external tools before entering version control.
- **Continuous Integration Pipeline (CI Pipeline)**: The automated process that converts committed project changes into validated asset builds.
- **Continuous Delivery Workflow (CD Workflow)**: The automated release process that promotes built assets to distribution environments.
- **Environment**: A logical deployment stage (Development, Staging, Production) that isolates content catalogs, runtime endpoints, and operational policies.
- **Remote Catalog**: The serialized metadata produced by Unity Addressables that lists asset locations, dependencies, and hash signatures for runtime resolution.
- **Asset Bundle**: The binary package created by Unity Addressables that contains one or more assets compiled for a target platform.
- **Initialization Scene**: The Unity scene or bootstrap code responsible for configuring services, fetching catalogs, and warming caches before gameplay.
- **Gameplay Session**: The runtime context after initialization where gameplay systems load, unload, and manage addressable content based on player actions.
- **Release Candidate Build (RC Build)**: A validated build artifact produced by the CI Pipeline for promotion into a higher environment.
- **Operational Telemetry**: The logs, metrics, and traces collected from pipelines, infrastructure, and clients to monitor health and performance.

## Domain Overview
Unity Addressables solve the challenge of delivering large quantities of rich media assets in a live game without binding every change to a full client update. The domain intersects creative production, automated engineering pipelines, cloud distribution, and runtime client logic. A successful Addressables strategy aligns these subdomains so that artists iterate rapidly, engineers maintain reliable automation, operations teams control risk through staged environments, and players receive new content with minimal friction.

The primary actors are the Design Studio, the CI Pipeline, the CD Workflow, the AWS infrastructure team, and the Unity client. The domain boundaries include asset authoring, serialization, storage, distribution, security, versioning, and runtime consumption. Cross-cutting concerns include governance over naming conventions, metadata tagging, and compliance requirements such as intellectual property protection or regional restrictions.

## Asset Lifecycle from Design Studio to CDN
### Asset Authoring and Source Control
Asset creation begins in the Design Studio where artists author textures, meshes, animations, audio, and configuration assets. Each asset is imported into Unity with consistent Addressable group settings: naming conventions, labels for feature targeting, and schemas for dependency metadata. Unity Addressables configuration is stored in version control so that group definitions remain reproducible across machines and automated build agents.

To ensure deterministic builds, the Design Studio integrates with version control branching strategies. Feature branches collect related Addressable changes, and pre-merge validation requires local Addressables builds against the Development environment configuration. Unity Addressables asset groups are annotated with rules such as compression format, remote load paths, and include or exclude filters that map to different platform targets.

### Continuous Integration Pipeline Execution
When changes merge into the main integration branch, the CI Pipeline orchestrates headless Unity execution with batch mode commands such as `-executeMethod`. The pipeline performs the following stages:

1. **Dependency Resolution**: The CI Pipeline agent installs the required Unity Editor version and pulls project packages. Cache layers reduce build time by reusing Library folders tied to the same manifest hash.
2. **Static Validation**: Automated validation scripts confirm that Addressable group settings match guardrails—for example, verifying that Production-only labels are absent from Development builds, or that asset sizes remain within defined thresholds.
3. **Build Invocation**: The pipeline triggers `AddressableAssetSettings.BuildPlayerContent()` to produce Asset Bundles and the Remote Catalog for the target platform. Build artifacts are versioned using semantic identifiers that encode build number, branch, and timestamp.
4. **Artifact Packaging**: The pipeline emits a manifest describing bundle names, hashes, dependencies, and size metadata. This manifest enriches the Remote Catalog with pipeline-specific metadata such as environment, platform, and content themes.
5. **Quality Gates**: Automated tests run against the built assets, including playmode smoke tests that load critical Addressable keys and regression tests that ensure deterministic Asset Bundle hashes. Only after passing quality gates does the build become an RC Build.

### Continuous Delivery Workflow to AWS CDN
The CD Workflow promotes RC Builds into environments through controlled approvals:

1. **Promotion Request**: Release managers review the manifest, change summary, and telemetry from validation jobs. Approval triggers promotion into the Development environment distribution bucket.
2. **Environment Segregation**: Each environment maps to dedicated Amazon Simple Storage Service (Amazon S3) buckets or prefixes—for example, `s3://game-addressables-dev`, `s3://game-addressables-stage`, and `s3://game-addressables-prod`. Bucket policies restrict write access to the CD Workflow while granting read access to the CDN origins.
3. **Upload and Versioning**: The CD Workflow uploads bundles and catalogs to environment-specific prefixes such as `/windows/v1.4.0/`. Version markers include semantic versions and Git commit identifiers. Hash files accompany each catalog for integrity verification.
4. **CloudFront Invalidation and Aliasing**: Amazon CloudFront distributions are configured per environment with behaviors tuned for cache time-to-live. Promotion updates the origin version pointer or creates a new cache invalidation targeting the manifest files. Stage and Production distributions use blue-green aliasing so that switching between catalog versions is instantaneous and reversible.
5. **Security and Compliance**: The workflow generates signed URLs or origin access control policies to prevent unauthorized downloads. Optional Lambda@Edge functions enforce geographic restrictions or dynamic manifest rewrites for region-specific content.

Operational telemetry is collected during each promotion: upload success metrics, invalidation completion times, and checksum verification logs. These data points feed dashboards that surface the health of the Addressables supply chain.

## Environment Strategy: Development, Staging, Production
The domain enforces strict separation between environments to minimize risk and maintain predictable rollouts.

- **Development Environment**: Supports rapid iteration. The CDN configuration points to the development S3 bucket with minimal caching to reflect updates immediately. Telemetry emphasizes error discovery, and Unity clients built for internal teams authenticate using developer credentials. Feature flags in the Remote Catalog can expose experimental content only to developer accounts.

- **Staging Environment**: Mirrors Production infrastructure but remains isolated. The Staging Remote Catalog includes release candidate bundles plus synthetic load scenarios. Automated smoke tests run on actual hardware or cloud device farms. CloudFront caching retains realistic time-to-live values to validate cache warm-up behavior. Staging telemetry is compared against baseline metrics to catch regressions in bundle sizes, download latency, or manifest parsing.

- **Production Environment**: Serves live players. Promotion into Production requires an explicit approval step and verification that Staging telemetry meets thresholds. Production catalogs maintain backward compatibility by retaining previous versions for rollback. CloudFront logging integrates with security analytics to detect unusual download patterns.

To support environment parity, the Unity project stores environment-specific Remote Load Path variables in Addressables profiles. Build scripts ingest environment parameters and update catalog URLs accordingly. The client runtime resolves the correct profile at startup based on configuration files, command line arguments, or secure remote settings.

## Build Pipeline Governance and Example Flow
A representative pipeline implementation uses GitHub Actions for automation:

1. **Trigger**: A push to the `main` branch or a manual dispatch for hotfixes triggers the workflow.
2. **Setup Stage**: The action installs Unity via the GameCI container, restores caches, and retrieves encrypted environment credentials (for example, AWS Identity and Access Management keys) from GitHub Secrets.
3. **Build Stage**: The workflow runs a custom C# build script that wraps `AddressableAssetSettings.BuildPlayerContent()` and writes build metadata to `BuildArtifacts/manifest.json`.
4. **Validation Stage**: Integration tests execute using Unity Test Runner in batch mode, verifying that key Addressable labels load successfully, that remote catalog URLs match the targeted environment, and that asset dependencies resolve without duplication.
5. **Publishing Stage**: The workflow uploads the contents of `BuildArtifacts` to the designated S3 prefix using the AWS Command Line Interface. Objects are tagged with metadata for environment, platform, and semantic version.
6. **Notification Stage**: Success or failure messages are sent to operational chat channels. The workflow records artifact metadata in a change log stored under `docs/releases/addressables-history.md` for auditability.

Example configuration snippet for GitHub Actions:

```yaml
jobs:
  build-addressables:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: game-ci/unity-builder@v4
        with:
          targetPlatform: StandaloneWindows64
          buildMethod: BuildScripts.AddressablesBuild.Execute
      - name: Upload bundles to S3
        run: |
          aws s3 sync BuildArtifacts s3://game-addressables-dev/windows/$(date +%Y%m%d%H%M%S)/ \
            --metadata version=${{ github.run_number }}
      - name: Create CloudFront invalidation
        run: |
          aws cloudfront create-invalidation \
            --distribution-id ${{ secrets.DEV_DISTRIBUTION_ID }} \
            --paths "/windows/latest/catalog.json" "/windows/latest/catalog.hash"
```

Promotion to Staging and Production reuses the same workflow with different credentials and distribution identifiers, ensuring that the process remains deterministic across environments.

## AWS CDN Architecture
The AWS CDN design uses Amazon S3 as the origin, Amazon CloudFront for global caching, and optional Amazon Route 53 for DNS management. Each environment receives its own CloudFront distribution with Origin Access Control to restrict direct S3 access. Signed URLs secure high-value content, while AWS WAF rules mitigate abuse. Logs from CloudFront and S3 feed into Amazon Kinesis Data Firehose, which loads the data into Amazon S3 for analysis via Amazon Athena. Operational teams monitor cache hit ratios, download latency percentiles, and error codes. Alert thresholds trigger on catalog download failures, unusually large bundle sizes, or spikes in bandwidth consumption.

Disaster recovery leverages cross-region replication of S3 buckets and secondary CloudFront distributions. The Remote Catalog stores alternative endpoints so that clients can fail over gracefully if the primary distribution becomes unavailable.

## Unity Client Initialization Flow
During startup, the Unity client follows a deterministic sequence governed by the Initialization Scene:

1. **Configuration Resolution**: The client loads local configuration files that specify the active environment and fallback URLs. Secure storage holds authentication tokens when required.
2. **Service Bootstrapping**: Systems such as logging, analytics, and save data initialize before network calls to ensure telemetry coverage. Dependency injection containers register Addressables services.
3. **Remote Catalog Fetch**: The client invokes `Addressables.InitializeAsync()` with a profile-specific Remote Load Path. If the Remote Catalog is newer than the cached copy, the client downloads the catalog and associated hash file.
4. **Catalog Validation**: Checksums ensure catalog integrity. If validation fails, the client falls back to the previous cached catalog or the embedded local catalog for offline support.
5. **Preload Strategy**: Critical assets such as user interface prefabs, localization tables, or hero avatars are preloaded using `Addressables.LoadAssetAsync`. This warm-up reduces hitching when the player enters menus or early gameplay sequences.
6. **Operational Telemetry Dispatch**: The client sends telemetry about catalog version, download duration, and any fallback events to the monitoring backend for operational visibility.

In Development builds, the initialization flow exposes developer tools that allow forcing catalog refreshes or switching environments without rebuilding the client. In Production builds, configuration changes require signed remote commands to avoid tampering.

## Addressable Usage During Gameplay Sessions
Gameplay systems interact with Unity Addressables through a layer of content services:

- **Streaming Encounters**: Open-world zones request biome-specific bundles as the player approaches a boundary. When the player crosses the boundary, the system calls `Addressables.LoadSceneAsync` with additive loading to stream in the new region. On exit, `Addressables.UnloadSceneAsync` frees memory.

- **Inventory and Cosmetics**: Player inventory systems resolve item definitions by key. When a player previews a cosmetic item, the system checks whether the bundle is cached. If not, it triggers an asynchronous load and displays a placeholder model until the final asset arrives. Metadata from the Remote Catalog indicates dependency bundles to queue in parallel.

- **Live Events**: Temporary events are toggled via labels in the Remote Catalog. Gameplay code queries label-based groups to populate event-specific user interface elements. The runtime can disable event bundles by removing the label from the catalog without patching the executable.

- **Error Handling**: If a bundle download fails, retry policies escalate from immediate reattempts to presenting a user message with troubleshooting steps. The client logs error codes, CDN endpoint identifiers, and network status to Operational Telemetry for analysis.

Memory management policies define maximum concurrent bundles based on platform constraints. Addressables `ResourceManager` handles reference counting; when reference counts drop to zero, bundles unload automatically. Streaming audio or video assets may use Unity Addressables in conjunction with UnityWebRequest streaming handlers to minimize memory usage.

## Governance, Observability, and Continuous Improvement
The Addressables domain relies on continuous feedback loops. Operational Telemetry informs refinements to asset compression, CDN caching policies, and client preloading heuristics. Postmortem reviews capture lessons from incidents such as catalog corruption or CDN outages, leading to updates in validation scripts and failover strategies. Regular audits verify that the Ubiquitous Language remains consistent across documentation, configuration files, and communication between teams.

Security reviews ensure that AWS credentials used by the CI Pipeline and CD Workflow follow the principle of least privilege and rotate regularly. Penetration testing validates that signed URLs, encryption policies, and client-side hardening prevent unauthorized asset extraction.

By aligning creative workflows, automated pipelines, AWS infrastructure, and runtime client behavior, Unity Addressables deliver a resilient foundation for live-service games. The domain thrives when each environment is respected, each promotion is observable, and every gameplay session benefits from predictable, low-latency access to the latest content.
