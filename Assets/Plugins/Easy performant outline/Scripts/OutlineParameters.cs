using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EPOOutline
{
    public class MeshPool
    {
        private Queue<Mesh> freeMeshes = new Queue<Mesh>();

        private List<Mesh> allMeshes = new List<Mesh>();

        public Mesh AllocateMesh()
        {
            while (freeMeshes.Count > 0 && freeMeshes.Peek() == null)
                freeMeshes.Dequeue();

            if (freeMeshes.Count == 0)
            {
                var mesh = new Mesh();
                mesh.MarkDynamic();
                allMeshes.Add(mesh);
                freeMeshes.Enqueue(mesh);
            }

            return freeMeshes.Dequeue();
        }

        public void ReleaseAllMeshes()
        {
            freeMeshes.Clear();
            foreach (var mesh in allMeshes)
                freeMeshes.Enqueue(mesh);
        }
    }

    public class OutlineParameters
    {
        public readonly MeshPool MeshPool = new MeshPool();

        public Camera Camera;
        public RenderTargetIdentifier Target;
        public RenderTargetIdentifier DepthTarget;
        public CommandBuffer Buffer;
        public DilateQuality DilateQuality = DilateQuality.Base;
        public int DilateIterations = 2;
        public int BlurIterations = 5;

        public Vector2 Scale = Vector2.one;

        public Rect? CustomViewport;

        public long OutlineLayerMask = -1;

        public int TargetWidth;
        public int TargetHeight;

        public float BlurShift = 1.0f;

        public float DilateShift = 1.0f;

        public bool UseHDR;

        public bool UseInfoBuffer = false;

        public bool IsEditorCamera;

        public BufferSizeMode PrimaryBufferSizeMode;
        public int PrimaryBufferSizeReference;

        public float PrimaryBufferScale = 0.1f;

        public StereoTargetEyeMask EyeMask;

        public int Antialiasing = 1;

        public BlurType BlurType = BlurType.Gaussian13x13;

        public LayerMask Mask = -1;

        public Mesh BlitMesh;

        public List<Outlinable> OutlinablesToRender = new List<Outlinable>();

        private bool isInitialized = false;
        
        public Vector2Int MakeScaledVector(int x, int y)
        {
            var fx = (float)x;
            var fy = (float)y;

            return new Vector2Int(Mathf.FloorToInt(fx * Scale.x), Mathf.FloorToInt(fy * Scale.y));
        }

        public void CheckInitialization()
        {
            if (isInitialized)
                return;

            Buffer = new CommandBuffer();

            isInitialized = true;
        }

        public void Prepare()
        {
            if (OutlinablesToRender.Count == 0)
                return;
            
            UseInfoBuffer = OutlinablesToRender.Find(x => x != null && ((x.DrawingMode & (OutlinableDrawingMode.Obstacle | OutlinableDrawingMode.Mask)) != 0 || x.ComplexMaskingEnabled)) != null;
            if (UseInfoBuffer)
                return;

            foreach (var target in OutlinablesToRender)
            {
                if ((target.DrawingMode & OutlinableDrawingMode.Normal) == 0)
                    continue;

                if (!CheckDiffers(target))
                    continue;

                UseInfoBuffer = true;
                break;
            }
        }

        private static bool CheckDiffers(Outlinable outlinable)
        {
            if (outlinable.RenderStyle == RenderStyle.Single)
                return CheckIfNonOne(outlinable.OutlineParameters);
            else
                return CheckIfNonOne(outlinable.FrontParameters) || CheckIfNonOne(outlinable.BackParameters);
        }

        private static bool CheckIfNonOne(Outlinable.OutlineProperties parameters)
        {
            return parameters.BlurShift != 1.0f || parameters.DilateShift != 1.0f;
        }
    }
}