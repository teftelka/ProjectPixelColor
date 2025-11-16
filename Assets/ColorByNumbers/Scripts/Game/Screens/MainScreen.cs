using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.ColorByNumbers
{
	public class MainScreen : ScreenGroup
	{
		#region Public Methods

		public void TryShowCreateScreen()
		{
			if (!NativePlugin.HasCameraPermission())
			{
				#if UNITY_IOS
				NativePlugin.RequestCameraPermission(gameObject.name, "OnRequestCameraPermissionFinished");
				#else
				PopupManager.Instance.Show("permissions_popup", new object[] { "Camera" });
				#endif
			}
			else
			{
				ShowCreateScreen();
			}
		}

		#endregion

		#region Private Methods

		private void OnRequestCameraPermissionFinished(string message)
		{
			if (message == "true")
			{
				ShowCreateScreen();
			}
			else
			{
				PopupManager.Instance.Show("permissions_popup", new object[] { "Camera" });
			}
		}

		private void ShowCreateScreen()
		{
			ShowSubScreen("create");
		}

		#endregion
	}
}
