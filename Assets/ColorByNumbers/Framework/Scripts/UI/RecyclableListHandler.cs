using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BBG
{
	public class RecyclableListHandler<T>
	{
		#region Classes

		private class Animation
		{
			private RectTransform	target;
			private int				index;
			private float			timer;
			private float			from;
			private float			to;
		}

		#endregion

		#region Inspector Variables

		#endregion

		#region Member Variables

		protected List<T>					dataObjects;
		protected object					globalData;
		protected int						maxItems;
		protected Vector2					overrideItemSize;
		protected int						itemCount;
		protected RecyclableListItem<T>		listItemPrefab;
		protected RectTransform				listContainer;
		protected ScrollRect				listScrollRect;

		protected GameObjectPool			listItemPool;
		protected List<RectTransform>		listItemPlaceholders;
		protected int						topItemIndex;
		protected int						bottomItemIndex;

		private bool		isAnimating;
		private Vector2		animateFrom;
		private Vector2		animateTo;
		private float		time;
		private float		animDuration;


		#endregion

		#region Properties

		public System.Action<RecyclableListItem<T>>	OnListItemCreated { get; set; }
		public System.Action<T>						OnListItemClicked { get; set; }

		public int		ItemCount		{ get { return itemCount; } }
		public Vector2	ListItemSize	{ get { return (overrideItemSize != Vector2.zero) ? overrideItemSize : listItemPrefab.RectT.sizeDelta; } }

		#endregion

		#region Constructor

		public RecyclableListHandler(List<T> dataObjects, RecyclableListItem<T> listItemPrefab, RectTransform listContainer, ScrollRect listScrollRect)
		{
			Init(dataObjects, int.MaxValue, listItemPrefab, listContainer, listScrollRect);
		}

		public RecyclableListHandler(List<T> dataObjects, int maxItems, RecyclableListItem<T> listItemPrefab, RectTransform listContainer, ScrollRect listScrollRect)
		{
			Init(dataObjects, maxItems, listItemPrefab, listContainer, listScrollRect);
		}

		private void Init(List<T> dataObjects, int maxItems, RecyclableListItem<T> listItemPrefab, RectTransform listContainer, ScrollRect listScrollRect)
		{
			this.dataObjects		= new List<T>(dataObjects);
			this.maxItems			= maxItems;
			this.listItemPrefab		= listItemPrefab;
			this.listContainer		= listContainer;
			this.listScrollRect		= listScrollRect;

			itemCount = Mathf.Min(maxItems, dataObjects.Count);

			listItemPool			= new GameObjectPool(listItemPrefab.gameObject, 0, GameObjectPool.CreatePoolContainer(listContainer));
			listItemPlaceholders	= new List<RectTransform>();
		}

		#endregion

		#region Public Methods

		public virtual void UpdateDataObjects(List<T> newDataObjects)
		{
			UpdateDataObjects(newDataObjects, globalData, maxItems);
		}

		public virtual void UpdateDataObjects(List<T> newDataObjects, object globalData)
		{
			UpdateDataObjects(newDataObjects, globalData, newDataObjects.Count);
		}

		public virtual void UpdateDataObjects(List<T> newDataObjects, object globalData, int maxItems)
		{
			this.globalData	= globalData;
			this.maxItems	= maxItems;

			dataObjects	= new List<T>(newDataObjects);
			itemCount	= Mathf.Min(maxItems, dataObjects.Count);

			SyncPlaceholdersObjects();

			Reset();
		}

		public virtual void UpdateOverrideItemSize(Vector2 overrideItemSize)
		{
			this.overrideItemSize = overrideItemSize;

			Reset();
		}

		public virtual void Setup(object globalData = null, Vector2 overrideItemSize = default)
		{
			this.globalData			= globalData;
			this.overrideItemSize	= overrideItemSize;

			listScrollRect.onValueChanged.AddListener(OnListScrolled);

			SyncPlaceholdersObjects();

			Reset();
		}

		public virtual void Reset(bool resetListPosition = true)
		{
			listScrollRect.velocity = Vector2.zero;

			if (resetListPosition)
			{
				listContainer.anchoredPosition = Vector2.zero;
			}

			RemoveAllListItems();

			UpdateList(true);

			StopAnimating();
		}

		public virtual void Refresh()
		{
			listScrollRect.velocity = Vector2.zero;

			for (int i = topItemIndex; i <= bottomItemIndex && i >= 0 && i < listItemPlaceholders.Count; i++)
			{
				RectTransform placeholder = listItemPlaceholders[i];

				if (placeholder.childCount == 1)
				{
					RecyclableListItem<T> listItem = placeholder.GetChild(0).GetComponent<RecyclableListItem<T>>();

					listItem.Setup(i, dataObjects[i], globalData);
				}
			}

			StopAnimating();
		}

		public virtual void ClearAll()
		{
			listContainer.anchoredPosition = Vector2.zero;

			UpdateDataObjects(new List<T>(), null);

			StopAnimating();
		}

		public virtual void ClearDataObjects()
		{
			listContainer.anchoredPosition = Vector2.zero;

			UpdateDataObjects(new List<T>());

			StopAnimating();
		}

		public virtual int GetDataObjectIndex(T item)
		{
			return dataObjects.IndexOf(item);
		}

		public virtual IEnumerator ScrollTo(T item, float duration)
		{
			return ScrollTo(GetDataObjectIndex(item), duration);
		}

		public virtual IEnumerator ScrollTo(int index, float duration)
		{
			if (index >= 0 && index < listItemPlaceholders.Count && listItemPlaceholders[index].gameObject.activeInHierarchy)
			{
				Vector2	itemPos				= listItemPlaceholders[index].anchoredPosition;
				Vector2 scrollToPos			= listContainer.anchoredPosition; ;

				if (listScrollRect.vertical)
				{
					float halfViewportHeight	= listScrollRect.viewport.rect.height / 2f;
					float itemMiddle			= -itemPos.y;
					float viewportMiddle		= listContainer.anchoredPosition.y + halfViewportHeight;
					float moveAmount			= itemMiddle - viewportMiddle;

					if (listContainer.rect.height - itemMiddle < halfViewportHeight)
					{
						moveAmount -= halfViewportHeight - (listContainer.rect.height - itemMiddle);
					}

					if (itemMiddle < halfViewportHeight)
					{
						moveAmount += halfViewportHeight - itemMiddle;
					}

					scrollToPos.y = listContainer.anchoredPosition.y + moveAmount;
				}
				else
				{
					float halfViewportWidth	= listScrollRect.viewport.rect.width / 2f;
					float itemMiddle		= itemPos.x;
					float viewportMiddle	= -listContainer.anchoredPosition.x + halfViewportWidth;
					float moveAmount		= itemMiddle - viewportMiddle;

					if (listContainer.rect.width - itemMiddle < halfViewportWidth)
					{
						moveAmount -= halfViewportWidth - (listContainer.rect.width - itemMiddle);
					}

					if (itemMiddle < halfViewportWidth)
					{
						moveAmount += halfViewportWidth - itemMiddle;
					}

					scrollToPos.x = listContainer.anchoredPosition.x - moveAmount;
				}

				return StartAnimating(scrollToPos, duration);
			}

			return null;
		}

		#endregion

		#region Protected Methods
		
		protected void SyncPlaceholdersObjects()
		{
			// Set all the placeholders we need that are already created to active
			for (int i = 0; i < itemCount && i < listItemPlaceholders.Count; i++)
			{
				listItemPlaceholders[i].gameObject.SetActive(true);
			}

			// Create any more placeholders we need to fill the list of data objects
			while (listItemPlaceholders.Count < itemCount)
			{
				GameObject		placeholder		= new GameObject("list_item");
				RectTransform	placholderRectT	= placeholder.AddComponent<RectTransform>();

				placholderRectT.SetParent(listContainer, false);

				placholderRectT.sizeDelta = ListItemSize;

				listItemPlaceholders.Add(placholderRectT);
			}

			// Set any placeholders we dont need to de-active
			for (int i = itemCount; i < listItemPlaceholders.Count; i++)
			{
				listItemPlaceholders[i].gameObject.SetActive(false);
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(listContainer);
		}

		protected L GetListItem<L>(int index) where L : Object
		{
			if (index >= 0 && index < listItemPlaceholders.Count)
			{
				RectTransform placeholder = listItemPlaceholders[index];

				if (placeholder != null && placeholder.gameObject.activeInHierarchy && placeholder.childCount > 0)
				{
					return placeholder.GetChild(0).gameObject.GetComponent<L>();
				}
			}

			return null;
		}
		
		#endregion // Protected Methods

		#region Private Methods

		private void OnListScrolled(Vector2 pos)
		{
			UpdateList();
		}

		private void RemoveAllListItems()
		{
			for (int i = 0; i < listItemPlaceholders.Count; i++)
			{
				RemoveListItem(listItemPlaceholders[i]);
			}
		}

		private void UpdateList(bool reset = false)
		{
			if (reset)
			{
				for (int i = 0; i < itemCount; i++)
				{
					if (IsVisible(listItemPlaceholders[i]))
					{
						topItemIndex = i;
						break;
					}
				}

				bottomItemIndex = FillList(topItemIndex, 1);
			}
			else
			{
				RecycleList();

				topItemIndex	= FillList(topItemIndex, -1);
				bottomItemIndex	= FillList(bottomItemIndex, 1);
			}
		}

		private int FillList(int startIndex, int indexInc)
		{
			int lastVisibleIndex = startIndex;

			for (int i = startIndex; i >= 0 && i < listItemPlaceholders.Count; i += indexInc)
			{
				RectTransform placeholder = listItemPlaceholders[i];

				if (!IsVisible(placeholder))
				{
					break;
				}

				lastVisibleIndex = i;

				if (placeholder.childCount == 0)
				{
					AddListItem(i, placeholder, indexInc == -1);
				}
			}

			return lastVisibleIndex;
		}

		private void RecycleList()
		{
			// If there are no items in the list then just return now
			if (listItemPlaceholders.Count == 0)
			{
				return;
			}

			for (int i = topItemIndex; i <= bottomItemIndex; i++)
			{
				RectTransform placeholder = listItemPlaceholders[i];

				if (IsVisible(placeholder))
				{
					break;
				}
				else if (placeholder.childCount == 1)
				{
					RemoveListItem(placeholder);

					topItemIndex++;
				}
			}

			for (int i = bottomItemIndex; i >= topItemIndex; i--)
			{
				RectTransform placeholder = listItemPlaceholders[i];

				if (IsVisible(placeholder))
				{
					break;
				}
				else if (placeholder.childCount == 1)
				{
					RemoveListItem(placeholder);

					bottomItemIndex--;
				}
			}

			// Check if top index is now greater than bottom index, if so then all elements were recycled so we need to find the new top
			if (topItemIndex > bottomItemIndex)
			{
				int				targetIndex 		= (topItemIndex < itemCount) ? topItemIndex : bottomItemIndex;
				RectTransform	targetPlaceholder	= listItemPlaceholders[targetIndex];

				bool lookUpList = false;

				if (listScrollRect.vertical)
				{
					float viewportTop = listContainer.anchoredPosition.y;

					lookUpList = -targetPlaceholder.anchoredPosition.y < viewportTop;
				}
				else
				{
					float viewportLeft = -listContainer.anchoredPosition.x;

					lookUpList = targetPlaceholder.anchoredPosition.x < viewportLeft;
				}

				if (lookUpList)
				{
					for (int i = targetIndex; i < itemCount; i++)
					{
						if (IsVisible(listItemPlaceholders[i]))
						{
							topItemIndex	= i;
							bottomItemIndex	= i;
							break;
						}
					}
				}
				else
				{
					for (int i = targetIndex; i >= 0; i--)
					{
						if (IsVisible(listItemPlaceholders[i]))
						{
							topItemIndex	= i;
							bottomItemIndex	= i;
							break;
						}
					}
				}
			}
		}

		private bool IsVisible(RectTransform placeholder)
		{
			if (!placeholder.gameObject.activeSelf)
			{
				return false;
			}

			RectTransform viewport = listScrollRect.viewport as RectTransform;

			float placeholderTop	= -placeholder.anchoredPosition.y - placeholder.rect.height / 2f;
			float placeholderbottom	= -placeholder.anchoredPosition.y + placeholder.rect.height / 2f;
			float placeholderLeft	= placeholder.anchoredPosition.x - placeholder.rect.width / 2f;
			float placeholderRight	= placeholder.anchoredPosition.x + placeholder.rect.width / 2f;

			float viewportTop		= listContainer.anchoredPosition.y;
			float viewportbottom	= listContainer.anchoredPosition.y + viewport.rect.height;
			float viewportLeft		= -listContainer.anchoredPosition.x;
			float viewportRight		= -listContainer.anchoredPosition.x + viewport.rect.width;

			return
			placeholderTop < viewportbottom &&
			placeholderbottom > viewportTop &&
			placeholderLeft < viewportRight &&
			placeholderRight > viewportLeft;
		}

		private void AddListItem(int index, RectTransform placeholder, bool addingOnTop)
		{
			bool itemInstantiated;

			RecyclableListItem<T> listItem = listItemPool.GetObject<RecyclableListItem<T>>(placeholder, out itemInstantiated);

			T dataObject = dataObjects[index];

			listItem.Index = index;

			if (OnListItemClicked != null)
			{
				listItem.Data				= dataObject;
				listItem.OnListItemClicked	= OnItemClicked;
			}

			if (overrideItemSize != Vector2.zero)
			{
				listItem.RectT.sizeDelta = overrideItemSize;
			}

			if (itemInstantiated)
			{
				if (OnListItemCreated != null)
				{
					OnListItemCreated(listItem);
				}

				listItem.Initialize(dataObject, globalData);
			}

			listItem.Setup(index, dataObject, globalData);
		}

		private void RemoveListItem(Transform placeholder)
		{
			if (placeholder.childCount == 1)
			{
				RecyclableListItem<T> listItem = placeholder.GetChild(0).GetComponent<RecyclableListItem<T>>();

				// Return the list item object to the pool
				GameObjectPool.ReturnObjectToPool(listItem.gameObject);

				// Notify that it has been removed from the list
				listItem.Removed();
			}
		}

		private void OnItemClicked(int index, object dataObject)
		{
			OnListItemClicked((T)dataObject);
		}

		private IEnumerator StartAnimating(Vector2 toPos, float duration)
		{
			isAnimating = true;
			animateFrom		= listContainer.anchoredPosition;
			animateTo		= toPos;
			time			= 0;
			animDuration	= duration;

			return DoAnim();
		}

		private void StopAnimating()
		{
			isAnimating = false;
		}

		private IEnumerator DoAnim()
		{
			while (isAnimating)
			{
				yield return new WaitForEndOfFrame();

				time += Time.deltaTime;

				if (time >= animDuration)
				{
					isAnimating = false;
				}

				float t = time / animDuration;
				Vector2 pos = Vector2.Lerp(animateFrom, animateTo, Utilities.EaseOut(t));

				listContainer.anchoredPosition = pos;
			}
		}

		#endregion
	}
}
