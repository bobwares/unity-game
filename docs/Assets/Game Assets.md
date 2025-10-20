# Game Assets

A game asset is any discrete piece of content (data, not compiled gameplay code) that a game engine can reference at edit time or load at run time to produce visuals, audio, UI, physics, or behavior. Assets are serialized files (and their metadata) that the engine imports, versions, bundles, streams, and instantiates to build the player experience.

Key properties

* Format: concrete file(s) on disk with well-defined extensions (e.g., .png, .fbx, .wav, .mat, .prefab, .anim, .uxml, .asset) plus engine-generated .meta.
* Purpose: supplies content or configuration—models, textures, animations, audio, levels, prefabs, VFX, UI, localization, input maps, data tables.
* Referencing: addressed by paths/guids/labels and referenced by other assets or code; engines resolve dependencies automatically at build/load.
* Lifecycle: created in DCC tools or the editor → imported/serialized by the engine → optionally packed (e.g., AssetBundles/Addressables) → loaded/instantiated at runtime → unloaded/reused per memory policy.
* Platform specificity: many assets are reprocessed per target (texture compression, mesh/animation formats, shader variants).
* Ownership and versioning: treated as source artifacts under version control; large binaries may be stored via LFS; changes are tracked like code.

What a game asset is not:

* Compiled executable logic (e.g., C#/C++ assemblies, IL2CPP binaries) and project settings. These are build artifacts or configuration, not “assets” in the content sense—though they reference assets.

Practical rule of thumb

* If it’s authored content or configuration that the engine imports and your code loads or instantiates (directly or via references), it’s a game asset. If it’s compiled code or the runtime itself, it isn’t.







Here’s a comprehensive reference list of **Unity asset types** used in game app development, including their **purpose**, **type classification**, **creation tool or method**, and **Unity documentation reference**.

---

### **Unity Asset Types Overview**

| **Asset Type**                        | **Purpose**                                               | **Type**             | **Created With / Imported From** | **Unity Docs**                                                                             |
| ------------------------------------- | --------------------------------------------------------- | -------------------- | -------------------------------- | ------------------------------------------------------------------------------------------ |
| **Scene (.unity)**                    | Defines layout, objects, and environment for a game level | Data/Structure       | Unity Editor                     | [Scenes](https://docs.unity3d.com/Manual/CreatingScenes.html)                              |
| **Prefab (.prefab)**                  | Reusable GameObject template with components              | Data/Code Container  | Unity Editor                     | [Prefabs](https://docs.unity3d.com/Manual/Prefabs.html)                                    |
| **Script (.cs)**                      | Contains game logic written in C#                         | Executable Code      | Visual Studio / Rider            | [C# Scripts](https://docs.unity3d.com/Manual/CreatingAndUsingScripts.html)                 |
| **Material (.mat)**                   | Defines surface appearance (color, texture, shader)       | Visual/Shader Data   | Unity Editor                     | [Materials](https://docs.unity3d.com/Manual/Materials.html)                                |
| **Shader (.shader)**                  | GPU program controlling surface rendering                 | Executable Shader    | Shader Graph / HLSL              | [Shaders](https://docs.unity3d.com/Manual/Shaders.html)                                    |
| **Texture (.png, .jpg, .tga, etc.)**  | 2D image data applied to materials                        | Visual Asset         | Photoshop, GIMP, etc.            | [Textures](https://docs.unity3d.com/Manual/Textures.html)                                  |
| **Sprite (.png, .psd)**               | 2D image for UI or 2D games                               | Visual Asset         | Photoshop, Illustrator           | [Sprites](https://docs.unity3d.com/Manual/Sprites.html)                                    |
| **Animation Clip (.anim)**            | Defines keyframe animation data                           | Animation            | Unity Animation Window           | [Animation Clips](https://docs.unity3d.com/Manual/AnimationClips.html)                     |
| **Animator Controller (.controller)** | Controls animation states and transitions                 | Logic Asset          | Animator Window                  | [Animator Controllers](https://docs.unity3d.com/Manual/AnimatorControllers.html)           |
| **Audio Clip (.wav, .mp3, .ogg)**     | Sound effect or music file                                | Audio Asset          | Audio editor / Import            | [Audio Clips](https://docs.unity3d.com/Manual/class-AudioClip.html)                        |
| **Audio Mixer (.mixer)**              | Manages audio routing and effects                         | Audio Asset          | Unity Audio Mixer                | [Audio Mixer](https://docs.unity3d.com/Manual/AudioMixer.html)                             |
| **Font (.ttf, .otf)**                 | Text rendering                                            | Visual Asset         | Font file import                 | [Fonts](https://docs.unity3d.com/Manual/class-Font.html)                                   |
| **UI Elements (UXML, USS)**           | Defines UI layouts and styles                             | UI Layout Code       | UI Toolkit                       | [UI Toolkit](https://docs.unity3d.com/Manual/UIE-USS.html)                                 |
| **ScriptableObject (.asset)**         | Stores reusable game data (config, stats, etc.)           | Data Object          | C# + Unity Editor                | [ScriptableObjects](https://docs.unity3d.com/Manual/class-ScriptableObject.html)           |
| **Physic Material (.physicMaterial)** | Defines physical surface properties                       | Physics Data         | Unity Editor                     | [Physics Materials](https://docs.unity3d.com/Manual/class-PhysicMaterial.html)             |
| **Mesh (.fbx, .obj)**                 | 3D geometry for objects                                   | Visual/3D Model      | Blender, Maya, 3ds Max           | [Meshes](https://docs.unity3d.com/Manual/class-Mesh.html)                                  |
| **Model (.fbx, .obj, .dae)**          | 3D model with animation and hierarchy                     | 3D Asset             | Blender, Maya                    | [Model Importing](https://docs.unity3d.com/Manual/HOWTO-importObject.html)                 |
| **Terrain Data (.asset)**             | Stores heightmaps, textures, and vegetation               | Environment Asset    | Terrain Editor                   | [Terrains](https://docs.unity3d.com/Manual/terrain-UsingTerrains.html)                     |
| **Lighting Data Asset**               | Stores precomputed lighting and reflection data           | Lighting Cache       | Generated during bake            | [Lighting Data](https://docs.unity3d.com/Manual/Lighting.html)                             |
| **NavMesh Data (.asset)**             | Defines navigable areas for AI                            | AI Navigation        | Unity Navigation                 | [NavMesh](https://docs.unity3d.com/Manual/nav-NavigationSystem.html)                       |
| **Timeline (.playable)**              | Manages cinematic sequences                               | Animation/Control    | Timeline Window                  | [Timeline](https://docs.unity3d.com/Manual/TimelineSection.html)                           |
| **Playable Asset (.playable)**        | Custom logic for Timeline playback                        | Executable Data      | C# API                           | [Playables](https://docs.unity3d.com/Manual/Playables.html)                                |
| **Lighting Probe Group (.asset)**     | Captures lighting for dynamic objects                     | Lighting Data        | Unity Editor                     | [Light Probes](https://docs.unity3d.com/Manual/LightProbes.html)                           |
| **Reflection Probe (.asset)**         | Captures environment reflections                          | Lighting Data        | Unity Editor                     | [Reflection Probes](https://docs.unity3d.com/Manual/class-ReflectionProbe.html)            |
| **Render Texture (.renderTexture)**   | Off-screen render target for post-processing              | Visual/Shader Output | Unity Editor                     | [Render Textures](https://docs.unity3d.com/Manual/class-RenderTexture.html)                |
| **Post-Processing Profile (.asset)**  | Configures post-processing stack effects                  | Visual Effect Config | Post-processing package          | [Post-Processing](https://docs.unity3d.com/Manual/PostProcessingOverview.html)             |
| **Addressable Asset**                 | Runtime-loadable asset managed via Addressables           | Dynamic Content      | Addressables System              | [Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest)            |
| **Asset Bundle (.bundle)**            | Packed group of assets for runtime loading                | Distribution Package | Unity Build Pipeline             | [Asset Bundles](https://docs.unity3d.com/Manual/AssetBundlesIntro.html)                    |
| **Localization Table (.asset)**       | Stores translated text and audio                          | Localization Data    | Unity Localization               | [Localization](https://docs.unity3d.com/Packages/com.unity.localization@latest)            |
| **Compute Shader (.compute)**         | GPU code for data-parallel operations                     | Executable Shader    | HLSL Editor                      | [Compute Shaders](https://docs.unity3d.com/Manual/class-ComputeShader.html)                |
| **VFX Graph Asset (.vfx)**            | Defines visual effects using VFX Graph                    | Particle/VFX         | Visual Effect Graph              | [VFX Graph](https://docs.unity3d.com/Manual/VisualEffectGraph.html)                        |
| **Shader Graph (.shadergraph)**       | Node-based shader authoring                               | Visual Shader        | Shader Graph                     | [Shader Graph](https://docs.unity3d.com/Manual/shader-graph.html)                          |
| **Animator Override Controller**      | Customizes animation controller states                    | Animation Override   | Animator                         | [Animator Override](https://docs.unity3d.com/Manual/class-AnimatorOverrideController.html) |
| **Timeline Signal Asset**             | Sends events from Timelines                               | Event Trigger        | Timeline                         | [Signals](https://docs.unity3d.com/Manual/TimelineSignals.html)                            |
| **Lighting Probe Proxy Volume**       | Improves indirect lighting                                | Lighting Asset       | Lighting Window                  | [LPPV](https://docs.unity3d.com/Manual/LightProbeProxyVolumes.html)                        |

---

### **Specialized Asset Types**

| **Category**      | **Asset Types**                                | **Description**                            |
| ----------------- | ---------------------------------------------- | ------------------------------------------ |
| **UI & UX**       | Canvas, Button, EventSystem, Prefab Variants   | Interactive UI elements for menus and HUDs |
| **Networking**    | ScriptableObject configs, Addressables, Scenes | Data for multiplayer/network sync          |
| **Physics**       | Rigidbodies, Colliders, Joints                 | Runtime physical behavior                  |
| **AI**            | NavMesh, NavMeshAgent, NavMeshObstacle         | Navigation and pathfinding data            |
| **Visual FX**     | Particle System, Shader Graph, VFX Graph       | Particle and visual effect configurations  |
| **Editor Assets** | Custom Editor Windows, Gizmos, Editor Scripts  | Tools extending Unity Editor               |

---

Would you like me to generate a **downloadable CSV or Markdown reference file** with all of these asset types categorized by subsystem (Rendering, Audio, Physics, Scripting, UI, etc.) for integration into your game design documentation set?
