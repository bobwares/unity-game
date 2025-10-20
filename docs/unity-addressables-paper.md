# Unity Addressables Domain Paper

## Ubiquitous Language
- **Unity Addressable Asset System (Unity Addressables)**: The official Unity package that manages dynamic loading, cataloging, and delivery of asset content through address keys.
- **Digital Content Creation Studio (Design Studio)**: The location where artists and designers produce assets inside Unity or external tools before entering version control.
- **Continuous Integration Pipeline (CI Pipeline)**: The automated process that converts committed project changes into validated asset builds.
- **Continuous Delivery Workflow (CD Workflow)**: The automated release process that promotes built assets to distribution environments.
- **Content Delivery Network (CDN)**: A geographically distributed cache that serves binary and text content to clients with low latency.
- **Amazon Web Services (AWS)**: The cloud platform that hosts the CDN origin, storage, and supporting services.
- **Amazon Simple Storage Service (Amazon S3)**: The object storage service used as the authoritative origin for remote Unity Addressables assets.
- **Amazon CloudFront**: The AWS service that provides global edge caching and request routing for assets stored in Amazon S3.
- **Amazon Route 53**: The DNS management service that maps human-readable hostnames to CDN endpoints.
- **Domain Name System (DNS)**: The naming system that resolves hostnames to IP addresses for CDN routing.
- **Internet Protocol (IP)**: The networking protocol that assigns numerical addresses used by clients when resolving CDN endpoints.
- **AWS Web Application Firewall (AWS WAF)**: The managed firewall service that enforces security rules on CDN requests.
- **AWS Identity and Access Management (AWS IAM)**: The AWS service that defines access policies for CI Pipeline, CD Workflow, and runtime actors.
- **Remote Catalog**: The serialized metadata produced by Unity Addressables that lists asset locations, dependencies, and hash signatures for runtime resolution.
- **Asset Bundle**: The binary package created by Unity Addressables that contains one or more assets compiled for a target platform.
- **Remote Load Path**: The runtime-resolved Uniform Resource Locator (URL) template that points to the CDN location of Remote Catalog files and Asset Bundles for a given environment.
- **Uniform Resource Locator (URL)**: The string identifier that specifies the location of a network resource.
- **JavaScript Object Notation (JSON)**: The text serialization format used by manifests, metadata, and configuration payloads.
- **Release Candidate Build (RC Build)**: A validated build artifact produced by the CI Pipeline for promotion into a higher environment.
- **Operational Telemetry**: The logs, metrics, and traces collected from pipelines, infrastructure, and clients to monitor health and performance.
- **Initialization Scene**: The Unity scene or bootstrap code responsible for configuring services, fetching catalogs, and warming caches before gameplay.
- **Gameplay Session**: The runtime context after initialization where gameplay systems load, unload, and manage addressable content based on player actions.
- **Build Artifact Manifest (Manifest)**: The JSON document emitted by the CI Pipeline that describes bundle names, hashes, dependencies, and semantic version metadata for a build.

## Domain Definition and Scope
Unity Addressables unify asset authoring, automated serialization, cloud delivery, and runtime consumption so that live-service games can evolve without shipping a full client patch for every change. The domain spans five collaborating subdomains: content creation in the Design Studio, deterministic builds in the CI Pipeline, controlled promotions in the CD Workflow, resilient distribution through AWS, and predictable runtime behavior inside the Unity client. Each subdomain maintains explicit contracts—naming conventions, metadata schemas, access policies, and telemetry expectations—to ensure that downstream systems receive assets that are consistent, verifiable, and reversible.【F:docs/remote-asset-loading-pattern.md†L1-L59】

The domain boundaries end once an Asset Bundle is loaded into runtime memory. Tooling for gameplay design, analytics, and live operations interacts with the Addressables domain through APIs and manifests but remains out of scope. Governance focuses on reproducibility, observability, and risk mitigation so that rapid content iteration does not compromise stability or security.【F:docs/remote-asset-loading-pattern.md†L61-L125】

## Narrative: From Design Studio to CDN
### Asset Ideation and Authoring
Asset creation begins in the Design Studio where artists author textures, meshes, animations, audio, and configuration assets. Designers assign Addressable keys, labels, and group schemas that encode compression format, build target, and environment affinity. Configuration assets that influence runtime logic—such as seasonal storefront definitions or encounter spawn tables—are organized into dedicated groups to support independent versioning. The Addressables settings asset is stored under version control to guarantee that environment-specific Remote Load Paths, profiles, and group rules are reproducible across workstations and build agents.【F:docs/remote-asset-loading-pattern.md†L15-L40】

The Design Studio enforces asset submission checklists. Unity import settings are validated locally, and contributors run targeted Addressables builds against the Development environment configuration prior to raising merge requests. This practice surfaces misconfigured dependencies, incorrect labels, or oversized bundles before automation begins. Metadata describing art source tools, texture compression budgets, and special licensing constraints accompanies each asset change so that downstream stages can enforce compliance.

### Source Control and Branch Management
Teams maintain long-lived mainline branches for each supported platform. Feature branches encapsulate Addressables changes, and pull requests require reviewers from both content and engineering disciplines. Merge policies include automated static analysis that verifies label usage, Remote Load Path bindings, and bundle size thresholds. The repository includes environment profile templates that map to Development, Staging, and Production endpoints, allowing developers to emulate any environment without editing committed files.

### Continuous Integration Pipeline Execution
Once changes merge into the integration branch, the CI Pipeline orchestrates headless Unity execution via batch mode. The pipeline performs five deterministic stages:

1. **Dependency Resolution**: The CI Pipeline agent installs the required Unity Editor version and restores project packages. Caching strategies reuse Library artifacts keyed by the project manifest hash to minimize cold-start time.
2. **Static Validation**: Custom scripts walk the AddressableAssetSettings object graph to confirm environment guardrails—for example, verifying that Production-only labels are absent from Development builds and that dependency graphs remain acyclic.
3. **Build Invocation**: The pipeline executes `AddressableAssetSettings.BuildPlayerContent()` through a curated build script. Build outputs include Asset Bundles, catalog files, hash files, and supporting binaries for the targeted platform.【F:docs/remote-asset-loading-pattern.md†L35-L65】
4. **Artifact Packaging**: The build script emits the Manifest containing bundle names, hash signatures, semantic versions, and Remote Load Path bindings. The Manifest also references environment toggles, feature flags, and rollback pointers for operational tooling.【F:docs/remote-asset-loading-pattern.md†L35-L69】
5. **Quality Gates**: Automated playmode smoke tests load critical Addressable keys to detect runtime regressions. Regression tests compare produced hash files against a baseline to confirm deterministic builds. Only a successful run is promoted to RC Build status.【F:docs/remote-asset-loading-pattern.md†L71-L100】

Artifacts are stored in a secure build cache managed by the CI Pipeline. Access requires AWS IAM roles scoped to build automation, preventing direct modification by individual contributors.

### Continuous Delivery Workflow to AWS
The CD Workflow ingests RC Builds and promotes them through Development, Staging, and Production environments with explicit approvals:

1. **Promotion Request**: Release managers inspect the Manifest, change summary, and telemetry from validation jobs. Approved requests trigger automated promotion to the Development environment distribution bucket.
2. **Environment Segregation**: Each environment maps to dedicated Amazon S3 buckets or prefixes such as `s3://game-addressables-dev`, `s3://game-addressables-stage`, and `s3://game-addressables-prod`. Bucket policies restrict write access to the CD Workflow while granting read access to the CDN origin identities.【F:docs/remote-asset-loading-pattern.md†L65-L90】
3. **Upload and Versioning**: Bundles, catalogs, hash files, and the Manifest are uploaded to environment-specific prefixes (for example, `/windows/v1.4.0/`). Semantic version tags and Git commit identifiers provide traceability. Each upload includes checksum validation and optional encryption metadata for sensitive assets.
4. **CloudFront Alignment**: Amazon CloudFront distributions are configured per environment with tailored cache time-to-live values. Promotion updates the origin pointer or issues targeted invalidations for catalog and manifest files. Stage and Production use blue-green aliasing so that switching between catalog versions is instantaneous and reversible.【F:docs/remote-asset-loading-pattern.md†L41-L88】
5. **Security Enforcement**: The CD Workflow generates signed URLs or applies CloudFront Origin Access Control. Optional Lambda@Edge policies rewrite manifests for regional overrides or enforce authentication tokens embedded in the Manifest.【F:docs/remote-asset-loading-pattern.md†L105-L145】

Operational telemetry from each promotion—upload durations, invalidation times, checksum results, and security rule evaluations—feeds dashboards that confirm the health of the supply chain and accelerate incident response.

## Environment Strategy: Development, Staging, Production
Environment isolation prevents experimental content from affecting live players while enabling rapid feedback loops:

- **Development Environment**: Optimized for iteration. Caching is minimized to expose updates immediately, and Remote Load Paths point to low-latency S3 prefixes. Unity clients authenticate using developer credentials and expose debugging tools that permit catalog refreshes or environment switching without rebuilding the client.【F:docs/remote-asset-loading-pattern.md†L76-L105】
- **Staging Environment**: Mirrors Production infrastructure. The Staging Remote Catalog includes release candidate bundles plus synthetic load scenarios. Automated smoke tests run on real hardware or cloud device farms to validate performance under realistic cache policies. Telemetry compares download latency, bundle sizes, and error rates against Production baselines before promotion.
- **Production Environment**: Serves live players and enforces strict governance. Promotions require multi-party approval and verification that Staging metrics meet thresholds. Production catalogs retain previous versions for rapid rollback. CloudFront logging integrates with security analytics to detect anomalous download patterns and to satisfy audit requirements.

Addressables profiles capture environment-specific Remote Load Paths. Build scripts accept an environment parameter that selects the appropriate profile, ensuring that the client and Manifest remain synchronized throughout the pipeline.

## AWS Distribution Architecture
AWS hosts the full delivery path from storage to edge caching. Amazon S3 provides durable storage with versioning enabled and replication across regions for disaster recovery. Amazon CloudFront fronts each S3 bucket and applies cache policies tuned per environment; Development favors short time-to-live values, while Production maximizes cache hit ratios to minimize origin load.【F:docs/remote-asset-loading-pattern.md†L41-L88】 Amazon Route 53 manages DNS records that map friendly hostnames such as `assets-dev.example.com` to CloudFront distributions. AWS WAF policies throttle abusive traffic, enforce geographic restrictions, and block malicious signatures. AWS IAM roles segregate permissions: the CI Pipeline and CD Workflow hold write access to specific prefixes, while runtime clients receive read-only signed URLs.【F:docs/remote-asset-loading-pattern.md†L107-L116】

CloudFront and S3 logs stream into Amazon Kinesis Data Firehose and ultimately land in Amazon S3 analytics buckets. Amazon Athena queries track cache hit ratios, download latency percentiles, asset popularity, and regional demand. Alarm thresholds trigger when catalog downloads fail, bundle sizes exceed policy limits, or bandwidth consumption spikes unexpectedly. Disaster recovery plans maintain secondary distributions in alternate regions and embed fallback endpoints in the Manifest so that clients can fail over gracefully if the primary CDN path becomes unavailable.【F:docs/remote-asset-loading-pattern.md†L71-L116】

## Build Pipeline Governance and Example Flow
A representative GitHub Actions workflow enforces deterministic builds:

1. **Trigger Stage**: Pushes to the `main` branch or manual dispatches for hotfixes start the workflow.
2. **Setup Stage**: The workflow installs Unity via the GameCI container, restores caches, and retrieves encrypted environment credentials from GitHub Secrets.
3. **Build Stage**: A custom C# build script wraps `AddressableAssetSettings.BuildPlayerContent()` and writes artifacts to a `BuildArtifacts` directory.
4. **Validation Stage**: Integration tests run through the Unity Test Runner in batch mode, verifying that key Addressable labels resolve, Remote Load Paths align with the targeted environment, and asset dependencies remain deduplicated.
5. **Publishing Stage**: The workflow uploads artifacts to the designated Amazon S3 prefix using the AWS Command Line Interface. Objects are tagged with Manifest metadata for environment, platform, and semantic version.
6. **Notification Stage**: Success or failure messages are dispatched to operational chat channels. The workflow records Manifest metadata in a change log for auditability.【F:docs/remote-asset-loading-pattern.md†L83-L105】

Promotion to Staging and Production reuses the same workflow with environment-specific credentials, distribution identifiers, and approval gates, ensuring that each release remains reproducible.

## Unity Client Initialization Flow
During startup, the Unity client executes a deterministic boot sequence governed by the Initialization Scene:

1. **Configuration Resolution**: The client loads local configuration files or secure remote settings that specify the active environment, fallback URLs, and authentication tokens. Development builds expose overrides for rapid testing, while Production builds restrict changes to signed remote commands.
2. **Service Bootstrapping**: Logging, analytics, save data systems, and dependency injection containers initialize prior to network calls so that telemetry captures the entire boot process.
3. **Remote Catalog Fetch**: The client calls `Addressables.InitializeAsync()` with the environment-specific Remote Load Path. If the remote hash differs from the cached hash, the client downloads the new catalog and its hash file.【F:docs/remote-asset-loading-pattern.md†L51-L65】
4. **Catalog Validation**: Checksums verify integrity. On failure, the client falls back to the previously cached catalog or the embedded local catalog for offline resilience.【F:docs/remote-asset-loading-pattern.md†L51-L65】
5. **Preload Strategy**: Critical assets—user interface prefabs, localization tables, or hero avatars—are preloaded using `Addressables.LoadAssetAsync`. The Manifest guides which bundles to fetch eagerly versus lazily.【F:docs/remote-asset-loading-pattern.md†L62-L95】
6. **Operational Telemetry Dispatch**: The client records catalog version, download duration, cache hits, and fallback events, then transmits the data to monitoring endpoints for near-real-time observability.【F:docs/remote-asset-loading-pattern.md†L71-L105】

This flow ensures that every session begins with a validated content graph and that service owners gain visibility into performance and reliability metrics.

## Addressable Usage During Gameplay Sessions
Gameplay systems interact with Unity Addressables through a content service layer that coordinates loading, caching, and unloading:

- **Streaming Worlds**: Open-world zones request biome-specific bundles as the player approaches a boundary. When the player crosses the boundary, the system calls `Addressables.LoadSceneAsync` with additive loading to stream in the new region. Upon exit, `Addressables.UnloadSceneAsync` frees memory.【F:docs/remote-asset-loading-pattern.md†L51-L65】
- **Inventory and Cosmetics**: Player inventory systems resolve item definitions by Addressable key. When a player previews a cosmetic item, the system checks whether the bundle is cached; if not, it triggers an asynchronous load while displaying a placeholder model. Manifest metadata indicates dependency bundles to queue in parallel.【F:docs/remote-asset-loading-pattern.md†L51-L65】
- **Live Events**: Temporary events are toggled via labels in the Remote Catalog. Gameplay code queries label-based groups to populate event-specific user interface elements. Removing a label from the Remote Catalog disables the event without patching the executable.【F:docs/remote-asset-loading-pattern.md†L66-L105】
- **Error Handling**: If a bundle download fails, retry policies escalate from immediate reattempts to presenting a user message with troubleshooting steps. The client logs error codes, CDN endpoint identifiers, and network status to Operational Telemetry for analysis.【F:docs/remote-asset-loading-pattern.md†L71-L116】

Memory budgets define the maximum number of concurrent bundles per platform. The Addressables `ResourceManager` reference counts loaded bundles, unloading them when no systems retain references. Streaming audio or video content can combine Addressables with UnityWebRequest streaming handlers to reduce peak memory consumption.

## Governance, Observability, and Continuous Improvement
The Addressables domain thrives on feedback loops. Operational Telemetry from the client, CDN, and AWS services feeds dashboards that highlight download latency, cache hit ratios, and error trends. Postmortem reviews following incidents—such as catalog corruption or CDN outages—result in updates to validation scripts, hash verification rules, or failover routing tables. Security reviews confirm that AWS IAM roles follow the principle of least privilege, that signed URLs rotate regularly, and that encryption policies protect premium assets. Periodic audits ensure that the Ubiquitous Language remains consistent across documentation, code, and operational tooling.【F:docs/remote-asset-loading-pattern.md†L71-L116】

By aligning creative workflows, automated pipelines, AWS infrastructure, and runtime client behavior, Unity Addressables deliver a resilient foundation for live-service games. Each environment promotion remains observable and reversible, every Remote Catalog is verifiable, and gameplay experiences benefit from predictable, low-latency access to evolving content.
