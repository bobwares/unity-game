# Unity Game Utilities

## Addressable Asset Loader
The `Assets/Scripts/Addressables/AddressableAssetLoader.cs` component centralizes the workflow for loading and releasing [Unity Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest) at runtime. Attach it to a scene object (for example, an empty `GameObject` named **Addressable Manager**) and call its asynchronous APIs from other scripts.

### Key Features
- **Asynchronous loading**: `LoadAssetAsync<T>` fetches an asset by its address, caches the handle, and throws descriptive exceptions when something goes wrong.
- **Prefab instantiation**: `InstantiateAsync` creates addressable prefabs with optional parent and world-space controls.
- **Handle tracking**: The component automatically keeps track of all loaded assets and instantiated prefabs so they can be safely released.
- **Automatic cleanup**: When the component is destroyed, it releases all tracked handles to prevent memory leaks.

### Example Usage
```csharp
using Assets.Scripts.Addressables;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private AddressableAssetLoader loader;

    private GameObject enemyInstance;

    private async void Start()
    {
        enemyInstance = await loader.InstantiateAsync("Enemies/BasicEnemy", transform);
        // Configure the spawned enemy here.
    }

    private void OnDestroy()
    {
        loader.ReleaseInstance(enemyInstance);
        loader.ReleaseAsset("Enemies/BasicEnemy");
    }
}
```

> **Tip:** The loader stores handles by the address string. Release assets and instances using the same address you used to load them, or call `ReleaseInstance` with the spawned `GameObject` when you are finished.
