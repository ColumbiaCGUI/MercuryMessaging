using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace MercuryMessaging.Editor
{
    /// <summary>
    /// Custom scripting define symbols used by MercuryMessaging.
    ///
    /// Supported networking backends:
    /// - FishNet (uses FISH_NET define)
    /// - Photon Fusion 2 (uses FUSION_WEAVER define)
    ///
    /// Note: PUN2 support was removed in 2025-12. Use FishNet or Fusion 2 instead.
    /// </summary>
    [InitializeOnLoad]
    public static class MercuryThirdPartyUtils
    {
        static MercuryThirdPartyUtils()
        {
            // Auto-detection for third-party integrations can be added here
            // Currently, FishNet and Fusion 2 manage their own define symbols
        }

        /// <summary>
        /// Adds a given scripting define symbol to all build target groups
        /// You can see all scripting define symbols ( not the internal ones, only the one for this project), in the PlayerSettings inspector
        /// </summary>
        /// <param name="defineSymbol">Define symbol.</param>
        public static void AddScriptingDefineSymbolToAllBuildTargetGroups(string defineSymbol)
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
                var defineSymbols = PlayerSettings.GetScriptingDefineSymbols(namedTarget).Split(';').Select(d => d.Trim()).ToList();

                if (!defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Add(defineSymbol);

                    try
                    {
                        PlayerSettings.SetScriptingDefineSymbols(namedTarget, string.Join(";", defineSymbols.ToArray()));
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Could not set Mercury " + defineSymbol + " defines for build target: " + target + " group: " + group + " " + e);
                    }
                }
            }
        }

        /// <summary>
        /// Removes MercuryMessaging's Script Define Symbols from project.
        /// Currently a no-op since supported backends (FishNet, Fusion 2) manage their own symbols.
        /// </summary>
        public static void CleanUpMercuryDefineSymbols()
        {
            // No custom define symbols to clean up
            // FishNet and Fusion 2 manage their own define symbols
        }
    }

    public class CleanUpDefinesOnMercuryDelete : UnityEditor.AssetModificationProcessor
    {
        public static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions rao)
        {
            if ("Assets/MercuryMessaging".Equals(assetPath))
            {
                MercuryThirdPartyUtils.CleanUpMercuryDefineSymbols();
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}