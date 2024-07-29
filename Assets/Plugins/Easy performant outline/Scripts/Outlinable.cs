﻿using System.Collections.Generic;
using UnityEngine;
using System;

namespace EPOOutline
{
    public enum DilateRenderMode
    {
        PostProcessing,
        EdgeShift
    }

    public enum RenderStyle
    {
        Single = 1,
        FrontBack = 2
    }

    [Flags]
    public enum OutlinableDrawingMode
    {
        Normal = 1,
        ZOnly = 2,
        GenericMask = 4,
        Obstacle = 8,
        Mask = 16
    }

    [Flags]
    public enum RenderersAddingMode
    {
        All = -1,
        None = 0,
        MeshRenderer = 1,
        SkinnedMeshRenderer = 2,
        SpriteRenderer = 4,
        Others = 4096
    }

    public enum BoundsMode
    {
        Default,
        ForceRecalculate,
        Manual
    }

    public enum ComplexMaskingMode
    {
        None,
        ObstaclesMode,
        MaskingMode
    }

    [ExecuteAlways]
    public class Outlinable : MonoBehaviour
    {
        private static List<TargetStateListener> tempListeners = new List<TargetStateListener>();
        
        private static HashSet<Outlinable> outlinables = new HashSet<Outlinable>();

        [System.Serializable]
        public class OutlineProperties
        {
#pragma warning disable CS0649
            [SerializeField]
            private bool enabled = true;

            public bool Enabled
            {
                get
                {
                    return enabled;
                }

                set
                {
                    enabled = value;
                }
            }

            [SerializeField]
            private Color color = Color.yellow;

            public Color Color
            {
                get
                {
                    return color;
                }

                set
                {
                    color = value;
                }
            }

            [SerializeField]
            [Range(0.0f, 1.0f)]
            private float dilateShift = 1.0f;

            public float DilateShift
            {
                get
                {
                    return dilateShift;
                }

                set
                {
                    dilateShift = value;
                }
            }

            [SerializeField]
            [Range(0.0f, 1.0f)]
            private float blurShift = 1.0f;

            public float BlurShift
            {
                get
                {
                    return blurShift;
                }

                set
                {
                    blurShift = value;
                }
            }

            [SerializeField, SerializedPassInfo("Fill style", "Hidden/EPO/Fill/")]
            private SerializedPass fillPass = new SerializedPass();

            public SerializedPass FillPass
            {
                get
                {
                    return fillPass;
                }
            }
#pragma warning restore CS0649
        }

        [SerializeField]
        private ComplexMaskingMode complexMaskingMode;
        
        [SerializeField]
        private OutlinableDrawingMode drawingMode = OutlinableDrawingMode.Normal;

        [SerializeField]
        private int outlineLayer = 0;

        [SerializeField]
        private List<OutlineTarget> outlineTargets = new List<OutlineTarget>();

        [SerializeField]
        private RenderStyle renderStyle = RenderStyle.Single;

#pragma warning disable CS0649
        [SerializeField]
        private OutlineProperties outlineParameters = new OutlineProperties();

        [SerializeField]
        private OutlineProperties backParameters = new OutlineProperties();

        [SerializeField]
        private OutlineProperties frontParameters = new OutlineProperties();

        private bool shouldValidateTargets = false;
        
#pragma warning restore CS0649

        public RenderStyle RenderStyle
        {
            get
            {
                return renderStyle;
            }

            set
            {
                renderStyle = value;
            }
        }

        public ComplexMaskingMode ComplexMaskingMode
        {
            get
            {
                return complexMaskingMode;
            }

            set
            {
                complexMaskingMode = value;
            }
        }

        public bool ComplexMaskingEnabled
        {
            get
            {
                return complexMaskingMode != ComplexMaskingMode.None;
            }
        }

        public OutlinableDrawingMode DrawingMode
        {
            get
            {
                return drawingMode;
            }

            set
            {
                drawingMode = value;
            }
        }

        public int OutlineLayer
        {
            get
            {
                return outlineLayer;
            }

            set
            {
                outlineLayer = value;
            }
        }

        public IReadOnlyList<OutlineTarget> OutlineTargets
        {
            get
            {
                return outlineTargets;
            }
        }

        public OutlineProperties OutlineParameters
        {
            get
            {
                return outlineParameters;
            }
        }

        public OutlineProperties BackParameters
        {
            get
            {
                return backParameters;
            }
        }
        
        public bool NeedFillMask
        {
            get
            {
                if ((drawingMode & OutlinableDrawingMode.Normal) == 0)
                    return false;

                if (renderStyle == RenderStyle.FrontBack)
                    return (frontParameters.Enabled || backParameters.Enabled) && (frontParameters.FillPass.Material != null || backParameters.FillPass.Material != null);
                else
                    return false;
            }
        }

        public OutlineProperties FrontParameters
        {
            get
            {
                return frontParameters;
            }
        }

        public bool IsObstacle
        {
            get
            {
                return (drawingMode & OutlinableDrawingMode.Obstacle) != 0;
            }
        }

        public bool TryAddTarget(OutlineTarget target)
        {
            outlineTargets.Add(target);
            ValidateTargets();

            return true;
        }

        public void RemoveTarget(OutlineTarget target)
        {
            outlineTargets.Remove(target);
            if (target.renderer != null)
            {
                var listener = target.renderer.GetComponent<TargetStateListener>();
                if (listener == null)
                    return;
                
                listener.RemoveCallback(this, UpdateVisibility);
            }
        }
        
        public OutlineTarget this[int index]
        {
            get
            {
                return outlineTargets[index];
            }

            set
            {
                outlineTargets[index] = value;
                ValidateTargets();
            }
        }

        private void Reset()
        {
            AddAllChildRenderersToRenderingList(RenderersAddingMode.SkinnedMeshRenderer | RenderersAddingMode.MeshRenderer | RenderersAddingMode.SpriteRenderer);
        }

        private void OnValidate()
        {
            outlineLayer = Mathf.Clamp(outlineLayer, 0, 63);
            shouldValidateTargets = true;
        }

        private void SubscribeToVisibilityChange(GameObject go)
        {
            var listener = go.GetComponent<TargetStateListener>();
            if (listener == null)
            {
                listener = go.AddComponent<TargetStateListener>();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(listener);
                UnityEditor.EditorUtility.SetDirty(go);
#endif
            }

            listener.RemoveCallback(this, UpdateVisibility);
            listener.AddCallback(this, UpdateVisibility);

            listener.ForceUpdate();
        }

        private void UpdateVisibility()
        {
            if (!enabled)
            {
                outlinables.Remove(this);
                return;
            }

            outlineTargets.RemoveAll(x => x.renderer == null);
            foreach (var target in OutlineTargets)
                target.IsVisible = target.renderer.isVisible;

            outlineTargets.RemoveAll(x => x.renderer == null);

            foreach (var target in outlineTargets)
            {
                if (target.IsVisible)
                {
                    outlinables.Add(this);
                    return;
                }
            }

            outlinables.Remove(this);
        }

        private void OnEnable()
        {
            UpdateVisibility();
        }

        private void OnDisable()
        {
            outlinables.Remove(this);
        }

        private void Awake()
        {
            ValidateTargets();
        }

        private void ValidateTargets()
        {
            outlineTargets.RemoveAll(x => x.renderer == null);
            foreach (var target in outlineTargets)
                SubscribeToVisibilityChange(target.renderer.gameObject);
        }

        private void OnDestroy()
        {
            outlinables.Remove(this);
        }
        
        public static void GetAllActiveOutlinables(Camera camera, List<Outlinable> outlinablesList)
        {
            outlinablesList.Clear();
            foreach (var outlinable in outlinables)
                outlinablesList.Add(outlinable);
        }

        private int GetSubmeshCount(Renderer renderer)
        {
            if (renderer is MeshRenderer)
                return renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount;
            else if (renderer is SkinnedMeshRenderer)
                return (renderer as SkinnedMeshRenderer).sharedMesh.subMeshCount;
            else
                return 1;
        }

        public void AddAllChildRenderersToRenderingList(RenderersAddingMode renderersAddingMode = RenderersAddingMode.All)
        {
            outlineTargets.Clear();
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                if (!MatchingMode(renderer, renderersAddingMode))
                    continue;

                var submeshesCount = GetSubmeshCount(renderer);
                for (var index = 0; index < submeshesCount; index++)
                    TryAddTarget(new OutlineTarget(renderer, index));
            }
        }

        private void Update()
        {
            if (!shouldValidateTargets)
                return;

            shouldValidateTargets = false;
            ValidateTargets();
        }

        private bool MatchingMode(Renderer renderer, RenderersAddingMode mode)
        {
            return 
                (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer) && !(renderer is SpriteRenderer) && (mode & RenderersAddingMode.Others) != RenderersAddingMode.None) ||
                (renderer is MeshRenderer && (mode & RenderersAddingMode.MeshRenderer) != RenderersAddingMode.None) ||
                (renderer is SpriteRenderer && (mode & RenderersAddingMode.SpriteRenderer) != RenderersAddingMode.None) ||
                (renderer is SkinnedMeshRenderer && (mode & RenderersAddingMode.SkinnedMeshRenderer) != RenderersAddingMode.None);
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            foreach (var target in outlineTargets)
            {
                if (target.Renderer == null || target.BoundsMode != BoundsMode.Manual)
                    continue;

                Gizmos.matrix = target.Renderer.transform.localToWorldMatrix;

                Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 0.2f);
                var size = target.Bounds.size;
                var scale = target.Renderer.transform.localScale;
                size.x /= scale.x;
                size.y /= scale.y;
                size.z /= scale.z;

                Gizmos.DrawCube(target.Bounds.center, size);
                Gizmos.DrawWireCube(target.Bounds.center, size);
            }
        }
#endif
    }
}