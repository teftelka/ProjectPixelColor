using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Coffee.UIExtensions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;

namespace BBG.ColorByNumbers
{
	#region Enums

	public enum PowerUp
	{
		None,
		Bomb,
		Wand
	}

	#endregion

	public class GameController : SaveableManager<GameController>
	{
		#region Inspector Variables

		[Tooltip("The number of levels that must be starte before an AD will appear.")]
		[SerializeField] private int numLevelsTillAd = 3;

		[Space]

		[Tooltip("List of all the categories that can be played in the game.")]
		[SerializeField] private List<CategoryInfo> categoryInfos = null;

		[Space]

		[Tooltip("The amount of coins that the player starts with.")]
		[SerializeField] private int startingCurrency = 100;

		[Tooltip("The amount of coins a bomb power up costs to use.")]
		[SerializeField] private int bombCost = 10;

		[Tooltip("The amount of coins a magic wand power up costs to use.")]
		[SerializeField] private int magicWandCost = 10;

		[Tooltip("The amount of coins a bomb power up costs.")]
		[SerializeField] private int bombRadius = 5;

		[Space]

		[Tooltip("The maximum size a single pixel cell can be when zoomed all the way in. Controls how much the player can zoom into a picture.")]
		[SerializeField] private float maxCellSize = 120;

		[Tooltip("When the picture is first displayed on the screen and is placed in the picture area, the picture is scaled down so a single pixel cell is not larger than this size.")]
		[SerializeField] private float maxStartingCellSize = 40;

		[Tooltip("Numbers will start to fade in when cells are larger than this")]
		[SerializeField] private float minCellSizeForNumbers = 40;

		[Tooltip("Space between the edge of the picture and the edge of the pictureContainer bounds.")]
		[SerializeField] private float edgePadding = 25;

		[Tooltip("The amount of alpha that is applied to the grayscale texture for each picture.")]
		[SerializeField] private float grayscaleColorAlpha = 0.4f;

		[Tooltip("The amount of alpha that is applied to the black texture that goes over the picture to indicate cells that correspond to the color number the player has selected.")]
		[SerializeField] private float selectionColorAlpha = 0.15f;

		[Tooltip("The amount of alpha that is applied to an incorrect colored cell.")]
		[SerializeField] private float incorrectColorAlpha = 0.3f;

		[Space]

		[Tooltip("The PictureScrollArea component that will handle the draggin/zooming of the picture.")]
		[SerializeField] private PictureScrollArea pictureScrollArea = null;

		[Tooltip("The RectTransform that specifies the bounds of the picture. The pictures textures will be scaled to fit in this RectTransform.")]
		[SerializeField] private RectTransform pictureContainer = null;

		[Tooltip("The ColorPaletteList component that handles displaying the colors in the active picture.")]
		[SerializeField] private ColorPaletteList colorPaletteList = null;

		[Tooltip("The PowerUpButton component for the bomb power up.")]
		[SerializeField] private PowerUpButton bombPowerUpButton = null;

		[Tooltip("The PowerUpButton component for the magic wand power up.")]
		[SerializeField] private PowerUpButton wandPowerUpButton = null;

		[Space]

		[Tooltip("The font to use for the numbers that appear on the picture grid.")]
		[SerializeField] private Font numbersTextFont = null;

		[Tooltip("The font size for the numbers that appear on the picture grid.")]
		[SerializeField] private int numbersTextFontSize = 65;

		[Tooltip("The amount of space to be applied between the characters for a single number. (EX. 13, increase this if you would like more space between 1 and 3)")]
		[SerializeField] private int numbersTextLetterSpacing = 0;

		[Tooltip("THe color of the numbers text.")]
		[SerializeField] private Color numbersTextColor = Color.black;
		
		[Tooltip("The width of the grid lines, should be an even number.")]
		[SerializeField] private float gridLineWidth = 6f;

		[Tooltip("The color of the vertical/horizontal grid lines.")]
		[SerializeField] private Color gridLineColor = Color.black;

		[Space]

		[Tooltip("Enables/Disables the magnifying glass. If disabled, the player will still be able to tap and hold to drag and color but the magnifying glass will not appear.")]
		[SerializeField] private bool enableMagnifyingGlass = true;

		[Tooltip("The amount of y space between where the mouse is and where the center of the magnifying glass is. Use this to move the magnifying glass up so the finger is not blocking the center point of the magnifying glass.")]
		[SerializeField] private float magnifyingGlassYOffset = 100;

		[Tooltip("The RectTransform that will be moved on the screen relative to where the mouse is.")]
		[SerializeField] private RectTransform magnifyingGlass = null;

		[Tooltip("Similar to pictureContainer, this is the bounds of the maginfying glass.")]
		[SerializeField] private Transform magnifyingGlassContainer = null;
		
		
		[SerializeField] private GameObject particleSystem;
		[SerializeField] private Button buttonOpenArea;

		#endregion

		#region Member Variables

		private List<Texture2D>		gameTextures;
		private int					selectedColorIndex;
		private PowerUp				selectedPowerUp;

		private float				pixelScale;
		private GameObjectPool		gridLinePool;

		private RectTransform		pictureContentArea;
		private Mask				pictureContentMask;
		private RawImage			pictureMaskImage;
		private RawImage			grayscaleImage;
		private RawImage			selectionImage;
		private RawImage			coloredImage;
		private ColorNumbersText	colorNumbersText;
		private CanvasGroup			gridLineContainer;

		private RectTransform		magnifyingPictureContent;
		private Mask				magnifyingPictureMask;
		private RawImage			magnifyingPictureMaskImage;
		private RawImage			magnifyingSelectionImage;
		private RawImage			magnifyingColoredImage;
		private ColorNumbersText	magnifyingColorNumbersText;
		private Transform			magnifyingGlassGridLineContainer;
		private List<RectTransform>	magnifyingGlassGridLines;
		
		private List<int> unlockedRegions = new List<int>();

// оверлей, который будет закрывать недоступные регионы
		private RawImage regionOverlayImage;
		private Texture2D regionOverlayTexture;

		#endregion

		#region Properties

		public override string SaveId { get { return "game"; } }

		public List<CategoryInfo>			CategoryInfos		{ get { return categoryInfos; } }
		public List<PictureInformation>		PictureInfos		{ get; private set; }
		public List<PictureInformation>		PlayedPictureInfos	{ get; private set; }
		public PictureInformation			ActivePictureInfo	{ get; private set; }
		public int							CurrencyAmount		{ get; private set; }
		public int							NumLevelsPlayed		{ get; private set; }

		private Color						SelectedColor		{ get { return ActivePictureInfo.Colors[selectedColorIndex]; } }
		private Texture2D					MaskTexture			{ get { return gameTextures[0]; } }
		private Texture2D					GrayscaleTexture	{ get { return gameTextures[1]; } }
		private Texture2D					ColoredTexture		{ get { return gameTextures[2]; } }
		private Texture2D					SelectionTexture	{ get { return gameTextures[3 + selectedColorIndex]; } }

		private bool isAreaCorrect = true;


		#endregion

		#region Unity Methods

		protected override void Awake()
		{
			base.Awake();

			magnifyingGlassGridLines = new List<RectTransform>();

			// Create the objects what will display the picture and cells to be colored in
			CreatePictureContentObjects();

			// Create the magnifying glass object
			if (enableMagnifyingGlass)
			{
				CreateMagnifyingGlass();
			}

			// Setup listeners for events that happen on the picture scroll area
			pictureScrollArea.OnClick		= OnPictureAreaClicked;
			pictureScrollArea.OnSelect		= OnPictureAreaSelected;
			pictureScrollArea.OnZoom		= OnPictureAreaZoomed;
			pictureScrollArea.OnMove		= OnPictureAreaMoved;
			pictureScrollArea.OnDragStart	= OnPictureAreaDragStart;
			pictureScrollArea.OnDragged		= OnPictureAreaDragged;
			pictureScrollArea.OnDragEnd		= OnPictureAreaDragEnd;

			// Create the Image template used for the grid lines
			GameObject gridLineTemplate = new GameObject("grid_line", typeof(RectTransform), typeof(Image));

			gridLineTemplate.GetComponent<Image>().color				= gridLineColor;
			gridLineTemplate.GetComponent<RectTransform>().sizeDelta	= Vector2.zero;

			gridLinePool = new GameObjectPool(gridLineTemplate, 10, gridLineContainer.transform);

			colorPaletteList.OnColorSelected = SelectColor;

			bombPowerUpButton.SetCost(bombCost);
			wandPowerUpButton.SetCost(magicWandCost);

			InitSave();
		}
		

		private void Update()
		{
		}

		#endregion

		#region Public Methods

		public void PictureSelected(PictureInformation pictureInfo)
		{
			Texture2D texture = pictureInfo.Completed
				? TextureController.Instance.LoadCompletedTexture(pictureInfo)
				: TextureController.Instance.LoadGrayscale(pictureInfo);

			if (pictureInfo.Completed)
			{
				// If the item is a completed item then show the completed level popup
				PopupManager.Instance.Show("completed_level_popup", new object[] { texture }, OnItemPopupClosed);
			}
			else if (pictureInfo.IsLocked)
			{
				// If the level is lock then display the popup to allow the user to unlock it
				PopupManager.Instance.Show("level_locked_popup", new object[] { texture, pictureInfo.UnlockAmount }, OnLockedItemPopupClosed);
			}
			else if (pictureInfo.HasProgress)
			{
				// If the level is not a completed list item and has progress then show the continue level popup
				PopupManager.Instance.Show("continue_level_popup", new object[] { texture }, OnItemPopupClosed);
			}
			else
			{
				OnPictureItemClicked(pictureInfo);
			}

			void OnItemPopupClosed(bool cancelled, object[] outData)
			{
				if (!cancelled)
				{
					string action = outData[0] as string;

					switch (action)
					{
						case "continue":
							OnPictureItemClicked(pictureInfo);
							break;
						case "restart":
							pictureInfo.ClearProgress();
							//unlockedRegions = new List<int> { 0 };
							OnPictureItemClicked(pictureInfo);
							break;
						case "delete":
							OnPictureItemDeleted(pictureInfo);
							break;
					}
				}
			}

			void OnLockedItemPopupClosed(bool cancelled, object[] outData)
			{
				if (!cancelled)
				{
					//Тут было CurrencyAmount
					if (10000 >= pictureInfo.UnlockAmount)
					{
						// Unlock the level
						UnlockLevel(pictureInfo);

						// Call the OnItemClicked callback to start the level
						OnPictureItemClicked(pictureInfo);
					}
					else
					{
						PopupManager.Instance.Show("not_enough_coins");
					}
				}
			}
		}

		/// <summary>
		/// Starts the level by setting up the picture area
		/// </summary>
		public void StartLevel(PictureInformation pictureInfo)
		{
			// Add the picture info to the list of played pictures
			if (!PlayedPictureInfos.Contains(pictureInfo))
			{
				PlayedPictureInfos.Insert(0, pictureInfo);
			}

			// Set the active level
			ActivePictureInfo = pictureInfo;
			

			// Setup the game
			SetupLevel(pictureInfo);

			ScreenManager.Instance.Show("game");
		}

		/// <summary>
		/// Invoked when the bomb powerup is selected
		/// </summary>
		public void OnBombSelected()
		{
			SetSelectedPowerUp(selectedPowerUp == PowerUp.Bomb ? PowerUp.None : PowerUp.Bomb);
		}

		/// <summary>
		/// Invoked when the magic wand powerup is selected
		/// </summary>
		public void OnMagicWandSelected()
		{
			SetSelectedPowerUp(selectedPowerUp == PowerUp.Wand ? PowerUp.None : PowerUp.Wand);
		}

		/// <summary>
		/// Unlocks the given PictureInformation
		/// </summary>
		public void UnlockLevel(PictureInformation pictureInfo)
		{
			if (CurrencyAmount >= pictureInfo.UnlockAmount)
			{
				CurrencyAmount -= pictureInfo.UnlockAmount;

				pictureInfo.SetUnlocked();
			}
		}

		/// <summary>
		/// Adds the given amount of currency
		/// </summary>
		public void AddCurrency(int amount)
		{
			CurrencyAmount += amount;
		}

		/// <summary>
		/// Deletes the levels progress and removes it from the PlayedPictureInfos list
		/// </summary>
		public void DeleteLevelProgress(PictureInformation pictureInformation)
		{
			pictureInformation.ClearProgress();
		}

		/// <summary>
		/// Auto completed the current active level by calling ColorCell for every pixel in the active picture info.
		/// </summary>
		public void CompleteActiveLevel()
		{
			if (ActivePictureInfo != null)
			{
				for (int y = 0; y < ActivePictureInfo.ColorNumbers.Count; y++)
				{
					for (int x = 0; x < ActivePictureInfo.ColorNumbers[y].Count; x++)
					{
						int colorNumber		= ActivePictureInfo.ColorNumbers[y][x];
						int progressNumber	= ActivePictureInfo.Progress[y][x];

						if (colorNumber != -1 && progressNumber != -1)
						{
							ColorCell(x, y, colorNumber, Vector2.zero);
						}
					}
				}

				CheckCompleted();

				colorPaletteList.UpdateCompleted();
			}
		}

		public bool IsAbleToColorArea(Vector2 startScreen, Vector2 endScreen, bool mustMatchSelectedColor = true)
		{
			// === Экран -> локальные координаты полотна ===
		    Vector2 localA, localB;
		    RectTransformUtility.ScreenPointToLocalPointInRectangle(pictureContentArea, startScreen, null, out localA);
		    RectTransformUtility.ScreenPointToLocalPointInRectangle(pictureContentArea, endScreen,   null, out localB);

		    Vector2 contentSize = new Vector2(pictureContentArea.rect.width, pictureContentArea.rect.height);
		    localA += contentSize * 0.5f;
		    localB += contentSize * 0.5f;

		    Vector2 minL = Vector2.Min(localA, localB);
		    Vector2 maxL = Vector2.Max(localA, localB);
		    minL = Vector2.Max(minL, Vector2.zero);
		    maxL = Vector2.Min(maxL, contentSize);
		    if (maxL.x <= minL.x || maxL.y <= minL.y) return false;

		    const float eps = 0.001f;
		    int xMin, yMin, xMax, yMax;
		    CalculateCellFromPosition(minL + new Vector2(eps, eps), contentSize, out xMin, out yMin);
		    CalculateCellFromPosition(maxL - new Vector2(eps, eps), contentSize, out xMax, out yMax);

		    xMin = Mathf.Clamp(xMin, 0, ActivePictureInfo.ColorNumbers[0].Count - 1);
		    xMax = Mathf.Clamp(xMax, 0, ActivePictureInfo.ColorNumbers[0].Count - 1);
		    yMin = Mathf.Clamp(yMin, 0, ActivePictureInfo.ColorNumbers.Count - 1);
		    yMax = Mathf.Clamp(yMax, 0, ActivePictureInfo.ColorNumbers.Count - 1);

		    // === 1-я проходка: проверяем однородность среди НЕЗАКРАШЕННЫХ клеток ===
		    int foundColor = int.MinValue; // целевая цифра незакрашенных клеток в выделении
		    bool hasUnpainted = false;

		    for (int y = yMin; y <= yMax; y++)
		    {
		        for (int x = xMin; x <= xMax; x++)
		        {
		            int num = ActivePictureInfo.ColorNumbers[y][x];
		            if (num == -1) continue; // фон не красим
		            
		            if (!IsCellInUnlockedRegion(x, y))
			            continue;

		            bool alreadyPainted = ActivePictureInfo.Progress[y][x] == -1;
		            if (alreadyPainted) continue; // уже закрашенные игнорим в проверке

		            if (!hasUnpainted)
		            {
		                foundColor = num;
		                hasUnpainted = true;
		                
		            }
		            else if (num != foundColor)
		            {
		                // среди НЕЗАКРАШЕННЫХ встретились разные цифры — отменяем
		                return false;
		            }
		        }
		    }

		    // Внутри нет незакрашенных подходящих клеток — делать нечего
		    if (!hasUnpainted) return true;

		    // Можно требовать совпадение с выбранным цветом
		    if (mustMatchSelectedColor && foundColor != selectedColorIndex)
		        return false;

		    return true;
		}

		#endregion

		#region Private Methods

		private void UpdateRegionOverlayTexture()
		{
			if (ActivePictureInfo == null || ActivePictureInfo.RegionIds == null)
				return;

			int width  = ActivePictureInfo.XCells;
			int height = ActivePictureInfo.YCells;

			if (regionOverlayTexture == null ||
			    regionOverlayTexture.width != width ||
			    regionOverlayTexture.height != height)
			{
				regionOverlayTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
				regionOverlayTexture.filterMode = FilterMode.Point;
				regionOverlayImage.texture = regionOverlayTexture;
			}

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int colorNumber = ActivePictureInfo.ColorNumbers[y][x];

					// фон не рисуем
					if (colorNumber == -1)
					{
						regionOverlayTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
						continue;
					}

					int regionId = ActivePictureInfo.RegionIds[y][x];

					bool isUnlocked = regionId >= 0 && ActivePictureInfo.UnlockedRegions.Contains(regionId);

					if (isUnlocked)
					{
						// доступная зона — прозрачная
						regionOverlayTexture.SetPixel(x, y, new Color(0, 0, 0, 0));
					}
					else
					{
						// закрытая зона — затемняем/маскируем
						// можно сделать полупрозрачный чёрный, чтобы чуть видно было
						regionOverlayTexture.SetPixel(x, y, new Color(255, 255, 255, 1f));
					}
				}
			}

			regionOverlayTexture.Apply();
		}
		
		/// <summary>
		/// Called when a PictureListItem is clicked
		/// </summary>
		private void OnPictureItemClicked(PictureInformation pictureInfo)
		{
			NumLevelsPlayed++;

			bool adShown = false;

			if (NumLevelsPlayed >= numLevelsTillAd)
			{
				NumLevelsPlayed = 0;

				#if BBG_MT_ADS
				adShown = BBG.MobileTools.MobileAdsManager.Instance.ShowInterstitialAd(() => { StartLevel(pictureInfo); });
				#endif
			}

			// If no ad was shown then start the level right away
			if (!adShown)
			{
				StartLevel(pictureInfo);
			}
		}

		/// <summary>
		/// Called when a PictureListItem is deleted
		/// </summary>
		private void OnPictureItemDeleted(PictureInformation pictureInfo)
		{
			DeleteLevelProgress(pictureInfo);
		}

		/// <summary>
		/// Selects the given powere
		/// </summary>
		private void SetSelectedPowerUp(PowerUp powerUp)
		{
			selectedPowerUp = powerUp;

			// Check if the player has enough to use the power up
			if ((selectedPowerUp == PowerUp.Bomb && CurrencyAmount < bombCost) ||
			    (selectedPowerUp == PowerUp.Wand && CurrencyAmount < magicWandCost))
			{
				selectedPowerUp = PowerUp.None;

				PopupManager.Instance.Show("not_enough_coins");
			}

			pictureScrollArea.PowerUpMode = selectedPowerUp != PowerUp.None;
			
			bombPowerUpButton.SetSelected(selectedPowerUp == PowerUp.Bomb);
			wandPowerUpButton.SetSelected(selectedPowerUp == PowerUp.Wand);
		}

		/// <summary>
		/// Setup the game using the given picture info by setting the size of the picture area and loading all the textures
		/// </summary>
		private void SetupLevel(PictureInformation pictureInfo)
		{
			// Clear any game resources hanging around for a previous game
			Clear();

			// Load all the textures needed by the level
			gameTextures = TextureController.Instance.LoadGameTextures(pictureInfo, grayscaleColorAlpha, selectionColorAlpha, incorrectColorAlpha);

			// Setup the mask that will end up hiding grid lines when there are blank tiles
			pictureContentMask.enabled	= MaskTexture != null;
			pictureMaskImage.texture			= MaskTexture;

			// Setup the images
			grayscaleImage.texture	= GrayscaleTexture;
			coloredImage.texture	= ColoredTexture;
			grayscaleImage.color	= Color.white;
			
			UpdateRegionOverlayTexture();

			// Setup the list of colors at the bottom of the screen
			colorPaletteList.SetupPaletteList(pictureInfo);

			SelectColor(0);
			

			// The color palette could change the size of the layout based on if its 1 row or 2 rows sow we have to wait for the
			// layout system to set the sizes before we can setup the picture area
			StartCoroutine(SetupPictureArea(pictureInfo));
		}

		/// <summary>
		/// Sets up the picture area.
		/// </summary>
		private IEnumerator SetupPictureArea(PictureInformation pictureInfo)
		{
			yield return new WaitForEndOfFrame();

			float areaWidth		= pictureScrollArea.RectT.rect.width - edgePadding * 2f;
			float areaHeight	= pictureScrollArea.RectT.rect.height - edgePadding * 2f;
			float contentWidth	= pictureInfo.XCells * maxCellSize;
			float contentHeight	= pictureInfo.YCells * maxCellSize;

			// Scale the contents width and height so they fit within the areas bounds
			float scaleToFitArea		= Mathf.Min(areaWidth / contentWidth, areaHeight / contentHeight, 1f);
			float scaleToMaxStarting	= Mathf.Min((pictureInfo.XCells * maxStartingCellSize) / contentWidth, (pictureInfo.YCells * maxStartingCellSize) / contentHeight, 1f);
			float scale					= Mathf.Min(scaleToFitArea, scaleToMaxStarting);
			float maxZoom				= (maxCellSize * pictureInfo.XCells) / (contentWidth * scale);

			// Reset the zoom of the pciture scroll area and set it's max zoom
			pictureScrollArea.enabled		= true;
			pictureContentArea.sizeDelta	= new Vector2(contentWidth, contentHeight);
			pictureContentArea.localScale	= new Vector3(scale, scale, 1f);
			pictureScrollArea.CurrentZoom	= 1f;
			pictureScrollArea.MaxZoom		= maxZoom;

			// Set the picture info for the ColorNumbersText to use
			colorNumbersText.PictureInfo = ActivePictureInfo;

			UpdateNumbers();

			// Make sure it's enabled. It gets disbled when a level is completed.
			colorNumbersText.enabled = true;

			SetupGridLines(pictureInfo, contentWidth / pictureInfo.XCells);

			SetOpenAreaButton(pictureInfo);
			
			// Setup the magnifying glass
			if (enableMagnifyingGlass)
			{
				magnifyingColorNumbersText.PictureInfo = ActivePictureInfo;
				SetupMagnifyingGlass(contentWidth, contentHeight);
			}
		}

		private void SetOpenAreaButton(PictureInformation pictureInfo)
		{
			buttonOpenArea.gameObject.SetActive(pictureInfo.UnlockedRegions.Count < pictureInfo.RegionsAmount);
		}

		/// <summary>
		/// Place the grid lines for the level
		/// </summary>
		private void SetupGridLines(PictureInformation pictureInfo, float cellSize)
		{
			for (int x = 0; x < pictureInfo.XCells + 1; x++)
			{
				SetupGridLine(gridLinePool.GetObject<RectTransform>(), x * cellSize - gridLineWidth / 2f, true);
			}

			for (int y = 0; y < pictureInfo.YCells + 1; y++)
			{
				SetupGridLine(gridLinePool.GetObject<RectTransform>(), y * cellSize - gridLineWidth / 2f, false);
			}
		}

		/// <summary>
		/// Setup one grid line
		/// </summary>
		private void SetupGridLine(RectTransform gridLine, float pos, bool vertical)
		{
			if (vertical)
			{
				gridLine.anchorMin			= new Vector2(0f, 0f);
				gridLine.anchorMax			= new Vector2(0f, 1f);
				gridLine.offsetMax			= Vector2.zero;
				gridLine.offsetMin			= Vector2.zero;
				gridLine.pivot				= new Vector2(0f, 0.5f);
				gridLine.anchoredPosition	= new Vector2(pos, gridLine.anchoredPosition.y);
				gridLine.sizeDelta			= new Vector2(gridLineWidth, gridLine.sizeDelta.y);
			}
			else
			{
				gridLine.anchorMin			= new Vector2(0f, 0f);
				gridLine.anchorMax			= new Vector2(1f, 0f);
				gridLine.offsetMax			= Vector2.zero;
				gridLine.offsetMin			= Vector2.zero;
				gridLine.pivot				= new Vector2(0.5f, 0f);
				gridLine.anchoredPosition	= new Vector2(gridLine.anchoredPosition.x, pos);
				gridLine.sizeDelta			= new Vector2(gridLine.sizeDelta.x, gridLineWidth);
			}
		}

		/// <summary>
		/// Setups the magnifying glass by instantiating the GameObjects and RawImages it needs
		/// </summary>
		private void SetupMagnifyingGlass(float contentWidth, float contentHeight)
		{
			magnifyingPictureContent.anchoredPosition	= Vector2.zero;
			magnifyingPictureContent.sizeDelta			= new Vector2(contentWidth, contentHeight);
			magnifyingColoredImage.texture				= ColoredTexture;
			magnifyingPictureMask.enabled				= MaskTexture != null;
			magnifyingPictureMaskImage.texture			= MaskTexture;
		}

		/// <summary>
		/// Selects the color at the give color index
		/// </summary>
		private void SelectColor(int colorIndex)
		{
			selectedColorIndex = colorIndex;

			// Set the selection texture
			selectionImage.texture = SelectionTexture;

			if (enableMagnifyingGlass)
			{
				magnifyingSelectionImage.texture = SelectionTexture;
			}
		}

		/// <summary>
		/// Colors a cell with the selected color at the given x/y cell coordinates
		/// </summary>
		private void ColorCell(int xCell, int yCell, int colorIndex, Vector2 position)
		{
			// Check if the cell still needs to be colored
			if (ActivePictureInfo.Progress[yCell][xCell] != -1)
			{
				int		correctColorIndex	= ActivePictureInfo.ColorNumbers[yCell][xCell];
				Color	color				= ActivePictureInfo.Colors[colorIndex];
				int paintAmount 		= ActivePictureInfo.CurrentPaintAmount[colorIndex];

				// If the given color index color equals the correct color
				if (colorIndex == correctColorIndex)
				{
					if (paintAmount > 0)
					{
						ActivePictureInfo.CurrentPaintAmount[colorIndex] -= 1;
						var b = colorPaletteList.GetComponentsInChildren<ColorListItem>();
						b[colorIndex].colorCountText.text = ActivePictureInfo.CurrentPaintAmount[colorIndex].ToString();
						
						//Debug.LogWarning(ActivePictureInfo.CurrentPaintAmount[colorIndex]);
						//Debug.LogWarning(ActivePictureInfo.StartPaintAmount[colorIndex]);
					
						Random random = new Random();
						int randomNumber = random.Next(0, 5);
						if (randomNumber == 2)
						{
							particleSystem.transform.position = position;
							particleSystem.GetComponent<UIParticle>().particles[0].Play();
						}
					
					
						// МЕНЯЕМ ЦИФРУ ЯЧЕЙКИ НА -1, ЗАТЕМ УМЕНЬШАЕМ КОЛИЧЕСТВО ОСТАВШИХСЯ ЯЧЕЕК ПО ЦВЕТУ
						ActivePictureInfo.Progress[yCell][xCell]	= -1;
						ActivePictureInfo.ColorsLeft[colorIndex]	-= 1;
						ActivePictureInfo.ReloadGrayscale			= true;

						ColoredTexture.SetPixel(xCell, yCell, color);

						UpdateNumbers();
					}

				}
				// Else it's the wrong color so color it with an alpha
				else
				{
					ActivePictureInfo.Progress[yCell][xCell]	= colorIndex;
					ActivePictureInfo.ReloadGrayscale			= true;

					color.a = Mathf.Clamp01(incorrectColorAlpha);

					ColoredTexture.SetPixel(xCell, yCell, color);
				}

				ColoredTexture.Apply();
			}
		}

		/// <summary>
		/// Checks if the level is completed
		/// </summary>
		private void CheckCompleted()
		{
			if (ActivePictureInfo.IsLevelComplete())
			{
				// If the level was completed while dragging then we need to cancel the drag or it will cause problems
				pictureScrollArea.CancelAllPointers();

				// Disable the PictureScrollArea so mouse wheel events do not get updated when the complete popup is active
				pictureScrollArea.enabled = false;

				// Disable the ColorNumbersText component, this fixes an issue where the numebers re-appear on the completed picture because
				// the progress is cleared
				colorNumbersText.enabled = false;

				// Award and currency for the level if the level has not already been completed and is has coins to award
				int awardAmount = (ActivePictureInfo.AwardOnComplete && !ActivePictureInfo.Completed) ? ActivePictureInfo.AwardAmount : 0;

				if (awardAmount > 0)
				{
					AddCurrency(awardAmount);
				}

				// Set the level completed and clear it's progress so it can be re-started
				ActivePictureInfo.SetCompleted();
				ActivePictureInfo.ClearProgress();

				// Show the level completed popup
				object[] popupData = {
					ColoredTexture,
					awardAmount
				};

				PopupManager.Instance.Show("level_complete_popup", popupData, null);
			}
		}
		
		private bool IsCellInUnlockedRegion(int xCell, int yCell)
		{
			// фон всё равно не красим
			if (ActivePictureInfo.ColorNumbers[yCell][xCell] == -1)
				return false;

			int regionId = ActivePictureInfo.RegionIds[yCell][xCell];
			return regionId >= 0 && ActivePictureInfo.UnlockedRegions.Contains(regionId);
		}
		

		/// <summary>
		/// Updates the numbers and grid lines, called whenever the picture area is moved / zoomed
		/// </summary>
		private void UpdateNumbers()
		{
			// Get the size of a cell on the screen
			float cellSize = (pictureContentArea.rect.width * pictureContentArea.localScale.x * pictureScrollArea.CurrentZoom) / ActivePictureInfo.XCells;

			// Check if the cell is large enough to start showing numbers
			if (cellSize >= minCellSizeForNumbers)
			{
				Vector3[] pictureWorldCorners		= new Vector3[4];
				Vector3[] scrollAreaWorldCorners	= new Vector3[4];

				pictureContentArea.GetWorldCorners(pictureWorldCorners);
				pictureScrollArea.RectT.GetWorldCorners(scrollAreaWorldCorners);

				// Calculate what cells are visible in the picture area
				float pictureLeft	= pictureWorldCorners[0].x;
				float pictureRight	= pictureWorldCorners[2].x;
				float pictureTop	= pictureWorldCorners[2].y;
				float pictureBottom	= pictureWorldCorners[0].y;

				float areaLeft		= scrollAreaWorldCorners[0].x;
				float areaRight		= scrollAreaWorldCorners[2].x;
				float areaTop		= scrollAreaWorldCorners[2].y;
				float areaBottom	= scrollAreaWorldCorners[0].y;

				float worldPictureWidth		= pictureRight - pictureLeft;
				float worldPictureHeight	= pictureTop - pictureBottom;

				float left		= Mathf.Max(0f, areaLeft - pictureLeft) / worldPictureWidth;
				float right		= Mathf.Max(0f, pictureRight - areaRight) / worldPictureWidth;
				float top		= Mathf.Max(0f, pictureTop - areaTop) / worldPictureHeight;
				float bottom	= Mathf.Max(0f, areaBottom - pictureBottom) / worldPictureHeight;

				int startX	= Mathf.FloorToInt(left * ActivePictureInfo.XCells);
				int startY	= Mathf.FloorToInt(bottom * ActivePictureInfo.YCells);
				int endX	= ActivePictureInfo.XCells - Mathf.FloorToInt(right * ActivePictureInfo.XCells) - 1;
				int endY	= ActivePictureInfo.YCells - Mathf.FloorToInt(top * ActivePictureInfo.YCells) - 1;

				UpdateNumbers(colorNumbersText, startX, startY, endX, endY);

				// Set the alpha of the numbers based on how large the cells are
				float diff				= (maxCellSize - minCellSizeForNumbers) / 1.25f;
				numbersTextColor.a		= (cellSize - minCellSizeForNumbers) / diff;
				colorNumbersText.color	= numbersTextColor;
			}
			// Else clear the numbers
			else
			{
				// Set it so the ColorNumbersText doesn't draw any celles
				UpdateNumbers(colorNumbersText, 0, 0, -1, -1);

				// Hide the numbers text
				numbersTextColor.a		= 0f;
				colorNumbersText.color	= numbersTextColor;
			}
		}

		/// <summary>
		/// Updates the magnifying glass using the new screen position of the mouse
		/// </summary>
		private void UpdateMagnifyingGlass(Vector2 screenPosition)
		{
			// Position the magnifying glass on the mouse cursor
			Vector3 worldPosition;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(pictureContentArea, screenPosition, null, out worldPosition);

			magnifyingGlass.position			= worldPosition;
			magnifyingGlass.anchoredPosition	= new Vector2(magnifyingGlass.anchoredPosition.x, magnifyingGlass.anchoredPosition.y + magnifyingGlassYOffset);

			// Position the content inside the magnifying glass so it lines up with where the cursor is
			Vector2 localPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(pictureContentArea, screenPosition, null, out localPosition);

			magnifyingPictureContent.anchoredPosition = -localPosition;

			Vector2 contentSize = new Vector2(
				magnifyingPictureContent.rect.width,
				magnifyingPictureContent.rect.height);
			
			localPosition.x += contentSize.x / 2f;
			localPosition.y += contentSize.y / 2f;

			// Update the visible numbers in the magnifying glass
			Vector2 leftPosition	= new Vector2(localPosition.x - magnifyingGlass.rect.width / 2f, localPosition.y);
			Vector2 rightPosition	= new Vector2(localPosition.x + magnifyingGlass.rect.width / 2f, localPosition.y);
			Vector2 topPosition		= new Vector2(localPosition.x, localPosition.y + magnifyingGlass.rect.height / 2f);
			Vector2 bottomPosition	= new Vector2(localPosition.x, localPosition.y - magnifyingGlass.rect.height / 2f);

			int startX, startY, endX, endY, tempX, tempY;

			CalculateCellFromPosition(leftPosition, contentSize, out startX, out tempY);
			CalculateCellFromPosition(rightPosition, contentSize, out endX, out tempY);
			CalculateCellFromPosition(topPosition, contentSize, out tempX, out endY);
			CalculateCellFromPosition(bottomPosition, contentSize, out tempX, out startY);

			UpdateNumbers(magnifyingColorNumbersText, startX, startY, endX, endY);
			UpdateMagnifyingGlassGridLines(startX, startY, endX, endY);
		}

		private void UpdateNumbers(ColorNumbersText numbersText, int startX, int startY, int endX, int endY)
		{
			// Update the numbers using the new start/end x/y cells that are visible on the scree
			numbersText.StartX	= startX;
			numbersText.StartY	= startY;
			numbersText.EndX	= endX;
			numbersText.EndY	= endY;
			numbersText.CellSize	= maxCellSize;

			// Set the position of the numbers
			float xPos = (float)startX * maxCellSize + maxCellSize / 2f;
			float yPos = -((float)(ActivePictureInfo.YCells - endY - 1) * maxCellSize + maxCellSize / 2f);

			numbersText.rectTransform.anchoredPosition = new Vector2(xPos, yPos);
			numbersText.SetAllDirty();
		}

		/// <summary>
		/// Updates the grid lines inside the magnifying glass
		/// </summary>
		private void UpdateMagnifyingGlassGridLines(int startX, int startY, int endX, int endY)
		{
			float cellSize = magnifyingPictureContent.rect.width / ActivePictureInfo.XCells;

			int grindLineIndex = 0;

			for (int x = startX; x <= endX; x++)
			{
				SetupMagnifyingGlassGridLine(x * cellSize - gridLineWidth / 2f, true, grindLineIndex);

				grindLineIndex++;
			}

			for (int y = startY; y <= endY; y++)
			{
				SetupMagnifyingGlassGridLine(y * cellSize - gridLineWidth / 2f, false, grindLineIndex);

				grindLineIndex++;
			}
		}

		/// <summary>
		/// Places a single grid line in the magnifying glass at the given position
		/// </summary>
		private void SetupMagnifyingGlassGridLine(float pos, bool vertical, int grindLineIndex)
		{
			RectTransform gridLine = null;

			// Get a new grid line instance to use
			if (grindLineIndex >= magnifyingGlassGridLines.Count)
			{
				// If theres no more to re-use then create a new one
				gridLine = gridLinePool.GetObject<RectTransform>(magnifyingGlassGridLineContainer);
				magnifyingGlassGridLines.Add(gridLine);
			}
			else
			{
				gridLine = magnifyingGlassGridLines[grindLineIndex];
			}

			SetupGridLine(gridLine, pos, vertical);
		}

		/// Закрашивает клетки внутри прямоугольника только если
		/// все НЕЗАКРАШЕННЫЕ (Progress != -1) клетки внутри имеют одну и ту же цифру.
		/// Уже закрашенные (Progress == -1) или фон (ColorNumbers == -1) игнорируются.
		public void ColorArea(Vector2 startScreen, Vector2 endScreen, bool mustMatchSelectedColor = true)
		{
		    // === Экран -> локальные координаты полотна ===
		    Vector2 localA, localB;
		    RectTransformUtility.ScreenPointToLocalPointInRectangle(pictureContentArea, startScreen, null, out localA);
		    RectTransformUtility.ScreenPointToLocalPointInRectangle(pictureContentArea, endScreen,   null, out localB);

		    Vector2 contentSize = new Vector2(pictureContentArea.rect.width, pictureContentArea.rect.height);
		    localA += contentSize * 0.5f;
		    localB += contentSize * 0.5f;

		    Vector2 minL = Vector2.Min(localA, localB);
		    Vector2 maxL = Vector2.Max(localA, localB);
		    minL = Vector2.Max(minL, Vector2.zero);
		    maxL = Vector2.Min(maxL, contentSize);
		    if (maxL.x <= minL.x || maxL.y <= minL.y) return;

		    const float eps = 0.001f;
		    int xMin, yMin, xMax, yMax;
		    CalculateCellFromPosition(minL + new Vector2(eps, eps), contentSize, out xMin, out yMin);
		    CalculateCellFromPosition(maxL - new Vector2(eps, eps), contentSize, out xMax, out yMax);

		    xMin = Mathf.Clamp(xMin, 0, ActivePictureInfo.ColorNumbers[0].Count - 1);
		    xMax = Mathf.Clamp(xMax, 0, ActivePictureInfo.ColorNumbers[0].Count - 1);
		    yMin = Mathf.Clamp(yMin, 0, ActivePictureInfo.ColorNumbers.Count - 1);
		    yMax = Mathf.Clamp(yMax, 0, ActivePictureInfo.ColorNumbers.Count - 1);

		    // === 1-я проходка: проверяем однородность среди НЕЗАКРАШЕННЫХ клеток ===
		    int foundColor = int.MinValue; // целевая цифра незакрашенных клеток в выделении
		    bool hasUnpainted = false;

		    for (int y = yMin; y <= yMax; y++)
		    {
		        for (int x = xMin; x <= xMax; x++)
		        {
		            int num = ActivePictureInfo.ColorNumbers[y][x];
		            if (num == -1) continue; // фон не красим
		            
		            // НОВОЕ: полностью игнорируем закрытые регионы
		            if (!IsCellInUnlockedRegion(x, y))
			            continue;

		            bool alreadyPainted = ActivePictureInfo.Progress[y][x] == -1;
		            if (alreadyPainted) continue; // уже закрашенные игнорим в проверке

		            if (!hasUnpainted)
		            {
		                foundColor = num;
		                hasUnpainted = true;
		            }
		            else if (num != foundColor)
		            {
		                // среди НЕЗАКРАШЕННЫХ встретились разные цифры — отменяем
		                return;
		            }
		        }
		    }

		    // Внутри нет незакрашенных подходящих клеток — делать нечего
		    if (!hasUnpainted) return;

		    // Можно требовать совпадение с выбранным цветом
		    if (mustMatchSelectedColor && foundColor != selectedColorIndex)
		        return;

		    // === 2-я проходка: красим только незакрашенные клетки с нужной цифрой ===
		    for (int y = yMin; y <= yMax; y++)
		    {
		        for (int x = xMin; x <= xMax; x++)
		        {
			        if (ActivePictureInfo.ColorNumbers[y][x] == foundColor &&
			            ActivePictureInfo.Progress[y][x] != -1 &&        // не трогаем уже закрашенные
			            IsCellInUnlockedRegion(x, y))
			        {
		                ColorCell(x, y, selectedColorIndex, startScreen);
		            }
		        }
		    }

		    SetSelectedPowerUp(PowerUp.None);
		    colorPaletteList.UpdateCompleted();
		    CheckCompleted();
		}

		public void OnButtonOpenAreaClick()
		{
			if (ActivePictureInfo.UnlockedRegions.Count < ActivePictureInfo.RegionsAmount)
			{
				ActivePictureInfo.UnlockedRegions.Add(ActivePictureInfo.UnlockedRegions.Count);
				UpdateRegionOverlayTexture();
				colorPaletteList.SetupPaletteList(ActivePictureInfo);
			}

			if (ActivePictureInfo.UnlockedRegions.Count == ActivePictureInfo.RegionsAmount 
			    && buttonOpenArea.isActiveAndEnabled)
			{
				buttonOpenArea.gameObject.SetActive(false);
			}
		}
		
				
		/// РАЗУКРАШИВАЕТ КВАДРАТ В УКАЗАННОЙ ПОЗИЦИИ, ПОЛУЧЕННОЙ С АРГУМЕНТА ИВЕНТА
		private void ColorPixel(Vector2 screenPosition)
		{
			Vector2 localPosition;

			//ПЕРЕВОДИМ ГЛОБАЛЬНУЮ ПОЗИЦИЮ КЛИКА В ПОЗИЦИЮ НА ЗОНЕ РАСКРАСКИ, ДОСТАВАЯ localPosition
			RectTransformUtility.ScreenPointToLocalPointInRectangle(pictureContentArea, screenPosition, null, out localPosition);

			//ПОЛУЧАЕМ РАЗМЕР ПОЛОТНА РАСКРАСКИ
			Vector2 contentSize = new Vector2(pictureContentArea.rect.width, pictureContentArea.rect.height);

			//СМЕЩАЕТ ЦЕНТР ПОЛОТНА (КООРДИНАТЫ 0,0) С СЕРЕДИНЫ В ЛЕВЫЙ НИЖНИЙ УГОЛ
			localPosition.x += contentSize.x / 2f;
			localPosition.y += contentSize.y / 2f;

			//ПРОВЕРЯЕМ, ЧТО КЛИК ПОПАЛ ВНУТРЬ ПРЯМОУГОЛЬНИКА (ПОЛОТНА
			if ((localPosition.x >= 0 && localPosition.y >= 0) && (localPosition.x <= contentSize.x && localPosition.y <= contentSize.y))
			{
				int xCell, yCell;

				//ОПРЕДЕЛЫЕМ КООРДИНАТЫ ПИКСЕЛЯ НА СЕТКЕ
				CalculateCellFromPosition(localPosition, contentSize, out xCell, out yCell);

				//ПРОВЕРЯЕМ МОЖНО ЛИ ЭТУ КЛЕТКУ ЗАКРАШИВАТЬ (-1 ЭТО ФОН)
				if (ActivePictureInfo.ColorNumbers[yCell][xCell] != -1 &&
				    IsCellInUnlockedRegion(xCell, yCell))
				{
					switch (selectedPowerUp)
					{
						case PowerUp.None:
							ColorCell(xCell, yCell, selectedColorIndex, screenPosition);
							colorPaletteList.UpdateCompleted();
							CheckCompleted();
							break;
						case PowerUp.Bomb:
							if (CurrencyAmount >= bombCost)
							{
								CurrencyAmount -= bombCost;

								FillArea(xCell, yCell);
								colorPaletteList.UpdateCompleted();
								CheckCompleted();
							}
							break;
						case PowerUp.Wand:
							if (CurrencyAmount >= magicWandCost)
							{
								CurrencyAmount -= magicWandCost;

								FillCluster(xCell, yCell);
								colorPaletteList.UpdateCompleted();
								CheckCompleted();
							}
							break;
					}

					// Now that a click has happened make sure any power up that may have been selected is no longer selected
					SetSelectedPowerUp(PowerUp.None);
				}
			}
		}

		/// <summary>
		/// Calculates the cell from position inside the content
		/// </summary>
		private void CalculateCellFromPosition(Vector2 localPosition, Vector2 contentSize, out int xCell, out int yCell)
		{
			Vector2	percentage = new Vector2(localPosition.x / contentSize.x, localPosition.y / contentSize.y);

		 	xCell = Mathf.FloorToInt(ActivePictureInfo.XCells * percentage.x);
			yCell = Mathf.FloorToInt(ActivePictureInfo.YCells * percentage.y);

			xCell = Mathf.Clamp(xCell, 0, ActivePictureInfo.XCells - 1);
			yCell = Mathf.Clamp(yCell, 0, ActivePictureInfo.YCells - 1);
		}

		/// <summary>
		/// Fills an area of the picture starting at the given x/y cell for a radius
		/// </summary>
		private void FillArea(int x, int y)
		{
			int xStart	= Mathf.Max(x - bombRadius, 0);
			int xEnd	= Mathf.Min(x + bombRadius, ActivePictureInfo.XCells - 1);

			int yStart	= Mathf.Max(y - bombRadius, 0);
			int yEnd	= Mathf.Min(y + bombRadius, ActivePictureInfo.YCells - 1);

			for (int i = xStart; i <= xEnd; i++)
			{
				for (int j = yStart; j <= yEnd; j++)
				{
					float distance = Vector2.Distance(new Vector2(i, j), new Vector2(x, y)) + 0.5f;

					if (distance <= (float)bombRadius)
					{
						ColorCell(i, j, ActivePictureInfo.ColorNumbers[j][i], Vector2.up);
					}
				}
			}
		}

		/// <summary>
		/// Fills this cell and all cells connected to the starting x/y cell that have the same color number
		/// </summary>
		private void FillCluster(int x, int y)
		{
			FillCluster(x, y, ActivePictureInfo.ColorNumbers[y][x], new HashSet<string>());
		}

		/// <summary>
		/// If the given x/y cell has the same number has the color number it fills the color in and recursively calls FillBlob on adjacent cells
		/// </summary>
		private void FillCluster(int x, int y, int colorIndex, HashSet<string> traversedCells)
		{
			// Check if this cell is within the bounds of the picture and has a color number and the
			if(x < 0 || x >= ActivePictureInfo.XCells ||
				y < 0 || y >= ActivePictureInfo.YCells ||
				ActivePictureInfo.ColorNumbers[y][x] == -1 ||
				ActivePictureInfo.ColorNumbers[y][x] != colorIndex)
			{
				return;
			}

			string cellKey = string.Format("{0}_{1}", x, y);

			// Check if this cell has already been traversed by FillBlob
			if (traversedCells.Contains(cellKey))
			{
				return;
			}

			// Check if this cell is already colored in with the correct color
			if (!ActivePictureInfo.HasProgress || ActivePictureInfo.Progress[y][x] != -1)
			{
				ColorCell(x, y, colorIndex, Vector2.zero);
			}

			// Add this cells key to the traversed hashset to indicate it has been processed
			traversedCells.Add(cellKey);

			// Recursively call recursively with the four cells adjacent to this cell
			FillCluster(x - 1, y, colorIndex, traversedCells);
			FillCluster(x + 1, y, colorIndex, traversedCells);
			FillCluster(x, y - 1, colorIndex, traversedCells);
			FillCluster(x, y + 1, colorIndex, traversedCells);
		}

		/// <summary>
		/// Invoked when the picture area is clicked without it moving of zooming
		/// </summary>
		private void OnPictureAreaClicked(Vector2 screenPosition)
		{
			if (selectedColorIndex == -1)
			{
				// No color selected, no point in calculating the clicked x/y cell
				return;
			}
			
			ColorPixel(screenPosition);
		}
		
		private void OnPictureAreaSelected(Vector2 start, Vector2 end)
		{
			ColorArea(start, end);
		}

		/// <summary>
		/// Invoked when the picture areas scale changes (ei. it zoomed in/out)
		/// </summary>
		private void OnPictureAreaZoomed(Vector2 screenPosition)
		{
			UpdateNumbers();

			float garyscaleAlpha = 1f - (pictureScrollArea.CurrentZoom - pictureScrollArea.MinZoom) / (pictureScrollArea.MaxZoom - pictureScrollArea.MinZoom);

			grayscaleImage.color	= new Color(1f, 1f, 1f, garyscaleAlpha);
			gridLineContainer.alpha = 1f - garyscaleAlpha;
		}

		/// <summary>
		/// Invoked when the picture areas position changes
		/// </summary>
		private void OnPictureAreaMoved(Vector2 screenPosition)
		{
			UpdateNumbers();
		}

		/// <summary>
		/// Invoked when dragging has started on the picture area
		/// </summary>
		private void OnPictureAreaDragStart(Vector2 screenPosition)
		{
			if (selectedColorIndex == -1)
			{
				return;
			}

			ColorPixel(screenPosition);

			// Update the position of the magnifying glass
			if (enableMagnifyingGlass)
			{
				magnifyingGlass.gameObject.SetActive(true);

				UpdateMagnifyingGlass(screenPosition);
			}
		}

		/// <summary>
		/// Invoked when the pointer has moved when dragging on the picture area
		/// </summary>
		private void OnPictureAreaDragged(Vector2 screenPosition)
		{
			if (selectedColorIndex == -1)
			{
				return;
			}

			ColorPixel(screenPosition);

			if (enableMagnifyingGlass)
			{
				UpdateMagnifyingGlass(screenPosition);
			}
		}

		/// <summary>
		/// Invoked when dragging has ended on the picture area
		/// </summary>
		private void OnPictureAreaDragEnd()
		{
			if (enableMagnifyingGlass)
			{
				magnifyingGlass.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Clears all game resources
		/// </summary>
		private void Clear()
		{
			pictureScrollArea.ResetObj();

			gridLinePool.ReturnAllObjectsToPool();
			gridLineContainer.alpha = 0f;

			if (enableMagnifyingGlass)
			{
				magnifyingGlassGridLines.Clear();
			}

			colorPaletteList.Clear();

			SetSelectedPowerUp(PowerUp.None);

			// Destroy all the textures except for the grayscale texture
			if (gameTextures != null)
			{
				for (int i = 0; i < gameTextures.Count; i++)
				{
					Texture2D texture = gameTextures[i];

					if (texture != GrayscaleTexture)
					{
						Destroy(texture);
					}
				}

				gameTextures.Clear();
			}
		}

		/// <summary>
		/// Creates the magnifying glass objects
		/// </summary>
		private void CreatePictureContentObjects()
		{
			GameObject pictureContent = new GameObject("picture_content");

			pictureContentArea = pictureContent.AddComponent<RectTransform>();
			pictureContentArea.SetParent(pictureContainer, false);

			pictureContentMask		= pictureContent.AddComponent<Mask>();
			pictureMaskImage		= pictureContent.AddComponent<RawImage>();
			grayscaleImage			= CreateContainerObj("grayscale_texture", pictureContentArea).AddComponent<RawImage>();
			selectionImage			= CreateContainerObj("selection_texture", pictureContentArea).AddComponent<RawImage>();
			gridLineContainer		= CreateContainerObj("grid_lines", pictureContentArea).AddComponent<CanvasGroup>();
			coloredImage			= CreateContainerObj("colored_texture", pictureContentArea).AddComponent<RawImage>();
			colorNumbersText		= CreateColorNumbersText(pictureContentArea);
// НОВОЕ: оверлей регионов
			regionOverlayImage      = CreateContainerObj("region_overlay", pictureContentArea).AddComponent<RawImage>();
			regionOverlayImage.raycastTarget = false; // чтобы не мешал кликам
			regionOverlayImage.color = Color.white;   // цвет берём из текстуры
			regionOverlayImage.transform.SetAsLastSibling();
			
		}

		/// <summary>
		/// Creates the magnifying glass objects
		/// </summary>
		private void CreateMagnifyingGlass()
		{
			GameObject pictureContent = new GameObject("picture_content");

			magnifyingPictureContent = pictureContent.AddComponent<RectTransform>();
			magnifyingPictureContent.SetParent(magnifyingGlassContainer, false);

			magnifyingPictureMaskImage			= pictureContent.AddComponent<RawImage>();
			magnifyingPictureMask				= pictureContent.AddComponent<Mask>();
			magnifyingSelectionImage			= CreateContainerObj("selection_texture", magnifyingPictureContent).AddComponent<RawImage>();
			magnifyingGlassGridLineContainer	= CreateContainerObj("grid_lines", magnifyingPictureContent).transform as RectTransform;
			magnifyingColoredImage				= CreateContainerObj("colored_texture", magnifyingPictureContent).AddComponent<RawImage>();
			magnifyingColorNumbersText			= CreateColorNumbersText(magnifyingPictureContent);
		}

		/// <summary>
		/// Creates a GameObject, sets it's parent, then sets the anchors to stretch to fill
		/// </summary>
		private GameObject CreateContainerObj(string containerName, Transform containersParent)
		{
			// Create numbers container
			GameObject		containerObj	= new GameObject(containerName);
			RectTransform	containerRectT	= containerObj.AddComponent<RectTransform>();

			containerRectT.SetParent(containersParent, false);

			containerRectT.anchoredPosition	= Vector2.zero;
			containerRectT.anchorMin		= Vector2.zero;
			containerRectT.anchorMax		= Vector2.one;
			containerRectT.offsetMax		= Vector2.zero;
			containerRectT.offsetMin		= Vector2.zero;

			return containerObj;
		}

		private ColorNumbersText CreateColorNumbersText(Transform parent)
		{
			ColorNumbersText numbersText = new GameObject("numbers", typeof(RectTransform)).AddComponent<ColorNumbersText>();

			numbersText.font				= numbersTextFont;
			numbersText.fontSize			= numbersTextFontSize;
			numbersText.color				= numbersTextColor;
			numbersText.text				= "0123456789";
			numbersText.verticalOverflow	= VerticalWrapMode.Overflow;
			numbersText.horizontalOverflow	= HorizontalWrapMode.Overflow;
			numbersText.LetterSpacing		= numbersTextLetterSpacing;

			numbersText.rectTransform.SetParent(parent, false);
			numbersText.rectTransform.sizeDelta	= Vector2.zero;
			numbersText.rectTransform.anchorMin	= new Vector2(0, 1);
			numbersText.rectTransform.anchorMax	= new Vector2(0, 1);
			numbersText.rectTransform.pivot		= new Vector2(0, 1);

			return numbersText;
		}

		#endregion

		#region Save Methods

		public override Dictionary<string, object> Save()
		{
			Dictionary<string, object>	json				= new Dictionary<string, object>();
			List<object>				pictureInfosJson	= new List<object>();

			for (int i = 0; i < PlayedPictureInfos.Count; i++)
			{
				pictureInfosJson.Add(PlayedPictureInfos[i].GetSaveData());
			}

			json["picture_infos"]	= pictureInfosJson;
			json["currency_amount"]	= CurrencyAmount;

			return json;
		}
		
		protected override void LoadSaveData(bool exists, JSONNode saveData)
		{
			Dictionary<string, JSONNode>	savedPictureInfos		= new Dictionary<string, JSONNode>();
			List<string>					pictureInfoLoadOrder	= new List<string>();

			// If the save file exists load the saved json
			if (exists)
			{
				JSONNode	json				= saveData;
				JSONArray	pictureInfosJson	= json["picture_infos"].AsArray;

				for (int i = 0; i < pictureInfosJson.Count; i++)
				{
					JSONNode	pictureInfoJson	= pictureInfosJson[i];
					string		id				= pictureInfoJson["id"].Value;

					// Need to keep track of the order the pictures were loaded in
					pictureInfoLoadOrder.Add(id);

					savedPictureInfos[id] = pictureInfoJson;
				}
				
				/*JSONNode regions = pictureInfosJson[0]["unlocked_regions"];
				foreach (JSONNode node in regions.AsArray)
				{
					unlockedRegions.Add(node.AsInt);
				}*/

				CurrencyAmount = json["currency_amount"].AsInt;
			}
			else
			{
				CurrencyAmount = startingCurrency;
			}

			PictureInformation[] playedPictureInfos = new PictureInformation[pictureInfoLoadOrder.Count];

			// Load the picture files
			PictureInfos = new List<PictureInformation>();

			Dictionary<string, PictureInformation> loadedPictureInfos = new Dictionary<string, PictureInformation>();

			for (int i = 0; i < categoryInfos.Count; i++)
			{
				CategoryInfo categoryInfo = categoryInfos[i];

				categoryInfo.PictureInfos = new List<PictureInformation>();

				for (int j = 0; j < categoryInfo.pictureFiles.Count; j++)
				{
					PictureInformation pictureInfo = new PictureInformation(categoryInfo.pictureFiles[j].text);

					// Check if we have already loaded this picture info. This can happen if a picture file appears in more than one category.
					if (loadedPictureInfos.ContainsKey(pictureInfo.Id))
					{
						// Add the already loaded picture info to the category so it will display in the library for that category
						categoryInfo.PictureInfos.Add(loadedPictureInfos[pictureInfo.Id]);
					}
					else
					{
						// Check if this picture info has saved data and if so load it
						if (savedPictureInfos.ContainsKey(pictureInfo.Id))
						{
							pictureInfo.LoadSaveData(savedPictureInfos[pictureInfo.Id]);

							// Add the picture info to the array of picture infos in the order it was loaded
							playedPictureInfos[pictureInfoLoadOrder.IndexOf(pictureInfo.Id)] = pictureInfo;
						}

						// Add the picture information to both the global list of all picture infos and the its specific category
						PictureInfos.Add(pictureInfo);
						categoryInfo.PictureInfos.Add(pictureInfo);

						loadedPictureInfos.Add(pictureInfo.Id, pictureInfo);
					}
				}
			}

			// Load the user created pictures
			List<string> userCreatedContents = CreatePictureController.Instance.GetUserCreatedPictureContents();

			for (int i = 0; i < userCreatedContents.Count; i++)
			{
				PictureInformation pictureInfo = new PictureInformation(userCreatedContents[i]);

				// Check if this picture info has saved data and if so load it
				if (savedPictureInfos.ContainsKey(pictureInfo.Id))
				{
					pictureInfo.LoadSaveData(savedPictureInfos[pictureInfo.Id]);

					// Add the picture info to the array of picture infos in the order it was loaded
					playedPictureInfos[pictureInfoLoadOrder.IndexOf(pictureInfo.Id)] = pictureInfo;
				}
			}

			// Remove any null picture infos, this means that they were in the save file but are no longer in pictureFiles (must have been removed)
			PlayedPictureInfos = new List<PictureInformation>(playedPictureInfos);

			for (int i = PlayedPictureInfos.Count - 1; i >= 0; i--)
			{
				if (PlayedPictureInfos[i] == null)
				{
					PlayedPictureInfos.RemoveAt(i);
				}
			}
		}

		#endregion
	}
}
