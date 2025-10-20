# Unity Architecture 

Unity is built as **three primary layers** that work together: **Toolchain**, **Engine Runtime**, and **Platform Backends**. Around those sit optional service systems (Unity Gaming Services, Cloud Build, CCD, etc.).

Here’s the architecture from top to bottom.

---

## 1. Unity Hub

* A **launcher and version manager**.
* Handles:

    * Installation of Editor versions and platform modules.
    * License management.
    * Project creation and switching.
    * Integration with Unity Cloud Build and Collaborate.

Hub doesn’t edit or build projects—it just orchestrates the toolchain.

---

## 2. Unity Editor

* The **authoring environment** built on the same engine core that ships in players.
* Provides:

    * Scene, Prefab, and UI editors.
    * Asset import pipeline (models, textures, audio, etc.).
    * Scripting integration (C#).
    * Build pipeline and packaging for each platform.
    * Profilers, inspectors, animation editors, shader graph, etc.
* Internally hosts the same **engine runtime** for Play Mode simulation.
* Extensible via **UnityEditor API**, custom inspectors, and Editor Windows.

---

## 3. Package Manager (UPM)

* Component system for modular engine features.
* Core packages (URP, HDRP, Input System, Cinemachine, Addressables, etc.).
* Custom/embedded packages for internal modules.
* Dependencies managed via `Packages/manifest.json`.

---

## 4. Asset Pipeline

* Converts external files into Unity’s serialized formats.
* Handles:

    * Import (FBX → Mesh, PSD → Texture2D, WAV → AudioClip).
    * GUID + `.meta` identity management.
    * Caching and dependency tracking in `Library/`.
* Feeds into **Addressables** and **AssetBundles** for distribution.

---

## 5. Scripting Layer

* Developer-facing code layer in **C#**.
* Compiled into managed assemblies under `Library/ScriptAssemblies`.
* Runtime choices:

    * **Mono**: Editor + dev builds (managed JIT).
    * **IL2CPP**: Production builds (C++ transpiled, AOT compiled).
* Optional:

    * **Burst** compiler: high-performance codegen for Jobs/ECS.
    * **C# Job System / Entities (DOTS)** for data-oriented parallel processing.

---

## 6. Engine Runtime (Core)

This is the **native C++ layer** that executes in both Editor and players.

Subsystems:

| Subsystem          | Description                                                       |
| ------------------ | ----------------------------------------------------------------- |
| **Rendering**      | SRP architecture, URP/HDRP, lighting, shaders, post-processing.   |
| **Physics**        | PhysX (3D) and Box2D (2D) backends, collisions, triggers, joints. |
| **Animation**      | Mecanim state machines, Timeline, Playables.                      |
| **Audio**          | Import, mixing, spatialization, DSP routing.                      |
| **UI**             | UGUI and UI Toolkit (UXML/USS).                                   |
| **Navigation**     | NavMesh, Agents, Obstacles, pathfinding.                          |
| **Asset I/O**      | Scene loading/unloading, Addressables, AssetBundles, Resources.   |
| **Scripting host** | Embeds Mono/IL2CPP runtime for managed assemblies.                |
| **Profiler hooks** | Runtime instrumentation for CPU, GPU, and memory analysis.        |

---

## 7. Platform Abstraction Layer

* Bridges engine subsystems to OS / hardware APIs.
* Modules per platform installed via Hub:

    * Graphics APIs: DirectX, Vulkan, Metal, OpenGL.
    * Audio backends: FMOD/OS-native.
    * Input drivers.
    * Windowing and threading systems.
    * File, network, and device I/O.

---

## 8. Player Build System

* Combines managed code (C# → IL/IL2CPP) + assets (scenes, bundles).
* Produces platform-specific binaries:

    * Windows/macOS/Linux executables.
    * Android APK/AAB.
    * iOS Xcode project.
    * WebGL output.
    * Console builds (proprietary toolchains).
* Handles compression, stripping, shader variant pruning, texture/audio compression per platform.

---

## 9. Runtime Content Systems

| System                           | Function                                                                        |
| -------------------------------- | ------------------------------------------------------------------------------- |
| **Resources/**                   | Legacy direct-load content folder.                                              |
| **StreamingAssets/**             | Raw files copied verbatim into player.                                          |
| **AssetBundles**                 | Manually packed groups of assets for runtime load.                              |
| **Addressables**                 | Logical addressing and dependency management; builds to bundles + catalog.json. |
| **Cloud Content Delivery (CCD)** | Unity-hosted CDN for Addressables bundles and catalogs.                         |

---

## 10. Unity Services Layer (optional cloud stack)

* **Cloud Build** – CI/CD for projects.
* **Analytics** – Telemetry and metrics.
* **Remote Config** – Feature flags and config variants.
* **Economy / Ads / In-App Purchasing** – Monetization systems.
* **Multiplayer** – Relay, Lobby, Matchmaker, Vivox Voice.
* **Crash & Performance Reporting** – Diagnostics aggregation.
* **Cloud Save** – Player data sync.

These live outside the engine and integrate via SDKs (UPM packages).

---

## 11. Supporting Systems

* **Profiler / Frame Debugger / Memory Profiler** – runtime analysis.
* **Test Framework** – edit/play mode testing.
* **Shader Graph, VFX Graph, Visual Scripting** – node-based content creation tools built atop the engine API.
* **Timeline / Cinemachine** – sequencing and camera control.

---

## Conceptual summary

```
Unity Hub
    ↓
Unity Editor  ←→  Package Manager / Asset Pipeline
    ↓
C# Scripts + Assets (.prefab, .unity, .mat, .fbx, etc.)
    ↓
Engine Runtime (Rendering, Physics, Audio, Animation, etc.)
    ↓
Platform Abstraction (DirectX, Metal, Vulkan, etc.)
    ↓
Built Player (with optional Services + CDN-delivered content)
```

--- 

In Unity, a “player” is the built application that runs your game outside the Editor.

Definition

* Player = the platform-specific executable plus its data files that Unity produces when you do Build or Build & Run. It embeds the Unity engine runtime and your compiled C# assemblies, but none of the Editor UI/APIs.

What a player includes

* Executable/container:

  * Windows: MyGame.exe + MyGame_Data
  * macOS: MyGame.app (bundle)
  * Linux: MyGame.x86_64 + MyGame_Data
  * iOS: Xcode project that compiles to an .ipa
  * Android: .apk or .aab
  * WebGL: index.html + build artifacts
  * Headless/server: console executable without graphics
* Managed code: your C# compiled to assemblies; IL2CPP players transpile IL to C++ and AOT-compile.
* Content: scenes and assets packaged into the player (and optionally external AssetBundles/Addressables).

How a player differs from the Editor

* No UnityEditor API. Code in UnityEditor.* namespaces does not exist in the player.
* Fixed runtime environment. No Inspector/Scene tools, no hot reimport; only your game loop and any in-game UI/tools you built.
* Stripping and optimizations. Code/engine modules and shader variants can be stripped; textures/audio are compressed per platform.
* Scripting backend. Editor typically runs Mono; players often use IL2CPP for performance/security.
* File system. You load data from StreamingAssets/ and persistentDataPath; there’s no AssetDatabase.
* Feature gates. Conditional code via scripting defines (e.g., UNITY_STANDALONE, UNITY_IOS).

Relationship to “executes in both Editor and players”

* The engine runtime (rendering, physics, animation, audio, etc.) is the same core technology used in:

  * Editor Play Mode (runs inside the Editor process)
  * Built players (your packaged app)
* Your game scripts (UnityEngine.*) generally run the same in both, but:

  * Editor-only code (UnityEditor.*) runs only in the Editor.
  * Behavior can diverge due to platform APIs, input, file paths, or build-time stripping.

Build types that matter

* Development Build: includes symbols, device console logs, Script Debugging, Autoconnect Profiler, Deep Profiling support.
* Release/Non-development Build: optimized, stripped, no debug helpers.

Where to configure “player” behavior

* Project Settings → Player: company/product name, bundle identifiers, resolution/presentation, icon/splash, scripting backend, API compatibility, IL2CPP settings, managed stripping level, etc.

Typical workflows

* Local test: Build & Run desktop player; compare against Editor Play Mode.
* Device test: Android APK/AAB or iOS via Xcode; use Development Build + Profiler and device logs.
* Streaming content: Ship a small player and load content at runtime via Addressables/AssetBundles from your CDN.

Rule of thumb

* If it’s your shipped app running on an end-user device or a CI/device build you run outside the Editor, it’s a “player.”
