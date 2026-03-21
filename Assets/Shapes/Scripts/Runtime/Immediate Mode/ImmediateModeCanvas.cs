// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Rendering;

	[ExecuteAlways] [RequireComponent( typeof(Canvas) )]
	public class ImmediateModeCanvas : ImmediateModeShapeDrawer {

		static ImCanvasContext canvasContext = new ImCanvasContext();

		Canvas canvas;
		Canvas Canvas => canvas = canvas != null ? canvas : GetComponent<Canvas>();
		RectTransform canvasRectTf;
		RectTransform CanvasRectTf => canvasRectTf = canvasRectTf != null ? canvasRectTf : GetComponent<RectTransform>();
		Camera camUI;
		Camera CamUI => camUI = camUI != null ? camUI : Canvas.worldCamera;
		List<ImmediateModePanel> panels = new List<ImmediateModePanel>();

		bool IsCameraBasedUI => Canvas.worldCamera != null && Canvas.renderMode is RenderMode.WorldSpace;

		public void Add( ImmediateModePanel panel ) => panels.Add( panel );
		public void Remove( ImmediateModePanel panel ) => panels.Remove( panel );

		protected void DrawPanels() {
			using( Draw.Scope ) {
				if( Canvas.renderMode == RenderMode.ScreenSpaceOverlay )
					Draw.Matrix *= canvasContext.worldToCanvas; // X = C_w2l * P_l2w
				foreach( ImmediateModePanel panel in panels ) {
					#if UNITY_EDITOR
					if( canvasContext.camera.cameraType is CameraType.SceneView && UnityEditor.SceneVisibilityManager.instance.IsHidden( panel.gameObject ) )
						continue; // don't draw hidden panels in the scene view
					#endif
					using( Draw.Scope ) {
						// todo: I think this is ultimately kinda messy only because
						// shapes currently assumes you want to always draw in world space, using the current VP matrix,
						// so we have to construct and compensate in weird ways with the camera when in overlay mode.
						// The proper way to do this is to allow Shapes to be drawn in camera space, or using a custom VP matrix. I think!
						// It should also be possible to cache these instead of recalculating on every draw, if there's some
						// event to detect when both size and position has changed, relative to the canvas
						if( Canvas.renderMode == RenderMode.ScreenSpaceOverlay )
							Draw.Matrix = ShapesMath.AffineMtxMul( Draw.Matrix, panel.transform.localToWorldMatrix );
						else
							Draw.Matrix = panel.transform.localToWorldMatrix;
						panel.DrawPanel( canvasContext );
					}
				}
			}
		}

		bool CameraShouldRenderUI( Camera cam ) {
			#if UNITY_EDITOR
			// always display UI in the scene view, unless the game object is invisible
			if( cam.cameraType is CameraType.SceneView )
				return UnityEditor.SceneVisibilityManager.instance.IsHidden( gameObject ) == false;
			#endif
			if( cam.cameraType is CameraType.Game ) {
				// game cameras should only draw overlay UI if the camera display matches the canvas target display
				if( canvas.renderMode == RenderMode.ScreenSpaceOverlay )
					return cam.targetDisplay == canvas.targetDisplay;
				return cam == CamUI; // for world space UI, we only render in the UI camera, if matching
			}
			return false; // don't render in any other camera types
		}

		public override void DrawShapes( Camera cam ) {
			if( Canvas.enabled == false )
				return;
			if( CameraShouldRenderUI( cam ) == false )
				return;
			using( Draw.Command( cam ) ) {
				Draw.ZTest = CompareFunction.Always;
				RectTransform cnvTf = CanvasRectTf;
				canvasContext.UpdateParams( Canvas, cam, cnvTf, DisplayAsWorldSpacePanel( cam ) ? cnvTf.localToWorldMatrix : GetOverlayToWorldMatrix( cam ) );
				// canvasContext.UpdateParams( Canvas, cam, cnvTf, cnvTf.localToWorldMatrix );
				Draw.Matrix = canvasContext.canvasToWorldNet;
				DrawCanvasShapes( canvasContext );
			}
		}

		bool DisplayAsWorldSpacePanel( Camera cam ) => cam.cameraType == CameraType.SceneView || ( IsCameraBasedUI && cam == Canvas.worldCamera );

		Matrix4x4 GetOverlayToWorldMatrix( Camera cam ) {
			// overlay cameras are a little more complicated,
			// we have to construct a canvasToWorld matrix
			float planeDistance = ( cam.nearClipPlane + cam.farClipPlane ) / 2;
			Transform camTf = cam.transform;
			Vector3 forward = camTf.forward;
			Vector3 origin = camTf.TransformPoint( 0, 0, planeDistance );

			float scale = 1;
			RectTransform rtf = (RectTransform)Canvas.transform;
			// if perspective, then
			if( cam.orthographic ) {
				scale = 2 * cam.orthographicSize / rtf.sizeDelta.y;
			} else {
				// some of this trig could actually be skipped both: A. by caching, and B. by reading the projection matrix slope instead
				double vFovHalfRad = ( cam.fieldOfView * ShapesMath.DEG_TO_RAD ) / 2.0;
				double halfYSize = (float)( planeDistance * Math.Tan( vFovHalfRad ) );
				scale = (float)( ( 2 * halfYSize ) / rtf.sizeDelta.y );
			}

			Vector3 rightScale = camTf.right * scale;
			Vector3 upScale = camTf.up * scale;
			Vector3 frwScale = forward * scale; // todo
			return new Matrix4x4( rightScale, upScale, frwScale, new Vector4( origin.x, origin.y, origin.z, 1 ) );
		}

		/// <summary>The method to override in order to draw immediate mode shapes.
		/// Note: This is called from an existing Draw.Command context</summary>
		public virtual void DrawCanvasShapes( ImCanvasContext ctx ) {}

	}

}