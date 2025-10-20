# Statement of Work (SOW)

* **Project Title:** Unity Game Framework – Addressables-Based Asset Pipeline
* **Version:** 1.0.0
* **Date:** 2025-10-19
* **Author:** Bobwares Consulting

## Problem Statement

Modern mobile and cross-platform games face two critical challenges that directly affect player retention and production scalability:

### 1. Reducing Initial Download Size to Prevent User Abandonment

Players increasingly expect near-instant access to entertainment. When a game’s initial download is too large or takes too long to launch, most users abandon the process before experiencing gameplay. Studies consistently show a steep drop in retention when installation exceeds 150 MB or when the first interaction is delayed beyond 10 seconds.

The first impression is decisive. The player must be able to install, open, and begin playing almost immediately. To achieve this, the game must separate **core content**—only what’s essential for the initial experience—from **secondary or extended content**, which can be fetched later. In this model, the base game package contains only what is required to reach the title screen, select a character, and begin play. Larger environments, high-resolution assets, and optional features are downloaded transparently in the background after the player is engaged.

This approach minimizes installation time, reduces app-store friction, and creates a smoother onboarding experience that keeps new users playing rather than waiting.

### 2. Integrating External Content Creation Studios Efficiently

As modern games expand, content creation is often distributed across multiple studios worldwide. Without a unified integration process, external teams can produce assets that are inconsistent, improperly structured, or incompatible with production standards—leading to delays, rework, and technical debt.

The project therefore also solves the **studio integration problem** by establishing a **structured collaboration model** that defines how outside studios contribute game assets safely and predictably. Each studio will operate under clear guidelines for asset organization, naming, optimization, and submission, ensuring all content can be validated, approved, and integrated seamlessly into the main project without disrupting builds or releases.


---

## Deliverables

### 1.  Unity Addressables Assets **Application Implementation Pattern**


The pattern provides guidance for structuring a game so players can install and begin playing within seconds. Only essential assets—menus, initial scenes, and the first playable area—are included in the base download.

All other content, such as extended environments, character models, or cinematic sequences, is organized into modular bundles that can be downloaded dynamically as the player progresses.

The pattern will include:

- Instructions on how the game developer accesses Addressable Assets in the game code and configuration.
- Guidance on how Unity Addressable Assets should be bundled in a way that minimizes initial download size, enables dynamic content loading, and supports continuous updates without full game republishing.
- The design of an Asset Publishing Pipeline
- How they are cataloged and published.
- The pattern establishes guidance for integrating external **content creation studios** into the production process. Each participating studio follows shared conventions for asset structure, naming, and optimization, ensuring all submissions align with the technical and performance expectations of the main game project.
- define a Unity Addressable Assets Publishing Pipeline.



### 2. Implementation of the Unity Addressable Assets Publishing Pipeline

The second deliverable is the **implementation of an Asset Publishing Pipeline**, which automates the process of building, validating, and distributing addressable asset bundles.

This pipeline operates independently of the main game source code through an **isolated Git repository** dedicated to asset production. External studios and internal art teams commit their content to this separate repository, keeping asset creation decoupled from game logic. The pipeline automatically builds, analyzes, and publishes validated assets for integration and release.

Key capabilities include:

* **Headless Unity Build Execution:** Automates bundle creation through command-line builds for repeatability and CI/CD integration.
* **Automated Bundle Analysis:** Detects missing references, duplicate dependencies, and inefficiencies before publishing.
* **Isolated Asset Repository Integration:** Enables independent asset submission and version tracking, preventing conflicts with the main game implementation repository.
* **AWS-Based Publishing and Distribution:** Publishes validated content to a scalable content delivery provider for global accessibility.
* **Versioned Release Management:** Maintains structured manifests and release metadata to support rollback, verification, and controlled deployment.

## Summary

Together, these solutions ensure that:

* Players begin gameplay within seconds of installation.
* The game footprint remains small and optimized for global mobile distribution.
* Content from multiple studios can be incorporated reliably through a governed submission and validation process.
* The development pipeline supports ongoing content expansion without requiring full re-releases or manual intervention.

