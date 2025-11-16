using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG
{
	public abstract class RecyclableListItem<T> : ClickableListItem
	{
		#region Abstract Methods

		public abstract void Initialize(T dataObject, object globalData);
		public abstract void Setup(int index, T dataObject, object globalData);
		public abstract void Removed();

		#endregion
	}
}
