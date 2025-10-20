# Unity Addressables Build Pipeline - Frequently Asked Questions (FAQ)

## Table of Contents
- [General Questions](#general-questions)
- [Repository Structure](#repository-structure)
- [Build Pipeline](#build-pipeline)
- [Multi-Platform Builds](#multi-platform-builds)
- [Environments & Deployment](#environments--deployment)
- [AWS Infrastructure](#aws-infrastructure)
- [Validation & Testing](#validation--testing)
- [Troubleshooting](#troubleshooting)
- [Performance & Optimization](#performance--optimization)
- [Security](#security)
- [Rollback & Recovery](#rollback--recovery)

---

## General Questions

### What are Unity Addressables?
Unity Addressables is Unity's official asset management system that allows you to load and manage game assets dynamically using string keys instead of direct references. This enables you to update game content without forcing players to download full client patches.

### Why do I need a build pipeline for Addressables?
A build pipeline automates the entire workflow of building, validating, and deploying Addressable assets across multiple platforms and environments. Without automation, manually building and uploading assets for iOS, Android, WebGL, and Windows for dev/staging/production environments is error-prone and time-consuming.

### What problems does this pattern solve?
- **Reduces client patch size**: Players only download changed bundles, not entire builds
- **Enables rapid content iteration**: Artists ship new assets in hours instead of days
- **Supports live-service games**: Deploy events, cosmetics, and updates without app store submissions
- **Provides production safety**: Staged rollouts with automated testing catch issues before reaching players

### How is this different from traditional Unity builds?
Traditional Unity builds package all assets into the executable. Addressables separate content from code, allowing you to:
- Update assets independently of the client
- Stream assets on-demand to reduce initial download size
- A/B test content by swapping catalogs
- Deploy platform-specific assets efficiently

---

## Repository Structure

### Should Addressable assets be in a separate repository?
**No, for most teams (1-50 people).** Keep everything in a monorepo:

**Pros:**
- Simpler workflow - artists and engineers work in one place
- Atomic commits - code and asset changes in same PR
- Unity's native workflow - Unity expects assets and code together
- Easier testing - PlayMode tests can reference both

**Only split if:**
- You have 50+ people with truly independent content/code teams
- Multiple games share asset libraries
- External studios create content without code access
- You have massive asset scale (100+ GB of source files)

### How do I organize assets within the Unity project?
```
Assets/
├── _Game/                    # Core game code (non-addressable)
├── AddressableAssets/        # Assets marked as Addressable
│   ├── Characters/
│   ├── Environments/
│   ├── UI/
│   └── LiveEvents/
└── Resources/                # Non-addressable, always bundled
```

Organize by **feature, not type**. Keep related assets together (models, textures, audio for a character).

### What should be in .gitignore?
```
# Unity
/Library/
/Temp/
/Obj/
/Build/

# Addressables build output
/ServerData/

# IDE
/.vs/
/.vscode/

# OS
.DS_Store
Thumbs.db
```

Use **Git LFS** for binary files:
```
*.fbx filter=lfs
*.psd filter=lfs
*.png filter=lfs
*.mp3 filter=lfs
```

---

## Build Pipeline

### How does the headless Unity build work?
The CI pipeline runs Unity in batch mode without the GUI:
```bash
Unity -quit -batchmode -projectPath . \
  -executeMethod BuildPipeline.BuildScripts.BuildAddressablesForPlatform \
  -buildTarget Android \
  -addressablesProfile Production
```

This executes your custom C# build script which calls Unity's Addressables build API.

### What is GameCI and why use it?
GameCI provides Docker containers with Unity pre-installed for CI/CD. Benefits:
- No need to install Unity on CI runners
- Consistent build environment
- Supports all Unity versions
- Free for open-source projects

### How long does a build take?
Typical times:
- **Android**: 5-15 minutes
- **iOS**: 10-20 minutes (requires macOS runner)
- **WebGL**: 10-25 minutes
- **Windows**: 5-10 minutes

First builds are slower due to cache misses. Subsequent builds with cache: 2-5 minutes.

### Can I build locally instead of CI?
Yes! Use the same command:
```bash
/Applications/Unity/Hub/Editor/2022.3.20f1/Unity.app/Contents/MacOS/Unity \
  -quit -batchmode -projectPath . \
  -executeMethod BuildPipeline.BuildScripts.BuildAddressablesForPlatform \
  -buildTarget Android \
  -addressablesProfile Development
```

Output goes to `ServerData/{platform}/`.

---

## Multi-Platform Builds

### Do I need separate builds for each platform?
**Yes.** Unity compiles assets differently per platform:
- **iOS/Android**: ASTC texture compression, Metal/Vulkan shaders
- **WebGL**: Brotli compression, single-threaded limitations
- **Windows**: DXT/BC compression, DirectX shaders

### How are platform-specific bundles organized in S3?
```
s3://bucket/
├── android/v123/
│   ├── catalog_android.json
│   └── bundle_ui_abc.bundle
├── ios/v123/
├── webgl/v123/
└── windows/v123/
```

Each platform has its own directory. Clients determine their platform at runtime and fetch the appropriate catalog.

### How does the client know which platform to use?
```csharp
string platform = Application.platform switch {
    RuntimePlatform.IPhonePlayer => "ios",
    RuntimePlatform.Android => "android",
    RuntimePlatform.WebGLPlayer => "webgl",
    _ => "windows"
};
string catalogUrl = $"https://cdn.yourgame.com/{platform}/latest/catalog.json";
```

### Can I build multiple platforms in parallel?
Yes! The GitHub Actions workflow uses a matrix strategy:
```yaml
strategy:
  matrix:
    platform: [Android, iOS, WebGL, Windows]
```

Each platform builds in parallel on separate runners.

### Why does iOS require a macOS runner?
Unity's iOS build process requires Xcode, which only runs on macOS. Android, WebGL, and Windows can build on Linux/Ubuntu runners.

---

## Environments & Deployment

### What are Dev, Staging, and Production environments?
- **Development**: Rapid iteration, minimal caching, internal team only
- **Staging**: Production mirror for validation, automated tests, QA access
- **Production**: Live players, requires approval, supports rollback

### How are environments isolated?
Each environment has:
- Separate S3 bucket (`mygame-addressables-dev/stage/prod`)
- Separate CloudFront distribution
- Separate CDN URL (`dev-cdn.`, `stage-cdn.`, `cdn.`)
- Different cache TTL settings

### How do I deploy to staging vs production?
Use GitHub Actions workflow dispatch:
```
Actions → Build and Deploy Addressables → Run workflow
  Environment: staging (or production)
  Platforms: android,ios,webgl,windows
```

Production deployments require manual approval (configured in GitHub environment settings).

### What is the `/latest/` alias?
Each platform has two paths:
- `/v123/` - Versioned, immutable content
- `/latest/` - Alias pointing to current version

Clients fetch from `/latest/`. Rollback updates `/latest/` to point to a previous version without changing client code.

### How often should I deploy?
**Depends on your content velocity:**
- **Dev**: Continuous (every merge to develop branch)
- **Staging**: Daily or per feature
- **Production**: Weekly for live-service games, or per major content drop

---

## AWS Infrastructure

### Why use Terraform instead of AWS Console?
**Infrastructure as Code** provides:
- Version control for infrastructure changes
- Reproducible deployments across environments
- Easy disaster recovery (rebuild from code)
- Collaboration through code reviews

### What AWS resources does this pattern use?
- **S3 Buckets**: Store asset bundles and catalogs
- **CloudFront**: CDN for global low-latency delivery
- **IAM Roles**: Secure access for CI/CD pipelines
- **CloudWatch**: Monitoring, logging, and alerts

### How much does AWS cost?
**Rough estimates (per month for small-medium game):**
- S3 Storage (100GB): ~$2.30
- CloudFront (1TB transfer): ~$85
- CloudWatch Logs/Metrics: ~$5
- **Total**: ~$92/month

Costs scale with player count and content size. Use CloudWatch to monitor actual usage.

### Do I need a custom domain for CDN?
No, CloudFront provides a default domain:
```
https://d111111abcdef8.cloudfront.net
```

But custom domains are recommended for production:
```
https://cdn.yourgame.com
```

Requires Route 53 DNS configuration and SSL certificate.

### How do I set up AWS credentials for GitHub Actions?
1. Create IAM user with minimal permissions (see Terraform IAM policy)
2. Generate access keys
3. Add to GitHub repository secrets:
    - `AWS_ACCESS_KEY_ID`
    - `AWS_SECRET_ACCESS_KEY`
    - `DEV_CLOUDFRONT_ID`, `STAGE_CLOUDFRONT_ID`, `PROD_CLOUDFRONT_ID`

**Never commit credentials to Git.**

---

## Validation & Testing

### What validations run during the build?
**Pre-build:**
- Addressables settings exist
- Groups are not empty
- Profiles are configured
- No duplicate assets across groups
- Asset labels are valid

**Post-build:**
- Bundle files exist
- Catalog and hash files present
- Bundle sizes under threshold (100MB default)
- Catalog integrity (hash verification)

### How do I run tests locally?
**Edit Mode tests:**
```bash
Unity -runTests -batchmode -testPlatform EditMode
```

**Play Mode tests:**
```bash
Unity -runTests -batchmode -testPlatform PlayMode
```

Or use Unity Test Runner window in the editor.

### What should I test in Play Mode?
- Addressables initialization succeeds
- Critical assets load successfully
- Catalog updates work correctly
- Fallback to cached catalog when offline
- Memory is released after unloading

### Why did my build fail validation?
Common reasons:
- **Bundle exceeds 100MB**: Split large groups or compress textures
- **Catalog hash mismatch**: Rebuild from clean state
- **Missing catalog files**: Check Addressables build settings
- **Empty groups**: Remove empty groups or add assets
- **Duplicate assets**: Use Addressables Analyze tool to find duplicates

---

## Troubleshooting

### Build succeeds but bundles are missing in S3
**Check:**
1. AWS credentials are valid
2. S3 bucket name matches config
3. GitHub Actions logs for upload errors
4. IAM permissions allow `s3:PutObject`

### Clients download old content after deployment
**Likely causes:**
1. CloudFront cache not invalidated - Check invalidation completed
2. Client cached old catalog - Clear app data or implement versioning
3. `/latest/` not updated - Verify S3 sync completed

### "Asset not found" errors in game
**Debugging steps:**
1. Check asset is marked as Addressable
2. Verify asset key/address spelling
3. Check asset is in correct group
4. Confirm bundle uploaded to S3
5. Test catalog URL in browser

### Build takes too long (>30 minutes)
**Optimizations:**
1. Enable GitHub Actions cache for `Library/` folder
2. Use incremental builds (don't clean unless necessary)
3. Build platforms in parallel (matrix strategy)
4. Reduce asset count or split into smaller groups

### Out of memory during build
**Solutions:**
1. Increase Unity batch mode memory: `-executeMethod BuildScripts.Build -batchmode -quit -maxHeapSize 16384`
2. Split large Addressable groups
3. Use streaming for video/audio assets
4. Compress textures before import

### iOS build fails on GitHub Actions
**Common issues:**
1. Using Ubuntu runner instead of macOS
2. Missing Xcode license acceptance
3. Provisioning profile not configured
4. Change matrix to `os: macos-latest` for iOS

---

## Performance & Optimization

### How do I reduce bundle sizes?
1. **Compress textures**: Use ASTC/ETC2 for mobile, BC for desktop
2. **Use texture atlases**: Combine small textures
3. **Optimize meshes**: Reduce poly count, remove unused vertices
4. **Compress audio**: Use Vorbis compression
5. **Strip shader variants**: Remove unused variants

### What compression should I use for bundles?
**Depends on content type:**
- **UI/Prefabs**: LZMA (max compression, loaded once)
- **Streaming audio/video**: Uncompressed or LZ4
- **General assets**: LZ4 (balanced)

```csharp
schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
```

### How do I optimize catalog size?
Catalogs can grow large with many assets:
1. Split into multiple catalogs by content type
2. Use remote catalog loading (not embedded)
3. Remove unused entries
4. Compress catalog files

### What's the best CloudFront cache strategy?
**Bundle files** (immutable):
```
Cache-Control: public, max-age=31536000, immutable
```

**Catalog files** (frequently updated):
```
Cache-Control: public, max-age=600  # 10 minutes
```

### How do I reduce CDN bandwidth costs?
1. Enable compression (Gzip/Brotli)
2. Increase cache TTL
3. Use proper asset bundles to avoid duplicates
4. Monitor CloudWatch for cache hit ratio (target >80%)
5. Use regional edge caches

---

## Security

### How do I secure S3 buckets?
The Terraform configuration includes:
```hcl
block_public_acls       = true
block_public_policy     = true
ignore_public_acls      = true
restrict_public_buckets = true
```

Only CloudFront can access S3 via Origin Access Control.

### Should I encrypt asset bundles?
**S3 at-rest encryption**: Yes, enable AES256 (included in Terraform)

**In-transit encryption**: Yes, CloudFront enforces HTTPS

**Bundle content encryption**: Usually unnecessary, but options:
1. Unity's Addressables doesn't provide native encryption
2. Use AssetBundle.LoadFromMemoryAsync with decryption
3. Implement custom encryption layer

### How do I prevent unauthorized downloads?
1. **Use signed URLs** for premium content
2. **Implement CloudFront signed cookies** for authenticated users
3. **Use AWS WAF** to block suspicious traffic patterns
4. **Geo-restriction** if needed for compliance

### How often should I rotate AWS credentials?
**Best practices:**
- Rotate every 90 days
- Use IAM roles instead of access keys when possible
- Monitor CloudTrail for unauthorized access
- Enable MFA for production deployments

---

## Rollback & Recovery

### How do I rollback to a previous version?
Use the rollback workflow:
```
Actions → Rollback Addressables → Run workflow
  Environment: production
  Version: previous (or specific build number)
  Platforms: all
```

This copies the previous version to `/latest/` and invalidates CloudFront cache.

### How fast is a rollback?
- **S3 copy**: 1-3 minutes
- **CloudFront invalidation**: 5-15 minutes to propagate globally
- **Total**: ~10-20 minutes for players to see old content

### What if I need to rollback a specific platform?
The rollback workflow supports platform selection:
```
Platforms: android,ios  # Only rollback mobile
```

### Are old versions kept in S3?
Yes, versioned builds are retained:
```
/android/v120/
/android/v121/
/android/v122/
/android/latest/  → points to v122
```

After rollback:
```
/android/latest/  → points to v121
```

### How do I test rollback before doing it in production?
1. Test in **Staging** environment first
2. Run rollback workflow with `environment: staging`
3. Verify client fetches old content
4. If successful, repeat in production

### What if CloudFront invalidation fails?
**Fallback options:**
1. Wait for TTL to expire naturally (max 24 hours)
2. Create new invalidation manually in AWS Console
3. Use low TTL (10 minutes) for catalogs to minimize window

### Can I rollback code changes?
No, this pattern only rolls back **Addressable assets**. Code changes require:
- Traditional Git revert
- Rebuild and redeploy client application
- App store submission (mobile)

---

## Advanced Topics

### Can I use this with Unity Cloud Build?
Yes, but you'll need to:
1. Export build output to S3 from Unity Cloud Build
2. Trigger GitHub Actions for deployment
3. Or implement cloud build post-process scripts

### How do I implement A/B testing?
Use catalog labels and feature flags:
```csharp
// Server determines which variant
bool showVariantB = FeatureFlags.IsEnabled("new_ui_variant");
string label = showVariantB ? "ui_variant_b" : "ui_variant_a";
Addressables.LoadAssetAsync<GameObject>(label);
```

Deploy both variants to same catalog with different labels.

### Can I use this with multiple games?
Yes! Create separate:
- Terraform workspaces per game
- S3 bucket prefixes: `s3://company-addressables/game1/`, `game2/`
- CloudFront distributions with different origins

### How do I handle region-specific content?
**Option 1: Multiple catalogs**
```
/android/us/catalog.json
/android/eu/catalog.json
/android/asia/catalog.json
```

**Option 2: Lambda@Edge**
Rewrite catalog requests based on geo-location.

**Option 3: Labels**
Use labels to filter region-specific assets at runtime.

### What about mobile data usage concerns?
1. Provide Wi-Fi-only download option
2. Show download size before fetching
3. Implement progressive loading
4. Use smaller bundles (<10MB)
5. Cache aggressively

---

## Best Practices

### Development Workflow
1. Artists mark assets as Addressable in Unity
2. Commit changes to feature branch
3. CI builds and validates automatically
4. Merge to `develop` → auto-deploys to Dev environment
5. Promote to Staging for QA testing
6. Manual approval to deploy to Production

### Naming Conventions
**Groups**: Use descriptive names
```
Characters_Heroes
Characters_Enemies
Environments_Forest
UI_MainMenu
```

**Labels**: Use consistent taxonomy
```
characters, environments, ui, live_event, premium_content
```

**Addresses**: Use hierarchical paths
```
characters/hero/warrior
environments/forest/tree_oak
ui/menu/main_menu_panel
```

### Monitoring Checklist
- [ ] CloudWatch dashboard for CDN metrics
- [ ] Alarms for 4xx/5xx error rates
- [ ] S3 storage growth monitoring
- [ ] Build success/failure notifications
- [ ] Client telemetry for download failures

### Pre-Production Checklist
- [ ] Test rollback procedure in staging
- [ ] Verify all platforms build successfully
- [ ] Run performance tests with actual hardware
- [ ] Check bundle sizes are reasonable
- [ ] Validate catalog integrity
- [ ] Test with poor network conditions
- [ ] Document deployment runbook

---

## Getting Help

### Where can I learn more about Unity Addressables?
- [Unity Addressables Documentation](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [Unity Addressables Best Practices](https://learn.unity.com/tutorial/addressables-best-practices)
- Unity forums and Discord communities

### My question isn't answered here
Common resources:
1. Check GitHub Actions logs for build errors
2. Review AWS CloudWatch logs
3. Test locally to isolate issue
4. Check Unity Console for runtime errors
5. Verify configuration files match expected format

### How do I contribute improvements?
1. Document the improvement needed
2. Test changes in Development environment
3. Create PR with clear description
4. Update this FAQ if adding new features

---

**Last Updated**: 2025-10-19  
**Pattern Version**: 1.0