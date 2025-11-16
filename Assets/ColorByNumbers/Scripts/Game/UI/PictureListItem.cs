using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG.ColorByNumbers
{
	public class PictureListItem : RecyclableListItem<PictureInformation>
	{
		#region Inspector Variables

		[Tooltip("The RawImage component that the grayscale texture will be set to.")]
		[SerializeField] private RawImage textureImage = null;

		[Tooltip("The GameObject that will be set to active if the level is locked.")]
		[SerializeField] private GameObject lockedIndicator = null;

		[Tooltip("The amount of coins required to unlock the level is set on this Text component.")]
		[SerializeField] private Text unlockAmountText = null;

		[Tooltip("This GameObject will be set to active if the level has progress and can be continued from a save.")]
		[SerializeField] private GameObject continueIcon = null;

		[Tooltip("This GameObject will be set to active if the level has been completed and is on the my works screen.")]
		[SerializeField] private GameObject completedIcon = null;

		#endregion

		#region Public Methods

		public override void Initialize(PictureInformation pictureInfo, object globalData)
		{

		}

		public override void Setup(int index, PictureInformation pictureInfo, object globalData)
		{
			SetupUI(pictureInfo);

			RefreshTexture(pictureInfo);

			completedIcon.SetActive(pictureInfo.Completed);
			continueIcon.SetActive(!pictureInfo.Completed && pictureInfo.HasProgress);
		}

		public override void Removed()
		{

		}

		#endregion

		#region Private Methods

		private void SetupUI(PictureInformation pictureInfo)
		{
			lockedIndicator.SetActive(pictureInfo.IsLocked);

			unlockAmountText.text = pictureInfo.UnlockAmount.ToString();
		}

		private void RefreshTexture(PictureInformation pictureInfo)
		{
			if (pictureInfo.Completed)
			{
				textureImage.texture = TextureController.Instance.LoadCompletedTexture(pictureInfo);
			}
			else
			{
				textureImage.texture = TextureController.Instance.LoadGrayscale(pictureInfo);
			}

			TextureUtilities.ScaleForTexture(textureImage.texture as Texture2D, textureImage.transform);
		}

		#endregion
	}
}
