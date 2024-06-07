using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Fiber.Managers;
using Fiber.Utilities;
using Fiber.LevelSystem;
using GamePlay.Shapes;
using Models;
using ScriptableObjects;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

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
		// private const string SHAPES_PATH = "Assets/_Main/Prefabs/Shapes";

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

			SetupElements();
			SetupGoal();

			EditorCoroutineUtility.StartCoroutine(Wait(), this);
			return;

			IEnumerator Wait()
			{
				yield return null;
				SetupDeckGrid();
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
			btn_AddDeckTab.clickable.clicked += AddDeckTab;

			// Options
			Goal_VE = rootVisualElement.Q<VisualElement>(nameof(Goal_VE));
			enum_LevelType = rootVisualElement.Q<EnumField>(nameof(enum_LevelType));
			uintField_LevelTypeArgument = rootVisualElement.Q<UnsignedIntegerField>(nameof(uintField_LevelTypeArgument));
			txt_LevelNo = rootVisualElement.Q<UnsignedIntegerField>(nameof(txt_LevelNo));
			btn_Save = rootVisualElement.Q<Button>(nameof(btn_Save));
			btn_Save.clickable.clicked += Save;
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
			closeButton.clickable.clicked += () => CloseTab(button, DeckTabsRow_VE, ref deckTabs, tabName);

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

			if (e.button.Equals(0)) // Left click - Place
			{
				cellInfo.ColorType = (ColorType)enum_GridColor.value;
				cellInfo.Color = colorDataSO.ColorDatas[(ColorType)enum_GridColor.value].Material.color;
				cellInfo.Button.style.backgroundColor = colorDataSO.ColorDatas[(ColorType)enum_GridColor.value].Material.color;
			}
			else if (e.button.Equals(1)) // Right click - Delete
			{
				cellInfo.ColorType = ColorType.None;
				cellInfo.Color = Color.white;
				cellInfo.Button.style.backgroundColor = Color.white;
			}
		}

		#endregion

		#region Deck Grid

		private bool hasDeckSetup = false;
		private List<DeckCellInfo[,]> deckCells = new List<DeckCellInfo[,]>();
		private Dictionary<string, List<Vector2Int>> shapePairs = new Dictionary<string, List<Vector2Int>>();
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
					deckCells[tabIndex][x, y].Coordinates = new Vector2Int(x, y);
					deckCells[tabIndex][x, y].Color = Color.white;
					deckCells[tabIndex][x, y].ColorType = ColorType.None;

					int _tabIndex = tabIndex;
					int x1 = x;
					int y1 = y;

					button.RegisterCallback<MouseDownEvent>(e => OnClickedDeckGrid(e, deckCells[_tabIndex][x1, y1]), TrickleDown.TrickleDown);

					row.Add(button);
				}
			}
		}

		private void OnClickedDeckGrid(MouseDownEvent e, DeckCellInfo deckCellInfo)
		{
			if (e.button.Equals(0)) // Left click - Place
			{
				deckCellInfo.ColorType = (ColorType)enum_DeckColor.value;
				deckCellInfo.Color = colorDataSO.ColorDatas[(ColorType)enum_DeckColor.value].Material.color;
				deckCellInfo.Button.style.backgroundColor = colorDataSO.ColorDatas[(ColorType)enum_DeckColor.value].Material.color;
			}
			else if (e.button.Equals(1)) // Right click - Delete
			{
				deckCellInfo.ColorType = ColorType.None;
				deckCellInfo.Color = Color.white;
				deckCellInfo.Button.style.backgroundColor = Color.white;
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
				LevelManager.Instance.Levels[index] = savedLevel;
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
				}
			}

			var levelDecks = loadedLevel.Deck.Shapes;
			for (int i = 0; i < levelDecks.Count; i++)
			{
				if (!i.Equals(0))
					AddDeckTab();

				foreach (var levelShapeCell in levelDecks[i].ShapeCells)
				{
					var cell = deckCells[i][levelShapeCell.ShapeCoordinates.x, levelShapeCell.ShapeCoordinates.y];
					cell.ColorType = levelShapeCell.ColorType;
					cell.Coordinates = levelShapeCell.ShapeCoordinates;
					cell.Color = colorDataSO.ColorDatas[levelShapeCell.ColorType].Material.color;
					cell.Button.style.backgroundColor = cell.Color;
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