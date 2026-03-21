using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>A Sphere shape component</summary>
	[ExecuteAlways]
	[AddComponentMenu( "Shapes/Sphere" )]
	public class Sphere : ShapeRenderer {

		[SerializeField] float radius = 1;
		/// <summary>Radius of the sphere, in the given RadiusSpace</summary>
		public float Radius {
			get => radius;
			set => SetFloatNow( ShapesMaterialUtils.propRadius, radius = Mathf.Max( 0f, value ) );
		}
		[SerializeField] ThicknessSpace radiusSpace = Shapes.ThicknessSpace.Meters;
		/// <summary>The space in which radius is defined</summary>
		public ThicknessSpace RadiusSpace {
			get => radiusSpace;
			set => SetIntNow( ShapesMaterialUtils.propRadiusSpace, (int)( radiusSpace = value ) );
		}

		private protected override void SetAllMaterialProperties() {
			SetFloat( ShapesMaterialUtils.propRadius, radius );
			SetInt( ShapesMaterialUtils.propRadiusSpace, (int)radiusSpace );
		}

		internal override bool HasDetailLevels => true;
		internal override bool HasScaleModes => false;
		private protected override void ShapeClampRanges() => radius = Mathf.Max( 0f, radius );
		private protected override void GetMaterials( Material[] mats ) => mats[0] = ShapesMaterialUtils.matSphere[BlendMode];
		private protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.SphereMesh[(int)detailLevel];

		private protected override Bounds GetUnpaddedLocalBounds_Internal() {
			float size = radiusSpace == ThicknessSpace.Meters ? 2 * radius : 0f;
			return new Bounds( Vector3.zero, new Vector3( size, size, size ) );
		}

	}

}