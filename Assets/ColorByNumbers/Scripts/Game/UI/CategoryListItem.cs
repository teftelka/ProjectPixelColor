using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.ColorByNumbers
{
	public class CategoryListItem : MonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private Text nameText = null;

		#endregion

		#region Member Variables

		private CategoryInfo categoryInfo;

		#endregion

		#region Properties

		public System.Action<CategoryInfo> OnCategoryListItemClicked { get; set; }

		#endregion

		#region Public Methods

		public void Setup(CategoryInfo categoryInfo)
		{
			this.categoryInfo = categoryInfo;

			nameText.text = categoryInfo.displayName;
		}

		public void OnClicked()
		{
			if (OnCategoryListItemClicked != null)
			{
				OnCategoryListItemClicked(categoryInfo);
			}
		}

		#endregion
	}
}
