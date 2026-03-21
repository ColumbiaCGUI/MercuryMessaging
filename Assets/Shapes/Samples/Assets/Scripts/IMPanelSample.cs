using UnityEngine;

namespace Shapes {

	public class IMPanelSample : ImmediateModePanel {

		[Range( 0, 1 )]
		public float fillAmount = 1;
		public Gradient colorGradient;
		public string title = "Title";

		public override void DrawPanelShapes( Rect rect, ImCanvasContext ctx ) {
			if( colorGradient == null )
				return; // just in case it hasn't initialized

			// Draw black background:
			Draw.Rectangle( rect, 8f, Color.black );

			// Draw the colored bar:
			Rect fillRect = Inset( rect, 8 ); // inset the rect a little bit, to give it some margin
			fillRect.width *= fillAmount;
			Draw.Rectangle( fillRect, colorGradient.Evaluate( fillAmount ) );

			// Draw white border:
			Draw.RectangleBorder( rect, 4f, 8f, Color.white );

			// Draw the title
			Draw.FontSize = 240;
			Vector2 topLeft = new Vector2( rect.xMin + 6f, rect.yMax + 6f );
			Draw.Text( topLeft, title, TextAlign.BaselineLeft );
		}

		Rect Inset( Rect r, float amount ) {
			return new Rect( r.x + amount, r.y + amount, r.width - amount * 2, r.height - amount * 2 );
		}

	}

}