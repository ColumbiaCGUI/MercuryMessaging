using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace EPOOutline
{
    [Flags]
    public enum ColorMask
    {
        None = 0,
        R = 1,
        G = 2,
        B = 4,
        A = 8
    }

    [System.Serializable]
    public class OutlineTarget
    {
        public bool IsVisible = false;

        [SerializeField]
        public ColorMask CutoutMask = ColorMask.A;

        [SerializeField]
        private float edgeDilateAmount = 5.0f;

        [SerializeField]
        private float frontEdgeDilateAmount = 5.0f;

        [SerializeField]
        private float backEdgeDilateAmount = 5.0f;

        [SerializeField]
        [FormerlySerializedAs("Renderer")]
        public Renderer renderer;

        [SerializeField]
        public int SubmeshIndex;

        [SerializeField]
        public BoundsMode BoundsMode = BoundsMode.Default;

        [SerializeField]
        public Bounds Bounds = new Bounds(Vector3.zero, Vector3.one);

        [SerializeField]
        [Range(0.0f, 1.0f)]
        public float CutoutThreshold = 0.5f;

        [SerializeField]
        public CullMode CullMode;

        [SerializeField]
        private string cutoutTextureName;

        [SerializeField]
        public DilateRenderMode DilateRenderingMode;

        [SerializeField]
        private int cutoutTextureIndex;
        
        private int? cutoutTextureId;
        
        public Renderer Renderer
        {
            get
            {
                return renderer;
            }
        }

        public bool UsesCutout
        {
            get
            {
                return !string.IsNullOrEmpty(cutoutTextureName);
            }
        }

        public int CutoutTextureIndex
        {
            get
            {
                return cutoutTextureIndex;
            }

            set
            {
                cutoutTextureIndex = value;
                if (cutoutTextureIndex < 0)
                {
                    Debug.LogError("Trying to set cutout texture index less than zero");
                    cutoutTextureIndex = 0;
                }
            }
        }
        
        public int ShiftedSubmeshIndex
        {
            get
            {
                return SubmeshIndex;
            }
        }

        public int CutoutTextureId
        {
            get
            {
                if (!cutoutTextureId.HasValue)
                    cutoutTextureId = Shader.PropertyToID(cutoutTextureName);

                return cutoutTextureId.Value;
            }
        }

        public string CutoutTextureName
        {
            get
            {
                return cutoutTextureName;
            }

            set
            {
                cutoutTextureName = value;
                cutoutTextureId = null;
            }
        }

        public float EdgeDilateAmount
        {
            get
            {
                return edgeDilateAmount;
            }

            set
            {
                if (value < 0)
                    edgeDilateAmount = 0;
                else
                    edgeDilateAmount = value;
            }
        }

        public float FrontEdgeDilateAmount
        {
            get
            {
                return frontEdgeDilateAmount;
            }

            set
            {
                if (value < 0)
                    frontEdgeDilateAmount = 0;
                else
                    frontEdgeDilateAmount = value;
            }
        }

        public float BackEdgeDilateAmount
        {
            get
            {
                return backEdgeDilateAmount;
            }

            set
            {
                if (value < 0)
                    backEdgeDilateAmount = 0;
                else
                    backEdgeDilateAmount = value;
            }
        }

        public OutlineTarget()
        {

        }

        public OutlineTarget(Renderer renderer, int submesh = 0)
        {
            SubmeshIndex = submesh;
            this.renderer = renderer;

            CutoutThreshold = 0.5f;
            cutoutTextureId = null;
            cutoutTextureName = string.Empty;
            CullMode = renderer is SpriteRenderer ? CullMode.Off : CullMode.Back;
            DilateRenderingMode = DilateRenderMode.PostProcessing;
            frontEdgeDilateAmount = 5.0f;
            backEdgeDilateAmount = 5.0f;
            edgeDilateAmount = 5.0f;
        }

        public OutlineTarget(Renderer renderer, string cutoutTextureName, float cutoutThreshold = 0.5f)
        {
            SubmeshIndex = 0;
            this.renderer = renderer;

            cutoutTextureId = Shader.PropertyToID(cutoutTextureName);
            CutoutThreshold = cutoutThreshold;
            this.cutoutTextureName = cutoutTextureName;
            CullMode = renderer is SpriteRenderer ? CullMode.Off : CullMode.Back;
            DilateRenderingMode = DilateRenderMode.PostProcessing;
            frontEdgeDilateAmount = 5.0f;
            backEdgeDilateAmount = 5.0f;
            edgeDilateAmount = 5.0f;
        }
    }
}