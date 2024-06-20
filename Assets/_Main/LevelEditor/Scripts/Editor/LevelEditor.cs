using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fiber.Managers;
using Fiber.Utilities;
using Fiber.LevelSystem;
using Fiber.Utilities.Extensions;
using GamePlay.Obstacles;
using Models;
using ScriptableObjects;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LevelEditor
{
	public class LevelEditor : EditorWindow
	{
		private static LevelEditor window;

		[SerializeField] private ColorDataSO colorDataSO;

		#region Elements

		[SerializeField] private VisualTreeAsset treeAsset;

		// Load
		private VisualElement Load_VE;
		private ObjectField levelField;
		private Button btn_Load;

		// Main Tabs
		private VisualElement Main_VE;
		private VisualElement MainTabRow_VE;

		// Obstacles
		private ListView listView_Obstacles;

		// Grid
		private VisualElement MainGrid_VE;
		private EnumField enum_GridColor;

		// Grid Setup
		private Vector2IntField v2Int_Size;
		private Button btn_Setup;
		private ScrollView Grid_SV;

		// Deck
		private VisualElement MainDeck_VE;
		private VisualElement DeckTabsRow_VE;
		private VisualElement Deck_VE;
		private EnumField enum_DeckColor;
		private Button btn_AddDeckTab;

		//Randomizer
		private UnsignedIntegerField uintField_ShapeCount;
		private VisualElement RandomizerColors_VE;
		private MinMaxSlider minMaxSlider_CellCount;
		private IntegerField intField_CellCountMin, intField_CellCountMax;
		private SliderInt slider_Wood1ObstaclePercentage, slider_Wood2ObstaclePercentage;
		private Button btn_Randomize;
		private ListView listView_RandomizerColors;

		// Options
		private VisualElement Goal_VE;
		private ListView listView_Goal;
		private EnumField enum_LevelType;
		private UnsignedIntegerField uintField_LevelTypeArgument;
		private UnsignedIntegerField txt_LevelNo;
		private Button btn_Save;

		#endregion

		private Level loadedLevel;
		private List<Goal> goals = new List<Goal>();

		#region Paths

		private const string LEVELS_PATH = "Assets/_Main/Prefabs/Levels/";
		private static readonly string LEVEL_BASE_PREFAB_PATH = $"{LEVELS_PATH}_BaseLevel.prefab";
		private const string OBSTACLES_PATH = "Assets/_Main/Prefabs/Obstacles";

		#endregion

		[MenuItem("Drop N Fold/Level Editor")]
		private static void ShowWindow()
		{
			window = GetWindow<LevelEditor>();
			window.titleContent = new GUIContent("Level Editor");
			window.Show();
		}

		private void CreateGUI()
		{
			InitFields();

			// Tabs
			InitMainTabs();
			InitDeckTabs();

			LoadObstacles();

			SetupElements();
			SetupRandomizer();
			SetupGoal();

			EditorCoroutineUtility.StartCoroutine(Wait(), this);
			return;

			IEnumerator Wait()
			{
				yield return null;
				SetupDeckGrid();
			}
		}

		private void OnGUI()
		{
			try
			{
				var e = Event.current;
				enum_GridColor.value = enum_DeckColor.value = e.keyCode switch
				{
					KeyCode.Alpha0 or KeyCode.Keypad0 => ColorType.None,
					KeyCode.Alpha1 or KeyCode.Keypad1 => ColorType.Blue,
					KeyCode.Alpha2 or KeyCode.Keypad2 => ColorType.Green,
					KeyCode.Alpha3 or KeyCode.Keypad3 => ColorType.Orange,
					KeyCode.Alpha4 or KeyCode.Keypad4 => ColorType.Pink,
					KeyCode.Alpha5 or KeyCode.Keypad5 => ColorType.Red,
					KeyCode.Alpha6 or KeyCode.Keypad6 => ColorType.Yellow,
				};
			}
			catch (SwitchExpressionException)
			{
			}
		}

		private void InitFields()
		{
			treeAsset.CloneTree(rootVisualElement);

			// Load
			Load_VE = rootVisualElement.Q<VisualElement>(nameof(Load_VE));
			levelField = EditorUtilities.CreateVisualElement<ObjectField>();
			levelField.label = "Load Level";
			levelField.style.flexGrow = 1;
			levelField.objectType = typeof(Level);
			Load_VE.Add(levelField);
			btn_Load = rootVisualElement.Q<Button>(nameof(btn_Load));
			btn_Load.clickable.clicked += Load;

			// Main Tabs
			Main_VE = rootVisualElement.Q<VisualElement>(nameof(Main_VE));
			MainTabRow_VE = rootVisualElement.Q<VisualElement>(nameof(MainTabRow_VE));

			// Obstacles
			listView_Obstacles = rootVisualElement.Q<ListView>(nameof(listView_Obstacles));

			// Grid Setup
			v2Int_Size = rootVisualElement.Q<Vector2IntField>(nameof(v2Int_Size));
			btn_Setup = rootVisualElement.Q<Button>(nameof(btn_Setup));
			btn_Setup.clickable.clicked += SetupGrid;
			// Grid
			MainGrid_VE = rootVisualElement.Q<VisualElement>(nameof(MainGrid_VE));
			Grid_SV = rootVisualElement.Q<ScrollView>(nameof(Grid_SV));
			enum_GridColor = rootVisualElement.Q<EnumField>(nameof(enum_GridColor));
			enum_GridColor.RegisterValueChangedCallback(evt => { enum_GridColor.style.backgroundColor = colorDataSO.ColorDatas[(ColorType)evt.newValue].Material.color; });

			// Deck
			MainDeck_VE = rootVisualElement.Q<VisualElement>(nameof(MainDeck_VE));
			Deck_VE = rootVisualElement.Q<VisualElement>(nameof(Deck_VE));
			DeckTabsRow_VE = rootVisualElement.Q<VisualElement>(nameof(DeckTabsRow_VE));
			btn_AddDeckTab = rootVisualElement.Q<Button>(nameof(btn_AddDeckTab));
			enum_DeckColor = rootVisualElement.Q<EnumField>(nameof(enum_DeckColor));
			enum_DeckColor.RegisterValueChangedCallback(evt => { enum_DeckColor.style.backgroundColor = colorDataSO.ColorDatas[(ColorType)evt.newValue].Material.color; });

			btn_AddDeckTab.clickable.clicked += AddDeckTab;

			// Randomizer
			uintField_ShapeCount = rootVisualElement.Q<UnsignedIntegerField>(nameof(uintField_ShapeCount));
			RandomizerColors_VE = rootVisualElement.Q<VisualElement>(nameof(RandomizerColors_VE));
			minMaxSlider_CellCount = rootVisualElement.Q<MinMaxSlider>(nameof(minMaxSlider_CellCount));
			intField_CellCountMin = rootVisualElement.Q<IntegerField>(nameof(intField_CellCountMin));
			intField_CellCountMax = rootVisualElement.Q<IntegerField>(nameof(intField_CellCountMax));
			slider_Wood1ObstaclePercentage = rootVisualElement.Q<SliderInt>(nameof(slider_Wood1ObstaclePercentage));
			slider_Wood2ObstaclePercentage = rootVisualElement.Q<SliderInt>(nameof(slider_Wood2ObstaclePercentage));
			btn_Randomize = rootVisualElement.Q<Button>(nameof(btn_Randomize));

			// Options
			Goal_VE = rootVisualElement.Q<VisualElement>(nameof(Goal_VE));
			enum_LevelType = rootVisualElement.Q<EnumField>(nameof(enum_LevelType));
			uintField_LevelTypeArgument = rootVisualElement.Q<UnsignedIntegerField>(nameof(uintField_LevelTypeArgument));
			txt_LevelNo = rootVisualElement.Q<UnsignedIntegerField>(nameof(txt_LevelNo));
			btn_Save = rootVisualElement.Q<Button>(nameof(btn_Save));
			btn_Save.clickable.clicked += Save;
		}

		private List<BaseObstacle> obstacles;
		private BaseObstacle selectedObstacle;

		private void LoadObstacles()
		{
			var obstacleObjects = EditorUtilities.LoadAllAssetsFromPath<Object>(OBSTACLES_PATH).ToArray();
			var obstaclePrefabs = EditorUtilities.LoadAllAssetsFromPath<BaseObstacle>(OBSTACLES_PATH);
			obstacles = new List<BaseObstacle>();
			obstacles.Add(null);
			foreach (var shape in obstaclePrefabs)
			{
				obstacles.Add(shape);
			}

			listView_Obstacles.makeItem = MakeItem;
			listView_Obstacles.bindItem = BindItem;
			listView_Obstacles.itemsSource = obstacles;
			return;

			VisualElement MakeItem() => EditorUtilities.CreateVisualElement<RadioButton>("radio");

			void BindItem(VisualElement element, int i)
			{
				var radio = (RadioButton)element;

				if (i == 0)
				{
					radio.name = radio.label = "None";
				}
				else
				{
					radio.name = "Shape_" + (i);
					radio.label = obstacles[i].name;
					LevelEditorUtilities.LoadAssetPreview(radio, obstacleObjects[i - 1], this);
				}

				radio.RegisterValueChangedCallback(evt => SelectObstacle(evt.newValue, obstacles[i]));
			}
		}

		private void SelectObstacle(bool selected, BaseObstacle obstacle)
		{
			if (!selected) return;
			selectedObstacle = obstacle;
		}

		private void SetupElements()
		{
			// Grid
			mainTabs[0].VisualElement.Add(MainGrid_VE);
			// Deck
			mainTabs[1].VisualElement.Add(MainDeck_VE);
		}

		private void SetupGoal()
		{
			listView_Goal = new ListView(goals)
			{
				headerTitle = "Gaols",
				showFoldoutHeader = true,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
				showBoundCollectionSize = true,
				reorderable = false,
				showAddRemoveFooter = true,
				makeItem = () =>
				{
					var elevatorDataVisualElement = new GoalDataVisualElement();
					var enumColor = elevatorDataVisualElement.Q<EnumField>("enum_Color");
					int j = 0;
					if (enumColor.userData is not null)
					{
						j = (int)enumColor.userData;
						enumColor.value = goals[j].ColorType;
					}

					var amount = elevatorDataVisualElement.Q<UnsignedIntegerField>("uintField_GoalAmount");
					int i = 0;
					if (amount.userData is not null)
					{
						i = (int)amount.userData;
						amount.value = (uint)goals[i].Amount;
					}

					enumColor.RegisterValueChangedCallback(evt =>
					{
						amount.style.backgroundColor = enumColor.style.backgroundColor = colorDataSO.ColorDatas[(ColorType)evt.newValue].Material.color;

						goals[(int)enumColor.userData].ColorType = (ColorType)evt.newValue;
					});
					amount.RegisterValueChangedCallback(evt => goals[(int)amount.userData].Amount = (int)evt.newValue);

					return elevatorDataVisualElement;
				},
				bindItem = (e, i) =>
				{
					goals[i] ??= new Goal();

					var enumColor = e.Q<EnumField>("enum_Color");
					var amount = e.Q<UnsignedIntegerField>("uintField_GoalAmount");
					amount.userData = i;
					enumColor.userData = i;
					amount.value = (uint)goals[i].Amount;
					enumColor.value = goals[i].ColorType;

					e.RegisterCallback<ChangeEvent<Goal>>(value => goals[i] = value.newValue);
				},
			};

			Goal_VE.Add(listView_Goal);
		}

		#region Tabs

		private struct Tab
		{
			public int Index;
			public readonly VisualElement VisualElement;
			public readonly Button TabButton;
			public readonly Button CloseButton;

			public Tab(int index, VisualElement visualElement, Button tabButton)
			{
				Index = index;
				VisualElement = visualElement;
				TabButton = tabButton;
				CloseButton = null;
			}

			public Tab(int index, VisualElement visualElement, Button tabButton, Button closeButton)
			{
				Index = index;
				VisualElement = visualElement;
				TabButton = tabButton;
				CloseButton = closeButton;
			}
		}

		private void SelectTab(Button button, VisualElement parent, IReadOnlyList<Tab> tabs)
		{
			var index = parent.IndexOf(button);
			for (var i = 0; i < tabs.Count; i++)
			{
				var tab = tabs[i];
				if (i.Equals(index))
				{
					tab.VisualElement.style.display = DisplayStyle.Flex;
					tab.TabButton.style.backgroundColor = new Color(.25f, .25f, .25f, 1);
					tab.TabButton.style.borderTopWidth = tab.TabButton.style.borderLeftWidth = tab.TabButton.style.borderRightWidth = 2;
					tab.TabButton.style.borderTopColor = tab.TabButton.style.borderLeftColor = tab.TabButton.style.borderRightColor = Color.white;
				}
				else
				{
					tab.VisualElement.style.display = DisplayStyle.None;
					tab.TabButton.style.backgroundColor = Color.grey;
					tab.TabButton.style.borderTopWidth = tab.TabButton.style.borderLeftWidth = tab.TabButton.style.borderRightWidth = 1;
					tab.TabButton.style.borderTopColor = tab.TabButton.style.borderLeftColor = tab.TabButton.style.borderRightColor = Color.black;
				}
			}
		}

		private void CloseTab(Button button, VisualElement parent, ref List<Tab> tabs, string tabName)
		{
			var index = parent.IndexOf(button);
			var closedTab = tabs[index];
			closedTab.VisualElement.RemoveFromHierarchy();
			closedTab.TabButton.RemoveFromHierarchy();
			tabs.RemoveAt(index);

			for (var i = 0; i < tabs.Count; i++)
			{
				var tab = tabs[i];
				tab.Index = i;
				tab.TabButton.text = tabName + " " + (i + 1);
			}
		}

		#region MainTabs

		private List<Tab> mainTabs = new List<Tab>();

		private void InitMainTabs()
		{
			AddMainTab();

			SelectTab(mainTabs[0].TabButton, MainTabRow_VE, mainTabs);
		}

		private void AddMainTab()
		{
			var gridButton = EditorUtilities.CreateVisualElement<Button>("tab-button");
			gridButton.focusable = false;
			gridButton.text = "Grid";
			gridButton.style.fontSize = 20;
			gridButton.style.unityFontStyleAndWeight = FontStyle.Bold;
			gridButton.clickable.clicked += () => SelectTab(gridButton, MainTabRow_VE, mainTabs);
			MainTabRow_VE.Add(gridButton);
			var ve1 = EditorUtilities.CreateVisualElement<VisualElement>("main");
			Main_VE.Add(ve1);
			mainTabs.Add(new Tab(0, ve1, gridButton));

			var deckButton = EditorUtilities.CreateVisualElement<Button>("tab-button");
			deckButton.focusable = false;
			deckButton.text = "Deck";
			deckButton.style.fontSize = 20;
			deckButton.style.unityFontStyleAndWeight = FontStyle.Bold;
			deckButton.clickable.clicked += () => SelectTab(deckButton, MainTabRow_VE, mainTabs);
			MainTabRow_VE.Add(deckButton);
			var ve2 = EditorUtilities.CreateVisualElement<VisualElement>("main");
			Main_VE.Add(ve2);
			mainTabs.Add(new Tab(1, ve2, deckButton));
		}

		#endregion

		#region Deck Tabs

		private List<Tab> deckTabs = new List<Tab>();

		private void InitDeckTabs()
		{
			AddDeckTab();

			SelectTab(deckTabs[0].TabButton, DeckTabsRow_VE, deckTabs);
		}

		private void AddDeckTab()
		{
			const string tabName = "Deck";
			int tabCount = deckTabs.Count;

			var button = EditorUtilities.CreateVisualElement<Button>("tab-button");
			button.focusable = false;
			button.text = tabName + " " + (tabCount + 1);

			var closeButton = EditorUtilities.CreateVisualElement<Button>("tab-close-button");
			closeButton.focusable = false;
			closeButton.text = "X";

			button.Add(closeButton);
			DeckTabsRow_VE.Add(button);

			var grid = EditorUtilities.CreateVisualElement<VisualElement>("grid");

			Deck_VE.Add(grid);

			var tab = new Tab(tabCount, grid, button, closeButton);
			deckTabs.Add(tab);

			button.clickable.clicked += () => SelectTab(button, DeckTabsRow_VE, deckTabs);
			closeButton.clickable.clicked += () =>
			{
				CloseTab(button, DeckTabsRow_VE, ref deckTabs, tabName);
				deckCells.RemoveAt(tabCount);

				for (var i = 0; i < deckCells.Count; i++)
				{
					for (int y = 0; y < deckCells[i].GetLength(1); y++)
					{
						for (int x = 0; x < deckCells[i].GetLength(0); x++)
						{
							deckCells[i][x, y].Button.userData = i;
						}
					}
				}
			};

			if (hasDeckSetup)
				AddDeckGrid(deckTabs.Count - 1);
		}

		#endregion

		#endregion

		#region Grid

		private CellInfo[,] gridCells;

		private void SetupGrid()
		{
			gridCells = new CellInfo[v2Int_Size.value.x, v2Int_Size.value.y];
			Grid_SV.Clear();

			for (int y = 0; y < v2Int_Size.value.y; y++)
			{
				var row = EditorUtilities.CreateVisualElement<VisualElement>("gridRow");
				Grid_SV.Add(row);
				for (int x = 0; x < v2Int_Size.value.x; x++)
				{
					gridCells[x, y] = new CellInfo();
					var button = EditorUtilities.CreateVisualElement<Button>("cell");
					button.focusable = false;
					gridCells[x, y].Coordinates = new Vector2Int(x, y);
					gridCells[x, y].Color = Color.white;
					gridCells[x, y].Button = button;

					int x1 = x;
					int y1 = y;
					button.RegisterCallback<MouseDownEvent>(e => OnGridCellClicked(e, gridCells[x1, y1]), TrickleDown.TrickleDown);

					row.Add(button);
				}
			}
		}

		private void OnGridCellClicked(IMouseEvent e, CellInfo cellInfo)
		{
			if (cellInfo.Button is null) return;
			if (selectedObstacle)
			{
				PlaceGridObstacle(cellInfo, selectedObstacle);
			}

			if (e.button.Equals(0) && (ColorType)enum_GridColor.value != ColorType.None && !selectedObstacle) // Left click - Place
			{
				cellInfo.ColorType = (ColorType)enum_GridColor.value;
				cellInfo.Color = colorDataSO.ColorDatas[(ColorType)enum_GridColor.value].Material.color;
				cellInfo.Button.style.backgroundColor = colorDataSO.ColorDatas[(ColorType)enum_GridColor.value].Material.color;
				cellInfo.Obstacle = null;
				cellInfo.Button.style.borderBottomWidth = cellInfo.Button.style.borderTopWidth = cellInfo.Button.style.borderLeftWidth = cellInfo.Button.style.borderRightWidth = 0;
			}

			if (e.button.Equals(1)) // Right click - Delete
			{
				cellInfo.ColorType = ColorType.None;
				cellInfo.Color = Color.white;
				cellInfo.Button.style.backgroundColor = Color.white;
				cellInfo.Button.text = "";
				cellInfo.Button.style.borderBottomWidth = cellInfo.Button.style.borderTopWidth = cellInfo.Button.style.borderLeftWidth = cellInfo.Button.style.borderRightWidth = 0;
			}
		}

		private void PlaceGridObstacle(CellInfo cellInfo, BaseObstacle obstacle)
		{
			cellInfo.Obstacle = obstacle;
			cellInfo.Button.text = obstacle.name;
			cellInfo.Button.style.borderBottomColor = cellInfo.Button.style.borderTopColor = cellInfo.Button.style.borderLeftColor = cellInfo.Button.style.borderRightColor = Color.black;
			cellInfo.Button.style.borderBottomWidth = cellInfo.Button.style.borderTopWidth = cellInfo.Button.style.borderLeftWidth = cellInfo.Button.style.borderRightWidth = 5;
		}

		#endregion

		#region Deck Grid

		private bool hasDeckSetup = false;
		private List<DeckCellInfo[,]> deckCells = new List<DeckCellInfo[,]>();
		private readonly Vector2Int deckSize = new Vector2Int(4, 4);

		private void SetupDeckGrid()
		{
			deckCells = new List<DeckCellInfo[,]>();
			for (int i = 0; i < deckTabs.Count; i++)
			{
				AddDeckGrid(i);
			}

			hasDeckSetup = true;
		}

		private void AddDeckGrid(int tabIndex)
		{
			deckCells.Add(new DeckCellInfo[deckSize.x, deckSize.y]);

			var grid = deckTabs[tabIndex].VisualElement;
			grid.Clear();

			for (int y = 0; y < deckSize.y; y++)
			{
				var row = EditorUtilities.CreateVisualElement<VisualElement>("gridRow");
				grid.Add(row);
				for (int x = 0; x < deckSize.x; x++)
				{
					deckCells[tabIndex][x, y] = new DeckCellInfo();
					var button = EditorUtilities.CreateVisualElement<Button>("cell");
					button.focusable = false;

					deckCells[tabIndex][x, y].Button = button;
					deckCells[tabIndex][x, y].Button.userData = tabIndex;
					deckCells[tabIndex][x, y].Coordinates = new Vector2Int(x, y);
					deckCells[tabIndex][x, y].Color = Color.white;
					deckCells[tabIndex][x, y].ColorType = ColorType.None;

					int _tabIndex = tabIndex;
					int x1 = x;
					int y1 = y;

					button.RegisterCallback<MouseDownEvent>(e => OnClickedDeckGrid(e, button, x1, y1), TrickleDown.TrickleDown);

					row.Add(button);
				}
			}
		}

		private void OnClickedDeckGrid(MouseDownEvent e, VisualElement button, int x, int y)
		{
			var deckCellInfo = deckCells[(int)button.userData][x, y];
			if (selectedObstacle)
			{
				PlaceObstacleToDeckCell(deckCellInfo, selectedObstacle);
			}

			if (e.button.Equals(0) && (ColorType)enum_GridColor.value != ColorType.None && !selectedObstacle) // Left click - Place
			{
				PlaceDeckCell(deckCellInfo, (ColorType)enum_DeckColor.value);
			}

			if (e.button.Equals(1)) // Right click - Delete
			{
				deckCellInfo.ColorType = ColorType.None;
				deckCellInfo.Color = Color.white;
				deckCellInfo.Button.style.backgroundColor = Color.white;

				deckCellInfo.Button.text = "";
				deckCellInfo.Button.style.borderBottomWidth = deckCellInfo.Button.style.borderTopWidth = deckCellInfo.Button.style.borderLeftWidth = deckCellInfo.Button.style.borderRightWidth = 0;
			}
		}

		private void PlaceDeckCell(DeckCellInfo deckCellInfo, ColorType colorType)
		{
			var data = colorDataSO.ColorDatas[colorType];
			deckCellInfo.ColorType = colorType;
			deckCellInfo.Color = data.Material.color;
			deckCellInfo.Button.style.backgroundColor = data.Material.color;
			deckCellInfo.Obstacle = null;
			deckCellInfo.Button.style.borderBottomWidth = deckCellInfo.Button.style.borderTopWidth = deckCellInfo.Button.style.borderLeftWidth = deckCellInfo.Button.style.borderRightWidth = 0;
		}

		private void PlaceObstacleToDeckCell(DeckCellInfo deckCellInfo, BaseObstacle obstacle)
		{
			deckCellInfo.Obstacle = obstacle;
			deckCellInfo.Button.text = obstacle.name;
			deckCellInfo.Button.style.borderBottomColor =
				deckCellInfo.Button.style.borderTopColor = deckCellInfo.Button.style.borderLeftColor = deckCellInfo.Button.style.borderRightColor = Color.black;
			deckCellInfo.Button.style.borderBottomWidth = deckCellInfo.Button.style.borderTopWidth = deckCellInfo.Button.style.borderLeftWidth = deckCellInfo.Button.style.borderRightWidth = 5;
		}

		private DeckCellInfo TryGetDeckCell(int index, int x, int y)
		{
			if (x >= 0 && x < deckCells[index].GetLength(0) && y >= 0 && y < deckCells[index].GetLength(1))
				return deckCells[index][x, y];
			return null;
		}

		#endregion

		#region Randomizer

		private readonly List<RandomizerColorData> randomizerColors = new List<RandomizerColorData>();

		private void SetupRandomizer()
		{
			// Randomizer Colors
			listView_RandomizerColors = new ListView(randomizerColors)
			{
				headerTitle = "Colors",
				showFoldoutHeader = true,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
				showBoundCollectionSize = true,
				reorderable = false,
				showAddRemoveFooter = true,
				makeItem = () =>
				{
					var randomizerColorDataVisualElement = new RandomizerColorDataVisualElement();
					var enumColor = randomizerColorDataVisualElement.Q<EnumField>("enum_RandomizerColor");
					int j = 0;
					if (enumColor.userData is not null)
					{
						j = (int)enumColor.userData;
						enumColor.value = randomizerColors[j].ColorType;
					}

					var amount = randomizerColorDataVisualElement.Q<SliderInt>("slider_RandomizerColorPercentage");
					int i = 0;
					if (amount.userData is not null)
					{
						i = (int)amount.userData;
						amount.value = randomizerColors[i].Percentage;
					}

					enumColor.RegisterValueChangedCallback(evt =>
					{
						amount.style.backgroundColor = enumColor.style.backgroundColor = colorDataSO.ColorDatas[(ColorType)evt.newValue].Material.color;

						randomizerColors[(int)enumColor.userData].ColorType = (ColorType)evt.newValue;
					});
					amount.RegisterValueChangedCallback(evt => randomizerColors[(int)amount.userData].Percentage = evt.newValue);

					return randomizerColorDataVisualElement;
				},
				bindItem = (e, i) =>
				{
					randomizerColors[i] ??= new RandomizerColorData();

					var enumColor = e.Q<EnumField>("enum_RandomizerColor");
					var percentage = e.Q<SliderInt>("slider_RandomizerColorPercentage");
					percentage.userData = i;
					enumColor.userData = i;
					percentage.value = randomizerColors[i].Percentage;
					enumColor.value = randomizerColors[i].ColorType;

					e.RegisterCallback<ChangeEvent<Goal>>(value => goals[i] = value.newValue);
				},
			};

			RandomizerColors_VE.Add(listView_RandomizerColors);

			// MinMax Cell Count
			minMaxSlider_CellCount.RegisterValueChangedCallback(evt =>
			{
				intField_CellCountMin.value = Mathf.RoundToInt(evt.newValue.x);
				intField_CellCountMax.value = Mathf.RoundToInt(evt.newValue.y);
			});
			intField_CellCountMin.RegisterValueChangedCallback(evt =>
			{
				var value = Mathf.Clamp(evt.newValue, minMaxSlider_CellCount.lowLimit, minMaxSlider_CellCount.highLimit);
				intField_CellCountMin.value = (int)value;
				minMaxSlider_CellCount.minValue = value;
			});
			intField_CellCountMax.RegisterValueChangedCallback(evt =>
			{
				var value = Mathf.Clamp(evt.newValue, minMaxSlider_CellCount.lowLimit, minMaxSlider_CellCount.highLimit);
				intField_CellCountMax.value = (int)value;
				minMaxSlider_CellCount.maxValue = value;
			});
			intField_CellCountMin.value = Mathf.RoundToInt(minMaxSlider_CellCount.value.x);
			intField_CellCountMax.value = Mathf.RoundToInt(minMaxSlider_CellCount.value.y);

			// Button
			btn_Randomize.clickable.clicked += Randomize;
		}

		private void Randomize()
		{
			const int maxTry = 10;
			if (randomizerColors.Count.Equals(0)) return;
			var colors = randomizerColors.Select(x => x.ColorType).ToList();
			var colorWeights = randomizerColors.Select(x => x.Percentage).ToList();

			foreach (var deckTab in deckTabs)
				deckTab.VisualElement.Clear();
			Deck_VE.Clear();
			DeckTabsRow_VE.Clear();

			deckTabs = new List<Tab>();
			SetupDeckGrid();

			for (int i = 0; i < uintField_ShapeCount.value; i++)
			{
				AddDeckTab();
				var randomCellCount = Random.Range((int)minMaxSlider_CellCount.value.x, (int)minMaxSlider_CellCount.value.y + 1);
				var currentCell = Random.Range(0, 2) == 0 ? deckCells[i][0, 0] : deckCells[i][deckCells[i].GetLength(0) - 1, 0];
				var previousCell = currentCell;
				var currentColor = colors.WeightedRandom(colorWeights);
				var previousColor = ColorType.None;

				for (int j = 0; j < randomCellCount; j++)
				{
					PlaceDeckCell(currentCell, currentColor);

					if (!slider_Wood1ObstaclePercentage.value.Equals(0))
					{
						var r = Random.Range(1, 101);
						if (r <= slider_Wood1ObstaclePercentage.value)
						{
							if (!currentCell.Obstacle)
							{
								// Wood 1 obstacle is at index 1
								PlaceObstacleToDeckCell(currentCell, obstacles[1]);
							}
						}
					}

					if (!slider_Wood2ObstaclePercentage.value.Equals(0))
					{
						var r = Random.Range(1, 101);
						if (r <= slider_Wood2ObstaclePercentage.value)
						{
							if (!currentCell.Obstacle)
							{
								// Wood 2 obstacle is at index 2
								PlaceObstacleToDeckCell(currentCell, obstacles[2]);
							}
						}
					}

					previousColor = currentColor;
					do
					{
						currentColor = colors.WeightedRandom(colorWeights);
					} while (currentColor == previousColor);

					previousCell = currentCell;
					int tryCount = 0;
					do
					{
						tryCount++;
						if (tryCount > maxTry) break;

						var randomDirection = Directions.AllDirections[Random.Range(0, Directions.AllDirections.Length)];
						var newCoor = currentCell.Coordinates + randomDirection;
						var newCell = TryGetDeckCell(i, newCoor.x, newCoor.y);
						if (newCell is null) continue;
						if (newCell.ColorType != ColorType.None) continue;
						currentCell = newCell;
					} while (currentCell == previousCell);

					if (tryCount > maxTry) break;
				}
			}
		}

		#endregion

		#region Save

		private GameObject levelBasePrefab;

		private void Save()
		{
			var source = AssetDatabase.LoadAssetAtPath<GameObject>(LEVEL_BASE_PREFAB_PATH);
			// Need to instantiate this prefab to the scene in order to create a variant
			levelBasePrefab = (GameObject)PrefabUtility.InstantiatePrefab(source);
			var levelBase = levelBasePrefab.GetComponent<Level>();

			EditorUtility.SetDirty(levelBasePrefab);
			EditorSceneManager.MarkSceneDirty(levelBasePrefab.scene);

			//
			SetupLevel(levelBase);
			//

			var levelPath = $"{LEVELS_PATH}Level_{txt_LevelNo.value:000}.prefab";
			var index = -1;
			if (loadedLevel)
			{
				var levelList = LevelManager.Instance.Levels.ToList();
				if (levelList.Contains(loadedLevel))
					index = levelList.IndexOf(loadedLevel);

				AssetDatabase.DeleteAsset(levelPath);
			}

			levelPath = AssetDatabase.GenerateUniqueAssetPath(levelPath);
			var savedLevel = PrefabUtility.SaveAsPrefabAsset(levelBasePrefab, levelPath, out var success).GetComponent<Level>();
			EditorUtility.ClearDirty(levelBasePrefab);

			AssetDatabase.Refresh();
			Debug.Log(success ? $"<color=lime>{levelPath} has saved!</color>" : $"<color=red>{levelPath} couldn't be saved!</color>");

			DestroyImmediate(levelBasePrefab);
			levelBasePrefab = null;

			// Change with the deleted level in the LevelManager
			if (!index.Equals(-1))
			{
				EditorUtility.SetDirty(LevelManager.Instance);
				LevelManager.Instance.Levels[index] = savedLevel;
				EditorSceneManager.SaveOpenScenes();
				EditorUtility.ClearDirty(LevelManager.Instance);
			}

			AssetDatabase.SaveAssets();
		}

		private void SetupLevel(Level level)
		{
			level.Grid.Setup(gridCells);
			level.Deck.Setup(deckCells);
			level.Setup((LevelType)enum_LevelType.value, (int)uintField_LevelTypeArgument.value);
			level.GoalManager.Setup(goals);
		}

		#endregion

		#region Load

		private void Load()
		{
			if (!levelField.value) return;

			loadedLevel = AssetDatabase.LoadAssetAtPath<Level>(AssetDatabase.GetAssetPath(levelField.value.GetInstanceID()));

			v2Int_Size.value = new Vector2Int(loadedLevel.Grid.GridCells.GetLength(0), loadedLevel.Grid.GridCells.GetLength(1));

			enum_LevelType.value = loadedLevel.LevelType;
			uintField_LevelTypeArgument.value = (uint)loadedLevel.LevelTypeArgument;

			txt_LevelNo.value = (uint)ParseLevelNo(loadedLevel.name);

			SetupGrid();

			var levelGridCells = loadedLevel.Grid.GridCells;
			for (int y = 0; y < levelGridCells.GetLength(1); y++)
			{
				for (int x = 0; x < levelGridCells.GetLength(0); x++)
				{
					var levelCell = levelGridCells[x, y];
					if (!levelCell.CurrentShapeCell) continue;

					var cell = gridCells[x, y];
					cell.ColorType = levelCell.CurrentShapeCell.ColorType;
					cell.Color = colorDataSO.ColorDatas[cell.ColorType].Material.color;
					cell.Button.style.backgroundColor = cell.Color;

					if (levelCell.CurrentShapeCell.CurrentObstacle)
					{
						var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(levelCell.CurrentShapeCell.CurrentObstacle);
						var obstaclePrefab = AssetDatabase.LoadAssetAtPath<BaseObstacle>(path);
						PlaceGridObstacle(cell, obstaclePrefab);
					}
				}
			}

			foreach (var deckTab in deckTabs)
				deckTab.VisualElement.Clear();
			Deck_VE.Clear();
			DeckTabsRow_VE.Clear();

			deckTabs = new List<Tab>();
			deckCells = new List<DeckCellInfo[,]>();
			SetupDeckGrid();

			var levelDecks = loadedLevel.Deck.Shapes;
			for (int i = 0; i < levelDecks.Count; i++)
			{
				AddDeckTab();

				foreach (var levelShapeCell in levelDecks[i].ShapeCells)
				{
					var cell = deckCells[i][levelShapeCell.ShapeCoordinates.x, levelShapeCell.ShapeCoordinates.y];
					cell.ColorType = levelShapeCell.ColorType;
					cell.Coordinates = levelShapeCell.ShapeCoordinates;
					cell.Color = colorDataSO.ColorDatas[levelShapeCell.ColorType].Material.color;
					cell.Button.style.backgroundColor = cell.Color;

					if (levelShapeCell.CurrentObstacle)
					{
						var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(levelShapeCell.CurrentObstacle);
						var obstaclePrefab = AssetDatabase.LoadAssetAtPath<BaseObstacle>(path);
						PlaceObstacleToDeckCell(cell, obstaclePrefab);
					}
				}
			}

			goals = new List<Goal>(loadedLevel.GoalManager.GoalDictionary.Values);

			listView_Goal.itemsSource = goals;
			listView_Goal.Rebuild();
			listView_Goal.RefreshItems();
		}

		private int ParseLevelNo(string levelName)
		{
			return int.TryParse(levelName.Substring(levelName.Length - 3, 3), out var levelNo) ? levelNo : 0;
		}

		#endregion
	}
}