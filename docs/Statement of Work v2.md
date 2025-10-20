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


---

## Deliverables

### 1. **Unity Addressables Implementation Pattern**

To ensure players can install and begin playing within seconds, the project adopts the [Unity Addressables system](https://docs.unity3d.com/Manual/com.unity.addressables.html) as the foundation for its asset management and delivery strategy. The pattern provides guidance for structuring a game so that only essential assets—menus, initial scenes, and the first playable area—are included in the base download, while all other content is organized into addressable bundles. These bundles enable dynamic loading, efficient memory management, and seamless background downloads, ensuring rapid startup, minimal storage impact, and scalable content updates throughout the game’s lifecycle.

*For more detailed implementation guidance, see the “Addressables: Planning and Best Practices” blog post from Unity.* ([unity.com](https://unity.com/blog/engine-platform/addressables-planning-and-best-practices))

---

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

The second deliverable is the **implementation of an automated Asset Publishing Pipeline**, responsible for building, validating, and distributing Unity Addressable asset bundles.
This implementation exists to **decouple external studios and content teams from the core game repository**, allowing asset production to function as an independent, modular workflow.

The implementation resides in a **dedicated Git repository** specifically designed for **asset lifecycle management**—from creation and validation to versioned publishing. External studios and internal art teams commit assets to this isolated repository, where automated workflows perform validation, build, and publishing tasks. Once validated, the assets are built, analyzed, and deployed to the configured **Content Delivery Network (CDN)** for consumption by the game’s runtime environment.

---

### **Pipeline Architecture and Tooling**

**Core Technologies**

* **Unity Editor (Headless Mode)** — Executes automated Addressable builds using CLI options (`-batchmode -nographics -executeMethod BuildPipelineEntryPoint`)
* **Git + Git LFS** — Provides source control and version management for large binary assets
* **GameCI GitHub Actions** — Manages CI/CD orchestration, integrating Unity build and validation jobs into GitHub pipelines
* **AWS S3 + CloudFront** — Hosts published Addressable bundles for scalable, global asset distribution
* **C# Editor Scripts** — Implement environment-aware orchestration logic inside the Unity Editor for build configuration, catalog generation, and environment selection (Dev, Stage, Production)
* **Manifest and Metadata Services** — JSON- or YAML-based manifests stored in S3 and optionally indexed via DynamoDB for version tracking and rollback

---

### **End-to-End Workflow**

1. **Studio Submission (Development Environment)**

  * External studios and internal teams push assets (models, textures, audio, prefabs, scenes) to the **asset repository** using Git LFS for large binaries.
  * A pre-commit or CI validation workflow checks asset naming, folder conventions, and optimization compliance.
  * The build environment variables (`ENV=DEV`) configure output directories and catalog URLs for the development tier.

2. **Automated Validation and Build (Staging Environment)**

  * GameCI executes **headless Unity builds** to compile and bundle Addressables (`unity-builder@v4` or later).
  * **Automated analysis** detects missing references, duplicate dependencies, or oversized assets.
  * Failed builds are flagged within the GitHub Actions workflow summary with logs and structured output.

3. **Versioning and Manifest Generation**

  * Successful builds generate manifest and catalog files that describe bundle dependencies and hashes.
  * The build is tagged and versioned (`stage-v0.4.2`) with associated metadata (commit SHA, timestamp, author).
  * A C# post-build script writes environment data and build metadata to JSON for downstream deployment jobs.

4. **Publishing to CDN (Production Environment)**

  * GameCI deploys validated bundles to **AWS S3** via GitHub Actions secrets for access keys.
  * **CloudFront** invalidations propagate updates globally, with cache TTLs defined per environment.
  * Stage and Production buckets are isolated (`s3://assets-stage`, `s3://assets-prod`) to support controlled promotion.

5. **Integration with Core Game Repository**

  * The game references the published Addressable catalogs via **remote URLs** defined in `AddressablesSettings.asset`.
  * Environment switching (`DEV`, `STAGE`, `PROD`) is handled by the **C# initialization workflow**, which loads the appropriate catalog endpoint at runtime.
  * This ensures that test builds and production clients always resolve assets from their correct environment tier.

---

### **Key Capabilities**

* **Headless Unity Build Execution**
  Automated bundle creation via GameCI command-line builds for deterministic output and CI/CD reproducibility.

* **Automated Bundle Analysis**
  Detects missing or duplicate references and optimizes dependencies prior to publishing.

* **Isolated Asset Repository Integration**
  Enables independent submission, branching, and version tracking separate from the main gameplay repository.

* **AWS-Based Publishing and Distribution**
  Deploys validated bundles to S3 with CloudFront caching for high-availability global distribution.

* **Versioned Release Management**
  Maintains structured manifests and metadata for rollback, verification, and controlled promotion across environments.


### **Outcome**

The Unity Addressables Publishing Pipeline enables scalable, multi-studio asset production integrated with GameCI automation and environment-aware deployment.
It ensures repeatable, validated content delivery from **Development → Staging → Production**, preserving repository isolation, maintaining traceability, and supporting enterprise-grade content operations within Unity’s Addressables framework.

---

Would you like me to append a **sequence diagram description** next (e.g., Studio → Git Push → GameCI Build → AWS Publish → Game Client Load)? It would align well as a visual appendix to this section.


## Summary

Together, these solutions ensure that:

* Players begin gameplay within seconds of installation.
* The game footprint remains small and optimized for global mobile distribution.
* Content from multiple studios can be incorporated reliably through a governed submission and validation process.
* The development pipeline supports ongoing content expansion without requiring full re-releases or manual intervention.

