// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	using UnityEngine;

	[ExecuteAlways]
	public class ImmediateModePanel : MonoBehaviour {

		ImmediateModeCanvas imCanvas;
		ImmediateModeCanvas ImCanvas => imCanvas != null ? imCanvas : ( imCanvas = GetComponentInParent<ImmediateModeCanvas>() );
		public bool Valid => ImCanvas != null;

		public virtual void OnEnable() {
			if( Valid )
				ImCanvas.Add( this );
			else {
				Debug.LogWarning( $"{nameof(ImmediateModePanel)} attached to {gameObject.name} is missing an {nameof(ImmediateModeCanvas)} component on its canvas", this );
			}
		}

		public virtual void OnDisable() {
			if( Valid )
				ImCanvas.Remove( this );
		}

		internal void DrawPanel( ImCanvasContext ctx ) {
			RectTransform tf = transform as RectTransform;
			DrawPanelShapes( tf.rect, ctx );
		}

		/// <summary>The method to draw the content of this panel</summary>
		/// <param name="rect">The rect of this panel, in UI space</param>
		/// <param name="ctx">The current UI drawing context</param>
		public virtual void DrawPanelShapes( Rect rect, ImCanvasContext ctx ) {
			// override this!
		}

	}

}