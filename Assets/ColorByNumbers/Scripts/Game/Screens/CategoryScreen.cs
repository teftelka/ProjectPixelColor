using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.ColorByNumbers
{
	public class CategoryScreen : Screen
	{
		#region Inspector Variables

		[Space]

		[Tooltip("The CategoryListItem prefab that will be instantiated and placed in the categoryListItemContainer for each CategoryInfo defined in the GameController.")]
		[SerializeField] private CategoryListItem categoryListItemPrefab = null;

		[Tooltip("The Transform all the CategoryItemUIs will be placed in.")]
		[SerializeField] private Transform categoryListItemContainer = null;

		#endregion

		#region Member Variables

		private GameObjectPool categoryListItemPool;

		#endregion

		#region Public Methods

		public override void Initialize()
		{
			base.Initialize();
			
			categoryListItemPool = new GameObjectPool(categoryListItemPrefab.gameObject, 1, GameObjectPool.CreatePoolContainer(transform));

			SetupCategoryItemList();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sets up list of items for the category screen
		/// </summary>
		private void SetupCategoryItemList()
		{
			for (int i = 0; i < GameController.Instance.CategoryInfos.Count; i++)
			{
				CategoryInfo		categoryInfo		= GameController.Instance.CategoryInfos[i];
				CategoryListItem	categoryListItem	= categoryListItemPool.GetObject<CategoryListItem>(categoryListItemContainer);

				categoryListItem.Setup(categoryInfo);

				categoryListItem.OnCategoryListItemClicked = OnCategoryListItemClicked;
			}
		}

		/// <summary>
		/// Called when a CategoryListItem is clicked
		/// </summary>
		public void OnCategoryListItemClicked(CategoryInfo categoryInfo)
		{
			ScreenManager.Instance.ShowSubScreen("main", "library", categoryInfo);
		}

		#endregion
	}
}
