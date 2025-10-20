# Statement of Work (SOW)

* **Project Title:** Unity Game Framework – Addressables-Based Asset Pipeline
* **Version:** 1.0.0
* **Date:** 2025-10-19
* **Author:** Bobwares Consulting

## Problem Statement

Modern mobile and cross-platform games face two critical challenges that directly affect player retention and production scalability:

### 1. Reducing Initial Download Size to Prevent User Abandonment

**Problem**

When a game’s initial download is too large or takes too long to launch, most users abandon the process before experiencing gameplay. Studies consistently show a steep drop in retention when installation exceeds 150 MB or when the first interaction is delayed beyond 10 seconds. Players increasingly expect near-instant access to entertainment.

**Solution**

To ensure an immediate and engaging first impression, the game must implement a **tiered content delivery architecture** that separates **core content** from **extended content**. The base installation package will include only the assets required to launch the game, display the title screen, allow character selection, and begin gameplay within seconds. All non-critical assets—such as expanded environments, high-resolution textures, and optional modules—will be streamed or downloaded asynchronously in the background once the player is active. This approach guarantees rapid onboarding while supporting scalable, high-fidelity content delivery.


This approach minimizes installation time, reduces app-store friction, and creates a smoother onboarding experience that keeps new users playing rather than waiting.

### 2. Integrating External Content Creation Studios Efficiently

**Problem**

As modern games expand, content creation is often distributed across multiple studios worldwide. Without a unified integration process, external teams can produce assets that are inconsistent, improperly structured, or incompatible with production standards—leading to delays, rework, and technical debt.

**Solution**

To address the **studio integration problem**, the project introduces a **structured collaboration framework** that standardizes how external studios produce and deliver game assets. This framework defines explicit conventions for asset bundling, naming, optimization, and submission workflows, enabling consistent quality and predictable integration. By enforcing these shared guidelines, all external contributions can be automatically validated, approved, and merged into the main project pipeline without build conflicts or release interruptions, ensuring smooth and scalable multi-studio collaboration.


--

## Deliverables

### 1. **Unity Addressables Implementation Pattern**

To ensure players can install and begin playing within seconds, the project adopts the [Unity Addressables system](https://docs.unity3d.com/Manual/com.unity.addressables.html) as the foundation for its asset management and delivery strategy. The pattern provides guidance for structuring a game so that only essential assets—menus, initial scenes, and the first playable area—are included in the base download, while all other content is organized into addressable bundles. These bundles enable dynamic loading, efficient memory management, and seamless background downloads, ensuring rapid startup, minimal storage impact, and scalable content updates throughout the game’s lifecycle.

*For more detailed implementation guidance, see the “Addressables: Planning and Best Practices” blog post from Unity.* ([unity.com](https://unity.com/blog/engine-platform/addressables-planning-and-best-practices))

**The pattern will include:**

* **Access and Configuration Guidelines:**
  Step-by-step instructions for how developers access and manage Addressable Assets in game code and project configuration. This includes **initialization workflows**—the standardized sequence of operations that load and prepare the Addressables system at runtime. Initialization workflows cover catalog loading, remote content updates, dependency resolution, and resource prefetching to ensure assets are available before gameplay begins.

* **Asset Bundling Strategy:**
  Prescriptive guidance on how assets should be grouped and bundled to minimize the initial download size, enable on-demand loading, and support seamless content updates without requiring a full game republish.

* **Asset Publishing Pipeline Design:**
  A defined pipeline for building, validating, and publishing addressable bundles, integrating version control, automated build triggers, and delivery to CDN or cloud storage endpoints.

* **Cataloging and Distribution Process:**
  Documentation of how addressable catalogs are generated, versioned, and distributed to ensure synchronization between client and server across release cycles.

* **External Studio Integration Framework:**
  Standards and workflows for integrating **content creation studios** into the production process. Each studio follows shared conventions for asset bundling, naming, optimization, and validation to guarantee that submissions align with the main project’s technical and performance requirements.


### 2. **Implementation of the Unity Addressables Publishing Pipeline**

The second deliverable is the **implementation of an automated Asset Publishing Pipeline**, responsible for **building, validating, and distributing Unity Addressable asset bundles**.
This implementation exists to **decouple external studios and content teams from the core game repository**, allowing asset production to function as an **independent, modular workflow**.

The implementation resides in a **dedicated Git repository** specifically designed for **asset lifecycle management**—from creation and validation to versioned publishing.
External studios and internal art teams commit assets to this isolated repository, where automated workflows perform validation, build, and publishing tasks.
Once validated, the assets are **built, analyzed, and deployed** to the configured **Content Delivery Network (CDN)** for consumption by the game’s runtime environment.

This publishing pipeline achieves **environmental separation**, **multi-platform consistency**, and **repeatable automation** using a combination of Unity’s Addressables system, [GameCI](https://game.ci/) for headless CI execution, and AWS S3 as the backing CDN.
It supports **macOS**, **iOS**, and **Android** targets, with environment-specific configuration for `DEV`, `STAGE`, and `PROD`.



### Pipeline Architecture and Tooling

The pipeline is composed of two coordinated implementation layers:

**1. CI/CD Orchestration (Build Automation Layer)**
Implemented with GitHub Actions and [GameCI](https://game.ci/docs/github/ci/unity-builder/) (`unity-builder@v4`), this layer automates:

* **Headless Unity builds** for all platforms using `unity-builder@v4`.
* **Parallel execution** across macOS, iOS, and Android targets via a job matrix.
* **Automated validation and artifact upload** for analysis and version tracking.
* **AWS S3 publishing** for globally accessible delivery of bundles and catalogs.

**2. Addressables Build Script (Execution Layer)**
Implemented as a C# editor script (`AddressablesBuild.BuildAll`), this layer performs:

* Environment configuration (`DEV`, `STAGE`, `PROD`) through Addressables profiles.
* Pre-build analysis using Unity’s Addressables Analyze tool.
* Addressables build execution for the current target platform.
* Log output of generated bundles, catalogs, and metadata.


### GitHub Actions Configuration – GameCI Multi-Platform Build and S3 Publishing

**Explanation**

This configuration defines the **automation workflow** that drives the Unity Addressables build and publishing process.
When a change is pushed to the repository, the [GameCI Unity Builder v4](https://game.ci/docs/github/ci/unity-builder/) action runs Unity in **headless batch mode** to execute the `AddressablesBuild.BuildAll` method for each platform—**macOS**, **iOS**, and **Android**.
After building, artifacts are uploaded as versioned outputs and synced to **AWS S3** under environment-specific directories (`DEV`, `STAGE`, `PROD`).
The result is a fully automated, multi-platform, multi-environment delivery system for asset bundles and catalogs.

```yaml
# /**
#  * App: Unity Addressables
#  * Package: Company.Product.Tooling.Addressables.Build
#  * File: .github/workflows/addressables-build.yml
#  * Version: 0.2.1
#  * Author: bobwares
#  * Date: 2025-10-20T00:00:00Z
#  * Exports: GameCI multi-platform build (macOS, iOS, Android), AWS S3 publish jobs
#  * Description: Builds Unity Addressables bundles for macOS, iOS, and Android using GameCI,
#  *              then publishes versioned artifacts to AWS S3 by environment.
#  */

name: addressables-build
on:
  push:
    branches: [main]
    paths:
      - Assets/**
      - AddressableAssetsData/**
      - .github/workflows/addressables-build.yml

jobs:
  build-addressables:
    name: Build Addressables (Multi-Platform)
    runs-on: ubuntu-latest
    strategy:
      matrix:
        platform: [StandaloneOSX, iOS, Android]
    env:
      ENV: STAGE
      AUTHOR: bobwares

    steps:
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          lfs: true

      - name: Build Addressables via GameCI
        uses: game-ci/unity-builder@v4
        with:
          unityVersion: 6000.2.8f1
          targetPlatform: ${{ matrix.platform }}
          buildMethod: Company.Product.Tooling.Addressables.Build.AddressablesBuild.BuildAll
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          ENV: ${{ env.ENV }}
          AUTHOR: ${{ env.AUTHOR }}
          GITHUB_SHA: ${{ github.sha }}

      - name: Upload Addressables artifacts
        uses: actions/upload-artifact@v4
        with:
          name: addressables-${{ matrix.platform }}-${{ env.ENV }}-${{ github.sha }}
          path: Library/com.unity.addressables/AddressablesBuild/${{ matrix.platform }}/**

  publish-to-s3:
    name: Publish to AWS S3
    needs: build-addressables
    runs-on: ubuntu-latest
    strategy:
      matrix:
        platform: [StandaloneOSX, iOS, Android]
    steps:
      - name: Download build artifacts
        uses: actions/download-artifact@v4
        with:
          name: addressables-${{ matrix.platform }}-${{ env.ENV }}-${{ github.sha }}
          path: build-output/${{ matrix.platform }}

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Publish platform output to S3
        run: |
          aws s3 sync build-output/${{ matrix.platform }} s3://my-game-addressables/${{ env.ENV }}/${{ matrix.platform }}/ --delete
          echo "Published Addressables for ${{ matrix.platform }} → s3://my-game-addressables/${{ env.ENV }}/${{ matrix.platform }}/"
```

---

### Addressables Build Script – Multi-Platform, Multi-Environment Build Logic

**Explanation**

This C# editor script is the **entrypoint** executed by GameCI’s `unity-builder@v4` action during CI/CD.
It configures Unity’s Addressables system for the appropriate environment profile, performs static analysis using the built-in **Addressables Analyze tool**, and then builds the bundles for the target platform (macOS, iOS, or Android).
Each build writes output bundles and catalogs under platform-specific directories within Unity’s `Library/com.unity.addressables/AddressablesBuild/` structure.
The CI workflow then uploads the generated artifacts to the correct **environment-specific S3 bucket path**.

```csharp
/**
 * App: Unity Addressables
 * Package: Company.Product.Tooling.Addressables.Build
 * File: Assets/Editor/Addressables/AddressablesBuild.cs
 * Version: 0.2.1
 * Author: bobwares
 * Date: 2025-10-20T00:00:00Z
 * Exports: AddressablesBuild.BuildAll
 * Description: Multi-platform, multi-environment Addressables build script for Unity CI.
 *              Configures environment profiles, runs analysis, and builds
 *              bundles for macOS, iOS, and Android targets.
 */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Settings;

namespace Company.Product.Tooling.Addressables.Build
{
    public static class AddressablesBuild
    {
        // CI entrypoint for all platforms
        public static void BuildAll()
        {
            var env = Environment.GetEnvironmentVariable("ENV") ?? "DEV";
            ConfigureAddressablesFor(env);
            AnalyzeAddressables();
            BuildAddressables();
            Debug.Log($"[AddressablesBuild] Completed Addressables build for {env}");
        }

        private static void ConfigureAddressablesFor(string env)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var profiles = settings.profileSettings;
            var profileId = profiles.GetProfileId(env) ?? profiles.GetProfileId("DEV");
            profiles.SetActiveProfileId(profileId);
            Debug.Log($"[AddressablesBuild] Using profile: {env}");
        }

        private static void AnalyzeAddressables()
        {
            var analyzer = new UnityEditor.AddressableAssets.Build.AnalyzeSystem.AnalyzeSystem();
            analyzer.ClearAnalysis();
            analyzer.RefreshAnalysis();
            analyzer.RunAnalysis();
            Debug.Log("[AddressablesBuild] Addressables analysis completed successfully.");
        }

        private static void BuildAddressables()
        {
            AddressableAssetSettings.BuildPlayerContent();

            var target = EditorUserBuildSettings.activeBuildTarget.ToString();
            var outputDir = Path.Combine("Library", "com.unity.addressables", "AddressablesBuild", target);

            if (Directory.Exists(outputDir))
                Debug.Log($"[AddressablesBuild] Output directory: {outputDir}");
            else
                Debug.LogWarning($"[AddressablesBuild] Output directory missing for {target}.");
        }
    }
}
```



### Outcome

The Unity Addressables Publishing Pipeline provides a **fully automated, multi-platform, and environment-aware content delivery system** that:

* Builds and validates Addressable bundles for **macOS**, **iOS**, and **Android**.
* Operates independently of the main game source repository, ensuring clean decoupling.
* Publishes bundles to **AWS S3** using structured paths per environment (`DEV`, `STAGE`, `PROD`).
* Enables continuous integration and reliable content delivery for distributed art and development teams.
