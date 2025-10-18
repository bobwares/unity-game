# Unity C# Tutorial

## Overview
This tutorial walks you through writing C# scripts for Unity, from setting up your development environment to deploying gameplay features using Unity's component-based architecture. Follow the steps in order the first time, then revisit the reference sections as needed when you build your own game systems.

## 1. Install Prerequisites
1. **Unity Hub & Editor**
   - Install [Unity Hub](https://unity.com/download).
   - Inside Unity Hub, add an LTS (Long-Term Support) Unity Editor version, e.g., *2022 LTS*.
2. **.NET SDK (optional but recommended)**
   - Unity bundles a C# compiler, but installing the [latest .NET SDK](https://dotnet.microsoft.com/download/dotnet) gives you command-line tools (`dotnet-format`, analyzers) that integrate with IDEs.
3. **IDE or Code Editor**
   - **Visual Studio** (Windows/macOS) – full debugger and Unity integration.
   - **Rider** or **Visual Studio Code** – lightweight options with Unity support plugins.

## 2. Create or Open a Unity Project
1. Launch Unity Hub and click **New Project**.
2. Choose a template (e.g., *3D (URP)* or *2D*), set the project location, and click **Create**.
3. After Unity loads, note the default folders: `Assets/`, `Packages/`, and `ProjectSettings/`.

## 3. Understand Unity's C# Script Structure
Unity scripts are standard C# classes that inherit from `MonoBehaviour`. Unity instantiates them as components attached to `GameObject`s.

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Serialized fields appear in the Inspector.
    [SerializeField]
    private float moveSpeed = 5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new(horizontal, 0f, vertical);
        rb.MovePosition(transform.position + direction * moveSpeed * Time.deltaTime);
    }
}
```

### Lifecycle Methods
- `Awake` – initialization that runs once when the object loads.
- `Start` – initialization after all `Awake` calls complete.
- `Update` – called every frame; use it for polling input or non-physics logic.
- `FixedUpdate` – called on a fixed timestep; use for physics interactions.
- `OnEnable` / `OnDisable` – register and unregister callbacks.

## 4. Create and Attach Scripts
1. In the **Project** window, right-click inside the `Assets/Scripts/` folder and choose **Create ▸ C# Script**.
2. Name the script (e.g., `PlayerController`). Unity will generate a `.cs` file with a class matching the filename.
3. Double-click the script to open it in your IDE. Unity automatically regenerates `.csproj` files when scripts change.
4. Drag the script onto a `GameObject` in the **Hierarchy** to attach it as a component.

> **Tip:** Keep scripts organized by feature. Create subfolders such as `Assets/Scripts/Gameplay/` and `Assets/Scripts/UI/` to avoid clutter.

## 5. Work with Serialized Fields and the Inspector
- Mark private fields with `[SerializeField]` to expose them in the Inspector without making them public.
- Use `public` fields for quick prototyping, but prefer serialized fields plus properties for clean encapsulation.
- Use `[Range(min, max)]` attributes to create sliders and `[Tooltip("...")]` to document parameters directly in the Inspector.

```csharp
[SerializeField, Range(0.5f, 10f)]
private float jumpForce = 4f;
```

## 6. Communicate Between Components
### Get References
- `GetComponent<T>()` finds components on the same GameObject.
- `GetComponentInChildren<T>()` and `GetComponentInParent<T>()` search related objects.
- Drag-and-drop references in the Inspector to avoid expensive runtime lookups for frequently used objects.

### Events and Messaging
- Use UnityEvents or C# events for decoupling systems.
- For global signals, consider `ScriptableObject`-based event channels.

```csharp
using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public event Action<float> Damaged;

    public void ApplyDamage(float amount)
    {
        Damaged?.Invoke(amount);
    }
}
```

## 7. Work with Prefabs and Addressables
1. Convert reusable GameObjects into prefabs by dragging them from the **Hierarchy** into the `Assets/Prefabs/` folder.
2. Instantiate prefabs at runtime using `Instantiate(prefab, position, rotation);`.
3. For scalable content delivery, mark prefabs or assets as Addressables and load them asynchronously using systems like `AddressableAssetLoader` from this repository.

## 8. Read and Write Data
- Use `ScriptableObject` assets for configuration data that ships with the game build.
- Use `JsonUtility` or third-party serializers to load/save runtime data (player progress, settings).
- Unity's persistent data path: `Application.persistentDataPath`.

```csharp
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int level;
    public int coins;
}

public static class SaveSystem
{
    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, "save.json");
        File.WriteAllText(path, json);
    }
}
```

## 9. Debug and Profile
- `Debug.Log`, `Debug.LogWarning`, and `Debug.LogError` output to the Console window.
- Use breakpoints in your IDE to inspect variables at runtime.
- The Unity Profiler (Window ▸ Analysis ▸ Profiler) helps identify CPU/GPU bottlenecks.
- Enable Deep Profiling when diagnosing complex behavior, but disable it for regular play mode due to overhead.

## 10. Build and Deploy
1. Open **File ▸ Build Settings...** and add scenes to the **Scenes In Build** list.
2. Choose the target platform (PC, Android, iOS, WebGL) and install any missing modules through Unity Hub.
3. Click **Build** (or **Build and Run**) to produce the final player.
4. Test on target hardware to verify input, performance, and platform-specific settings.

## Additional Resources
- [Unity Learn](https://learn.unity.com/) – official tutorials and projects.
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/) – authoritative API documentation.
- [Community Forums](https://forum.unity.com/) and [Unity Answers](https://answers.unity.com/) – ask questions and learn from others.

## Next Steps
- Explore coroutines (`StartCoroutine`) for timed sequences.
- Investigate the new Input System package for modern input mappings.
- Learn about Scriptable Render Pipelines (URP/HDRP) to tailor visuals to your project.

By following these steps, you can confidently write C# scripts that take advantage of Unity's component system, editor tooling, and deployment pipeline.
