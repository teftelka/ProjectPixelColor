using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG
{
	public class ScreenGroup : Screen
	{
		#region Classes

		[System.Serializable]
		private class SubScreen
		{
			public Screen				screen		= null;
			public ScreenGroupNavButton	navButton	= null;
		}

		#endregion

		#region Inspector Variables

		[Space]

		[SerializeField] private List<SubScreen> subScreens = null;

		#endregion

		#region Member Variables

		private SubScreen currentSubScreen;

		#endregion

		#region Public Methods

		public override void Initialize()
		{
			base.Initialize();

			if (subScreens.Count > 0)
			{
				RectTransform screensContainer = ScreenManager.CreateScreenContainer(transform, siblingIndex: 0);

				for (int i = 0; i < subScreens.Count; i++)
				{
					SubScreen subScreen = subScreens[i];

					subScreen.screen.transform.SetParent(screensContainer, false);

					subScreen.screen.Initialize();
					subScreen.screen.gameObject.SetActive(true);
					subScreen.screen.Hide(false, true);
				}

				//ShowSubScreen(subScreens[0], true, null);
			}
		}

		public void ShowSubScreen(string subScreenId)
		{
			ShowSubScreen(subScreenId, false);
		}

		public void ShowSubScreen(string subScreenId, bool immediate)
		{
			ShowSubScreen(subScreenId, immediate, null);
		}

		public void ShowSubScreen(string subScreenId, bool immediate, object[] data)
		{
			SubScreen subScreen = GetSubScreen(subScreenId);

			if (subScreen != null && currentSubScreen != subScreen)
			{
				ShowSubScreen(subScreen, immediate, data);
			}
		}

		public override void OnShowing(bool back, object[] data)
		{
			base.OnShowing(back, data);

			if (currentSubScreen != null)
			{
				currentSubScreen.screen.OnShowing(back, FormatSubSreenData(data, back));
			}
			else
			{
				ShowSubScreen(subScreens[0], true, null);
			}
		}

		public override void OnHiding()
		{
			base.OnHiding();
			
			if (currentSubScreen != null)
			{
				currentSubScreen.screen.OnHiding();
			}
		}

		#endregion

		#region Private Methods

		private SubScreen GetSubScreen(string screenId)
		{
			for (int i = 0; i < subScreens.Count; i++)
			{
				SubScreen subScreen = subScreens[i];

				if (subScreen.screen.Id == screenId)
				{
					return subScreen;
				}
			}

			return null;
		}

		private void ShowSubScreen(SubScreen subScreen, bool immediate, object[] data)
		{
			bool transitionLeft = currentSubScreen == null || subScreens.IndexOf(currentSubScreen) > subScreens.IndexOf(subScreen);

			if (currentSubScreen != null)
			{
				currentSubScreen.screen.Hide(transitionLeft, immediate);

				if (currentSubScreen.navButton != null)
				{
					currentSubScreen.navButton.SetSelected(false);
				}
			}

			subScreen.screen.Show(transitionLeft, immediate, FormatSubSreenData(data, false));

			if (subScreen.navButton != null)
			{
				subScreen.navButton.SetSelected(true);
			}

			currentSubScreen = subScreen;
		}

		private object[] FormatSubSreenData(object[] data, bool back)
		{
			List<object> subData = new List<object>();

			subData.Add(back);

			if (data != null)
			{
				subData.AddRange(data);
			}

			return subData.ToArray();
		}

		#endregion
	}
}
