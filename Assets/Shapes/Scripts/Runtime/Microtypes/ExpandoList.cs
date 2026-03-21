// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

using System;
using System.Collections.Generic;

namespace Shapes {

	/// <summary>A list that allows you to get/set by index, even if out of range</summary>
	public class ExpandoList<T> {

		public List<T> list = new List<T>();

		public T this[ int i ] {
			get {
				if( i < 0 )
					throw new IndexOutOfRangeException();
				if( i >= list.Count )
					return default; // no need to expand to this size since it'll always be default anyway
				return list[i];
			}
			set {
				if( i < 0 )
					throw new IndexOutOfRangeException();
				int countPre = list.Count;
				if( i < countPre ) {
					list[i] = value;
				} else {
					int elementGapSize = i - countPre;
					if( elementGapSize > 0 ) // list.AddRange( Enumerable.Repeat<T>( default, elementGapSize ) );
						for( int j = 0; j < elementGapSize; j++ )
							list.Add( default );
					list.Add( value );
				}
			}
		}

		public void SetCountToAtLeast( int minCount ) {
			int countPre = list.Count;
			if( countPre < minCount ) {
				int toAdd = minCount - countPre;
				for( int j = 0; j < toAdd; j++ )
					list.Add( default );
			}
		}

		public void Add( T item ) => list.Add( item );
		public void Clear() => list.Clear();

		public void ClearAndSetMinCapacity( int minCapacity ) {
			list.Clear();
			if( list.Capacity < minCapacity )
				list.Capacity = minCapacity;
		}


	}

}