﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if URP_OUTLINE && UNITY_2019_1_OR_NEWER
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering.Universal;
#else
using UnityEngine.Rendering.LWRP;
#endif
#endif

#if HDRP_OUTLINE
using UnityEngine.Rendering.HighDefinition;
#endif

namespace EPOOutline
{
    public static class PipelineFetcher
    {
#if UNITY_2019_1_OR_NEWER
        public static RenderPipelineAsset CurrentAsset
        {
            get
            {
#if UNITY_2019_3_OR_NEWER
                var pipeline = QualitySettings.renderPipeline;
                if (pipeline == null)
                    pipeline = GraphicsSettings.renderPipelineAsset;
#else
                    var pipeline = GraphicsSettings.renderPipelineAsset;
#endif

                return pipeline;
            }
        }
#endif
    }

#if (URP_OUTLINE || HDRP_OUTLINE) && UNITY_EDITOR && UNITY_2019_1_OR_NEWER
    public static class PipelineAssetUtility
    {
        public static RenderPipelineAsset CurrentAsset
        {
            get
            {
                return PipelineFetcher.CurrentAsset;
            }
        }

        public static HashSet<RenderPipelineAsset> ActiveAssets
        {
            get
            {
                var set = new HashSet<RenderPipelineAsset>();

                if (GraphicsSettings.renderPipelineAsset != null)
                    set.Add(GraphicsSettings.renderPipelineAsset);

#if UNITY_2019_3_OR_NEWER
                var qualitySettingNames = QualitySettings.names;
                for (var index = 0; index < qualitySettingNames.Length; index++)
                {
                    var assset = QualitySettings.GetRenderPipelineAssetAt(index);
                    if (assset == null)
                        continue;

                    set.Add(assset);
                }
#endif

                return set;
            }
        }

#if URP_OUTLINE
        public static object GetDefault(Type type)
        {
            if(type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
        
        public static RenderPipelineAsset CreateAsset(string path)
        {
            var method = typeof(UniversalRenderPipelineAsset)
                .GetMethod("CreateRendererAsset", BindingFlags.NonPublic | BindingFlags.Static);

            var methodParams = method.GetParameters();
            object[] parameters = new object[methodParams.Length];
            for (var index = 0; index < methodParams.Length; index++)
                parameters[index] = GetDefault(methodParams[index].ParameterType);

            var possibleParameters = new object[] {path + " renderer.asset", (RendererType)1, false, "Renderer"};
            for (var index = 0; index < Mathf.Min(possibleParameters.Length, parameters.Length); index++)
                parameters[index] = possibleParameters[index];
            
            var data = method.Invoke(null, parameters) as ScriptableRendererData;

            var pipeline = UniversalRenderPipelineAsset.Create(data);
            
            AssetDatabase.CreateAsset(pipeline, path + ".asset");

            return pipeline;
        }
#endif

#if HDRP_OUTLINE
        public static RenderPipelineAsset CreateHDRPAsset()
        {
            return ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
        }
#endif

        public static bool IsURPOrLWRP(RenderPipelineAsset asset)
        {
            return asset != null &&
                (asset.GetType().Name.Equals("LightweightRenderPipelineAsset") ||
                asset.GetType().Name.Equals("UniversalRenderPipelineAsset"));
        }

        public static bool IsHDRP(RenderPipelineAsset asset)
        {
            return asset != null &&
                asset.GetType().Name.Equals("HDRenderPipelineAsset");
        }

#if URP_OUTLINE
        public static ScriptableRendererData GetRenderer(RenderPipelineAsset asset)
        {
            using (var so = new SerializedObject(asset))
            {
                so.Update();

#if URP_OUTLINE
#if UNITY_2019_3_OR_NEWER
                var rendererDataList = so.FindProperty("m_RendererDataList");
                var assetIndex = so.FindProperty("m_DefaultRendererIndex");
                var item = rendererDataList.GetArrayElementAtIndex(assetIndex.intValue);

                return item.objectReferenceValue as ScriptableRendererData;
#else
                var rendererData = so.FindProperty("m_RendererData");
                return rendererData.objectReferenceValue as ScriptableRendererData;
#endif
#else
                return null;
#endif
            }
        }

        public static bool IsAssetContainsSRPOutlineFeature(RenderPipelineAsset asset)
        {
            var data = GetRenderer(asset);

            return data.rendererFeatures.Find(x => x is URPOutlineFeature) != null;
        }
#endif
    }
#endif
}