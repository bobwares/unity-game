using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides a simple API for loading assets stored under the Resources folder.
/// The loader keeps a cache of instantiated assets so that repeated calls are fast
/// and allows unloading of assets when they are no longer needed.
/// </summary>
public class AssetLoader : MonoBehaviour
{
    private static AssetLoader _instance;
    private readonly Dictionary<string, UnityEngine.Object> _cache = new();

    /// <summary>
    /// Ensures a single AssetLoader exists in the scene.
    /// If none is present a new GameObject will be created automatically.
    /// </summary>
    public static AssetLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject(nameof(AssetLoader));
                _instance = go.AddComponent<AssetLoader>();
                DontDestroyOnLoad(go);
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Loads an asset synchronously from the Resources folder.
    /// </summary>
    /// <typeparam name="T">Type of the asset to load.</typeparam>
    /// <param name="resourcePath">Relative path to the asset inside the Resources folder.</param>
    /// <returns>The loaded asset instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the path is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the asset cannot be found.</exception>
    public T Load<T>(string resourcePath) where T : UnityEngine.Object
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            throw new ArgumentException("Resource path cannot be null or empty.", nameof(resourcePath));
        }

        if (_cache.TryGetValue(resourcePath, out var cachedAsset))
        {
            return cachedAsset as T;
        }

        var asset = Resources.Load<T>(resourcePath);
        if (asset == null)
        {
            throw new InvalidOperationException($"Failed to load asset at path '{resourcePath}'.");
        }

        _cache[resourcePath] = asset;
        return asset;
    }

    /// <summary>
    /// Asynchronously loads an asset from the Resources folder.
    /// </summary>
    /// <typeparam name="T">Type of the asset to load.</typeparam>
    /// <param name="resourcePath">Relative path to the asset inside the Resources folder.</param>
    /// <param name="onLoaded">Callback invoked when the asset is available.</param>
    public void LoadAsync<T>(string resourcePath, Action<T> onLoaded) where T : UnityEngine.Object
    {
        if (onLoaded == null)
        {
            throw new ArgumentNullException(nameof(onLoaded));
        }

        StartCoroutine(LoadCoroutine(resourcePath, onLoaded));
    }

    private IEnumerator LoadCoroutine<T>(string resourcePath, Action<T> onLoaded) where T : UnityEngine.Object
    {
        if (_cache.TryGetValue(resourcePath, out var cachedAsset))
        {
            onLoaded?.Invoke(cachedAsset as T);
            yield break;
        }

        ResourceRequest request = Resources.LoadAsync<T>(resourcePath);
        yield return request;

        if (request.asset == null)
        {
            Debug.LogError($"Failed to load asset at path '{resourcePath}'.");
            onLoaded?.Invoke(null);
            yield break;
        }

        _cache[resourcePath] = request.asset;
        onLoaded?.Invoke(request.asset as T);
    }

    /// <summary>
    /// Removes the cached asset and optionally unloads it from memory.
    /// </summary>
    /// <param name="resourcePath">Relative path to the asset inside the Resources folder.</param>
    /// <param name="unload">
    /// If true, the asset is also unloaded from memory. Use this for large assets like textures or audio.
    /// </param>
    public void Unload(string resourcePath, bool unload = false)
    {
        if (_cache.TryGetValue(resourcePath, out var cachedAsset))
        {
            if (unload)
            {
                Resources.UnloadAsset(cachedAsset);
            }

            _cache.Remove(resourcePath);
        }
    }

    /// <summary>
    /// Clears all cached assets, optionally unloading them from memory.
    /// </summary>
    /// <param name="unload">If true, unloads all cached assets from memory.</param>
    public void ClearCache(bool unload = false)
    {
        if (unload)
        {
            foreach (var asset in _cache.Values)
            {
                Resources.UnloadAsset(asset);
            }
        }

        _cache.Clear();
    }
}
