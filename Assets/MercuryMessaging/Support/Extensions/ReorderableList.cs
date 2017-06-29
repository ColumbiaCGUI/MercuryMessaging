using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MercuryMessaging.Support.Extensions
{
	public class ReorderableListAttribute : PropertyAttribute {}

	[System.Serializable]
	public class ReorderableList<T> : IList<T>
	{
		public List<T> _list;

		#region Implementation of IEnumerable

		public ReorderableList()
		{
			_list = new List<T>();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation of ICollection<T>

		public void Add(T item)
		{
			_list.Add(item);
		}

		public void Clear()
		{
			_list.Clear();
		}

		public bool Contains(T item)
		{
			return _list.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return _list.Remove(item);
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public bool IsReadOnly
		{
			get { return ((IList<T>)_list).IsReadOnly; }
		}

		#endregion

		#region Implementation of IList<T>

		public int IndexOf(T item)
		{
			return _list.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_list.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_list.RemoveAt(index);
		}

		public T this[int index]
		{
			get { return _list[index]; }
			set { _list[index] = value; }
		}

		#endregion
	}
}
