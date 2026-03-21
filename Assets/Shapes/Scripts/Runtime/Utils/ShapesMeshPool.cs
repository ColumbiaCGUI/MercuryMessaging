using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesMeshPool {

		public static int MeshCountInPool => meshPool.Count;
		public static int MeshesAllocatedCount => meshesAllocated;
		public static int MeshCountInUse => MeshesAllocatedCount - MeshCountInPool;
		static int meshesAllocated = 0;
		static Stack<Mesh> meshPool = new Stack<Mesh>();

		public static Mesh GetMesh() {
			if( meshPool.Count > 0 ) {
				Mesh m = meshPool.Pop();
				m.Clear();
				return m;
			}
			meshesAllocated++;
			return new Mesh { name = "Pooled Mesh", hideFlags = HideFlags.DontSave };
		}

		public static void Release( Mesh m ) {
			meshPool.Push( m );
		}

	}

}