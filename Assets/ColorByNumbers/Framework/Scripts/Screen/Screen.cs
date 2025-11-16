using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BBG
{
	public class Screen : AdjustRectTransformForSafeArea
	{
		#region Classes

		[System.Serializable]
		private class OnTransitionEvent : UnityEngine.Events.UnityEvent {}

		[System.Serializable]
		private class TransitionInfo
		{
			public enum Type
			{
				Fade,
				Swipe,
				Both
			}

			public bool					animate					= false;
			public Type					animationType			= Type.Fade;
			public float				animationDuration		= 0;
			public float				swipeOffset				= 0;
			public UIAnimation.Style	animationStyle			= UIAnimation.Style.Linear;
			public AnimationCurve		animationCurve			= null;
			public OnTransitionEvent	onTransition			= null;

			public float SwipeOffset { get; set; }
		}

		#endregion // Classes

		#region Enums

		public enum State
		{
			None,
			Showing,
			Shown,
			Hiding,
			Hidden
		}

		#endregion // Enums

		#region Inspector Variables

		[SerializeField] private string	id				= "";
		[SerializeField] private bool	addToBackStack	= true;
		[Space]
		[SerializeField] private TransitionInfo	showTransition = null;
		[SerializeField] private TransitionInfo	hideTransition = null;

		#endregion

		#region Properties

		public string	Id				{ get { return id; } }
		public bool		AddToBackStack	{ get { return addToBackStack; } }
		public State	ScreenState		{ get; private set; }
		public object[]	CurrentData		{ get; private set; }

		#endregion

		#region Public Methods

		public virtual void Initialize()
		{
		}

		public virtual void OnShowing(bool back, object[] data)
		{
			ScreenState = State.Showing;
		}

		public virtual void OnHiding()
		{
			ScreenState = State.Hiding;
		}

		public virtual void OnShown(bool back)
		{
			ScreenState = State.Shown;
		}

		public virtual void OnHidden(bool back)
		{
			ScreenState = State.Hidden;
		}

		public void Show(bool back, bool immediate, object[] data)
		{
			CurrentData = data;
			
			OnShowing(back, data);

			immediate = immediate | !showTransition.animate;

			if (immediate)
			{
				SetVisibility(true);
				OnShown(back);
			}
			else
			{
				if (!back)
				{
					transform.SetAsLastSibling();
					showTransition.SwipeOffset = 1f;
				}
				else
				{
					showTransition.SwipeOffset = 1f - showTransition.swipeOffset;
				}

				Transition(showTransition, back, true);
			}
		}

		public void Hide(bool back, bool immediate)
		{
			OnHiding();

			immediate = immediate | !hideTransition.animate;

			if (immediate)
			{
				SetVisibility(false);
				OnHidden(back);
			}
			else
			{
				if (back)
				{
					transform.SetAsLastSibling();
					hideTransition.SwipeOffset = 1f;
				}
				else
				{
					hideTransition.SwipeOffset = 1f - hideTransition.swipeOffset;
				}

				Transition(hideTransition, back, false);
			}
		}

		#endregion

		#region Private Methods

		private void Transition(TransitionInfo transitionInfo, bool back, bool show)
		{
			// Make sure the screen is showing for the animation
			SetVisibility(true);

			switch (transitionInfo.animationType)
			{
				case TransitionInfo.Type.Fade:
					StartFadeAnimation(transitionInfo, show, back);
					break;
				case TransitionInfo.Type.Swipe:
					StartSwipeAnimation(transitionInfo, show, back);
					break;
				case TransitionInfo.Type.Both:
					StartFadeAnimation(transitionInfo, show, back);
					StartSwipeAnimation(transitionInfo, show, back, false);
					break;
			}

			transitionInfo.onTransition.Invoke();
		}

		/// <summary>
		/// Starts the fade screen transition animation
		/// </summary>
		private void StartFadeAnimation(TransitionInfo transitionInfo, bool show, bool back, bool setupListener = true)
		{
			float fromAlpha	= show ? 0f : 1f;
			float toAlpha	= show ? 1f : 0f;

			UIAnimation anim = UIAnimation.Alpha(CG, fromAlpha, toAlpha, transitionInfo.animationDuration);
			anim.style = transitionInfo.animationStyle;
			anim.startOnFirstFrame = true;

			anim.animationCurve = transitionInfo.animationCurve;

			if (setupListener)
			{
				SetupAnimationListener(anim, show, back);
			}

			anim.Play();
		}

		/// <summary>
		/// Starts the swipe screen transition animation
		/// </summary>
		private void StartSwipeAnimation(TransitionInfo transitionInfo, bool show, bool back, bool setupListener = true)
		{
			float screenWidth	= RectT.rect.width;
			float fromX			= 0f;
			float toX			= 0f;

			if (show && back)
			{
				fromX = -screenWidth * transitionInfo.SwipeOffset;
				toX = 0;
			}
			else if (show && !back)
			{
				fromX = screenWidth;
				toX = 0;
			}
			else if (!show && back)
			{
				fromX = 0;
				toX = screenWidth;
			}
			else if (!show && !back)
			{
				fromX = 0;
				toX = -screenWidth * transitionInfo.SwipeOffset;
			}

			UIAnimation anim = UIAnimation.PositionX(RectT, fromX, toX, transitionInfo.animationDuration);
			anim.style = transitionInfo.animationStyle;
			anim.animationCurve = transitionInfo.animationCurve;
			anim.startOnFirstFrame = true;

			if (setupListener)
			{
				SetupAnimationListener(anim, show, back);
			}

			anim.Play();
		}

		void SetupAnimationListener(UIAnimation anim, bool show, bool back)
		{
			anim.OnAnimationFinished = ((_) => 
			{
				if (!show)
				{
					SetVisibility(false);
					OnHidden(back);
				}
				else
				{
					OnShown(back);
				}
			});
		}

		/// <summary>
		/// Sets the visibility.
		/// </summary>
		private void SetVisibility(bool isVisible)
		{
			CG.alpha			= isVisible ? 1f : 0f;
			CG.interactable		= isVisible ? true : false;
			CG.blocksRaycasts	= isVisible ? true : false;

			if (isVisible)
			{
				RectT.anchoredPosition = new Vector2(0, RectT.anchoredPosition.y);
			}
		}

		#endregion
	}
}
