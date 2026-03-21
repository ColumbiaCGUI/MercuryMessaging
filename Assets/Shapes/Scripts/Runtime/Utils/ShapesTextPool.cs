using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public abstract class ShapesObjPool<T, P> : MonoBehaviour where T : Component where P : ShapesObjPool<T, P> {
		const int ALLOCATION_COUNT_WARNING = 500;
		const int ALLOCATION_COUNT_CAP = 1000;

		int ElementCount => elementsPassive.Count + elementsActive.Count;
		Stack<T> elementsPassive = new Stack<T>();
		Dictionary<int, T> elementsActive = new Dictionary<int, T>();
		public T ImmediateModeElement => GetElement( -1 );

		public static int InstanceElementCount => InstanceExists ? Instance.ElementCount : 0;
		public static int InstanceElementCountActive => InstanceExists ? Instance.elementsActive.Count : 0;
		public static bool InstanceExists => instance != null;

		public abstract string PoolTypeName { get; } // override pls

		static P instance;
		public static P Instance {
			get {
				if( instance == null ) {
					instance = FindAnyObjectByType<P>();
					if( instance == null )
						instance = CreatePool();
				}

				return instance;
			}
		}

		static P CreatePool() {
			GameObject holder = new();
			if( Application.isPlaying )
				DontDestroyOnLoad( holder ); // might be a lil gross, not sure
			P text = holder.AddComponent<P>();
			holder.hideFlags = HideFlags.HideAndDontSave; // todo: debug
			return text;
		}

		void ClearData() {
			// clear any residual children if things reload
			for( int i = transform.childCount - 1; i >= 0; i-- )
				transform.GetChild( i ).gameObject.DestroyBranched();
			elementsPassive.Clear();
			elementsActive.Clear();
		}

		void OnEnable() {
			this.gameObject.name = $"Shapes {PoolTypeName} Pool";
			ClearData();
			instance = (P)this;
		}

		void OnDisable() {
			ClearData();
		}

		public T GetElement( int id ) {
			if( elementsActive.TryGetValue( id, out T tmp ) == false )
				tmp = AllocateElement( id );
			return tmp;
		}

		public T AllocateElement( int id ) {
			T elem = null;
			// try find non-null passive elements
			while( elem == null && elementsPassive.Count > 0 )
				elem = elementsPassive.Pop();

			// if no passive elment found, create it
			if( elem == null )
				elem = CreateElement( id );

			// assign it to the active list
			elementsActive.Add( id, elem );
			return elem;
		}

		public void ReleaseElement( int id ) {
			if( elementsActive.TryGetValue( id, out T tmp ) ) {
				elementsActive.Remove( id );
				elementsPassive.Push( tmp );
			} else {
				// Debug.LogError( $"Failed to remove text element [{id}] from text pool" );
			}
		}

		T CreateElement( int id ) {
			int totalCount = ElementCount;
			if( totalCount > ALLOCATION_COUNT_CAP ) {
				Debug.LogError( $"Text element allocation cap of {ALLOCATION_COUNT_CAP} reached. You are probably leaking and not properly disposing {PoolTypeName.ToLower()} elements" );
				return null;
			}

			if( totalCount > ALLOCATION_COUNT_WARNING )
				Debug.LogWarning( $"Allocating more than {ALLOCATION_COUNT_WARNING} {PoolTypeName.ToLower()} elements. You are probably leaking and not properly disposing text objects" );

			GameObject elem = new GameObject( id == -1 ? $"Immediate Mode {PoolTypeName}" : id.ToString() );
			elem.transform.SetParent( transform, false );
			elem.transform.localPosition = Vector3.zero;
			elem.hideFlags = HideFlags.HideAndDontSave;

			T tmp = elem.AddComponent<T>();
			OnCreatedNewComponent( tmp );
			return tmp;
		}

		public abstract void OnCreatedNewComponent( T comp );

	}

	[ExecuteAlways] public class ShapesTextPool : ShapesObjPool<TextMeshProShapes, ShapesTextPool> {

		public override string PoolTypeName => "Text";

		public override void OnCreatedNewComponent( TextMeshProShapes comp ) {
			comp.textWrappingMode = TextWrappingModes.NoWrap;
			comp.overflowMode = TextOverflowModes.Overflow;
			// mesh renderer should exist now due to TMP requiring the component
			comp.GetComponent<MeshRenderer>().enabled = false;
		}

	}

}