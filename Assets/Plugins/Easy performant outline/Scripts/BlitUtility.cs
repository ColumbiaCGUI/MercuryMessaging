using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEngine.Profiling;
using Unity.Collections;

namespace EPOOutline
{
    public static class BlitUtility
    {
        private static readonly int MainTexHash = Shader.PropertyToID("_MainTex");

        private static Vector4[] normals = new Vector4[]
            {
                new Vector4(-0.578f, -0.578f, -0.578f),
                new Vector4(0.578f, -0.578f, -0.578f),
                new Vector4(0.578f, 0.578f, -0.578f),
                new Vector4(-0.578f, 0.578f, -0.578f),
                new Vector4(-0.578f, 0.578f, 0.578f),
                new Vector4(0.578f, 0.578f, 0.578f),
                new Vector4(0.578f, -0.578f, 0.578f),
                new Vector4(-0.578f, -0.578f, 0.578f)
            };

        private static Vector4[] tempVertecies =
            {
                new Vector4(-0.5f, -0.5f, -0.5f, 1),
                new Vector4(0.5f, -0.5f, -0.5f, 1),
                new Vector4(0.5f, 0.5f, -0.5f, 1),
                new Vector4(-0.5f, 0.5f, -0.5f, 1),
                new Vector4(-0.5f, 0.5f, 0.5f, 1),
                new Vector4(0.5f, 0.5f, 0.5f, 1),
                new Vector4(0.5f, -0.5f, 0.5f, 1),
                new Vector4(-0.5f, -0.5f, 0.5f, 1)
            };

        private static VertexAttributeDescriptor[] vertexParams =
                new VertexAttributeDescriptor[]
                    {
                        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
                        new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32)
                    };

        public struct MeshSetupResult
        {
            public readonly int VertexIndex;
            public readonly int TriangleIndex;

            public MeshSetupResult(int vertexIndex, int triangleIndex)
            {
                VertexIndex = vertexIndex;
                TriangleIndex = triangleIndex;
            }
        }

        private static ushort[] indecies = new ushort[4096 * 5];

        private static Vertex[] vertices = new Vertex[4096];
        private static Matrix4x4[] matrices = new Matrix4x4[4096];
        private static int itemsToDraw = 0;

        private static bool? supportsInstancing;

        private static bool SupportsInstancing
        {
            get
            {
                if (supportsInstancing.HasValue)
                    return supportsInstancing.Value;

                supportsInstancing = SystemInfo.supportsInstancing;
                return supportsInstancing.Value;
            }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct Vertex
        {
            public Vector4 Position;
            public Vector3 Normal;
        }

        private static void UpdateBounds(Renderer renderer, OutlineTarget target)
        {
            if (target.renderer is MeshRenderer)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter.sharedMesh != null)
                    meshFilter.sharedMesh.RecalculateBounds();
            }
            else if (target.renderer is SkinnedMeshRenderer)
            {
                var skinedMeshRenderer = renderer as SkinnedMeshRenderer;
                if (skinedMeshRenderer.sharedMesh != null)
                    skinedMeshRenderer.sharedMesh.RecalculateBounds();
            }
        }

        public static void PrepareForRendering(OutlineParameters parameters)
        {
            if (parameters.BlitMesh == null)
                parameters.BlitMesh = parameters.MeshPool.AllocateMesh();

            var result = SupportsInstancing ? 
                             SetupForInstancing(parameters) :
                             SetupForBruteForce(parameters);

            if (!result.HasValue)
                return;

            const MeshUpdateFlags flags = MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices;
            
            parameters.BlitMesh.SetVertexBufferParams(result.Value.VertexIndex, attributes: vertexParams);
            parameters.BlitMesh.SetVertexBufferData(vertices, 0, 0, result.Value.VertexIndex, 0, flags);
            parameters.BlitMesh.SetIndexBufferParams(result.Value.TriangleIndex, IndexFormat.UInt16);
            parameters.BlitMesh.SetIndexBufferData(indecies, 0, 0, result.Value.TriangleIndex, flags);

            parameters.BlitMesh.subMeshCount = 1;
            parameters.BlitMesh.SetSubMesh(0, new SubMeshDescriptor(0, result.Value.TriangleIndex, MeshTopology.Triangles), flags);
        }

        private static MeshSetupResult? SetupForInstancing(OutlineParameters parameters)
        {
            if (vertices.Length < 8)
            {
                Array.Resize(ref vertices, 16);
                Array.Resize(ref indecies, vertices.Length * 5);
            }

            MeshSetupResult? result = null;
            var triangleIndex = 0;
            var currentIndex = 0;

            indecies[triangleIndex++] = (ushort)currentIndex;
            indecies[triangleIndex++] = (ushort)(currentIndex + 2);
            indecies[triangleIndex++] = (ushort)(currentIndex + 1);

            indecies[triangleIndex++] = (ushort)currentIndex;
            indecies[triangleIndex++] = (ushort)(currentIndex + 3);
            indecies[triangleIndex++] = (ushort)(currentIndex + 2);

            indecies[triangleIndex++] = (ushort)(currentIndex + 2);
            indecies[triangleIndex++] = (ushort)(currentIndex + 3);
            indecies[triangleIndex++] = (ushort)(currentIndex + 4);

            indecies[triangleIndex++] = (ushort)(currentIndex + 2);
            indecies[triangleIndex++] = (ushort)(currentIndex + 4);
            indecies[triangleIndex++] = (ushort)(currentIndex + 5);

            indecies[triangleIndex++] = (ushort)(currentIndex + 1);
            indecies[triangleIndex++] = (ushort)(currentIndex + 2);
            indecies[triangleIndex++] = (ushort)(currentIndex + 5);

            indecies[triangleIndex++] = (ushort)(currentIndex + 1);
            indecies[triangleIndex++] = (ushort)(currentIndex + 5);
            indecies[triangleIndex++] = (ushort)(currentIndex + 6);

            indecies[triangleIndex++] = (ushort)currentIndex;
            indecies[triangleIndex++] = (ushort)(currentIndex + 7);
            indecies[triangleIndex++] = (ushort)(currentIndex + 4);

            indecies[triangleIndex++] = (ushort)currentIndex;
            indecies[triangleIndex++] = (ushort)(currentIndex + 4);
            indecies[triangleIndex++] = (ushort)(currentIndex + 3);

            indecies[triangleIndex++] = (ushort)(currentIndex + 5);
            indecies[triangleIndex++] = (ushort)(currentIndex + 4);
            indecies[triangleIndex++] = (ushort)(currentIndex + 7);

            indecies[triangleIndex++] = (ushort)(currentIndex + 5);
            indecies[triangleIndex++] = (ushort)(currentIndex + 7);
            indecies[triangleIndex++] = (ushort)(currentIndex + 6);

            indecies[triangleIndex++] = (ushort)currentIndex;
            indecies[triangleIndex++] = (ushort)(currentIndex + 6);
            indecies[triangleIndex++] = (ushort)(currentIndex + 7);

            indecies[triangleIndex++] = (ushort)currentIndex;
            indecies[triangleIndex++] = (ushort)(currentIndex + 1);
            indecies[triangleIndex++] = (ushort)(currentIndex + 6);

            for (var index = 0; index < 8; index++)
            {
                vertices[currentIndex++] = new Vertex()
                                           {
                                               Position = tempVertecies[index],
                                               Normal = normals[index]
                                           };
            }

            result = new MeshSetupResult(currentIndex, triangleIndex);

            var itemIndex = 0;
            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if (outlinable.DrawingMode != OutlinableDrawingMode.Normal)
                    continue;

                foreach (var target in outlinable.OutlineTargets)
                {
                    var renderer = target.Renderer;
                    if (!target.IsVisible)
                        continue;

                    var pretransformedBounds = false;
                    var bounds = new Bounds();
                    if (target.BoundsMode == BoundsMode.Manual)
                    {
                        bounds = target.Bounds;
                        var size = bounds.size;
                        var rendererScale = renderer.transform.localScale;
                        size.x /= rendererScale.x;
                        size.y /= rendererScale.y;
                        size.z /= rendererScale.z;
                        bounds.size = size;
                    }
                    else
                    {
                        if (target.BoundsMode == BoundsMode.ForceRecalculate)
                            UpdateBounds(target.Renderer, target);

                        var meshRenderer = renderer as MeshRenderer;
                        var index = (meshRenderer == null ? 0 : meshRenderer.subMeshStartIndex) + target.SubmeshIndex;
                        var filter = meshRenderer == null ? null : meshRenderer.GetComponent<MeshFilter>();
                        var mesh = filter == null ? null : filter.sharedMesh;
                        
                        if (mesh != null && mesh.subMeshCount > index)
                        {
                            bounds = mesh.GetSubMesh(index).bounds;
                            pretransformedBounds = meshRenderer.isPartOfStaticBatch;
                        }
                        else
                        {
                            pretransformedBounds = true;
                            bounds = renderer.bounds;
                        }
                    }

                    if (pretransformedBounds)
                        matrices[itemIndex++] = Matrix4x4.TRS(bounds.center, Quaternion.identity, bounds.size);
                    else
                    {
                        var targetTransform = target.renderer.transform;
                        var size = bounds.size;
                        matrices[itemIndex++] = targetTransform.localToWorldMatrix * Matrix4x4.Translate(bounds.center) * Matrix4x4.Scale(size);
                    }
                }
            }

            itemsToDraw = itemIndex;

            return result;
        }

        private static MeshSetupResult? SetupForBruteForce(OutlineParameters parameters)
        {
            const int numberOfVertices = 8;

            var currentIndex = 0;
            var triangleIndex = 0;
            var expectedCount = 0;
            foreach (var outlinable in parameters.OutlinablesToRender)
                expectedCount += numberOfVertices * outlinable.OutlineTargets.Count;

            if (vertices.Length < expectedCount)
            {
                Array.Resize(ref vertices, expectedCount * 2);
                Array.Resize(ref indecies, vertices.Length * 5);
            }

            foreach (var outlinable in parameters.OutlinablesToRender)
            {
                if (outlinable.DrawingMode != OutlinableDrawingMode.Normal)
                    continue;

                foreach (var target in outlinable.OutlineTargets)
                {
                    var renderer = target.Renderer;
                    if (!target.IsVisible)
                        continue;

                    var pretransformedBounds = false;
                    var bounds = new Bounds();
                    if (target.BoundsMode == BoundsMode.Manual)
                    {
                        bounds = target.Bounds;
                        var size = bounds.size;
                        var rendererScale = renderer.transform.localScale;
                        size.x /= rendererScale.x;
                        size.y /= rendererScale.y;
                        size.z /= rendererScale.z;
                        bounds.size = size;
                    }
                    else
                    {
                        if (target.BoundsMode == BoundsMode.ForceRecalculate)
                            UpdateBounds(target.Renderer, target);

                        var meshRenderer = renderer as MeshRenderer;
                        var index = (meshRenderer == null ? 0 : meshRenderer.subMeshStartIndex) + target.SubmeshIndex;
                        var filter = meshRenderer == null ? null : meshRenderer.GetComponent<MeshFilter>();
                        var mesh = filter == null ? null : filter.sharedMesh;

                        if (mesh != null && mesh.subMeshCount > index)
                            bounds = mesh.GetSubMesh(index).bounds;
                        else
                        {
                            pretransformedBounds = true;
                            bounds = renderer.bounds;
                        }
                    }

                    Vector4 boundsSize = bounds.size;
                    boundsSize.w = 1;

                    var boundsCenter = (Vector4)bounds.center;

                    var transformMatrix = Matrix4x4.identity;
                    var normalTransformMatrix = Matrix4x4.identity;
                    if (!pretransformedBounds && (target.BoundsMode == BoundsMode.Manual || !renderer.isPartOfStaticBatch))
                    {
                        transformMatrix = target.renderer.transform.localToWorldMatrix;
                        normalTransformMatrix = Matrix4x4.Rotate(renderer.transform.rotation);
                    }

                    indecies[triangleIndex++] = (ushort)currentIndex;
                    indecies[triangleIndex++] = (ushort)(currentIndex + 2);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 1);

                    indecies[triangleIndex++] = (ushort)currentIndex;
                    indecies[triangleIndex++] = (ushort)(currentIndex + 3);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 2);

                    indecies[triangleIndex++] = (ushort)(currentIndex + 2);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 3);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 4);

                    indecies[triangleIndex++] = (ushort)(currentIndex + 2);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 4);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 5);

                    indecies[triangleIndex++] = (ushort)(currentIndex + 1);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 2);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 5);

                    indecies[triangleIndex++] = (ushort)(currentIndex + 1);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 5);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 6);

                    indecies[triangleIndex++] = (ushort)currentIndex;
                    indecies[triangleIndex++] = (ushort)(currentIndex + 7);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 4);

                    indecies[triangleIndex++] = (ushort)currentIndex;
                    indecies[triangleIndex++] = (ushort)(currentIndex + 4);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 3);

                    indecies[triangleIndex++] = (ushort)(currentIndex + 5);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 4);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 7);

                    indecies[triangleIndex++] = (ushort)(currentIndex + 5);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 7);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 6);

                    indecies[triangleIndex++] = (ushort)currentIndex;
                    indecies[triangleIndex++] = (ushort)(currentIndex + 6);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 7);

                    indecies[triangleIndex++] = (ushort)currentIndex;
                    indecies[triangleIndex++] = (ushort)(currentIndex + 1);
                    indecies[triangleIndex++] = (ushort)(currentIndex + 6);

                    for (var index = 0; index < numberOfVertices; index++)
                    {
                        var normal = normalTransformMatrix * normals[index];

                        var vert = tempVertecies[index];
                        var scaledVert = new Vector4(vert.x * boundsSize.x, vert.y * boundsSize.y, vert.z * boundsSize.z, 1);

                        var vertex = new Vertex()
                                     {
                                         Position = transformMatrix * (boundsCenter + scaledVert),
                                         Normal = normal
                                     };

                        vertices[currentIndex++] = vertex;
                    }
                }
            }

            return new MeshSetupResult(currentIndex, triangleIndex);
        }

        public static void Blit(OutlineParameters parameters, RenderTargetIdentifier source, RenderTargetIdentifier destination, RenderTargetIdentifier destinationDepth, Material material, CommandBuffer targetBuffer, int pass = -1, Rect? viewport = null)
        {
            var buffer = targetBuffer == null ? parameters.Buffer : targetBuffer;
            buffer.SetRenderTarget(destination, destinationDepth);
            if (viewport.HasValue)
                parameters.Buffer.SetViewport(viewport.Value);

            buffer.SetGlobalTexture(MainTexHash, source);

            if (SupportsInstancing)
                buffer.DrawMeshInstanced(parameters.BlitMesh, 0, material, pass, matrices, itemsToDraw);
            else
                buffer.DrawMesh(parameters.BlitMesh, Matrix4x4.identity, material, 0, pass);
        }

        public static void Draw(OutlineParameters parameters, RenderTargetIdentifier target, RenderTargetIdentifier depth, Material material, Rect? viewport = null)
        {
            parameters.Buffer.SetRenderTarget(target, depth);
            if (viewport.HasValue)
                parameters.Buffer.SetViewport(viewport.Value);

            if (SupportsInstancing)
                parameters.Buffer.DrawMeshInstanced(parameters.BlitMesh, 0, material, -1, matrices, itemsToDraw);
            else
                parameters.Buffer.DrawMesh(parameters.BlitMesh, Matrix4x4.identity, material, 0, -1);
        }
    }
}