# Unity CLI (iOS AssetBundles)

Run this from your Mac. It targets iOS and points at your project path.

```bash
/Applications/Unity/Hub/Editor/6000.2.8f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "/Users/bobware/projects/unity-game" \
  -executeMethod BuildCI.BuildAssetBundles \
  -buildTarget iOS \
  -logFile "/Users/bobware/projects/unity-game/Logs/build-assetbundles-iOS.log" \
  --bundleOutput="/Users/bobware/projects/unity-game/Build/AssetBundles/iOS" \
  --bundleCompression="LZ4"
```

## Build script (C#) and placement

Create the file below at: `Assets/Editor/BuildCI.cs`

```csharp
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildCI
{
    public static void BuildAssetBundles()
    {
        var args = Environment.GetCommandLineArgs();

        string Arg(string name)
        {
            var idx = Array.IndexOf(args, name);
            return idx >= 0 && idx + 1 < args.Length ? args[idx + 1] : null;
        }

        string output = Arg("--bundleOutput") ?? Path.Combine(Environment.CurrentDirectory, "Build/AssetBundles/iOS");
        string compression = Arg("--bundleCompression") ?? "LZ4";
        string targetStr = Arg("-buildTarget") ?? "iOS";

        if (!Enum.TryParse(targetStr, true, out BuildTarget target))
            target = BuildTarget.iOS;

        Directory.CreateDirectory(output);

        BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle;
        switch (compression.ToUpperInvariant())
        {
            case "LZ4": options |= BuildAssetBundleOptions.ChunkBasedCompression; break;
            case "UNCOMPRESSED": options |= BuildAssetBundleOptions.UncompressedAssetBundle; break;
            case "LZMA": break;
            default: options |= BuildAssetBundleOptions.ChunkBasedCompression; break;
        }

        try
        {
            var manifest = BuildPipeline.BuildAssetBundles(output, options, target);
            if (manifest == null)
                throw new Exception("BuildAssetBundles returned null.");
            Debug.Log($"AssetBundles built to: {output}");
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            EditorApplication.Exit(1);
        }
    }
}
```

## Optional: shell wrapper and placement

Create an executable script at `scripts/build_assetbundles_ios.sh`:

```bash
#!/usr/bin/env bash
set -euo pipefail

/Applications/Unity/Hub/Editor/6000.2.8f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "/Users/bobware/projects/unity-game" \
  -executeMethod BuildCI.BuildAssetBundles \
  -buildTarget iOS \
  -logFile "/Users/bobware/projects/unity-game/Logs/build-assetbundles-iOS.log" \
  --bundleOutput="/Users/bobware/projects/unity-game/Build/AssetBundles/iOS" \
  --bundleCompression="LZ4"
```

Make it executable:

```bash
chmod +x scripts/build_assetbundles_ios.sh
```

## Notes (concise)

* Place the C# file under `Assets/Editor` so Unity finds the editor code.
* Ensure your assets have AssetBundle names set in the Inspector; this script builds all named bundles for the specified target.
* `-buildTarget iOS` ensures platform-specific import and compression settings suitable for iOS.
