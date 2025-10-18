/**
 * App: Unity Game
 * Package: Assets.Scripts.Addressables
 * File: AddressableAssetLoader.cs
 * Version: 0.1.0
 * Turns: 1
 * Author: gpt-5-codex
 * Date: 2025-10-18T02:57:22Z
 * Exports: AddressableAssetLoader
 * Description: Provides a reusable component for loading, instantiating, and releasing
 *              Unity Addressable assets at runtime. Each public method documents the
 *              required addressable workflow for safe asset management.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.Addressables
{
    /// <summary>
    /// Component that centralizes asynchronous loading and releasing of Unity Addressable assets.
    /// Attach this script to a scene object to manage the lifecycle of dynamically loaded content.
    /// </summary>
    public class AddressableAssetLoader : MonoBehaviour
    {
        private readonly Dictionary<string, AsyncOperationHandle> _assetHandles = new();
        private readonly HashSet<AsyncOperationHandle<GameObject>> _instantiatedHandles = new();

        /// <summary>
        /// Asynchronously loads an addressable asset by its address and caches the handle for reuse.
        /// </summary>
        /// <typeparam name="T">The expected Unity object type (e.g., Texture2D, AudioClip).</typeparam>
        /// <param name="address">The address string defined in the Addressables window.</param>
        /// <returns>The loaded asset.</returns>
        /// <exception cref="ArgumentException">Thrown when the address is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the load operation fails.</exception>
        public async Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address must be a non-empty string.", nameof(address));
            }

            if (_assetHandles.TryGetValue(address, out var cachedHandle))
            {
                return (T)cachedHandle.Result;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
                throw new InvalidOperationException($"Failed to load addressable asset at '{address}'.");
            }

            _assetHandles[address] = handle;
            return handle.Result;
        }

        /// <summary>
        /// Instantiates an addressable prefab asynchronously and tracks the handle for later release.
        /// </summary>
        /// <param name="address">The prefab address.</param>
        /// <param name="parent">Optional parent transform for the instance.</param>
        /// <param name="instantiateInWorldSpace">Whether to keep world position when parenting.</param>
        /// <returns>The instantiated <see cref="GameObject"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the address is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when instantiation fails.</exception>
        public async Task<GameObject> InstantiateAsync(
            string address,
            Transform parent = null,
            bool instantiateInWorldSpace = false)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address must be a non-empty string.", nameof(address));
            }

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(
                address,
                parent,
                instantiateInWorldSpace);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
                throw new InvalidOperationException($"Failed to instantiate addressable prefab at '{address}'.");
            }

            _instantiatedHandles.Add(handle);
            return handle.Result;
        }

        /// <summary>
        /// Releases a previously loaded asset handle associated with the provided address.
        /// </summary>
        /// <param name="address">The address used to load the asset.</param>
        /// <returns>True if the handle was released; otherwise false.</returns>
        public bool ReleaseAsset(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            if (_assetHandles.TryGetValue(address, out var handle))
            {
                Addressables.Release(handle);
                _assetHandles.Remove(address);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Releases an instantiated prefab and its addressable handle.
        /// </summary>
        /// <param name="instance">The instantiated <see cref="GameObject"/>.</param>
        /// <returns>True if the instance was tracked and released; otherwise false.</returns>
        public bool ReleaseInstance(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            AsyncOperationHandle<GameObject>? handleToRemove = null;

            foreach (var handle in _instantiatedHandles)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result == instance)
                {
                    handleToRemove = handle;
                    break;
                }
            }

            Addressables.ReleaseInstance(instance);

            if (handleToRemove.HasValue)
            {
                _instantiatedHandles.Remove(handleToRemove.Value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ensures that all cached handles are released when the component is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            foreach (var handle in _assetHandles.Values)
            {
                Addressables.Release(handle);
            }

            _assetHandles.Clear();

            foreach (var handle in _instantiatedHandles)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    Addressables.ReleaseInstance(handle.Result);
                }
                else
                {
                    Addressables.Release(handle);
                }
            }

            _instantiatedHandles.Clear();
        }
    }
}
