using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.ColorByNumbers
{
	public class MyWorksScreen : Screen
	{
		#region Inspector Variables

		[Space]

		[Tooltip("The PictureListItem prefab that will be instantiated and placed in the library and my works lists.")]
		[SerializeField] private PictureListItem pictureListItemPrefab = null;

		[Tooltip("The RectTransform component that the list items for the library will be placed under.")]
		[SerializeField] private RectTransform myWorksListContainer = null;

		[Tooltip("The ScrollRect component that the list items for the library will be placed in.")]
		[SerializeField] private ScrollRect myWorksListScrollRect = null;

		[Tooltip("The placeholder GameObject that is set to active when there are not items in the my works list.")]
		[SerializeField] private GameObject myWorksNoLevelsContainer = null;

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
			
			UpdateList();

			myWorksNoLevelsContainer.SetActive(GameController.Instance.PlayedPictureInfos.Count == 0);
		}

		#endregion

		#region Private Methods

		private void UpdateList()
		{
			if (listHandler == null)
			{
				listHandler = new RecyclableListHandler<PictureInformation>(GameController.Instance.PlayedPictureInfos, pictureListItemPrefab, myWorksListContainer, myWorksListScrollRect);
				listHandler.OnListItemClicked = OnListItemClicked;
				listHandler.Setup();
			}
			else
			{
				listHandler.UpdateDataObjects(GameController.Instance.PlayedPictureInfos);
			}
		}

		private void OnListItemClicked(PictureInformation pictureInfo)
		{
			GameController.Instance.PictureSelected(pictureInfo);
		}

		#endregion
	}
}