using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MercuryMessaging.Support.ThirdParty
{
    /// <summary>
	/// Custom scripting define symbols used by MercuryMessaging
	/// This class has been adapted from Photon's PhotonEditorUtils.cs
    /// 
	/// </summary>
    [InitializeOnLoad]
    public static class MercuryThirdPartyUtils
    {
        // True if Photon Pun 2 API is available
        public static bool HasPun;

        static MercuryThirdPartyUtils()
        {
            HasPun = Type.GetType("Photon.Pun.PhotonNetwork, Assembly-CSharp") != null || Type.GetType("Photon.Pun.PhotonNetwork, Assembly-CSharp-firstpass") != null || Type.GetType("Photon.Pun.PhotonNetwork, PhotonUnityNetworking") != null;

            if (HasPun)
            {
                // MOUNTING SYMBOLS
                #if !PHOTON_AVAILABLE
                AddScriptingDefineSymbolToAllBuildTargetGroups("PHOTON_AVAILABLE");
                #endif
            }
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

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToList();

                if (!defineSymbols.Contains(defineSymbol))
                {
                    defineSymbols.Add(defineSymbol);

                    try
                    {
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defineSymbols.ToArray()));
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Could not set Mercury " + defineSymbol + " defines for build target: " + target + " group: " + group + " " + e);
                    }
                }
            }
        }

        /// <summary>
        /// Removes MercuryMessaging's Script Define Symbols from project
        /// </summary>
        public static void CleanUpMercuryDefineSymbols()
        {
            foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);

                if (group == BuildTargetGroup.Unknown)
                {
                    continue;
                }

                var defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group)
                    .Split(';')
                    .Select(d => d.Trim())
                    .ToList();

                List<string> newDefineSymbols = new List<string>();
                foreach (var symbol in defineSymbols)
                {
                    if ("PHOTON_AVAILABLE".Equals(symbol))
                    {
                        continue;
                    }

                    newDefineSymbols.Add(symbol);
                }

                try
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", newDefineSymbols.ToArray()));
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Could not set clean up Mercury's define symbols for build target: {0} group: {1}, {2}", target, group, e);
                }
            }
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