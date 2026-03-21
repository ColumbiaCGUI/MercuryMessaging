// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using System;
using Shapes;
using TMPro;
using UnityEngine;

namespace Shapes {

	// hack bc for some reason TMP 3.0.9 (2022.3) doesn't have TextWrappingModes
	#if !UNITY_6000_0_OR_NEWER
	public enum TextWrappingModes {
		NoWrap = 0,
		Normal = 1,
		[Obsolete( "Only supported in Unity 6", false )]
		PreserveWhitespace = 2,
		[Obsolete( "Only supported in Unity 6", false )]
		PreserveWhitespaceNoWrap = 3
	};
	#endif

	public class TextMeshProShapes : TextMeshPro {

		[SerializeField] protected float curvature;
		/// <inheritdoc cref="TextStyle.curvature"/>
		public float Curvature {
			get => curvature;
			set {
				if( curvature == value ) return;
				m_havePropertiesChanged = true;
				curvature = value;
				SetVerticesDirty();
			}
		}

		[SerializeField] protected Vector2 curvaturePivot;
		/// <inheritdoc cref="TextStyle.curvaturePivot"/>
		public Vector2 CurvaturePivot {
			get => curvaturePivot;
			set {
				if( curvaturePivot == value ) return;
				m_havePropertiesChanged = true;
				curvaturePivot = value;
				SetVerticesDirty();
			}
		}

		// hack bc for some reason TMP 3.0.9 (2022.3) doesn't have TextWrappingModes
		#if !UNITY_6000_0_OR_NEWER
		public TextWrappingModes textWrappingMode {
			get => enableWordWrapping ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
			set => this.enableWordWrapping = value != TextWrappingModes.NoWrap;
		}
		#endif

		protected override void OnEnable() {
			base.OnEnable();
			this.OnPreRenderText += ApplyDeformation;
		}

		protected override void OnDisable() {
			base.OnDisable();
			this.OnPreRenderText -= ApplyDeformation;
		}

		void ApplyDeformation( TMP_TextInfo obj ) {
			if( curvature == 0 )
				return; // no deformation
			Vector3 cPiv = curvaturePivot; // z = 0
			// Iterate over each character in the text
			foreach( TMP_CharacterInfo character in textInfo.characterInfo ) {
				// Skip invisible characters
				if( !character.isVisible )
					continue;
				int rootIndex = character.vertexIndex;
				Vector3[] vertices = textInfo.meshInfo[character.materialReferenceIndex].vertices;
				for( int i = 0; i < 4; i++ ) {
					vertices[rootIndex + i] = Bend( vertices[rootIndex + i] - cPiv, curvature ) + cPiv;
				}
			}
		}

		static Vector3 Bend( Vector3 p, float curvature ) {
			float a = 1 - curvature * p.y;
			float t = p.x * a;
			float k = curvature / a;
			float tk = t * k;
			return new Vector3(
				t * ShapesMath.Sinc( tk ),
				t * ShapesMath.Cosinc( tk ) + p.y,
				p.z
			);
		}

	}

}