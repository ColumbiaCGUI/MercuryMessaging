using UnityEngine;

namespace Shapes {

	public class IMCanvasSample : ImmediateModeCanvas {

		// this is called automatically by the base class, in an existing Draw.Command context
		public override void DrawCanvasShapes( ImCanvasContext ctx ) {
			// The Rect input above is the full region of the UI,
			// usually the region of the entire screen, in UI coordinates

			// Draw a large ring, fitting it both horizontally and vertically:
			float radius = ( Mathf.Min( ctx.canvasRect.width, ctx.canvasRect.height ) / 2 ) * 0.9f;
			Draw.Ring( Vector3.zero, Quaternion.identity, radius, thickness: 1, new Color( 1, 1, 1, 0.3f ) );

			// Draw a rounded border around the whole screen:
			Draw.RectangleBorder( ctx.canvasRect, 8f, cornerRadius: 16, Color.white );

			// Draws all ImmediateModePanel child objects.
			// in this case, they are health/stamina/magic bars:
			base.DrawPanels();

			// Draw a crosshair in the middle
			Draw.Disc( Vector3.zero, 4f );
			Vector2 a = new Vector2( 14, 0 );
			Vector2 b = new Vector2( 28, 0 );
			for( int i = 0; i < 4; i++ ) {
				Draw.Line( a, b, 4f, LineEndCap.Round );
				a = ShapesMath.Rotate90CCW( a );
				b = ShapesMath.Rotate90CCW( b );
			}
		}

	}

}