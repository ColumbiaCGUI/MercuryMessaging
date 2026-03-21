// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using UnityEngine;

namespace Shapes {

	public class ImCanvasContext {
		/// <summary>The camera rendering this UI</summary>
		public Camera camera;
		/// <summary>The canvas this UI is being drawn in</summary>
		public Canvas canvas;
		/// <summary>The full region the canvas is drawn to, in UI coordinates. Usually this is the full screen rect</summary>
		public Rect canvasRect;
		public Matrix4x4 worldToCanvas;
		public Matrix4x4 canvasToWorld;
		public Matrix4x4 canvasToWorldNet;

		internal void UpdateParams( Canvas canvas, Camera camera, RectTransform cnvTf, Matrix4x4 canvasToWorldNet ) {
			this.camera = camera;
			this.canvas = canvas;
			canvasRect = cnvTf.rect;
			worldToCanvas = cnvTf.worldToLocalMatrix;
			canvasToWorld = cnvTf.localToWorldMatrix;
			this.canvasToWorldNet = canvasToWorldNet;
		}
	}

}