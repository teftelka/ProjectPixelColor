using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if BBG_MT_ADS
using BBG.MobileTools;
#endif

namespace BBG
{
	[RequireComponent(typeof(Button))]
	public class RewardAdButton : MonoBehaviour
	{
		[System.Serializable]
		public class OnRewardAdEvent : UnityEngine.Events.UnityEvent {}

		#region Inspector Variables
		
		[SerializeField] private OnRewardAdEvent onRewardAdShowing	= null;
		[SerializeField] private OnRewardAdEvent onRewardAdClosed	= null;
		[SerializeField] private OnRewardAdEvent onRewardGranted	= null;

		#endregion

		#region Unity Methods

		private void Start()
		{
			gameObject.SetActive(false);

			#if BBG_MT_ADS
			bool areRewardAdsEnabled = MobileAdsManager.Instance.AreRewardAdsEnabled;
			#else
			bool areRewardAdsEnabled = false;
			#endif

			if (areRewardAdsEnabled)
			{
				UpdateUI();

				#if BBG_MT_ADS
				MobileAdsManager.Instance.OnRewardAdLoaded	+= UpdateUI;
				MobileAdsManager.Instance.OnAdsRemoved		+= OnAdsRemoved;
				#endif

				gameObject.GetComponent<Button>().onClick.AddListener(OnClicked);
			}
		}

		#endregion

		#region Private Methods

		private void UpdateUI()
		{
			#if BBG_MT_ADS
			gameObject.SetActive(MobileAdsManager.Instance.RewardAdState == AdNetworkHandler.AdState.Loaded);
			#else
			gameObject.SetActive(false);
			#endif
		}

		private void OnAdsRemoved()
		{
			#if BBG_MT_ADS
			MobileAdsManager.Instance.OnRewardAdLoaded	-= UpdateUI;
			MobileAdsManager.Instance.OnAdsRemoved		-= OnAdsRemoved;
			#endif

			gameObject.SetActive(false);
		}

		private void OnClicked()
		{
			gameObject.SetActive(false);

			onRewardAdShowing?.Invoke();

			#if BBG_MT_ADS
			MobileAdsManager.Instance.ShowRewardAd(OnRewardAdClosed, OnRewardAdGranted);
			#endif
		}

		private void OnRewardAdGranted()
		{
			onRewardGranted?.Invoke();
		}

		private void OnRewardAdClosed()
		{
			onRewardAdClosed?.Invoke();
		}

		#endregion
	}
}
