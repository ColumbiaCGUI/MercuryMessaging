using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	struct ShapeDrawState {
		public Mesh mesh;
		public Material mat;
		public int submesh;

		internal bool CompatibleWith( ShapeDrawState other ) => mesh == other.mesh && submesh == other.submesh && mat == other.mat;
	}

	// this is the type that is submitted to the render call
	struct ShapeDrawCall {
		public ShapeDrawState drawState;
		public MaterialPropertyBlock mpb;
		bool usingOverrideMpb;
		public int count;
		public Matrix4x4 matrix;
		public Matrix4x4[] matrices;
		bool instanced;

		public ShapeDrawCall( ShapeDrawState drawState, Matrix4x4 matrix, MaterialPropertyBlock mpbOverride = null ) {
			this.count = 1;
			this.drawState = drawState;
			this.matrix = matrix;
			this.instanced = false;
			this.usingOverrideMpb = mpbOverride != null;
			this.mpb = usingOverrideMpb ? mpbOverride : ObjectPool<MaterialPropertyBlock>.Alloc();
			matrices = null;
		}

		public ShapeDrawCall( ShapeDrawState drawState, int count, Matrix4x4[] matrices, MaterialPropertyBlock mpbOverride = null ) {
			this.count = count;
			this.drawState = drawState;
			this.matrices = matrices;
			this.instanced = true;
			this.usingOverrideMpb = mpbOverride != null;
			this.mpb = usingOverrideMpb ? mpbOverride : ObjectPool<MaterialPropertyBlock>.Alloc();
			matrix = default;
		}

#if SHAPES_URP && UNITY_6000_0_OR_NEWER
		// if on URP, render graph is used (compatibility mode off), on Unity 6:
		public void AddToCommandBuffer( RasterCommandBuffer cmd ) {
			if( instanced )
				cmd.DrawMeshInstanced( drawState.mesh, drawState.submesh, drawState.mat, 0, matrices, count, mpb );
			else {
				cmd.DrawMesh( drawState.mesh, matrix, drawState.mat, drawState.submesh, 0, mpb );
			}
		}
#endif
		// all other render pipelines use this one
		public void AddToCommandBuffer( CommandBuffer cmd ) {
			if( instanced )
				cmd.DrawMeshInstanced( drawState.mesh, drawState.submesh, drawState.mat, 0, matrices, count, mpb );
			else {
				cmd.DrawMesh( drawState.mesh, matrix, drawState.mat, drawState.submesh, 0, mpb );
			}
		}

		public void Cleanup() { // called after this draw call has rendered
			if( usingOverrideMpb == false ) {
				mpb.Clear();
				ObjectPool<MaterialPropertyBlock>.Free( mpb );
			} else {
				mpb = null;
			}
			if( instanced )
				ArrayPool<Matrix4x4>.Free( matrices );
			drawState.mat = null; // to ensure we don't have references to assets lying around
			drawState.mesh = null;
		}

	}

}