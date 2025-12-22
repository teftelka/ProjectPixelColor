using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBG.ColorByNumbers
{
	public class PictureInformation
	{
		#region Member Variables

		private string			fileContents;
		private bool			isFileLoaded;
		private bool			isIdLoaded;

		private string			id;
		private int				xCells;
		private int				yCells;
		private List<List<int>>	colorNumbers;
		private List<Color>		colors;
		private List<int>		startPaintAmount;
		private bool			hasBlankCells;
		private bool			isLocked;
		private int				unlockAmount;
		private bool			awardOnComplete;
		private int				awardAmount;
		private List<List<int>> regionIds;
		private int regionsAmount;

		private List<int> unlockedRegions;
		//private int colorsCount;

		// Saved matrix of color numbers, -1 means it's colored in, number >= 0 means it still needs to be colored
		private List<List<int>>	progress;
		private List<int>		colorsLeft;
		private List<int>		currentPaintAmount;
		private List<List<int>>	colorsAvailable;
		private bool			unlocked;
		private bool			completed;

		#endregion

		#region Properties

		/// <summary>
		/// If true then the grayscale for the menu screen needs to be re-loaded and not use the one in the cache
		/// </summary>
		public bool ReloadGrayscale { get; set; }

		/// <summary>
		/// Gets the unique id of the picture
		/// </summary>
		public string Id
		{
			get
			{
				if (!isIdLoaded)
				{
					LoadIdFromPictureFile();
				}

				return id;
			}
		}
		
		public List<List<int>> RegionIds
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return regionIds;
			}
		}
		
		public int RegionsAmount
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return regionsAmount;
			}
		}
		
		public List<List<int>> ColorsAvailable
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return colorsAvailable;
			}
		}

		/// <summary>
		/// Gets the number of X cells in the picture
		/// </summary>
		/// <value>The X cells.</value>
		public int XCells
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return xCells;
			}
		}

		/// <summary>
		/// Gets the number of Y cells in the picture
		/// </summary>
		/// <value>The Y cells.</value>
		public int YCells
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return yCells;
			}
		}

		/// <summary>
		/// Gets a matrix of each cell and the color number for the cell
		/// </summary>
		public List<List<int>> ColorNumbers
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return colorNumbers;
			}
		}

		/// <summary>
		/// Gets a list of all the colors in the picture, the index of the color is it's number
		/// </summary>
		public List<Color> Colors
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return colors;
			}
		}
		
		public List<int> StartPaintAmount
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return startPaintAmount;
			}
		}
		
		public List<int> CurrentPaintAmount
		{
			get
			{
				if (currentPaintAmount == null)
				{
					InitProgress();
				}

				return currentPaintAmount;
			}
		}
		
		public List<int> UnlockedRegions
		{
			get
			{
				if (unlockedRegions == null)
				{
					InitProgress();
				}

				return unlockedRegions;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has blank cells.
		/// </summary>
		public bool HasBlankCells
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return hasBlankCells;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is locked until purchased with in-game currency
		/// </summary>
		public bool IsLocked
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return isLocked && !unlocked;
			}
		}

		/// <summary>
		/// Gets the unlock amount
		/// </summary>
		public int UnlockAmount
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return unlockAmount;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance awards in-game currency when completed
		/// </summary>
		public bool AwardOnComplete
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return awardOnComplete;
			}
		}

		/// <summary>
		/// Gets the award amount
		/// </summary>
		public int AwardAmount
		{
			get
			{
				if (!isFileLoaded)
				{
					LoadPictureFile();
				}

				return awardAmount;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has saved progress
		/// </summary>
		public bool HasProgress
		{
			get
			{
				return progress != null;
			}
		}

		public List<List<int>> Progress
		{
			get
			{
				if (progress == null)
				{
					InitProgress();
				}

				return progress;
			}
		}

		public List<int> ColorsLeft
		{
			get
			{
				if (colorsLeft == null)
				{
					InitProgress();
				}

				return colorsLeft;
			}
		}

		public bool Completed
		{
			get
			{
				return completed;
			}
		}

		#endregion

		#region Public Methods

		public PictureInformation(string content)
		{
			fileContents = content.Replace("\r", "");
		}

		/// <summary>
		/// Unlocks this PictureInformation instance so the user can play any number of times
		/// </summary>
		public void SetUnlocked()
		{
			unlocked = true;
		}

		/// <summary>
		/// Sets this PictureInformation instance completed
		/// </summary>
		public void SetCompleted(bool isComplete = true)
		{
			completed = isComplete;
		}

		/// <summary>
		/// Clears any progress, makes it so this instance was never started
		/// </summary>
		public void ClearProgress()
		{
			progress		= null;
			colorsLeft		= null;
			currentPaintAmount	= null;
			ReloadGrayscale	= true;
			unlockedRegions	= null;
		}

		/// <summary>
		/// Checks if a given color is complete (Has all it's pixels colored in)
		/// </summary>
		public bool IsColorComplete(int colorIndex)
		{
			return HasProgress && colorsLeft[colorIndex] == 0;
		}

		/// <summary>
		/// Checks if this PictureInformation has all of it's pixels colored in
		/// </summary>
		public bool IsLevelComplete()
		{
			// Check if the level has any progress, it cant be complete if it has even been started yet
			if (HasProgress)
			{
				bool allColorsComplete = true;

				// Check if each of the colors are complete
				for (int i = 0; i < colors.Count; i++)
				{
					if (!IsColorComplete(i))
					{
						allColorsComplete = false;

						break;
					}
				}

				return allColorsComplete;
			}

			return false;
		}

		public Dictionary<string, object> GetSaveData()
		{
			Dictionary<string, object> saveData = new Dictionary<string, object>();

			saveData["has_progress"] = HasProgress;

			if (HasProgress)
			{
				saveData["progress"]	= Progress;
				saveData["colors_left"]	= ColorsLeft;
				saveData["paint_amount"]	= CurrentPaintAmount;
				saveData["unlocked_regions"]	= UnlockedRegions;
			}

			saveData["id"]			= Id;
			saveData["completed"]	= completed;
			saveData["unlocked"]	= unlocked;

			return saveData;
		}

		public void LoadSaveData(JSONNode saveData)
		{
			if (saveData["has_progress"].AsBool)
			{
				progress	= new List<List<int>>();
				colorsLeft	= new List<int>();
				currentPaintAmount = new List<int>();
				unlockedRegions = new List<int>();

				foreach (JSONArray list in saveData["progress"].AsArray)
				{
					List<int> temp = new List<int>();

					foreach (JSONNode item in list)
					{
						temp.Add(item.AsInt);
					}

					progress.Add(temp);
				}

				foreach (JSONNode item in saveData["colors_left"].AsArray)
				{
					colorsLeft.Add(item.AsInt);
				}
				
				foreach (JSONNode paint in saveData["paint_amount"].AsArray)
				{
					currentPaintAmount.Add(paint.AsInt);
				}
				
				foreach (JSONNode node in saveData["unlocked_regions"].AsArray)
				{
					unlockedRegions.Add(node.AsInt);
				}
			}

			completed	= saveData["completed"].AsBool;
			unlocked	= saveData["unlocked"].AsBool;
		}

		public void InitProgress()
		{
			if (!isFileLoaded)
			{
				LoadPictureFile();
			}

			progress	= new List<List<int>>();
			colorsLeft	= new List<int>();
			currentPaintAmount = new List<int>();
			unlockedRegions = new List<int> { 0 };
			
			// ДЕЛАЕТ СПИСОК НУЛЕЙ В КОЛИЧЕСТВЕ РАВНОМ КОЛИЧЕСИВУ ЦВЕТОВ. ПОЧЕМУ НЕ ПРОСТО ХРАНИТЬ КОЛИЧЕСТВО????
			//ВЫГЛЯДИТ ТАК - (0,0,0,0,0,0,0,0,0)
			for (int i = 0; i < colors.Count; i++)
			{
				colorsLeft.Add(0);
				var a = startPaintAmount[i];
				currentPaintAmount.Add(a);
			}

			// Copy the colorNumbers matrix
			for (int i = 0; i < colorNumbers.Count; i++)
			{
				//СПИСОК СТРОК С ЦИФРАМИ КАРТИНКИ
				progress.Add(new List<int>(colorNumbers[i]));

				// ПЕРЕБИРАЕТ КАЖДУЮ СТРОКУ
				for (int j = 0; j < colorNumbers[i].Count; j++)
				{
					//БЕРЕТ N СТРОКУ И K  - КОЛИЧЕСТВО ЦИФР В СТРОКЕ
					//НАПРИМЕР colorNumbers[0][0] БУДЕТ ПРОСТО -1 ИЗ ПЕРВОЙ СТРОКИ И ПЕРВОГО СТОЛБЦА
					int colorIndex = colorNumbers[i][j];

					//НА ВЫХОДЕ ПОЛУЧАЕМ МАССИВ С КОЛИЧЕСВТОМ НЕЗАКРАШЕННЫХ ЯЧЕЕК ПО КАЖДОМУ ЦВЕТА ВИДА  colorsLeft = [2, 2, 2]
					if (colorIndex != -1)
					{
						colorsLeft[colorIndex]++;
					}
				}
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Loads just the levels id from the file
		/// </summary>
		private void LoadIdFromPictureFile()
		{
			int secondLineStartIndex	= fileContents.IndexOf('\n') + 1;
			int secondNewlineIndex		= fileContents.IndexOf('\n', secondLineStartIndex);
			int length					= secondNewlineIndex - secondLineStartIndex;

			id			= fileContents.Substring(secondLineStartIndex, length);
			isIdLoaded	= true;
		}

		/// <summary>
		/// Parses the picture file.
		/// </summary>
		private void LoadPictureFile()
		{
			//ПОЛУЧАЕМ СПИСОК ВСЕХ СТРОК ИЗ ТЕКСТОВОГО ФАЙЛА И ЗАПИСЫВАЕМ КАЖДУЮ В ОТЕДЬНУЮ ЯЧЕЙКУ СПИСКА
			List<string[]> lines = ParseCSVLines(fileContents);

			if (lines.Count == 0)
			{
				Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, there are no lines in the file.");

				return;
			}

			int index = 0;

			//formatVersion = lines[index][0];
			index++;

			id = lines[index][0];
			index++;

			// Get the level lock info
			if (!ParseBool(lines[index], 0, out isLocked) || !ParseInt(lines[index], 1, out unlockAmount))
			{
				Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, could not parse level lock information.");

				return;
			}

			index++;

			// Get the award info
			if (!ParseBool(lines[index], 0, out awardOnComplete) || !ParseInt(lines[index], 1, out awardAmount))
			{
				Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, could not parse level lock information.");

				return;
			}

			index++;

			// Get the number of x and y cells in the picture
			if (!ParseInt(lines[index], 0, out xCells) || !ParseInt(lines[index], 1, out yCells))
			{
				Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, could not parse xCells and/or yCells.");

				return;
			}

			index++;

			// Get a list of integers that represent what colors each pixel are
			colorNumbers = new List<List<int>>();

			for (int i = index; i < yCells + index; i++)
			{
				if (i >= lines.Count)
				{
					Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, no enough lines when parse color numbers.");

					return;
				}

				colorNumbers.Add(new List<int>());

				for (int j = 0; j < xCells; j++)
				{
					int number;

					if (!ParseInt(lines[i], j, out number))
					{
						Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, could not parse color number.");

						return;
					}

					if (number == -1)
					{
						hasBlankCells = true;
					}

					colorNumbers[i - index].Add(number);
				}
			}

			index += yCells;

			var colorsCount = Convert.ToInt32(lines[index][0]);

			index++;
			

			// Get the list of colors in the picture
			colors = new List<Color>();
			startPaintAmount = new List<int>();

			for (int i = index; i < index + colorsCount; i++)
			{
				float r, g, b;

				if (!ParseFloat(lines[i], 0, out r) ||
					!ParseFloat(lines[i], 1, out g) ||
					!ParseFloat(lines[i], 2, out b))
				{
					Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, could not parse color information. " + lines[i]);

					return;
				}

				if(ParseInt(lines[i], 3, out var paintAmount))
				{
					startPaintAmount.Add(paintAmount);
				}
				else
				{
					startPaintAmount.Add(2000);
				}

				colors.Add(new Color(r, g, b, 1f));

				if (colors.Count != startPaintAmount.Count)
				{
					//Debug.LogError("colors count mismatch");
				}
			}
			
			index += colorsCount;
			
			colorsAvailable = new List<List<int>>();
			
			regionsAmount = Convert.ToInt32(lines[index][0]);

			for (int i = 0; i < regionsAmount; i++)
			{
				colorsAvailable.Add(new List<int>());
			}

			index++;

			// Get a list of integers that represent what regions MINE
			regionIds = new List<List<int>>();
			

			for (int i = index; i < yCells + index; i++)
			{
				if (i >= lines.Count)
				{
					Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, no enough lines when parse color numbers.");

					return;
				}

				regionIds.Add(new List<int>());
				

				for (int j = 0; j < xCells; j++)
				{
					int number;

					if (!ParseInt(lines[i], j, out number))
					{
						Debug.LogError("[PictureInformation] ParsePictureFile: Malformed file contents, could not parse color number.");

						return;
					}

					if (number != -1)
					{
						if (!colorsAvailable[number].Contains(colorNumbers[i - index][j]))
						{
							colorsAvailable[number].Add(colorNumbers[i - index][j]);
						}
					}
					
					regionIds[i - index].Add(number);
				}
			}


			List<int> used = new List<int>();

			for (int i = 0; i < colorsAvailable.Count; i++)
			{
				if (i > 0)
				{
					colorsAvailable[i].RemoveAll(x => used.Contains(x));
				}

				foreach (int value in colorsAvailable[i])
				{
					used.Add(value);
				}
			}
			
			isIdLoaded		= true;
			isFileLoaded	= true;
		}
		
		/// <summary>
		/// Parses the CSV file and seperate the lines
		/// </summary>
		private List<string[]> ParseCSVLines(string csv)
		{
			List<string[]>	lines		= new List<string[]>();
			string[]		csvLines	= csv.Split('\n');

			for (int i = 0; i < csvLines.Length; i++)
			{
				string line = csvLines[i].Replace("\r", "").Trim();

				if (!string.IsNullOrEmpty(line))
				{
					lines.Add(line.Split(','));
				}
			}

			return lines;
		}
		
		/// <summary>
		/// Helper method that converts a string at the given index into an integer, returns false if it fails
		/// </summary>
		private bool ParseInt(string[] line, int index, out int value)
		{
			value = 0;

			if (index >= line.Length)
			{
				return false;
			}

			if (!int.TryParse(line[index], out value))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Helper method that converts a string at the given index into an float, returns false if it fails
		/// </summary>
		private bool ParseFloat(string[] line, int index, out float value)
		{
			value = 0;

			if (index >= line.Length)
			{
				return false;
			}

			if (!float.TryParse(line[index], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Helper method that converts a string at the given index into a boolean, returns false if it fails
		/// </summary>
		private bool ParseBool(string[] line, int index, out bool value)
		{
			value = false;

			if (index >= line.Length)
			{
				return false;
			}

			if (!bool.TryParse(line[index], out value))
			{
				return false;
			}

			return true;
		}

		#endregion
	}
}