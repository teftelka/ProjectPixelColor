using System.Collections;
using System.Collections.Generic;
using BBG.ColorByNumbers;
using UnityEngine;
using UnityEngine.UI;

namespace BBG
{
	[RequireComponent(typeof(Button))]
	public class ScreenOpenAreaButton : UIMonoBehaviour
	{
		#region Inspector Variables

		[SerializeField] private float fadeDuration = 0.5f;

		#endregion

		#region Properties

		public Button Button { get { return gameObject.GetComponent<Button>(); } }

		#endregion

		#region Unity Methods

		private void Start()
		{
			Button.onClick.AddListener(OnButtonClicked);

			CG.alpha = 1f; }

		#endregion

		#region Private Methods

		private void OnButtonClicked()
		{
			GameController.Instance.OnButtonOpenAreaClick(this);
		}
		
		

		#endregion
	}
}
