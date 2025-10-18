# Remote Asset Loading Pattern for Unity Games

## Use Case

- **Scenario**: A live-service Unity game must deliver large textures, models, audio, and configuration files to players worldwide without forcing a full client patch each time content changes.
- **Goals**:
  - Keep the base application size small for faster installs and updates.
  - Ship fresh content (for example, seasonal cosmetics, events, or balance tweaks) rapidly.
  - Serve assets reliably across regions with low latency and high cache-hit rates.
  - Support experimentation (A/B testing), rollbacks, and staged rollouts.
  - Enforce content integrity and protect paid or licensed assets.

## High-Level Architecture

1. **Authoring & Build Stage**
   - Artists and designers produce Unity assets that are grouped into Addressables or AssetBundles.
   - A build pipeline (for example, GitHub Actions or Jenkins) exports bundles and their metadata.
   - Versioned bundles are uploaded to an **Asset Bucket** (Amazon S3) while manifest data is written to DynamoDB or stored as versioned JSON files.

2. **Content Delivery**
   - **AWS CloudFront** retrieves bundles from the S3 origin and provides global edge caching.
   - A **custom CDN layer** or reverse proxy fronts CloudFront to supply bespoke routing, regional overrides, authentication, and analytics.

3. **Runtime Consumption (Unity Client)**
   - The client retrieves a remote manifest from the CDN that lists bundle versions, hashes, and URLs.
   - Missing or outdated bundles are downloaded through `UnityWebRequest` or the Addressables Remote Catalog workflow and cached locally.
   - Integrity checks verify bundle hashes before loading.

4. **Operational Support**
   - Monitoring uses CloudWatch, S3/CloudFront logs, and CDN analytics.
   - CI/CD orchestration automates bundle builds, manifest publication, release toggles, and rollbacks.

## Implementation Pattern

### 1. Asset Build Pipeline
- Organize assets into **AssetBundle groups** with Addressables.
- Execute build scripts (for example, `AddressableAssetSettings.BuildPlayerContent()`) in CI/CD.
- After building, upload bundles and catalog files (such as `catalog.json` and hash files) to S3 with versioned prefixes (for example, `s3://game-assets/live/v2024.09.01/`).
- Produce a `manifest.json` that captures bundle names, sizes, hashes (CRC or SHA256), dependencies, signed CDN URLs or relative paths, target platforms, and minimum client versions.

### 2. AWS Infrastructure
- **S3 Bucket**: Enable versioning, separate staging and production prefixes, and restrict access to pipeline roles.
- **CloudFront Distribution**:
  - Origin: S3.
  - Cache policy: long TTL, forward query strings for signed URLs, enable GZip/Brotli.
  - Optionally require signed URLs or Origin Access Control for restricted assets.
- **Custom CDN Layer**:
  - Place a reverse proxy or partner CDN in front of CloudFront for fine-grained routing, WAF policies, and analytics.
  - Optionally incorporate API Gateway/Lambda to mint signed URLs or tokens.

### 3. Unity Client Integration
- **Bootstrap Loader**:
  - On startup, fetch the remote manifest (for example, `https://cdn.example.com/live/manifest.json`).
  - Compare manifest versions with the local cache to determine downloads or evictions.
- **Downloader**:
  - Use `UnityWebRequestAssetBundle.GetAssetBundle()` to stream bundles, then store them under `Application.persistentDataPath`.
  - Maintain a local index that maps bundle hashes to file paths for reuse across sessions.
- **Integrity & Security**:
  - Validate hashes before loading bundles.
  - Optionally encrypt bundles (for example, AES) and manage keys via AWS Secrets Manager or Cognito-issued tokens.
- **Load Flow**:
  1. Load the remote catalog or manifest.
  2. Download required bundles asynchronously.
  3. Update the local index and instantiate assets using Addressables (`Addressables.LoadAssetAsync<T>(key)`).

### 4. Versioning & Rollouts
- Retain multiple manifest versions (for example, `v2024.09.01`, `v2024.09.05`).
- Use feature flags or manifest metadata to toggle content for player segments.
- Roll back by pointing the CDN alias or manifest URL to the previous version.

### 5. Operations & Monitoring
- Enable CloudFront and S3 access logs; aggregate with AWS Athena for usage metrics.
- Track CDN hit/miss ratios, bandwidth per region, and latency.
- Configure CloudWatch alarms for error spikes or unusual latency and monitor AWS Cost Explorer for bandwidth costs.

## Implementation Steps Summary

1. **Set up infrastructure**
   - Create S3 buckets (for example, `game-assets-staging`, `game-assets-prod`).
   - Configure IAM roles for CI/CD and read-only distribution.
   - Create CloudFront distributions and wire DNS so the custom CDN fronts CloudFront.

2. **Automate asset builds**
   - Script Unity CI using `-batchmode -executeMethod BuildScript.BuildAddressables`.
   - Upload outputs (`bundles/`, `catalog.json`, `hash.json`, `manifest.json`) to versioned S3 paths.
   - Invalidate or version CloudFront caches when manifest pointers change.

3. **Publish manifest**
   - Update a stable pointer (for example, `live/manifest.json`) to reference the latest version manifest.
   - Optionally sign manifest URLs with short-lived tokens for secure clients.

4. **Client code**
   - Implement `AssetManifestManager` to fetch and parse the manifest.
   - Implement `AssetCacheManager` to download, store, hash-check, and evict bundles.
   - Integrate with Addressables or a custom loader to load assets on demand.

5. **Testing**
   - Host manifests and bundles locally during development to simulate CDN behavior.
   - Write automated tests that cover manifest parsing, bundle downloads, hash validation, and fallback logic.
   - Load-test the CDN to validate concurrency and throughput targets.

6. **Deployment & Lifecycle**
   - Stage new content under a staging path and run QA builds pointed to staging manifests.
   - Promote content by updating the CDN alias or copying manifests into the live path.
   - Monitor post-release metrics and roll back quickly if anomalies appear.

## Additional Considerations

- **Cache Busting**: Use content-based hashes in bundle filenames to prevent stale caches.
- **Offline Support**: Ship a minimal offline manifest with the game for initial boot scenarios.
- **Compliance**: Sanitize or anonymize access logs to meet GDPR/CCPA requirements.
- **Disaster Recovery**: Replicate S3 buckets across regions and configure multi-CDN routing (for example, Route53 latency-based routing).
- **Cost Optimization**: Compress bundles, enable CloudFront tiered caching, and tune TTLs to minimize origin fetches.
- **Security**: Require signed URLs or JWT tokens and rotate signing keys regularly.
- **Tooling**: Provide dashboards for content teams to inspect manifest entries, bundle sizes, and release status.

