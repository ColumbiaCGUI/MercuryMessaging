// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	using UnityEngine;
	using UnityEditor;

	/// <summary>A helper type to inherit from when you want an inspector/editor that draws immediate mode shapes</summary>
	public class ImmediateModeShapeDrawerEditor : Editor {

		/// <summary>Override this to draw Shapes in immediate mode. This is called once per camera. You can draw using this code: using(Draw.Command(cam)){ // Draw here }</summary>
		/// <param name="cam">The camera that is currently rendering</param>
		public virtual void DrawShapes( Camera cam ) {
			// override this and draw shapes in immediate mode here
		}

		void OnCameraPreRender( Camera cam ) {
			switch( cam.cameraType ) {
				case CameraType.Preview:
				case CameraType.Reflection:
					return; // Don't render in preview windows or in reflection probes in case we run this script in the editor
			}

			DrawShapes( cam );
		}

		#if (SHAPES_URP || SHAPES_HDRP)
			public virtual void OnEnable() => UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += DrawShapesSRP;
			public virtual void OnDisable() => UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= DrawShapesSRP;
			void DrawShapesSRP( UnityEngine.Rendering.ScriptableRenderContext ctx, Camera cam ) => OnCameraPreRender( cam );
		#else
		public virtual void OnEnable() => Camera.onPreRender += OnCameraPreRender;
		public virtual void OnDisable() => Camera.onPreRender -= OnCameraPreRender;
		#endif

	}

}