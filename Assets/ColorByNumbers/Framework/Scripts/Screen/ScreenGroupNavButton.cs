using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace BBG
{
	public class ScreenGroupNavButton : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private List<Graphic>		graphics		= null;
		[SerializeField] private Color				normalColor		= Color.white;
		[SerializeField] private Color				selectedColor	= Color.white;

		#endregion

		#region Unity Methods

		public void SetSelected(bool isSelected)
		{
			for (int i = 0; i < graphics.Count; i++)
			{
				graphics[i].color = isSelected ? selectedColor : normalColor;
			}
		}

		#endregion
	}
}
