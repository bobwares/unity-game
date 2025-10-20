# Statement of Work (SOW)

* **Project Title:** Unity Game Framework – Addressables-Based Asset Pipeline
* **Version:** 1.0.0
* **Date:** 2025-10-19
* **Author:** Bobwares Consulting

---

Understood. Here’s the revised and more **high-level Problem Statement** — with all technical implementation details (like Unity Addressables, CCD, and S3/CDN) removed and replaced by outcome-focused, client-friendly language suitable for an executive or SOW introduction.

---

## Problem Statement

Modern mobile and cross-platform games face two critical challenges that directly affect player retention and production scalability:

### 1. Reducing Initial Download Size to Prevent User Abandonment

Players increasingly expect near-instant access to entertainment. When a game’s initial download is too large or takes too long to launch, most users abandon the process before experiencing gameplay. Studies consistently show a steep drop in retention when installation exceeds 150 MB or when the first interaction is delayed beyond 10 seconds.

The first impression is decisive. The player must be able to install, open, and begin playing almost immediately. To achieve this, the game must separate **core content**—only what’s essential for the initial experience—from **secondary or extended content**, which can be fetched later. In this model, the base game package contains only what is required to reach the title screen, select a character, and begin play. Larger environments, high-resolution assets, and optional features are downloaded transparently in the background after the player is engaged.

This approach minimizes installation time, reduces app-store friction, and creates a smoother onboarding experience that keeps new users playing rather than waiting.

### 2. Integrating External Content Creation Studios Efficiently

As modern games expand, content creation is often distributed across multiple studios worldwide. Without a unified integration process, external teams can produce assets that are inconsistent, improperly structured, or incompatible with production standards—leading to delays, rework, and technical debt.

The project therefore also solves the **studio integration problem** by establishing a **structured collaboration model** that defines how outside studios contribute game assets safely and predictably. Each studio will operate under clear guidelines for asset organization, naming, optimization, and submission, ensuring all content can be validated, approved, and integrated seamlessly into the main project without disrupting builds or releases.

Together, these solutions ensure that:

* Players begin gameplay within seconds of installation.
* The game footprint remains small and optimized for global mobile distribution.
* Content from multiple studios can be incorporated reliably through a governed submission and validation process.
* The development pipeline supports ongoing content expansion without requiring full re-releases or manual intervention.

This combination of performance optimization and studio integration creates a foundation for a modern, scalable game that maintains player engagement while supporting continuous global content production.

# Solution



## 2. Scope of Work

### 2.1 In-Scope

#### A. Unity Project Setup and Configuration

* Configure a clean Unity 6000.2.8f1 project with standard directory conventions (`Assets/`, `Scripts/`, `Addressables/`, `Editor/Build/`).
* Integrate required packages: **Addressables**, **Cloud Content Delivery (CCD) SDK**, and build automation scripts.
* Define platform targets (iOS and Android) with consistent build behavior.

#### B. Asset Management and Classification

* Define a unified **asset taxonomy** (mesh, texture, animation, audio, prefab, scene, scriptable object).
* Establish labeling, grouping, and dependency conventions.
* Specify which assets are bundled locally vs streamed remotely.

#### C. Addressables Build and Deployment Pipeline

* Implement Addressables configuration for both **local** and **remote** bundles.
* Configure Unity **Analyze** tool for dependency validation.
* Create **macOS CLI build scripts** for headless builds in CI/CD.
* Implement secure publishing workflow to CCD or AWS S3 with versioned catalogs.

#### D. Performance Best Practices Guide

* Develop a reference manual outlining **Addressables performance and bundling best practices**.
* Include bundle sizing rules, scene partitioning strategies, compression recommendations, and Analyze policy guidance.
* Document scenarios for sub-10 s install, 5 s startup, and progressive scene streaming.

#### E. Continuous Integration and Quality Gates

* Configure GitHub Actions or Jenkins pipelines to automate build, validation, and deployment.
* Enforce Analyze checks and Addressables validation in CI.
* Generate reports on bundle size, duplication, and dependency graph.

#### F. Documentation and Training

* Produce developer documentation covering:

    * Project setup and directory structure.
    * CLI build and publish commands.
    * Studio workflow for asset submission and release promotion.
* Conduct one technical training session for studio developers.

---

### 2.2 Out-of-Scope

* Gameplay logic, mechanics, or visual effects.
* Third-party license or store-publication costs.
* Cloud infrastructure costs (AWS, CCD, CDN).
* Analytics, monetization, or ad integrations.
* Asset creation by content studios.

---

## 3. Deliverables and Milestones

| # | Deliverable                       | Description                                                                     | Target Date | Acceptance Criteria                                             |
| - | --------------------------------- | ------------------------------------------------------------------------------- | ----------- | --------------------------------------------------------------- |
| 1 | Unity Framework Setup             | Base Unity project configured with Addressables and remote delivery integration | Week 2      | Builds successfully for iOS and Android; Addressables enabled.  |
| 2 | Asset Taxonomy & Labeling Rules   | Classification schema and naming/labeling conventions                           | Week 3      | Approved taxonomy; assets categorized and validated.            |
| 3 | Addressables Build Pipeline       | End-to-end CLI build and publish workflow                                       | Week 5      | Build passes Analyze validation; remote assets load in client.  |
| 4 | Addressables Best Practices Guide | Comprehensive performance and bundling guide                                    | Week 6      | Document reviewed and adopted by team; passes technical review. |
| 5 | Remote Distribution Integration   | Automated upload and catalog synchronization (CCD/S3)                           | Week 8      | CI/CD deployment validated; versioned catalog visible remotely. |
| 6 | QA & Validation Pipeline          | Automated Analyze and smoke tests                                               | Week 9      | All builds pass validation; no unresolved dependencies.         |
| 7 | Documentation & Training          | Technical documentation and recorded training                                   | Week 10     | Client sign-off on documentation package.                       |

---

## 4. Technical Approach

### 4.1 System Architecture

1. Source assets → Addressables Groups + Labels → Build Bundles + Catalogs → Upload to CCD/S3 → Client loads remote catalog at runtime.
2. Profiles (Dev/Staging/Prod) control RemoteBuildPath and RemoteLoadPath.
3. Catalogs and bundles are hash-versioned for cache validity and rollback.

### 4.2 Addressables Design

* **Groups:** Base App (Local), On-Demand (Remote), Shared Dependencies.
* **Packing:** Pack Together by Label for co-loaded assets; Pack Separately for volatile content.
* **Labels:** Encode load intent (e.g., base, lobby, adventure-core).
* **Analyze:** Run duplicate and dependency checks every build.
* **Versioning:** Content-hash naming; rollback via CCD Badge or S3 prefix reversion.

### 4.3 Runtime Loading and Memory Patterns

* `InitializeAsync` → `CheckForCatalogUpdates` → `UpdateCatalogs` on startup.
* Prefetch next content with `GetDownloadSizeAsync` and `DownloadDependenciesAsync`.
* Use `LoadSceneAsync(Additive)` for streaming scenes while base scene runs.
* Release assets via `Release` / `ReleaseInstance` when scenes unload.
* Maintain a handle registry and eviction policy for low-memory devices.

### 4.4 Packaging and Performance Best Practices

* Base bundle: only what’s needed for <5 s startup.
* Playable chunks: 10–50 MB typical mobile target.
* Use immutable hash-named bundles and shared groups for cache reuse.
* Split large scenes into additive sub-scenes.
* Platform compression: ASTC/ETC2 textures, streamed audio for music/VO.

### 4.5 CI/CD Workflow

**Pipeline stages:** Validate → Build → Upload → Smoke Test → Promote/Rollback.

**Validate**

* Run batchmode Analyze; fail on duplicates or missing refs.

**Build**

* `AddressableAssetSettings.BuildPlayerContent()` creates bundles + catalog + manifest.

**Upload & Publish**

* CCD: create Release and move Badge (latest/candidate).
* S3: upload to versioned prefix and update alias (`latest`).

**Smoke Test**

* Headless runtime test downloads and loads a sentinel label.

**Promote/Rollback**

* Promote badge/alias on success; rollback in <2 minutes if errors detected.

Example CLI (macOS):
/Applications/Unity/Hub/Editor/6000.2.8f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath /path/to/project -executeMethod Build.AddressablesPipeline.BuildContentAndPublish -buildTarget iOS

### 4.6 CCD vs S3/CDN Integration

**CCD:** Buckets per env; immutable Releases; Badges for promotion.
`https://cds.cloud.unity3d.com/content/<project>/<bucket>/<badge>/[BuildTarget]`

**S3/CDN:** Versioned prefixes and alias (e.g., `prod/latest/ios/`); CloudFront invalidation on promote.
`https://cdn.example.com/prod/latest/ios/`

### 4.7 Telemetry and Testing

* Telemetry: catalog update result, download size/time, memory usage, exceptions.
* Tests: Playmode (`GetDownloadSizeAsync`, `LoadSceneAsync`), Editor (Analyze rules).
* Target SLOs: startup ≤ 5 s, 95th percentile chunk ≤ 5 s download, Addressables crash rate ≤ 0.1%.

### 4.8 Risk and Mitigation

| Risk                               | Impact | Mitigation                                    |
| ---------------------------------- | ------ | --------------------------------------------- |
| Duplicate bundle dependencies      | High   | Enforce Analyze gate; refactor shared groups. |
| Large initial download             | High   | Micro-chunking and label-based prefetch.      |
| Memory pressure on low-end devices | Medium | Aggressive release and bundle eviction.       |
| Bad remote release                 | High   | CCD/S3 rollback within 2 minutes.             |

---

## 5. Signatures

| Party      | Name     | Title                    | Signature | Date |
| ---------- | -------- | ------------------------ | --------- | ---- |
| Client     |          |                          |           |      |
| Contractor | Bobwares | AI Coding Agent Engineer |           |      |

---

Would you like me to append an **Appendix A** with sample Addressables profile templates, RemoteLoadPath variables, and the shell command snippets for both CCD and S3 publishing?
