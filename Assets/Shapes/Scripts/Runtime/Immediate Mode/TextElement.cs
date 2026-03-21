using System;
using System.Globalization;
using System.Text;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	/// <summary>Helper type to draw persistent and more performant text. Note: make sure you dispose when done with it. Also, this does not support changing render state such as ZTest or blending modes</summary>
	public class TextElement : IDisposable {
		static int idCounter = 0;
		public static int GetNextId() => idCounter++;
		public readonly int id;
		public TextMeshProShapes Tmp => ShapesTextPool.Instance.GetElement( id );
		public TextElement() => this.id = GetNextId();
		public void Dispose() => ShapesTextPool.Instance.ReleaseElement( id );

		StringBuilder sb = new StringBuilder();

		public void ClearText() {
			sb.Clear();
			Tmp.SetText( sb );
		}

		// This garbage is to work around unity not having the modern .net runtime:
		public void AppendInt( int value, ReadOnlySpan<char> format = default, int maxCharCount = 12 ) {
			Span<char> chars = stackalloc char[maxCharCount];
			value.TryFormat( chars, out int charCount, format, CultureInfo.InvariantCulture );
			AppendString( chars[..charCount] );
		}

		public void AppendFloat( float value, ReadOnlySpan<char> format = default, int maxCharCount = 32 ) {
			Span<char> chars = stackalloc char[maxCharCount];
			value.TryFormat( chars, out int charCount, format, CultureInfo.InvariantCulture );
			AppendString( chars[..charCount] );
		}

		public void AppendDouble( double value, ReadOnlySpan<char> format = default, int maxCharCount = 32 ) {
			Span<char> chars = stackalloc char[maxCharCount];
			value.TryFormat( chars, out int charCount, format, CultureInfo.InvariantCulture );
			AppendString( chars[..charCount] );
		}

		public void AppendString( ReadOnlySpan<char> stringValue ) {
			sb.Append( stringValue );
			Tmp.SetText( sb );
		}

	}

}