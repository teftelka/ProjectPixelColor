using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.ColorByNumbers
{
	public class LibraryScreen : Screen
	{
		#region Inspector Variables

		[Space]

		[Tooltip("The PictureListItem prefab that will be instantiated and placed in the library and my works lists.")]
		[SerializeField] private PictureListItem pictureListItemPrefab = null;

		[Tooltip("The GridLayoutGroup component that the list items for the library will be placed under.")]
		[SerializeField] private RectTransform libraryListContainer = null;

		[Tooltip("The ScrollRect component that the list items for the library will be placed in.")]
		[SerializeField] private ScrollRect libraryListScrollRect = null;

		#endregion

		#region Member Variables

		private RecyclableListHandler<PictureInformation>	listHandler;
		private CategoryInfo								selectedCategoryInfo;

		#endregion

		#region Public Methods

		public override void Initialize()
		{
			base.Initialize();
		}

		public override void OnShowing(bool back, object[] data)
		{
			base.OnShowing(back, data);

			CategoryInfo categoryInfo = null;

			if (!(bool)data[0])
			{
				categoryInfo = data[1] as CategoryInfo;

				if (categoryInfo == null)
				{
					Debug.LogError("[LibraryScreen] Selected CategoryInfo is null");
					return;
				}
			}

			if (categoryInfo != null && selectedCategoryInfo != categoryInfo)
			{
				selectedCategoryInfo = categoryInfo;

				UpdateList(true);
			}
			else
			{
				UpdateList(false);
			}
		}

		#endregion

		#region Private Methods

		private void UpdateList(bool reset)
		{
			if (listHandler == null)
			{
				listHandler = new RecyclableListHandler<PictureInformation>(selectedCategoryInfo.PictureInfos, pictureListItemPrefab, libraryListContainer, libraryListScrollRect);
				listHandler.OnListItemClicked = OnListItemClicked;
				listHandler.Setup();
			}
			else if (reset)
			{
				listHandler.UpdateDataObjects(selectedCategoryInfo.PictureInfos);
			}
			else
			{
				listHandler.Refresh();
			}
		}

		private void OnListItemClicked(PictureInformation pictureInfo)
		{
			GameController.Instance.PictureSelected(pictureInfo);
		}

		#endregion
	}
}
