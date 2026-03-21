// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using System;
using UnityEngine;

namespace Shapes {

	public enum ShapeCulling {
		/// <summary>This one will use exact bounds based on the parameters of your shape. Note: This does *not* take screen space sizing into account,
		/// make sure you add padding to compensate for that!</summary>
		CalculatedLocal,
		/// <summary>Simple Global means the bounds will be the globally defined ones in the mesh primitives, in the Shapes settings.
		/// By default, this effectively turns off culling, making shapes always render, even when off-screen</summary>
		SimpleGlobal
	}

}