using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.ColorByNumbers
{
	public class CreateScreen : Screen
	{
		#region Public Methods

		public override void OnShowing(bool back, object[] data)
		{
			base.OnShowing(back, data);

			CreatePictureController.Instance.StartCapturing();
		}

		public override void OnHiding()
		{
			base.OnHiding();

			CreatePictureController.Instance.StopCapturing();
		}

		#endregion
	}
}
