using ClientCore;
using ClientCore.Statistics;
using ClientGUI;
using DTAClient.Domain;
using DTAClient.Domain.Multiplayer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DTAClient.DXGUI.Multiplayer.CnCNet;
using DTAClient.DXGUI.Generic;
using DTAClient.Online;
using DTAClient.Online.EventArguments;
using Localization;
using System.Diagnostics;

namespace DTAClient.DXGUI.Multiplayer.GameLobby
{
    /// <summary>
    /// A generic base for all game lobbies (Skirmish, LAN and CnCNet).
    /// Contains the common logic for parsing game options and handling player info.
    /// </summary>
    public abstract class GameLobbyBase : XNAWindow
    {
        protected const int MAX_PLAYER_COUNT = 8;
        protected const int PLAYER_OPTION_VERTICAL_MARGIN = 12;
        protected const int PLAYER_OPTION_HORIZONTAL_MARGIN = 3;
        protected const int PLAYER_OPTION_CAPTION_Y = 6;
        private const int DROP_DOWN_HEIGHT = 21;
        protected readonly string BTN_LAUNCH_GAME = "Launch Game".L10N("UI:Main:ButtonLaunchGame");
        protected readonly string BTN_LAUNCH_READY = "I'm Ready".L10N("UI:Main:ButtonIAmReady");
        protected readonly string BTN_LAUNCH_NOT_READY = "Not Ready".L10N("UI:Main:ButtonNotReady");

        private readonly string FavoriteMapsLabel = "Favorite Maps".L10N("UI:Main:FavoriteMaps");

        private const string SPMUSIC_SETTINGS = "Client/MusicSettings.ini";
        private const string SPSOUND_INI = "spsound.ini";

        private const int RANK_NONE = 0;
        private const int RANK_EASY = 1;
        private const int RANK_MEDIUM = 2;
        private const int RANK_HARD = 3;

        /// <summary>
        /// Creates a new instance of the game lobby base.
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="iniName">The name of the lobby in GameOptions.ini.</param>
        /// <param name="mapLoader"></param>
        /// <param name="isMultiplayer"></param>
        /// <param name="discordHandler"></param>
        public GameLobbyBase(
            WindowManager windowManager,
            string iniName,
            MapLoader mapLoader,
            bool isMultiplayer,
            DiscordHandler discordHandler
        ) : base(windowManager)
        {
            _iniSectionName = iniName;
            MapLoader = mapLoader;
            this.isMultiplayer = isMultiplayer;
            this.discordHandler = discordHandler;
        }

        private string _iniSectionName;

        protected XNAPanel PlayerOptionsPanel;

        protected XNAPanel GameOptionsPanel;

        protected List<MultiplayerColor> MPColors;

        protected List<GameLobbyCheckBox> CheckBoxes = new List<GameLobbyCheckBox>();
        protected List<GameLobbyDropDown> DropDowns = new List<GameLobbyDropDown>();

        protected GameLobbyCheckBox chkRandomOnly = null;

        protected DiscordHandler discordHandler;

        protected MapLoader MapLoader;
        /// <summary>
        /// The list of multiplayer game mode maps.
        /// Each is an instance of a map for a specific game mode.
        /// </summary>
        protected GameModeMapCollection GameModeMaps => MapLoader.GameModeMaps;

        protected GameModeMapFilter gameModeMapFilter;

        private GameModeMap _gameModeMap;

        protected int StartMusicIndex { get; set; }
        protected int ConflictMusicIndex { get; set; }

        protected int LoadingScreenIndex { get; set; }

        /// <summary>
        /// The currently selected game mode.
        /// </summary>
        protected GameModeMap GameModeMap
        {
            get => _gameModeMap;
            set
            {
                var oldGameModeMap = _gameModeMap;
                _gameModeMap = value;
                if (value != null && oldGameModeMap != value)
                    UpdateDiscordPresence();
            }
        }

        protected Map Map => GameModeMap?.Map;
        protected GameMode GameMode => GameModeMap?.GameMode;

        protected XNAClientDropDown[] ddPlayerNames;
        protected XNAClientDropDown[] ddPlayerSides;
        protected XNAClientDropDown[] ddPlayerColors;
        protected XNAClientDropDown[] ddPlayerStarts;
        protected XNAClientDropDown[] ddPlayerTeams;

        protected XNAClientButton btnPlayerExtraOptionsOpen;
        protected PlayerExtraOptionsPanel PlayerExtraOptionsPanel;

        protected XNALabel lblName;
        protected XNALabel lblSide;
        protected XNALabel lblColor;
        protected XNALabel lblStart;
        protected XNALabel lblTeam;

        protected XNAClientButton btnLeaveGame;
        protected GameLaunchButton btnLaunchGame;
        protected XNAClientButton btnPickRandomMap;
        protected XNALabel lblMapName;
        protected XNALabel lblMapAuthor;
        protected XNALabel lblGameMode;
        protected XNALabel lblMapSize;

        protected MapPreviewBox MapPreviewBox;

        protected XNAMultiColumnListBox lbGameModeMapList;
        protected XNAClientDropDown ddGameModeMapFilter;
        protected XNALabel lblGameModeSelect;
        protected XNAContextMenu mapContextMenu;
        private XNAContextMenuItem toggleFavoriteItem;

        protected XNASuggestionTextBox tbMapSearch;

        public CheaterWindow cheaterWindow;

        protected List<PlayerInfo> Players = new List<PlayerInfo>();
        protected List<PlayerInfo> AIPlayers = new List<PlayerInfo>();

        protected virtual PlayerInfo FindLocalPlayer() => Players.Find(p => p.Name == ProgramConstants.PLAYERNAME);

        protected bool PlayerUpdatingInProgress { get; set; }

        protected Texture2D[] RankTextures;

        /// <summary>
        /// The seed used for randomizing player options.
        /// </summary>
        protected int RandomSeed { get; set; }

        /// <summary>
        /// An unique identifier for this game.
        /// </summary>
        protected int UniqueGameID { get; set; }
        protected int SideCount { get; private set; }
        protected int RandomSelectorCount { get; private set; } = 1;

        public bool bSettingsLoaded { get; set; } = false;

        protected List<int[]> RandomSelectors = new List<int[]>();

        private readonly bool isMultiplayer = false;

        private MatchStatistics matchStatistics;

        private bool disableGameOptionUpdateBroadcast = false;

        protected EventHandler<MultiplayerNameRightClickedEventArgs> MultiplayerNameRightClicked;

        /// <summary>
        /// If set, the client will remove all starting waypoints from the map
        /// before launching it.
        /// </summary>
        protected bool RemoveStartingLocations { get; set; } = false;
        protected IniFile GameOptionsIni { get; private set; }

        protected XNAClientButton BtnSaveLoadGameOptions { get; set; }

        private XNAContextMenu loadSaveGameOptionsMenu { get; set; }

        private LoadOrSaveGameOptionPresetWindow loadOrSaveGameOptionPresetWindow;

        private readonly string[] LightNameArray =
        {
            "Red",
            "Blue",
            "Green",
            "Level",
            "Ground",
            "Ambient"
        };

        private void chkRandomOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkRandomOnly.Checked || !bSettingsLoaded)
                return;
 
            foreach (var ddSide in ddPlayerSides)
            {
                if (!ddSide.HelperTag || ddSide.SelectedIndex != ddSide.Items.Count - 1)
                    ddSide.SelectedIndex = 0;
            }

            CheckFactionsSelectable();
        }

        public override void Initialize()
        {
            Name = _iniSectionName;
            //if (WindowManager.RenderResolutionY < 800)
            //    ClientRectangle = new Rectangle(0, 0, WindowManager.RenderResolutionX, WindowManager.RenderResolutionY);
            //else
            ClientRectangle = new Rectangle(0, 0, WindowManager.RenderResolutionX - 60, WindowManager.RenderResolutionY - 32);
            WindowManager.CenterControlOnScreen(this);
            BackgroundTexture = AssetLoader.LoadTexture("gamelobbybg.png");

            RankTextures = new Texture2D[4]
            {
                AssetLoader.LoadTexture("rankNone.png"),
                AssetLoader.LoadTexture("rankEasy.png"),
                AssetLoader.LoadTexture("rankNormal.png"),
                AssetLoader.LoadTexture("rankHard.png")
            };

            MPColors = MultiplayerColor.LoadColors();

            GameOptionsIni = new IniFile(ProgramConstants.GetBaseResourcePath() + "GameOptions.ini");

            InitializeGameOptionsPanel();

            PlayerOptionsPanel = new XNAPanel(WindowManager);
            PlayerOptionsPanel.Name = "PlayerOptionsPanel";
            PlayerOptionsPanel.ClientRectangle = new Rectangle(GameOptionsPanel.X - 401, 12, 395, GameOptionsPanel.Height);
            //PlayerOptionsPanel.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 192), 1, 1);
            PlayerOptionsPanel.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;

            InitializePlayerExtraOptionsPanel();

            btnLeaveGame = new XNAClientButton(WindowManager);
            btnLeaveGame.Name = "btnLeaveGame";
            btnLeaveGame.ClientRectangle = new Rectangle(Width - 143, Height - 28, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
            btnLeaveGame.Text = "Leave Game".L10N("UI:Main:LeaveGame");
            btnLeaveGame.LeftClick += BtnLeaveGame_LeftClick;

            btnLaunchGame = new GameLaunchButton(WindowManager, RankTextures);
            btnLaunchGame.Name = "btnLaunchGame";
            btnLaunchGame.ClientRectangle = new Rectangle(12, btnLeaveGame.Y, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
            btnLaunchGame.Text = "Launch Game".L10N("UI:Main:LaunchGame");
            btnLaunchGame.LeftClick += BtnLaunchGame_LeftClick;

            MapPreviewBox = new MapPreviewBox(WindowManager, Players, AIPlayers, MPColors,
                GameOptionsIni.GetStringValue("General", "Sides", String.Empty).Split(','),
                GameOptionsIni);
            MapPreviewBox.Name = "MapPreviewBox";
            MapPreviewBox.ClientRectangle = new Rectangle(PlayerOptionsPanel.X,
                PlayerOptionsPanel.Bottom + 6,
                GameOptionsPanel.Right - PlayerOptionsPanel.X,
                Height - PlayerOptionsPanel.Bottom - 65);
            MapPreviewBox.FontIndex = 1;
            MapPreviewBox.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            MapPreviewBox.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 1, 1);
            MapPreviewBox.ToggleFavorite += MapPreviewBox_ToggleFavorite;

            lblMapName = new XNALabel(WindowManager);
            lblMapName.Name = "lblMapName";
            lblMapName.ClientRectangle = new Rectangle(MapPreviewBox.X,
                MapPreviewBox.Bottom + 3, 0, 0);
            lblMapName.FontIndex = 1;
            lblMapName.Text = "Map:".L10N("UI:Main:Map");

            lblMapAuthor = new XNALabel(WindowManager);
            lblMapAuthor.Name = "lblMapAuthor";
            lblMapAuthor.ClientRectangle = new Rectangle(MapPreviewBox.Right,
                lblMapName.Y, 0, 0);
            lblMapAuthor.FontIndex = 1;
            lblMapAuthor.Text = "By".L10N("UI:Main:AuthorBy") + " ";

            lblGameMode = new XNALabel(WindowManager);
            lblGameMode.Name = "lblGameMode";
            lblGameMode.ClientRectangle = new Rectangle(lblMapName.X,
                lblMapName.Bottom + 3, 0, 0);
            lblGameMode.FontIndex = 1;
            lblGameMode.Text = "Game mode:".L10N("UI:Main:GameModeLabel");

            lblMapSize = new XNALabel(WindowManager);
            lblMapSize.Name = "lblMapSize";
            lblMapSize.ClientRectangle = new Rectangle(lblGameMode.ClientRectangle.X,
                lblGameMode.ClientRectangle.Bottom + 3, 0, 0);
            lblMapSize.FontIndex = 1;
            lblMapSize.Text = "Size:".L10N("UI:Main:MapSize") + " ";
            lblMapSize.Visible = false;

            lbGameModeMapList = new XNAMultiColumnListBox(WindowManager);
            lbGameModeMapList.Name = "lbMapList";  // keep as lbMapList for legacy INI compatibility
            lbGameModeMapList.ClientRectangle = new Rectangle(btnLaunchGame.X, GameOptionsPanel.Y + 23,
                MapPreviewBox.X - btnLaunchGame.X - 6,
                MapPreviewBox.Bottom - 23 - GameOptionsPanel.Y);
            lbGameModeMapList.SelectedIndexChanged += LbGameModeMapList_SelectedIndexChanged;
            lbGameModeMapList.RightClick += LbGameModeMapList_RightClick;
            lbGameModeMapList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            //lbGameModeMapList.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 192), 1, 1);
            lbGameModeMapList.LineHeight = 16;
            lbGameModeMapList.DrawListBoxBorders = true;
            lbGameModeMapList.AllowKeyboardInput = true;
            lbGameModeMapList.AllowRightClickUnselect = false;

            mapContextMenu = new XNAContextMenu(WindowManager);
            mapContextMenu.Name = nameof(mapContextMenu);
            mapContextMenu.Width = 100;
            mapContextMenu.AddItem("Delete Map".L10N("UI:Main:DeleteMap"), DeleteMapConfirmation, null, CanDeleteMap);
            toggleFavoriteItem = new XNAContextMenuItem
            {
                Text = "Favorite".L10N("UI:Main:Favorite"),
                SelectAction = ToggleFavoriteMap
            };
            mapContextMenu.AddItem(toggleFavoriteItem);
            AddChild(mapContextMenu);

            XNAPanel rankHeader = new XNAPanel(WindowManager);
            rankHeader.BackgroundTexture = AssetLoader.LoadTexture("rank.png");
            rankHeader.ClientRectangle = new Rectangle(0, 0, rankHeader.BackgroundTexture.Width,
                19);

            XNAListBox rankListBox = new XNAListBox(WindowManager);
            rankListBox.TextBorderDistance = 2;

            lbGameModeMapList.AddColumn(rankHeader, rankListBox);

            lbGameModeMapList.AddColumn("MAP NAME".L10N("UI:Main:MapNameHeader"), lbGameModeMapList.Width - RankTextures[1].Width - 3);

            ddGameModeMapFilter = new XNAClientDropDown(WindowManager);
            ddGameModeMapFilter.Name = "ddGameMode"; // keep as ddGameMode for legacy INI compatibility
            ddGameModeMapFilter.ClientRectangle = new Rectangle(lbGameModeMapList.Right - 150, GameOptionsPanel.Y, 150, 21);
            ddGameModeMapFilter.SelectedIndexChanged += DdGameModeMapFilter_SelectedIndexChanged;

            ddGameModeMapFilter.AddItem(CreateGameFilterItem(FavoriteMapsLabel, new GameModeMapFilter(GetFavoriteGameModeMaps)));
            foreach (GameMode gm in GameModeMaps.GameModes)
                ddGameModeMapFilter.AddItem(CreateGameFilterItem(gm.UIName, new GameModeMapFilter(GetGameModeMaps(gm))));

            lblGameModeSelect = new XNALabel(WindowManager);
            lblGameModeSelect.Name = "lblGameModeSelect";
            lblGameModeSelect.ClientRectangle = new Rectangle(lbGameModeMapList.X, ddGameModeMapFilter.Y + 2, 0, 0);
            lblGameModeSelect.FontIndex = 1;
            lblGameModeSelect.Text = "GAME MODE:".L10N("UI:Main:GameMode");

            tbMapSearch = new XNASuggestionTextBox(WindowManager);
            tbMapSearch.Name = "tbMapSearch";
            tbMapSearch.ClientRectangle = new Rectangle(lbGameModeMapList.X,
                lbGameModeMapList.Bottom + 3, lbGameModeMapList.Width, 21);
            tbMapSearch.Suggestion = "Search map...".L10N("UI:Main:SearchMapTip");
            tbMapSearch.MaximumTextLength = 64;
            tbMapSearch.InputReceived += TbMapSearch_InputReceived;

            btnPickRandomMap = new XNAClientButton(WindowManager);
            btnPickRandomMap.Name = "btnPickRandomMap";
            btnPickRandomMap.ClientRectangle = new Rectangle(btnLaunchGame.Right + 157, btnLaunchGame.Y, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
            btnPickRandomMap.Text = "Pick Random Map".L10N("UI:Main:PickRandomMap");
            btnPickRandomMap.LeftClick += BtnPickRandomMap_LeftClick;
            btnPickRandomMap.Disable();

            AddChild(lblMapName);
            AddChild(lblMapAuthor);
            AddChild(lblGameMode);
            AddChild(lblMapSize);
            AddChild(MapPreviewBox);

            AddChild(lbGameModeMapList);
            AddChild(tbMapSearch);
            AddChild(lblGameModeSelect);
            AddChild(ddGameModeMapFilter);

            AddChild(GameOptionsPanel);
            AddChild(BtnSaveLoadGameOptions);
            AddChild(loadSaveGameOptionsMenu);
            AddChild(loadOrSaveGameOptionPresetWindow);

            string[] checkBoxes = GameOptionsIni.GetStringValue(_iniSectionName, "CheckBoxes", String.Empty).Split(',');

            foreach (string chkName in checkBoxes)
            {
                GameLobbyCheckBox chkBox = new GameLobbyCheckBox(WindowManager);
                chkBox.Name = chkName;
                AddChild(chkBox);
                chkBox.GetAttributes(GameOptionsIni);
                CheckBoxes.Add(chkBox);
                chkBox.CheckedChanged += ChkBox_CheckedChanged;
            }

            chkRandomOnly = CheckBoxes.Find(chk => chk.Name == "chkRandomOnly");
            if (chkRandomOnly != null)
                chkRandomOnly.CheckedChanged += chkRandomOnly_CheckedChanged;

            string[] labels = GameOptionsIni.GetStringValue(_iniSectionName, "Labels", String.Empty).Split(',');

            foreach (string labelName in labels)
            {
                XNALabel label = new XNALabel(WindowManager);
                label.Name = labelName;
                AddChild(label);
                label.GetAttributes(GameOptionsIni);
            }

            string[] dropDowns = GameOptionsIni.GetStringValue(_iniSectionName, "DropDowns", String.Empty).Split(',');

            foreach (string ddName in dropDowns)
            {
                GameLobbyDropDown dropdown = new GameLobbyDropDown(WindowManager);
                dropdown.Name = ddName;
                AddChild(dropdown);
                dropdown.GetAttributes(GameOptionsIni);
                DropDowns.Add(dropdown);
                dropdown.SelectedIndexChanged += Dropdown_SelectedIndexChanged;
            }

            AddChild(PlayerOptionsPanel);
            AddChild(PlayerExtraOptionsPanel);
            AddChild(btnLaunchGame);
            AddChild(btnLeaveGame);
            AddChild(btnPickRandomMap);

            cheaterWindow = new CheaterWindow(WindowManager);

            DarkeningPanel dp = new DarkeningPanel(WindowManager);
            dp.AddChild(cheaterWindow);

            AddChild(dp);
            dp.CenterOnParent();

            cheaterWindow.CenterOnParent();
            cheaterWindow.Disable();
        }

        private static XNADropDownItem CreateGameFilterItem(string text, GameModeMapFilter filter)
        {
            return new XNADropDownItem
            {
                Text = text,
                Tag = filter
            };
        }

        protected bool IsFavoriteMapsSelected() => ddGameModeMapFilter.SelectedItem?.Text == FavoriteMapsLabel;

        private List<GameModeMap> GetFavoriteGameModeMaps() =>
            GameModeMaps.Where(gmm => gmm.IsFavorite).ToList();

        private Func<List<GameModeMap>> GetGameModeMaps(GameMode gm) => () =>
            GameModeMaps.Where(gmm => gmm.GameMode == gm).ToList();

        private void InitializePlayerExtraOptionsPanel()
        {
            PlayerExtraOptionsPanel = new PlayerExtraOptionsPanel(WindowManager);
            PlayerExtraOptionsPanel.ClientRectangle = new Rectangle(PlayerOptionsPanel.X, PlayerOptionsPanel.Y, PlayerOptionsPanel.Width, PlayerOptionsPanel.Height);
            PlayerExtraOptionsPanel.OptionsChanged += PlayerExtraOptions_OptionsChanged;
        }

        private void RefreshBthPlayerExtraOptionsOpenTexture()
        {
            var texture = GetPlayerExtraOptions().IsDefault() ? "comboBoxArrow.png" : "comboBoxArrow-highlight.png";
            btnPlayerExtraOptionsOpen.IdleTexture = AssetLoader.LoadTexture(texture);
            btnPlayerExtraOptionsOpen.HoverTexture = AssetLoader.LoadTexture(texture);
        }

        private void InitializeGameOptionsPanel()
        {
            GameOptionsPanel = new XNAPanel(WindowManager);
            GameOptionsPanel.Name = nameof(GameOptionsPanel);
            GameOptionsPanel.ClientRectangle = new Rectangle(Width - 411, 12, 399, 289);
            //GameOptionsPanel.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 192), 1, 1);
            GameOptionsPanel.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;

            loadOrSaveGameOptionPresetWindow = new LoadOrSaveGameOptionPresetWindow(WindowManager);
            loadOrSaveGameOptionPresetWindow.Name = nameof(loadOrSaveGameOptionPresetWindow);
            loadOrSaveGameOptionPresetWindow.PresetLoaded += (sender, s) => HandleGameOptionPresetLoadCommand(s);
            loadOrSaveGameOptionPresetWindow.PresetSaved += (sender, s) => HandleGameOptionPresetSaveCommand(s);
            loadOrSaveGameOptionPresetWindow.Disable();
            var loadConfigMenuItem = new XNAContextMenuItem()
            {
                Text = "Load".L10N("UI:Main:LoadPreset"),
                SelectAction = () => loadOrSaveGameOptionPresetWindow.Show(true)
            };
            var saveConfigMenuItem = new XNAContextMenuItem()
            {
                Text = "Save".L10N("UI:Main:SavePreset"),
                SelectAction = () => loadOrSaveGameOptionPresetWindow.Show(false)
            };

            loadSaveGameOptionsMenu = new XNAContextMenu(WindowManager);
            loadSaveGameOptionsMenu.Name = nameof(loadSaveGameOptionsMenu);
            loadSaveGameOptionsMenu.ClientRectangle = new Rectangle(0, 0, 75, 0);
            loadSaveGameOptionsMenu.Items.Add(loadConfigMenuItem);
            loadSaveGameOptionsMenu.Items.Add(saveConfigMenuItem);

            BtnSaveLoadGameOptions = new XNAClientButton(WindowManager);
            BtnSaveLoadGameOptions.Name = nameof(BtnSaveLoadGameOptions);
            BtnSaveLoadGameOptions.ClientRectangle = new Rectangle(Width - 12, 14, 18, 22);
            BtnSaveLoadGameOptions.IdleTexture = AssetLoader.LoadTexture("comboBoxArrow.png");
            BtnSaveLoadGameOptions.HoverTexture = AssetLoader.LoadTexture("comboBoxArrow.png");
            BtnSaveLoadGameOptions.LeftClick += (sender, args) =>
            {
                loadSaveGameOptionsMenu.Open(new Point(BtnSaveLoadGameOptions.X - 74, BtnSaveLoadGameOptions.Y));
            };
        }

        protected void HandleGameOptionPresetSaveCommand(GameOptionPresetEventArgs e) => HandleGameOptionPresetSaveCommand(e.PresetName);

        protected void HandleGameOptionPresetSaveCommand(string presetName)
        {
            string error = AddGameOptionPreset(presetName);
            if (!string.IsNullOrEmpty(error))
                AddNotice(error);
        }

        protected void HandleGameOptionPresetLoadCommand(GameOptionPresetEventArgs e) => HandleGameOptionPresetLoadCommand(e.PresetName);

        protected void HandleGameOptionPresetLoadCommand(string presetName)
        {
            if (LoadGameOptionPreset(presetName))
                AddNotice("Game option preset loaded succesfully.".L10N("UI:Main:PresetLoaded"));
            else
                AddNotice(string.Format("Preset {0} not found!".L10N("UI:Main:PresetNotFound"), presetName));
        }

        protected void AddNotice(string message) => AddNotice(message, Color.White);

        protected abstract void AddNotice(string message, Color color);

        private void BtnPickRandomMap_LeftClick(object sender, EventArgs e) => PickRandomMap();

        private void TbMapSearch_InputReceived(object sender, EventArgs e) => ListMaps();

        private void Dropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (disableGameOptionUpdateBroadcast)
                return;

            var dd = (GameLobbyDropDown)sender;
            dd.HostSelectedIndex = dd.SelectedIndex;
            OnGameOptionChanged();
        }

        private void ChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (disableGameOptionUpdateBroadcast)
                return;

            var checkBox = (GameLobbyCheckBox)sender;
            checkBox.HostChecked = checkBox.Checked;
            OnGameOptionChanged();
        }

        /// <summary>
        /// Initializes the underlying window class.
        /// </summary>
        protected void InitializeWindow()
        {
            base.Initialize();
            lblMapAuthor.X = MapPreviewBox.Right - lblMapAuthor.Width;
            lblMapAuthor.TextAnchor = LabelTextAnchorInfo.LEFT;
            lblMapAuthor.AnchorPoint = new Vector2(MapPreviewBox.Right, lblMapAuthor.Y);
        }

        protected virtual void OnGameOptionChanged()
        {
            CheckDisallowedSides();
            CheckTeamLimit();

            btnLaunchGame.SetRank(GetRank());
        }

        protected void DdGameModeMapFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            gameModeMapFilter = ddGameModeMapFilter.SelectedItem.Tag as GameModeMapFilter;

            tbMapSearch.Text = string.Empty;
            tbMapSearch.OnSelectedChanged();

            ListMaps();

            if (lbGameModeMapList.SelectedIndex == -1)
                lbGameModeMapList.SelectedIndex = 0; // Select default GameModeMap
            else
                ChangeMap(GameModeMap);
        }

        protected void BtnPlayerExtraOptions_LeftClick(object sender, EventArgs e) => PlayerExtraOptionsPanel.Enable();

        protected void ApplyPlayerExtraOptions(string sender, string message)
        {
            var playerExtraOptions = PlayerExtraOptions.FromMessage(message);

            if (playerExtraOptions.IsForceRandomSides != PlayerExtraOptionsPanel.IsForcedRandomSides())
                AddPlayerExtraOptionForcedNotice(playerExtraOptions.IsForceRandomSides, "side selection".L10N("UI:Main:SideAsANoun"));

            if (playerExtraOptions.IsForceRandomColors != PlayerExtraOptionsPanel.IsForcedRandomColors())
                AddPlayerExtraOptionForcedNotice(playerExtraOptions.IsForceRandomColors, "color selection".L10N("UI:Main:ColorAsANoun"));

            if (playerExtraOptions.IsForceRandomStarts != PlayerExtraOptionsPanel.IsForcedRandomStarts())
                AddPlayerExtraOptionForcedNotice(playerExtraOptions.IsForceRandomStarts, "start selection".L10N("UI:Main:StartPositionAsANoun"));

            if (playerExtraOptions.IsForceRandomTeams != PlayerExtraOptionsPanel.IsForcedRandomTeams())
                AddPlayerExtraOptionForcedNotice(playerExtraOptions.IsForceRandomTeams, "team selection".L10N("UI:Main:TeamAsANoun"));

            if (playerExtraOptions.IsUseTeamStartMappings != PlayerExtraOptionsPanel.IsUseTeamStartMappings())
                AddPlayerExtraOptionForcedNotice(!playerExtraOptions.IsUseTeamStartMappings, "auto ally".L10N("UI:Main:AutoAllyAsANoun"));

            SetPlayerExtraOptions(playerExtraOptions);
            UpdateMapPreviewBoxEnabledStatus();
        }

        private void AddPlayerExtraOptionForcedNotice(bool disabled, string type)
            => AddNotice(disabled ?
                string.Format("The game host has disabled {0}".L10N("UI:Main:HostDisableSection"), type) :
                string.Format("The game host has enabled {0}".L10N("UI:Main:HostEnableSection"), type));

        private List<GameModeMap> GetSortedGameModeMaps()
        {
            var gameModeMaps = gameModeMapFilter.GetGameModeMaps();

            return gameModeMaps;
        }

        protected void ListMaps()
        {
            lbGameModeMapList.SelectedIndexChanged -= LbGameModeMapList_SelectedIndexChanged;

            lbGameModeMapList.ClearItems();
            lbGameModeMapList.SetTopIndex(0);

            lbGameModeMapList.SelectedIndex = -1;

            int mapIndex = -1;
            int skippedMapsCount = 0;

            var isFavoriteMapsSelected = IsFavoriteMapsSelected();
            var maps = GetSortedGameModeMaps();

            for (int i = 0; i < maps.Count; i++)
            {
                var gameModeMap = maps[i];
                if (tbMapSearch.Text != tbMapSearch.Suggestion)
                {
                    if (!gameModeMap.Map.Name.ToUpper().Contains(tbMapSearch.Text.ToUpper()))
                    {
                        skippedMapsCount++;
                        continue;
                    }
                }

                XNAListBoxItem rankItem = new XNAListBoxItem();
                if (gameModeMap.Map.IsCoop)
                {
                    if (StatisticsManager.Instance.HasBeatCoOpMap(gameModeMap.Map.Name, gameModeMap.GameMode.UIName))
                        rankItem.Texture = RankTextures[Math.Abs(2 - gameModeMap.GameMode.CoopDifficultyLevel) + 1];
                    else
                        rankItem.Texture = RankTextures[0];
                }
                else
                    rankItem.Texture = RankTextures[GetDefaultMapRankIndex(gameModeMap) + 1];

                XNAListBoxItem mapNameItem = new XNAListBoxItem();
                var mapNameText = gameModeMap.Map.Name;
                if (isFavoriteMapsSelected)
                    mapNameText += $" - {gameModeMap.GameMode.UIName}";

                mapNameItem.Text = Renderer.GetSafeString(mapNameText, lbGameModeMapList.FontIndex);

                if ((gameModeMap.Map.MultiplayerOnly || gameModeMap.GameMode.MultiplayerOnly) && !isMultiplayer)
                    mapNameItem.TextColor = UISettings.ActiveSettings.DisabledItemColor;
                mapNameItem.Tag = gameModeMap;

                XNAListBoxItem[] mapInfoArray = {
                    rankItem,
                    mapNameItem,
                };

                lbGameModeMapList.AddItem(mapInfoArray);

                if (gameModeMap == GameModeMap)
                    mapIndex = i - skippedMapsCount;
            }

            if (mapIndex > -1)
            {
                lbGameModeMapList.SelectedIndex = mapIndex;
                while (mapIndex > lbGameModeMapList.LastIndex)
                    lbGameModeMapList.TopIndex++;
            }

            lbGameModeMapList.SelectedIndexChanged += LbGameModeMapList_SelectedIndexChanged;
        }

        protected abstract int GetDefaultMapRankIndex(GameModeMap gameModeMap);

        private void LbGameModeMapList_RightClick(object sender, EventArgs e)
        {
            if (lbGameModeMapList.HoveredIndex < 0 || lbGameModeMapList.HoveredIndex >= lbGameModeMapList.ItemCount)
                return;

            lbGameModeMapList.SelectedIndex = lbGameModeMapList.HoveredIndex;

            if (!mapContextMenu.Items.Any(i => i.VisibilityChecker == null || i.VisibilityChecker()))
                return;

            toggleFavoriteItem.Text = GameModeMap.IsFavorite ? "Remove Favorite".L10N("UI:Main:RemoveFavorite") : "Add Favorite".L10N("UI:Main:AddFavorite");

            mapContextMenu.Open(GetCursorPoint());
        }

        private bool CanDeleteMap()
        {
            return Map != null && !Map.Official && !isMultiplayer;
        }

        private void DeleteMapConfirmation()
        {
            if (Map == null)
                return;

            var messageBox = XNAMessageBox.ShowYesNoDialog(WindowManager, "Delete Confirmation".L10N("UI:Main:DeleteMapConfirmTitle"),
                string.Format("Are you sure you wish to delete the custom map {0}?".L10N("UI:Main:DeleteMapConfirmText"), Map.Name));
            messageBox.YesClickedAction = DeleteSelectedMap;
        }

        private void MapPreviewBox_ToggleFavorite(object sender, EventArgs e) =>
            ToggleFavoriteMap();

        protected virtual void ToggleFavoriteMap()
        {
            GameModeMap.IsFavorite = UserINISettings.Instance.ToggleFavoriteMap(Map.Name, GameMode.Name, GameModeMap.IsFavorite);
            MapPreviewBox.RefreshFavoriteBtn();
        }

        protected void RefreshForFavoriteMapRemoved()
        {
            if (!gameModeMapFilter.GetGameModeMaps().Any())
            {
                LoadDefaultGameModeMap();
                return;
            }

            ListMaps();
            if (IsFavoriteMapsSelected())
                lbGameModeMapList.SelectedIndex = 0; // the map was removed while viewing favorites
        }

        private void DeleteSelectedMap(XNAMessageBox messageBox)
        {
            try
            {
                MapLoader.DeleteCustomMap(GameModeMap);

                tbMapSearch.Text = string.Empty;
                if (GameMode.Maps.Count == 0)
                {
                    // this will trigger another GameMode to be selected
                    GameModeMap = GameModeMaps.Find(gm => gm.GameMode.Maps.Count > 0);
                }
                else
                {
                    // this will trigger another Map to be selected
                    lbGameModeMapList.SelectedIndex = lbGameModeMapList.SelectedIndex == 0 ? 1 : lbGameModeMapList.SelectedIndex - 1;
                }

                ListMaps();
                ChangeMap(GameModeMap);
            }
            catch (IOException ex)
            {
                Logger.Log($"Deleting map {Map.BaseFilePath} failed! Message: {ex.Message}");
                XNAMessageBox.Show(WindowManager, "Deleting Map Failed".L10N("UI:Main:DeleteMapFailedTitle"),
                    "Deleting map failed! Reason:".L10N("UI:Main:DeleteMapFailedText") + " " + ex.Message);
            }
        }

        private void LbGameModeMapList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbGameModeMapList.SelectedIndex < 0 || lbGameModeMapList.SelectedIndex >= lbGameModeMapList.ItemCount)
            {
                ChangeMap(null);
                return;
            }

            XNAListBoxItem item = lbGameModeMapList.GetItem(1, lbGameModeMapList.SelectedIndex);

            GameModeMap = (GameModeMap)item.Tag;

            ChangeMap(GameModeMap);
        }

        private void PickRandomMap()
        {
            int totalPlayerCount = Players.Count(p => p.SideId < ddPlayerSides[0].Items.Count - 1)
                   + AIPlayers.Count;
            List<Map> maps = GetMapList(totalPlayerCount);
            if (maps.Count < 1)
                return;

            int random = new Random().Next(0, maps.Count);
            GameModeMap = GameModeMaps.Find(gmm => gmm.GameMode == GameMode && gmm.Map == maps[random]);

            Logger.Log("PickRandomMap: Rolled " + random + " out of " + maps.Count + ". Picked map: " + Map.Name);

            ChangeMap(GameModeMap);
            tbMapSearch.Text = string.Empty;
            tbMapSearch.OnSelectedChanged();
            ListMaps();
        }

        private List<Map> GetMapList(int playerCount)
        {
            List<Map> mapList = new List<Map>(GameMode.Maps.Where(x => x.MaxPlayers == playerCount));
            if (mapList.Count < 1 && playerCount <= MAX_PLAYER_COUNT)
                return GetMapList(playerCount + 1);
            else
                return mapList;
        }

        /// <summary>
        /// Refreshes the map selection UI to match the currently selected map
        /// and game mode.
        /// </summary>
        protected void RefreshMapSelectionUI()
        {
            if (GameMode == null)
                return;

            int gameModeMapFilterIndex = ddGameModeMapFilter.Items.FindIndex(i => i.Text == GameMode.UIName);

            if (gameModeMapFilterIndex == -1)
                return;

            if (ddGameModeMapFilter.SelectedIndex == gameModeMapFilterIndex)
                DdGameModeMapFilter_SelectedIndexChanged(this, EventArgs.Empty);

            ddGameModeMapFilter.SelectedIndex = gameModeMapFilterIndex;
        }

        /// <summary>
        /// Initializes the player option drop-down controls.
        /// </summary>
        protected void InitPlayerOptionDropdowns()
        {
            ddPlayerNames = new XNAClientDropDown[MAX_PLAYER_COUNT];
            ddPlayerSides = new XNAClientDropDown[MAX_PLAYER_COUNT];
            ddPlayerColors = new XNAClientDropDown[MAX_PLAYER_COUNT];
            ddPlayerStarts = new XNAClientDropDown[MAX_PLAYER_COUNT];
            ddPlayerTeams = new XNAClientDropDown[MAX_PLAYER_COUNT];

            int playerOptionVecticalMargin = GameOptionsIni.GetIntValue(Name, "PlayerOptionVerticalMargin", PLAYER_OPTION_VERTICAL_MARGIN);
            int playerOptionHorizontalMargin = GameOptionsIni.GetIntValue(Name, "PlayerOptionHorizontalMargin", PLAYER_OPTION_HORIZONTAL_MARGIN);
            int playerOptionCaptionLocationY = GameOptionsIni.GetIntValue(Name, "PlayerOptionCaptionLocationY", PLAYER_OPTION_CAPTION_Y);
            int playerNameWidth = GameOptionsIni.GetIntValue(Name, "PlayerNameWidth", 136);
            int sideWidth = GameOptionsIni.GetIntValue(Name, "SideWidth", 91);
            int colorWidth = GameOptionsIni.GetIntValue(Name, "ColorWidth", 79);
            int startWidth = GameOptionsIni.GetIntValue(Name, "StartWidth", 49);
            int teamWidth = GameOptionsIni.GetIntValue(Name, "TeamWidth", 46);
            int locationX = GameOptionsIni.GetIntValue(Name, "PlayerOptionLocationX", 25);
            int locationY = GameOptionsIni.GetIntValue(Name, "PlayerOptionLocationY", 24);

            // InitPlayerOptionDropdowns(136, 91, 79, 49, 46, new Point(25, 24));

            string[] sides = ClientConfiguration.Instance.Sides.Split(',');
            SideCount = sides.Length;

            List<string> selectorNames = new List<string>();
            GetRandomSelectors(selectorNames, RandomSelectors);
            RandomSelectorCount = RandomSelectors.Count + 1;
            MapPreviewBox.RandomSelectorCount = RandomSelectorCount;

            string randomColor = GameOptionsIni.GetStringValue("General", "RandomColor", "255,255,255");

            for (int i = MAX_PLAYER_COUNT - 1; i > -1; i--)
            {
                var ddPlayerName = new XNAClientDropDown(WindowManager);
                ddPlayerName.Name = "ddPlayerName" + i;
                ddPlayerName.ClientRectangle = new Rectangle(locationX,
                    locationY + (DROP_DOWN_HEIGHT + playerOptionVecticalMargin) * i,
                    playerNameWidth, DROP_DOWN_HEIGHT);
                ddPlayerName.AddItem(String.Empty);
                if (GameMode != null && GameMode.Name == "Difficulty Tier")
                {
                    ddPlayerName.AddItem("Insane AI".L10N("UI:Main:InsaneAI"));
                    ddPlayerName.AddItem("Brutal AI".L10N("UI:Main:BrutalAI"));
                    ddPlayerName.AddItem("Abyss AI".L10N("UI:Main:AbyssAI"));
                }
                else
                {
                    ddPlayerName.AddItem("Easy AI".L10N("UI:Main:EasyAI"));
                    ddPlayerName.AddItem("Normal AI".L10N("UI:Main:NormalAI"));
                    ddPlayerName.AddItem("Hard AI".L10N("UI:Main:HardAI"));
                }
                ddPlayerName.AllowDropDown = true;
                ddPlayerName.SelectedIndexChanged += CopyPlayerDataFromUI;
                ddPlayerName.RightClick += MultiplayerName_RightClick;
                ddPlayerName.Tag = true;

                var ddPlayerSide = new XNAClientDropDown(WindowManager);
                ddPlayerSide.Name = "ddPlayerSide" + i;
                ddPlayerSide.ClientRectangle = new Rectangle(
                    ddPlayerName.Right + playerOptionHorizontalMargin,
                    ddPlayerName.Y, sideWidth, DROP_DOWN_HEIGHT);
                ddPlayerSide.AddItem("Random".L10N("UI:Main:Random"), LoadTextureOrNull("randomicon.png"));
                foreach (string randomSelector in selectorNames)
                    ddPlayerSide.AddItem(randomSelector, LoadTextureOrNull(randomSelector + "icon.png"));
                foreach (string sideName in sides)
                    ddPlayerSide.AddItem(sideName, LoadTextureOrNull(sideName + "icon.png"));
                ddPlayerSide.AllowDropDown = false;
                ddPlayerSide.SelectedIndexChanged += CopyPlayerDataFromUI;
                ddPlayerSide.Tag = true;

                var ddPlayerColor = new XNAClientDropDown(WindowManager);
                ddPlayerColor.Name = "ddPlayerColor" + i;
                ddPlayerColor.ClientRectangle = new Rectangle(
                    ddPlayerSide.Right + playerOptionHorizontalMargin,
                    ddPlayerName.Y, colorWidth, DROP_DOWN_HEIGHT);
                ddPlayerColor.AddItem("Random".L10N("UI:Main:Random"), AssetLoader.GetColorFromString(randomColor));
                foreach (MultiplayerColor mpColor in MPColors)
                    ddPlayerColor.AddItem(mpColor.Name, mpColor.XnaColor);
                ddPlayerColor.AllowDropDown = false;
                ddPlayerColor.SelectedIndexChanged += CopyPlayerDataFromUI;
                ddPlayerColor.Tag = false;

                var ddPlayerTeam = new XNAClientDropDown(WindowManager);
                ddPlayerTeam.Name = "ddPlayerTeam" + i;
                ddPlayerTeam.ClientRectangle = new Rectangle(
                    ddPlayerColor.Right + playerOptionHorizontalMargin,
                    ddPlayerName.Y, teamWidth, DROP_DOWN_HEIGHT);
                ddPlayerTeam.AddItem("-");
                ProgramConstants.TEAMS.ForEach(ddPlayerTeam.AddItem);
                ddPlayerTeam.AllowDropDown = false;
                ddPlayerTeam.SelectedIndexChanged += CopyPlayerDataFromUI;
                ddPlayerTeam.Tag = true;

                var ddPlayerStart = new XNAClientDropDown(WindowManager);
                ddPlayerStart.Name = "ddPlayerStart" + i;
                ddPlayerStart.ClientRectangle = new Rectangle(
                    ddPlayerTeam.Right + playerOptionHorizontalMargin,
                    ddPlayerName.Y, startWidth, DROP_DOWN_HEIGHT);
                for (int j = 1; j < 9; j++)
                    ddPlayerStart.AddItem(j.ToString());
                ddPlayerStart.AllowDropDown = false;
                ddPlayerStart.SelectedIndexChanged += CopyPlayerDataFromUI;
                ddPlayerStart.Visible = false;
                ddPlayerStart.Enabled = false;
                ddPlayerStart.Tag = true;

                ddPlayerNames[i] = ddPlayerName;
                ddPlayerSides[i] = ddPlayerSide;
                ddPlayerColors[i] = ddPlayerColor;
                ddPlayerStarts[i] = ddPlayerStart;
                ddPlayerTeams[i] = ddPlayerTeam;

                PlayerOptionsPanel.AddChild(ddPlayerName);
                PlayerOptionsPanel.AddChild(ddPlayerSide);
                PlayerOptionsPanel.AddChild(ddPlayerColor);
                PlayerOptionsPanel.AddChild(ddPlayerStart);
                PlayerOptionsPanel.AddChild(ddPlayerTeam);
            }

            btnPlayerExtraOptionsOpen = new XNAClientButton(WindowManager);
            btnPlayerExtraOptionsOpen.Name = nameof(btnPlayerExtraOptionsOpen);
            btnPlayerExtraOptionsOpen.ClientRectangle = new Rectangle(0, 0, 0, 0);
            btnPlayerExtraOptionsOpen.IdleTexture = AssetLoader.LoadTexture("comboBoxArrow.png");
            btnPlayerExtraOptionsOpen.HoverTexture = AssetLoader.LoadTexture("comboBoxArrow.png");
            btnPlayerExtraOptionsOpen.LeftClick += BtnPlayerExtraOptions_LeftClick;
            btnPlayerExtraOptionsOpen.Visible = false;

            lblName = new XNALabel(WindowManager);
            lblName.Name = "lblName";
            lblName.Text = "PLAYER".L10N("UI:Main:Player");
            lblName.FontIndex = 1;
            lblName.ClientRectangle = new Rectangle(ddPlayerNames[0].X, playerOptionCaptionLocationY, 0, 0);

            lblSide = new XNALabel(WindowManager);
            lblSide.Name = "lblSide";
            lblSide.Text = "SIDE".L10N("UI:Main:Side");
            lblSide.FontIndex = 1;
            lblSide.ClientRectangle = new Rectangle(ddPlayerSides[0].X, playerOptionCaptionLocationY, 0, 0);

            lblColor = new XNALabel(WindowManager);
            lblColor.Name = "lblColor";
            lblColor.Text = "COLOR".L10N("UI:Main:Color");
            lblColor.FontIndex = 1;
            lblColor.ClientRectangle = new Rectangle(ddPlayerColors[0].X, playerOptionCaptionLocationY, 0, 0);

            lblStart = new XNALabel(WindowManager);
            lblStart.Name = "lblStart";
            lblStart.Text = "START".L10N("UI:Main:Start");
            lblStart.FontIndex = 1;
            lblStart.ClientRectangle = new Rectangle(ddPlayerStarts[0].X, playerOptionCaptionLocationY, 0, 0);
            lblStart.Visible = false;

            lblTeam = new XNALabel(WindowManager);
            lblTeam.Name = "lblTeam";
            lblTeam.Text = "TEAM".L10N("UI:Main:Team");
            lblTeam.FontIndex = 1;
            lblTeam.ClientRectangle = new Rectangle(ddPlayerTeams[0].X, playerOptionCaptionLocationY, 0, 0);

            PlayerOptionsPanel.AddChild(btnPlayerExtraOptionsOpen);
            PlayerOptionsPanel.AddChild(lblName);
            PlayerOptionsPanel.AddChild(lblSide);
            PlayerOptionsPanel.AddChild(lblColor);
            PlayerOptionsPanel.AddChild(lblStart);
            PlayerOptionsPanel.AddChild(lblTeam);

            CheckDisallowedSides();
            CheckTeamLimit();
        }

        protected virtual void PlayerExtraOptions_OptionsChanged(object sender, EventArgs e)
        {
            var playerExtraOptions = GetPlayerExtraOptions();

            for (int i = 0; i < ddPlayerSides.Length; i++)
                EnablePlayerOptionDropDown(ddPlayerSides[i], i, !playerExtraOptions.IsForceRandomSides);

            for (int i = 0; i < ddPlayerTeams.Length; i++)
                EnablePlayerOptionDropDown(ddPlayerTeams[i], i, !playerExtraOptions.IsForceRandomTeams);

            for (int i = 0; i < ddPlayerColors.Length; i++)
                EnablePlayerOptionDropDown(ddPlayerColors[i], i, !playerExtraOptions.IsForceRandomColors);

            for (int i = 0; i < ddPlayerStarts.Length; i++)
                EnablePlayerOptionDropDown(ddPlayerStarts[i], i, !playerExtraOptions.IsForceRandomStarts);

            UpdateMapPreviewBoxEnabledStatus();
            RefreshBthPlayerExtraOptionsOpenTexture();
        }

        private void EnablePlayerOptionDropDown(XNAClientDropDown clientDropDown, int playerIndex, bool enable)
        {
            var pInfo = GetPlayerInfoForIndex(playerIndex);
            var allowOtherPlayerOptionsChange = AllowPlayerOptionsChange() && pInfo != null;
            clientDropDown.AllowDropDown = enable && (allowOtherPlayerOptionsChange || pInfo?.Name == ProgramConstants.PLAYERNAME);
            if (!clientDropDown.AllowDropDown)
                clientDropDown.SelectedIndex = clientDropDown.SelectedIndex > 0 ? 0 : clientDropDown.SelectedIndex;
        }

        protected PlayerInfo GetPlayerInfoForIndex(int playerIndex)
        {
            if (playerIndex < Players.Count)
                return Players[playerIndex];

            if (playerIndex < Players.Count + AIPlayers.Count)
                return AIPlayers[playerIndex - Players.Count];

            return null;
        }

        protected PlayerExtraOptions GetPlayerExtraOptions() => PlayerExtraOptionsPanel.GetPlayerExtraOptions();

        protected void SetPlayerExtraOptions(PlayerExtraOptions playerExtraOptions) => PlayerExtraOptionsPanel?.SetPlayerExtraOptions(playerExtraOptions);

        protected string GetTeamMappingsError() => GetPlayerExtraOptions()?.GetTeamMappingsError();

        private Texture2D LoadTextureOrNull(string name) =>
            AssetLoader.AssetExists(name) ? AssetLoader.LoadTexture(name) : null;

        /// <summary>
        /// Loads random side selectors from GameOptions.ini
        /// </summary>
        /// <param name="selectorNames">TODO comment</param>
        /// <param name="selectorSides">TODO comment</param>
        private void GetRandomSelectors(List<string> selectorNames, List<int[]> selectorSides)
        {
            List<string> keys = GameOptionsIni.GetSectionKeys("RandomSelectors");

            if (keys == null)
                return;

            foreach (string randomSelector in keys)
            {
                List<int> randomSides = new List<int>();
                try
                {
                    string[] tmp = GameOptionsIni.GetStringValue("RandomSelectors", randomSelector, string.Empty).Split(',');
                    randomSides = Array.ConvertAll(tmp, int.Parse).Distinct().ToList();
                    randomSides.RemoveAll(x => (x >= SideCount || x < 0));
                }
                catch (FormatException) { }

                if (randomSides.Count > 1)
                {
                    selectorNames.Add(randomSelector);
                    selectorSides.Add(randomSides.ToArray());
                }
            }
        }

        protected abstract void BtnLaunchGame_LeftClick(object sender, EventArgs e);

        protected abstract void BtnLeaveGame_LeftClick(object sender, EventArgs e);

        /// <summary>
        /// Updates Discord Rich Presence with actual information.
        /// </summary>
        /// <param name="resetTimer">Whether to restart the "Elapsed" timer or not</param>
        protected abstract void UpdateDiscordPresence(bool resetTimer = false);

        /// <summary>
        /// Resets Discord Rich Presence to default state.
        /// </summary>
        protected void ResetDiscordPresence() => discordHandler?.UpdatePresence();

        protected void LoadDefaultGameModeMap()
        {
            if (ddGameModeMapFilter.Items.Count > 0)
            {
                ddGameModeMapFilter.SelectedIndex = GetDefaultGameModeMapFilterIndex();

                lbGameModeMapList.SelectedIndex = 0;
            }
        }

        protected int GetDefaultGameModeMapFilterIndex()
        {
            return ddGameModeMapFilter.Items.FindIndex(i => (i.Tag as GameModeMapFilter)?.Any() ?? false);
        }

        protected GameModeMapFilter GetDefaultGameModeMapFilter()
        {
            return ddGameModeMapFilter.Items[GetDefaultGameModeMapFilterIndex()].Tag as GameModeMapFilter;
        }

        private int GetSpectatorSideIndex() => SideCount + RandomSelectorCount;

        protected void CheckTeamLimit()
        {
            // Player
            for (int pId = 0; pId < Players.Count; pId++)
                for (int i = 0; i < ddPlayerTeams[pId].Items.Count; i++)
                    ddPlayerTeams[pId].Items[i].Selectable = true;

            // AI
            for (int aiId = 0; aiId < AIPlayers.Count; aiId++)
            {
                int index = Players.Count + aiId;
                for (int i = 0; i < ddPlayerTeams[index].Items.Count; i++)
                    ddPlayerTeams[index].Items[i].Selectable = true;
            }

            if (GameMode != null && GameMode.TeamsLimited)
            {
                // Player
                for (int pId = 0; pId < Players.Count; pId++)
                {
                    PlayerInfo pInfo = Players[pId];
                    ddPlayerTeams[pId].Items[0].Selectable = pInfo.SideId == GetSpectatorSideIndex();
                    ddPlayerTeams[pId].Items[3].Selectable = false;
                    ddPlayerTeams[pId].Items[4].Selectable = false;

                    switch (ddPlayerTeams[pId].SelectedIndex)
                    {
                        case 0:
                            if (pInfo.SideId != GetSpectatorSideIndex())
                                ddPlayerTeams[pId].SelectedIndex = 1;
                            break;
                        case 3:
                        case 4:
                            ddPlayerTeams[pId].SelectedIndex = 1;
                            break;
                    }
                }
                // AI
                for (int aiId = 0; aiId < AIPlayers.Count; aiId++)
                {
                    int index = Players.Count + aiId;

                    ddPlayerTeams[index].Items[0].Selectable = false;
                    ddPlayerTeams[index].Items[3].Selectable = false;
                    ddPlayerTeams[index].Items[4].Selectable = false;

                    switch (ddPlayerTeams[index].SelectedIndex)
                    {
                        case 0:
                        case 3:
                        case 4:
                            ddPlayerTeams[index].SelectedIndex = 1;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Applies disallowed side indexes to the side option drop-downs
        /// and player options.
        /// </summary>
        protected void CheckDisallowedSides()
        {
            var disallowedSideArray = GetDisallowedSides();
            int defaultSide = 0;
            int allowedSideCount = disallowedSideArray.Count(b => b == false);

            if (allowedSideCount == 1)
            {
                // Disallow Random

                for (int i = 0; i < disallowedSideArray.Length; i++)
                {
                    if (!disallowedSideArray[i])
                        defaultSide = i + RandomSelectorCount;
                }

                foreach (XNADropDown dd in ddPlayerSides)
                {
                    //dd.Items[0].Selectable = false;
                    for (int i = 0; i < RandomSelectorCount; i++)
                        dd.Items[i].Selectable = false;
                }
            }
            else
            {
                foreach (XNADropDown dd in ddPlayerSides)
                {
                    //dd.Items[0].Selectable = true;
                    for (int i = 0; i < RandomSelectorCount; i++)
                        dd.Items[i].Selectable = true;
                }
            }

            var concatPlayerList = Players.Concat(AIPlayers);

            // Disable custom random groups if all or all except one of included sides are unavailable.
            int c = 0;
            var playerInfos = concatPlayerList.ToList();
            foreach (int[] randomSides in RandomSelectors)
            {
                int disableCount = 0;

                foreach (int side in randomSides)
                {
                    if (disallowedSideArray[side])
                        disableCount++;
                }

                bool disabled = disableCount >= randomSides.Length - 1;

                foreach (XNADropDown dd in ddPlayerSides)
                    dd.Items[1 + c].Selectable = !disabled;

                foreach (PlayerInfo pInfo in playerInfos)
                {
                    if (pInfo.SideId == 1 + c && disabled)
                        pInfo.SideId = defaultSide;
                }

                c++;
            }

            // Go over the side array and either disable or enable the side
            // dropdown options depending on whether the side is available
            for (int i = 0; i < disallowedSideArray.Length; i++)
            {
                bool disabled = disallowedSideArray[i];

                if (disabled)
                {
                    foreach (XNADropDown dd in ddPlayerSides)
                        dd.Items[i + RandomSelectorCount].Selectable = false;

                    // Change the sides of players that use the disabled 
                    // side to the default side
                    foreach (PlayerInfo pInfo in playerInfos)
                    {
                        if (pInfo.SideId == i + RandomSelectorCount)
                            pInfo.SideId = defaultSide;
                    }
                }
                else
                {
                    foreach (XNADropDown dd in ddPlayerSides)
                        dd.Items[i + RandomSelectorCount].Selectable = true;
                }
            }

            // If only 1 side is allowed, change all players' sides to that
            if (allowedSideCount == 1)
            {
                foreach (PlayerInfo pInfo in playerInfos)
                {
                    if (pInfo.SideId == 0)
                        pInfo.SideId = defaultSide;
                }
            }

            if (Map != null && Map.CoopInfo != null)
            {
                // Disallow spectator

                foreach (PlayerInfo pInfo in playerInfos)
                {
                    if (pInfo.SideId == GetSpectatorSideIndex())
                        pInfo.SideId = defaultSide;
                }

                foreach (XNADropDown dd in ddPlayerSides)
                {
                    if (dd.Items.Count > GetSpectatorSideIndex())
                        dd.Items[SideCount + RandomSelectorCount].Selectable = false;
                }
            }
            else
            {
                foreach (XNADropDown dd in ddPlayerSides)
                {
                    if (dd.Items.Count > SideCount + RandomSelectorCount)
                        dd.Items[SideCount + RandomSelectorCount].Selectable = true;
                }
            }

            CheckFactionsSelectable();

            // disable random and random selectors
            if (disallowedSideArray.Count(b => b == true) > 0)
            {
                foreach (XNADropDown dd in ddPlayerSides)
                {
                    dd.Items[0].Selectable = false;
                    for (int i = 0; i < RandomSelectorCount; i++)
                        dd.Items[i].Selectable = false;
                }
            }
        }

        public void CheckFactionsSelectable()
        {
            // egg sides
            /*foreach (var dd in ddPlayerSides)
            {
                dd.Items[RandomSelectorCount + 11].Selectable = UserINISettings.Instance.EggSide4 ? true : false;
                dd.Items[RandomSelectorCount + 12].Selectable = UserINISettings.Instance.EggSide1 ? true : false;
                dd.Items[RandomSelectorCount + 13].Selectable = UserINISettings.Instance.EggSide2 ? true : false;
                dd.Items[RandomSelectorCount + 14].Selectable = UserINISettings.Instance.EggSide3 ? true : false;
            }*/

            // random only
            foreach (var ddSide in ddPlayerSides)
            {
                int totalCount = ddSide.Items.Count;
                if (ddSide.HelperTag)
                    totalCount -= 1;

                for (int i = 1; i < totalCount; ++i)
                    ddSide.Items[i].Selectable = !(chkRandomOnly != null && chkRandomOnly.Checked);
            }
        }

        /// <summary>
        /// Gets a list of side indexes that are disallowed.
        /// </summary>
        /// <returns>A list of disallowed side indexes.</returns>
        protected bool[] GetDisallowedSides()
        {
            var returnValue = new bool[SideCount];

            if (Map != null && Map.CoopInfo != null)
            {
                // Co-Op map disallowed side logic

                foreach (int disallowedSideIndex in Map.CoopInfo.DisallowedPlayerSides)
                    returnValue[disallowedSideIndex] = true;
            }

            if (GameMode != null)
            {
                foreach (int disallowedSideIndex in GameMode.DisallowedPlayerSides)
                    returnValue[disallowedSideIndex] = true;
            }

            foreach (var checkBox in CheckBoxes)
                checkBox.ApplyDisallowedSideIndex(returnValue);

            return returnValue;
        }

        /// <summary>
        /// Randomizes options of both human and AI players
        /// and returns the options as an array of PlayerHouseInfos.
        /// </summary>
        /// <returns>An array of PlayerHouseInfos.</returns>
        protected virtual PlayerHouseInfo[] Randomize(List<TeamStartMapping> teamStartMappings, bool gsCustomizeRestriction = false)
        {
            int totalPlayerCount = Players.Count + AIPlayers.Count;
            PlayerHouseInfo[] houseInfos = new PlayerHouseInfo[totalPlayerCount];

            for (int i = 0; i < totalPlayerCount; i++)
                houseInfos[i] = new PlayerHouseInfo();

            // Gather list of spectators
            for (int i = 0; i < Players.Count; i++)
                houseInfos[i].IsSpectator = Players[i].SideId == GetSpectatorSideIndex();

            // Gather list of available colors

            List<int> freeColors = new List<int>();

            for (int cId = 0; cId < MPColors.Count; cId++)
                freeColors.Add(cId);

            if (Map.CoopInfo != null)
            {
                foreach (int colorIndex in Map.CoopInfo.DisallowedPlayerColors)
                    freeColors.Remove(colorIndex);
            }

            foreach (PlayerInfo player in Players)
                freeColors.Remove(player.ColorId - 1); // The first color is Random

            foreach (PlayerInfo aiPlayer in AIPlayers)
                freeColors.Remove(aiPlayer.ColorId - 1);

            // Gather list of available starting locations

            List<int> freeStartingLocations = new List<int>();
            List<int> takenStartingLocations = new List<int>();

            for (int i = 0; i < Map.MaxPlayers; i++)
                freeStartingLocations.Add(i);

            for (int i = 0; i < Players.Count; i++)
            {
                if (!houseInfos[i].IsSpectator)
                {
                    freeStartingLocations.Remove(Players[i].StartingLocation - 1);
                    //takenStartingLocations.Add(Players[i].StartingLocation - 1);
                    // ^ Gives everyone with a selected location a completely random
                    // location in-game, because PlayerHouseInfo.RandomizeStart already
                    // fills the list itself
                }
            }

            for (int i = 0; i < AIPlayers.Count; i++)
                freeStartingLocations.Remove(AIPlayers[i].StartingLocation - 1);

            foreach (var teamStartMapping in teamStartMappings.Where(mapping => mapping.IsBlock))
                freeStartingLocations.Remove(teamStartMapping.StartingWaypoint);

            // Randomize options

            Random random = new Random(RandomSeed);

            for (int i = 0; i < totalPlayerCount; i++)
            {
                PlayerInfo pInfo;
                PlayerHouseInfo pHouseInfo = houseInfos[i];

                if (i < Players.Count)
                    pInfo = Players[i];
                else
                    pInfo = AIPlayers[i - Players.Count];

                pHouseInfo.RandomizeSide(pInfo, SideCount, random, GetDisallowedSides(), RandomSelectors, RandomSelectorCount);

                pHouseInfo.RandomizeColor(pInfo, freeColors, MPColors, random);
                pHouseInfo.RandomizeStart(pInfo, random, freeStartingLocations, takenStartingLocations, teamStartMappings.Any());
            }


            /*
             * 随机平衡分配 gsCustomizeRestriction 
             * 游戏一共有3个阵营，每个阵营有4个子阵营
             * 子阵营类型分别是：进攻 防守 支援 综合
             * 新的阵营选择器的规则是：同一个队伍内不能有相同的子阵营，以及不能有相同的子阵营类型
             * 假设阵营为A,B,C，子阵营则为A1 A2 A3 A4, B1 B2 B3 B4, C1 C2 C3 C4
             * 
             * 目前因为C4做不完，所以允许两个C1
             */
            if (gsCustomizeRestriction)
            {
                // get players in a team (random teams ignored)
                Dictionary<int, HashSet<int>> teamsPlayers = new Dictionary<int, HashSet<int>>();
                for (int pId = 0; pId < totalPlayerCount; pId++)
                {
                    PlayerInfo pInfo;
                    PlayerHouseInfo pHouseInfo = houseInfos[pId];

                    if (pId < Players.Count)
                        pInfo = Players[pId];
                    else
                        pInfo = AIPlayers[pId - Players.Count];

                    int teamId = pInfo.TeamId;
                    // teamid 0 -> random
                    if (teamId > 0)
                    {
                        if (!teamsPlayers.ContainsKey(teamId))
                        {
                            teamsPlayers[teamId] = new HashSet<int> { };
                        }
                        bool notExisted = teamsPlayers[teamId].Add(pId);
                        Debug.Assert(notExisted);
                    }
                }

                foreach (var teamPlayers in teamsPlayers.Values)
                {
                    Debug.Assert(GS_Side.FactionSidesCount == 4);
                    if (teamPlayers.Count > 4)
                    {
                        // 队伍多于4人的情况不做任何限制
                        break;
                    }

                    HashSet<int> sideUsed = new HashSet<int>(); // 子阵营不能重复

                    IList<int> sideTypeCountMax = new int[4] { 2, 1, 1, 1 }; // 子阵营类型上限
                    int[] sideTypeCount = new int[4];

                    bool validateSide(int sideId)
                    {
                        int sideType = sideId % 4; // 子阵营类型 0 1 2 3
                        if (sideUsed.Contains(sideId)) return false; // 子阵营重复
                        if (sideTypeCount[sideType] + 1 > sideTypeCountMax[sideType]) return false; // 子阵营类型上限
                        return true;
                    }

                    void applySide(int sideId, int pId)
                    {
                        int sideType = sideId % 4; // 子阵营类型 0 1 2 3
                        bool notExisted = sideUsed.Add(sideId);
                        Debug.Assert(notExisted);
                        sideTypeCount[sideType] += 1;
                        Debug.Assert(sideTypeCount[sideType] <= sideTypeCountMax[sideType]);

                        houseInfos[pId].SideIndex = sideId;
                    }

                    foreach (var pId in teamPlayers)
                    {
                        if (validateSide(houseInfos[pId].SideIndex))
                        {
                            applySide(houseInfos[pId].SideIndex, pId);
                        }
                        else
                        {
                            bool sideAdded = false;
                            List<int> sidesRoll = new List<int>();
                            for (int newSide = 0; newSide < SideCount; newSide++) sidesRoll.Add(newSide);
                            sidesRoll = sidesRoll.OrderBy(a => random.Next()).ToList(); // shuffle lists
                            foreach (int newSide in sidesRoll)
                            {
                                if (validateSide(newSide))
                                {
                                    applySide(newSide, pId);
                                    sideAdded = true;
                                    break;
                                }
                            }
                            if (!sideAdded)
                            {
                                throw new Exception("开心的笔：随机算法可能出现问题，每种可能的情况都不符合已有规则。请和游戏开发者联系！");
                            }
                        }
                    }
                }
            }

            return houseInfos;
        }

        /// <summary>
        /// Writes spawn.ini. Returns the player house info returned from the randomizer.
        /// </summary>
        private PlayerHouseInfo[] WriteSpawnIni(bool bForceSpeed = false, bool gsCustomizeRestriction = false)
        {
            Logger.Log("Writing spawn.ini");

            File.Delete(ProgramConstants.GamePath + ProgramConstants.SPAWNER_SETTINGS);

            if (Map.IsCoop)
            {
                foreach (PlayerInfo pInfo in Players)
                    pInfo.TeamId = 1;

                foreach (PlayerInfo pInfo in AIPlayers)
                    pInfo.TeamId = 1;
            }

            var teamStartMappings = PlayerExtraOptionsPanel.GetTeamStartMappings();

            PlayerHouseInfo[] houseInfos = Randomize(teamStartMappings, gsCustomizeRestriction);

            IniFile spawnIni = new IniFile(ProgramConstants.GamePath + ProgramConstants.SPAWNER_SETTINGS);

            IniSection settings = new IniSection("Settings");

            settings.SetStringValue("Name", ProgramConstants.PLAYERNAME);
            settings.SetStringValue("Scenario", ProgramConstants.SPAWNMAP_INI);
            settings.SetStringValue("UIGameMode", GameMode.UIName);
            settings.SetStringValue("UIMapName", Map.Name);
            settings.SetIntValue("PlayerCount", Players.Count);
            int myIndex = Players.FindIndex(c => c.Name == ProgramConstants.PLAYERNAME);
            settings.SetIntValue("Side", houseInfos[myIndex].InternalSideIndex);
            settings.SetBooleanValue("IsSpectator", houseInfos[myIndex].IsSpectator);
            settings.SetIntValue("Color", houseInfos[myIndex].ColorIndex);
            settings.SetStringValue("CustomLoadScreen", LoadingScreenController.GetLoadScreenName(houseInfos[myIndex].InternalSideIndex.ToString()));
            settings.SetIntValue("AIPlayers", AIPlayers.Count);
            settings.SetIntValue("Seed", RandomSeed);
            if (GetPvPTeamCount() > 1)
                settings.SetBooleanValue("CoachMode", true);
            if (GetGameType() == GameType.Coop)
                settings.SetBooleanValue("AutoSurrender", false);
            spawnIni.AddSection(settings);
            WriteSpawnIniAdditions(spawnIni);

            foreach (GameLobbyCheckBox chkBox in CheckBoxes)
                chkBox.ApplySpawnINICode(spawnIni);

            foreach (GameLobbyDropDown dd in DropDowns)
                dd.ApplySpawnIniCode(spawnIni);

            // Apply forced options from GameOptions.ini

            List<string> forcedKeys = GameOptionsIni.GetSectionKeys("ForcedSpawnIniOptions");

            if (forcedKeys != null)
            {
                foreach (string key in forcedKeys)
                {
                    spawnIni.SetStringValue("Settings", key,
                        GameOptionsIni.GetStringValue("ForcedSpawnIniOptions", key, String.Empty));
                }
            }

            GameMode.ApplySpawnIniCode(spawnIni); // Forced options from the game mode
            Map.ApplySpawnIniCode(spawnIni, Players.Count + AIPlayers.Count,
                AIPlayers.Count, GameMode.CoopDifficultyLevel); // Forced options from the map

            // Player options

            int otherId = 1;

            for (int pId = 0; pId < Players.Count; pId++)
            {
                PlayerInfo pInfo = Players[pId];
                PlayerHouseInfo pHouseInfo = houseInfos[pId];

                if (pInfo.Name == ProgramConstants.PLAYERNAME)
                    continue;

                string sectionName = "Other" + otherId;

                spawnIni.SetStringValue(sectionName, "Name", pInfo.Name);
                spawnIni.SetIntValue(sectionName, "Side", pHouseInfo.InternalSideIndex);
                spawnIni.SetBooleanValue(sectionName, "IsSpectator", pHouseInfo.IsSpectator);
                spawnIni.SetIntValue(sectionName, "Color", pHouseInfo.ColorIndex);
                spawnIni.SetStringValue(sectionName, "Ip", GetIPAddressForPlayer(pInfo));
                spawnIni.SetIntValue(sectionName, "Port", pInfo.Port);

                otherId++;
            }

            // The spawner assigns players to SpawnX houses based on their in-game color index
            List<int> multiCmbIndexes = new List<int>();
            var sortedColorList = MPColors.OrderBy(mpc => mpc.GameColorIndex).ToList();

            for (int cId = 0; cId < sortedColorList.Count; cId++)
            {
                for (int pId = 0; pId < Players.Count; pId++)
                {
                    if (houseInfos[pId].ColorIndex == sortedColorList[cId].GameColorIndex)
                        multiCmbIndexes.Add(pId);
                }
            }

            if (AIPlayers.Count > 0)
            {
                for (int aiId = 0; aiId < AIPlayers.Count; aiId++)
                {
                    int multiId = multiCmbIndexes.Count + aiId + 1;

                    string keyName = "Multi" + multiId;

                    spawnIni.SetIntValue("HouseHandicaps", keyName, AIPlayers[aiId].AILevel);
                    spawnIni.SetIntValue("HouseCountries", keyName, houseInfos[Players.Count + aiId].InternalSideIndex);
                    spawnIni.SetIntValue("HouseColors", keyName, houseInfos[Players.Count + aiId].ColorIndex);
                }
            }

            for (int multiId = 0; multiId < multiCmbIndexes.Count; multiId++)
            {
                int pIndex = multiCmbIndexes[multiId];
                if (houseInfos[pIndex].IsSpectator)
                    spawnIni.SetBooleanValue("IsSpectator", "Multi" + (multiId + 1), true);
            }

            // Write alliances, the code is pretty big so let's take it to another class
            AllianceHolder.WriteInfoToSpawnIni(Players, AIPlayers, multiCmbIndexes, houseInfos.ToList(), teamStartMappings, spawnIni);

            for (int pId = 0; pId < Players.Count; pId++)
            {
                int startingWaypoint = houseInfos[multiCmbIndexes[pId]].StartingWaypoint;

                // -1 means no starting location at all - let the game itself pick the starting location
                // using its own logic
                if (startingWaypoint > -1)
                {
                    int multiIndex = pId + 1;
                    spawnIni.SetIntValue("SpawnLocations", "Multi" + multiIndex,
                        startingWaypoint);
                }
            }

            for (int aiId = 0; aiId < AIPlayers.Count; aiId++)
            {
                int startingWaypoint = houseInfos[Players.Count + aiId].StartingWaypoint;

                if (startingWaypoint > -1)
                {
                    int multiIndex = Players.Count + aiId + 1;
                    spawnIni.SetIntValue("SpawnLocations", "Multi" + multiIndex,
                        startingWaypoint);
                }
            }

            // force game speed
            int iSpeed = 0;
            foreach (GameLobbyDropDown dropDown in DropDowns)
            {
                if (dropDown.OptionName == "Game Speed")
                    iSpeed = dropDown.SelectedIndex;
            }

            if (bForceSpeed)
            {
                if (iSpeed == 6)
                    spawnIni.SetIntValue("Settings", "GameSpeed", 1);
                else
                {
                    spawnIni.SetIntValue("Settings", "GameSpeed", 2);
                }
            }

            spawnIni.WriteIniFile();

            return houseInfos;
        }

        /// <summary>
        /// Returns the number of teams with human players in them.
        /// Does not count spectators and human players that don't have a team set.
        /// </summary>
        /// <returns>The number of human player teams in the game.</returns>
        private int GetPvPTeamCount()
        {
            int[] teamPlayerCounts = new int[4];
            int playerTeamCount = 0;

            foreach (PlayerInfo pInfo in Players)
            {
                if (pInfo.IsAI || IsPlayerSpectator(pInfo))
                    continue;

                if (pInfo.TeamId > 0)
                {
                    teamPlayerCounts[pInfo.TeamId - 1]++;
                    if (teamPlayerCounts[pInfo.TeamId - 1] == 2)
                        playerTeamCount++;
                }
            }

            return playerTeamCount;
        }

        /// <summary>
        /// Checks whether the specified player has selected Spectator as their side.
        /// </summary>
        /// <param name="pInfo">The player.</param>
        /// <returns>True if the player is a spectator, otherwise false.</returns>
        private bool IsPlayerSpectator(PlayerInfo pInfo)
        {
            if (pInfo.SideId == GetSpectatorSideIndex())
                return true;

            return false;
        }

        protected virtual string GetIPAddressForPlayer(PlayerInfo player) => "0.0.0.0";

        /// <summary>
        /// Override this in a derived class to write game lobby specific code to
        /// spawn.ini. For example, CnCNet game lobbies should write tunnel info
        /// in this method.
        /// </summary>
        /// <param name="iniFile">The spawn INI file.</param>
        protected virtual void WriteSpawnIniAdditions(IniFile iniFile)
        {
            // Do nothing by default
        }

        private void InitializeMatchStatistics(PlayerHouseInfo[] houseInfos)
        {
            matchStatistics = new MatchStatistics(ProgramConstants.GAME_VERSION, UniqueGameID,
                Map.Name, GameMode.UIName, Players.Count, Map.IsCoop);

            bool isValidForStar = true;
            foreach (GameLobbyCheckBox checkBox in CheckBoxes)
            {
                if ((checkBox.MapScoringMode == CheckBoxMapScoringMode.DenyWhenChecked && checkBox.Checked) ||
                    (checkBox.MapScoringMode == CheckBoxMapScoringMode.DenyWhenUnchecked && !checkBox.Checked))
                {
                    isValidForStar = false;
                    break;
                }
            }

            matchStatistics.IsValidForStar = isValidForStar;

            for (int pId = 0; pId < Players.Count; pId++)
            {
                PlayerInfo pInfo = Players[pId];
                matchStatistics.AddPlayer(pInfo.Name, pInfo.Name == ProgramConstants.PLAYERNAME,
                    false, pInfo.SideId == SideCount + RandomSelectorCount, houseInfos[pId].SideIndex + 1, pInfo.TeamId,
                    MPColors.FindIndex(c => c.GameColorIndex == houseInfos[pId].ColorIndex), 10);
            }

            for (int aiId = 0; aiId < AIPlayers.Count; aiId++)
            {
                var pHouseInfo = houseInfos[Players.Count + aiId];
                PlayerInfo aiInfo = AIPlayers[aiId];
                matchStatistics.AddPlayer("AI", false, true, false,
                    pHouseInfo.SideIndex + 1, aiInfo.TeamId,
                    MPColors.FindIndex(c => c.GameColorIndex == pHouseInfo.ColorIndex),
                    aiInfo.ReversedAILevel);
            }
        }

        private string GetPlayerMusicSide(PlayerHouseInfo[] houseInfos)
        {
            int myIndex = Players.FindIndex(c => c.Name == ProgramConstants.PLAYERNAME);
            switch (houseInfos[myIndex].SideIndex)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return "GDI";
                case 4:
                case 5:
                case 6:
                case 7:
                    return "Nod";
                case 8:
                case 9:
                case 10:
                case 11:
                    return "Scrin";
                default:
                    Random random = new Random();
                    if (Convert.ToBoolean(random.Next(0, 2)))
                        return "GDI";
                    else
                        return "Nod";
            }
        }

        /// <summary>
        /// Writes spawnmap.ini.
        /// </summary>
        private void WriteMap(PlayerHouseInfo[] houseInfos, bool bForceSpeed = false)
        {
            File.Delete(ProgramConstants.GamePath + ProgramConstants.SPAWNMAP_INI);
            File.Delete(ProgramConstants.GamePath + SPSOUND_INI);

            Logger.Log("Writing map.");

            Logger.Log("Loading map INI from " + Map.CompleteFilePath);

            IniFile mapIni = Map.GetMapIni();

            IniFile globalCodeIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/GlobalCode.ini");

            MapCodeHelper.ApplyMapCode(mapIni, GameMode.GetMapRulesIniFile());
            MapCodeHelper.ApplyMapCode(mapIni, globalCodeIni);

            // apply multiplayer options
            if (bForceSpeed)
            {
                IniFile multiplayerOptionsIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/MultiplayerOptions.ini");
                IniFile.ConsolidateIniFiles(mapIni, multiplayerOptionsIni);
            }

            // apply fast options
            if (bForceSpeed && Players.Count() - AIPlayers.Count() > 3)
            {
                IniFile spawnIni = new IniFile(ProgramConstants.GamePath + ProgramConstants.SPAWNER_SETTINGS);
                if (spawnIni.GetIntValue("Settings", "GameSpeed", 2) == 1)
                {
                    IniFile fastOptionsIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/FastOptions.ini");
                    IniFile.ConsolidateIniFiles(mapIni, fastOptionsIni);
                }
            }

            foreach (GameLobbyCheckBox checkBox in CheckBoxes)
                checkBox.ApplyMapCode(mapIni, GameMode);

            foreach (GameLobbyDropDown dropDown in DropDowns)
                dropDown.ApplyMapCode(mapIni, GameMode);

            mapIni.MoveSectionToFirst("MultiplayerDialogSettings"); // Required by YR

            ManipulateStartingLocations(mapIni, houseInfos);

            // Map Settings
            if (mapIni.GetDoubleValue("Lighting", "Ambient", 0.30f) <= 0.44f)
                mapIni.SetDoubleValue("Lighting", "Ambient", 0.44f);
            foreach (string strName in LightNameArray)
            {
                mapIni.SetDoubleValue("Lighting", "Ion" + strName, mapIni.GetDoubleValue("Lighting", strName, 0.35f));
                mapIni.SetDoubleValue("Lighting", "Dominator" + strName, mapIni.GetDoubleValue("Lighting", strName, 0.35f));
            }
            mapIni.SetStringValue("Basic", "TiberiumDeathToVisceroid", "yes");

            if (mapIni.GetBooleanValue("BaseInfo", "Ambient.Wind.Cold", false))
                mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_Cold");
            else if (mapIni.GetBooleanValue("BaseInfo", "Ambient.Wind.MountLow", false))
                mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_MountLow");
            else if (mapIni.GetBooleanValue("BaseInfo", "Ambient.Wind.MountHigh", false))
                mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_MountHigh");
            else if (mapIni.GetBooleanValue("BaseInfo", "Ambient.Wind.Disable", false))
                mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "NULLQAQ");

            // ReShade Settings
            ProgramConstants.SetupPreset(bForceSpeed);
            StreamWriter shaderIniWriter = new StreamWriter(ProgramConstants.GamePath + "GameShaders/TCMainShader.ini");
            if (!UserINISettings.Instance.NoReShade)
            {
                string strTechniques = "UI_Before,Colourfulness";
                string strExtraLines = String.Empty;

                if (UserINISettings.Instance.EnhancedLaser > 0)
                {
                    strTechniques += ",BlitLaser";
                }
                if (UserINISettings.Instance.EnhancedLight > 0)
                {
                    strTechniques += ",AnimMask";
                }
                if (UserINISettings.Instance.HighDetail == 3)
                {
                    strTechniques += ",MagicBloom";
                }

                double flAmbient = mapIni.GetDoubleValue("Lighting", "Ambient", 1.00f);

                // Clouds
                bool bLightClouds = false;
                if (flAmbient > 0.85f)
                    bLightClouds = true;

                string strLightClouds = mapIni.GetStringValue("BaseInfo", "RedAmbEffect", "none").ToLower();
                if (strLightClouds == "true")
                    bLightClouds = true;
                else if (strLightClouds == "false")
                    bLightClouds = false;

                if (UserINISettings.Instance.CloudsEffect == 1) //2
                {
                    //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_CLOUD_O);
                    mapIni.SetStringValue("Basic", "AltNextScenario", "O_");
                }
                else if (strLightClouds == "none")
                {
                    //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_CLOUD_N);
                    mapIni.SetStringValue("Basic", "AltNextScenario", "N_");
                }
                else if (bLightClouds)
                {
                    //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_CLOUD_D);
                    mapIni.SetStringValue("Basic", "AltNextScenario", "D_");
                }
                else
                {
                    //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_CLOUD_N);
                    mapIni.SetStringValue("Basic", "AltNextScenario", "N_");
                }

                if (mapIni.GetBooleanValue("BaseInfo", "IsSnow", false)) // Snow
                {
                    if (mapIni.GetBooleanValue("BaseInfo", "IsSnowNight", false))
                    {
                        //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_SNOWNIGHT);
                        strExtraLines += ClientConfiguration.SHADER_SNOWNIGHT_SETUP;
                        if (UserINISettings.Instance.HighDetail == 3)
                        {
                            strTechniques += ",PPFXBloom";
                        }
                        if (UserINISettings.Instance.HighDetail > 0)
                        {
                            strTechniques += ",FilmicPass";
                        }

                        if (UserINISettings.Instance.CloudsEffect > 0)
                        {
                            strTechniques += bLightClouds ?
                                ",RedAmbMapMag" : ",CloudMapMag";
                        }
                        if (UserINISettings.Instance.Displacement > 0)
                        {
                            strTechniques += ",DisplacementMap";
                        }
                        if (UserINISettings.Instance.CloudsEffect > 0)
                        {
                            strTechniques += ",LightMapMag";
                        }

                        if (UserINISettings.Instance.HighDetail >= 2)
                        {
                            strTechniques += ",AmbientLight";
                        }
                        if (UserINISettings.Instance.HighDetail >= 1)
                        {
                            strTechniques += ",Levels";
                        }

                        mapIni.SetStringValue("Basic", "NextScenario", "S_Shader");

                        // Tint
                        if (strLightClouds == "none")
                            strExtraLines += ClientConfiguration.TC_TINT_SNOWNIGHT;
                        else if (bLightClouds)
                            strExtraLines += ClientConfiguration.SHADER_TINT_SNOWNIGHT_VANILLA;
                        else
                            strExtraLines += ClientConfiguration.SHADER_TINT_SNOW_LIGHT;
                    }
                    else
                    {
                        //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_SNOWDAY);
                        strExtraLines += ClientConfiguration.SHADER_SNOWDAY_SETUP;
                        if (UserINISettings.Instance.HighDetail == 3)
                        {
                            strTechniques += ",PPFXBloom";
                        }
                        if (UserINISettings.Instance.HighDetail > 0)
                        {
                            strTechniques += ",FilmicPass";
                        }

                        if (UserINISettings.Instance.CloudsEffect > 0)
                        {
                            strTechniques += bLightClouds ?
                                ",RedAmbMapMag" : ",CloudMapMag";
                        }
                        if (UserINISettings.Instance.Displacement > 0)
                        {
                            strTechniques += ",DisplacementMap";
                        }
                        if (UserINISettings.Instance.CloudsEffect > 0)
                        {
                            strTechniques += ",LightMapMag";
                        }
                        if (UserINISettings.Instance.EnhancedLight > 0)
                        {
                            strTechniques += ",AnimMask";
                        }

                        if (UserINISettings.Instance.HighDetail >= 2)
                        {
                            strTechniques += ",AmbientLight";
                        }
                        if (UserINISettings.Instance.HighDetail >= 1)
                        {
                            strTechniques += ",Levels";
                        }

                        mapIni.SetStringValue("Basic", "NextScenario", "A_Shader");

                        // Tint
                        if (strLightClouds == "none")
                            strExtraLines += ClientConfiguration.TC_TINT_SNOWDAY;
                        else if (bLightClouds)
                            strExtraLines += ClientConfiguration.SHADER_TINT_SNOWDAY_VANILLA;
                        else
                            strExtraLines += ClientConfiguration.SHADER_TINT_SNOW_LIGHT;
                    }
                }
                else
                {
                    bool bIsNight = false;
                    bool bIsNight2 = false;
                    if (flAmbient <= 0.86f)
                    {
                        switch (mapIni.GetStringValue("Map", "Theater", "URBAN"))
                        {
                            case "SNOW":
                            case "DESERT":
                            case "TEMPERATE":
                                if (flAmbient < 0.70f)
                                    bIsNight2 = true;
                                else
                                    bIsNight = true;
                                break;
                            default:
                                if (flAmbient <= 0.70f)
                                    bIsNight = true;
                                else if (flAmbient < 0.50f)
                                    bIsNight2 = true;
                                break;
                        }
                    }

                    if (mapIni.GetStringValue("BaseInfo", "IsNight2", "none").ToLower() == "true") // Night2
                        bIsNight2 = true;
                    else if (mapIni.GetStringValue("BaseInfo", "IsNight2", "none").ToLower() == "false")
                        bIsNight2 = false;

                    if (bIsNight2)
                    {
                        //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_NIGHT2);
                        strExtraLines += ClientConfiguration.SHADER_GEN_SETUP;
                        if (UserINISettings.Instance.CloudsEffect > 0)
                        {
                            strTechniques += bLightClouds ?
                                ",RedAmbMapMag" : ",CloudMapMag";
                        }
                        if (UserINISettings.Instance.Displacement > 0)
                        {
                            strTechniques += ",DisplacementMap";
                        }
                        if (UserINISettings.Instance.CloudsEffect > 0)
                        {
                            strTechniques += ",LightMapMag";
                        }
                        if (UserINISettings.Instance.EnhancedLight > 0)
                        {
                            strTechniques += ",AnimMask";
                        }

                        if (UserINISettings.Instance.HighDetail > 0)
                        {
                            strTechniques += ",FilmicPass";
                        }

                        if (UserINISettings.Instance.HighDetail >= 2)
                        {
                            strTechniques += ",AmbientLight";
                        }
                        if (UserINISettings.Instance.HighDetail >= 1)
                        {
                            strTechniques += ",Levels";
                        }

                        mapIni.SetStringValue("Basic", "NextScenario", "M_Shader");

                        // Tint
                        if (strLightClouds == "none")
                            strExtraLines += ClientConfiguration.TC_TINT_NIGHT2;
                        else if (bLightClouds)
                            strExtraLines += ClientConfiguration.SHADER_TINT_NIGHT_VANILLA;
                        else
                            strExtraLines += ClientConfiguration.SHADER_TINT_NONE;
                    }
                    else
                    {
                        if (mapIni.GetStringValue("BaseInfo", "IsNight", "none").ToLower() == "true") // Vanilla Day / Night
                            bIsNight = true;
                        else if (mapIni.GetStringValue("BaseInfo", "IsNight", "none").ToLower() == "false")
                            bIsNight = false;

                        if (bIsNight)
                        {
                            //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_NIGHT);
                            strExtraLines += ClientConfiguration.SHADER_GEN_SETUP;
                            if (UserINISettings.Instance.CloudsEffect > 0)
                            {
                                strTechniques += bLightClouds ?
                                    ",RedAmbMapMag" : ",CloudMapMag";
                            }
                            if (UserINISettings.Instance.Displacement > 0)
                            {
                                strTechniques += ",DisplacementMap";
                            }
                            if (UserINISettings.Instance.CloudsEffect > 0)
                            {
                                strTechniques += ",LightMapMag";
                            }

                            if (UserINISettings.Instance.HighDetail > 0)
                            {
                                strTechniques += ",FilmicPass";
                            }

                            if (UserINISettings.Instance.HighDetail >= 2)
                            {
                                strTechniques += ",AmbientLight";
                            }
                            if (UserINISettings.Instance.HighDetail >= 1)
                            {
                                strTechniques += ",Levels";
                            }

                            mapIni.SetStringValue("Basic", "NextScenario", "N_Shader");

                            // Tint
                            if (strLightClouds == "none")
                                strExtraLines += ClientConfiguration.TC_TINT_NIGHT;
                            else if (bLightClouds)
                                strExtraLines += ClientConfiguration.SHADER_TINT_NIGHT_VANILLA;
                            else
                                strExtraLines += ClientConfiguration.SHADER_TINT_NONE;
                        }
                        else
                        {
                            //shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_DAY);
                            strExtraLines += ClientConfiguration.SHADER_GEN_SETUP;
                            if (UserINISettings.Instance.CloudsEffect > 0)
                            {
                                strTechniques += bLightClouds ?
                                    ",RedAmbMapMag" : ",CloudMapMag";
                            }
                            if (UserINISettings.Instance.Displacement > 0)
                            {
                                strTechniques += ",DisplacementMap";
                            }
                            if (UserINISettings.Instance.CloudsEffect > 0)
                            {
                                strTechniques += ",LightMapMag";
                            }

                            if (UserINISettings.Instance.HighDetail > 0)
                            {
                                strTechniques += ",FilmicPass";
                            }

                            mapIni.SetStringValue("Basic", "NextScenario", "D_Shader");

                            // Tint
                            if (strLightClouds == "none")
                                strExtraLines += ClientConfiguration.TC_TINT_DAY;
                            else if (bLightClouds)
                                strExtraLines += ClientConfiguration.SHADER_TINT_DAY;
                            else
                                strExtraLines += ClientConfiguration.SHADER_TINT_DAY;
                        }
                    }
                }

                strTechniques += ",Tint,UI_After";
                if (UserINISettings.Instance.WheelZoom)
                {
                    strTechniques += ",Magnifier";
                }
                /*switch (UserINISettings.Instance.AntiAliasing)
                {
                    case 1:
                        strTechniques += ",SMAA";
                        break;
                    case 2:
                        strTechniques += ",FXAA";
                        break;
                }*/
                if (UserINISettings.Instance.AntiAliasing == 1)
                {
                    strTechniques += ",FXAA";
                }

                shaderIniWriter.WriteLine(ClientConfiguration.SHADER_TECHNIQUE_1 + strTechniques);
                shaderIniWriter.WriteLine(ClientConfiguration.SHADER_TECHNIQUE_2 + strTechniques);

                if (!String.IsNullOrEmpty(strExtraLines))
                {
                    shaderIniWriter.WriteLine(strExtraLines);
                }
            }
            else
            {
                shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_DEFAULT); // Default
                mapIni.SetStringValue("Basic", "Name", "");
            }
            shaderIniWriter.WriteLine(shaderIniWriter.NewLine);
            shaderIniWriter.Close();

            Random random = new Random();
            IniFile musicSettingsIni = new IniFile(ProgramConstants.GamePath + SPMUSIC_SETTINGS);

            // Game Music Settings
            IniFile musicListIni = new IniFile(ProgramConstants.GamePath + "INI/MusicListTC.ini");
            IniFile musicConfigIni = new IniFile(ProgramConstants.GamePath + "INI/MusicConfigTC.ini");
            if (UserINISettings.Instance.SmartMusic && UserINISettings.Instance.MusicType < 2)
            {
                StartMusicIndex = musicSettingsIni.GetIntValue("Settings", "NextStartMusicIndex", 1);
                ConflictMusicIndex = musicSettingsIni.GetIntValue("Settings", "NextConflictMusicIndex", 1);

                string sideName = GetPlayerMusicSide(houseInfos);

                string[] indexArray = { "1", "2", "3", "4", "5" };
                List<string> tempArray = indexArray.ToList();
                tempArray.Remove(Convert.ToString(ConflictMusicIndex));
                indexArray = tempArray.ToArray();

                for (int i = 0; i < indexArray.Length; i++)
                {
                    int randomInt = random.Next(i, indexArray.Length);
                    string tempIndex = indexArray[i];
                    indexArray[i] = indexArray[randomInt];
                    indexArray[randomInt] = tempIndex;
                }

                musicConfigIni.SetStringValue("SP_Start", "Repeat", "yes");
                musicConfigIni.SetStringValue("SP_Conflict1", "Repeat", "yes");
                musicConfigIni.SetStringValue("SP_Conflict2", "Repeat", "yes");
                musicConfigIni.SetStringValue("SP_Conflict3", "Repeat", "yes");
                musicConfigIni.SetStringValue("SP_Conflict4", "Repeat", "yes");
                musicConfigIni.SetStringValue("SP_Conflict5", "Repeat", "yes");

                if (UserINISettings.Instance.MusicType == 0)
                {
                    musicConfigIni.SetStringValue("SP_Start", "Sound",
                        musicListIni.GetStringValue("GameStart", sideName + Convert.ToString(StartMusicIndex), "gdi_start_1"));

                    musicConfigIni.SetStringValue("SP_Conflict1", "Sound",
                        musicListIni.GetStringValue("GameConflict", sideName + Convert.ToString(ConflictMusicIndex), "gdi_conflict_1"));

                    musicConfigIni.SetStringValue("SP_Conflict2", "Sound",
                        musicListIni.GetStringValue("GameConflict", sideName + indexArray[0], "gdi_conflict_2"));

                    musicConfigIni.SetStringValue("SP_Conflict3", "Sound",
                        musicListIni.GetStringValue("GameConflict", sideName + indexArray[1], "gdi_conflict_3"));

                    musicConfigIni.SetStringValue("SP_Conflict4", "Sound",
                        musicListIni.GetStringValue("GameConflict", sideName + indexArray[2], "gdi_conflict_4"));

                    musicConfigIni.SetStringValue("SP_Conflict5", "Sound",
                        musicListIni.GetStringValue("GameConflict", sideName + indexArray[2], "gdi_conflict_5"));
                }
                else if (UserINISettings.Instance.MusicType == 1)
                {
                    musicConfigIni.SetStringValue("SP_Start", "Sound", "cc_start_" + Convert.ToString(StartMusicIndex));
                    musicConfigIni.SetStringValue("SP_Conflict1", "Sound", "cc_conflict_" + Convert.ToString(ConflictMusicIndex));
                    musicConfigIni.SetStringValue("SP_Conflict2", "Sound", "cc_conflict_" + indexArray[0]);
                    musicConfigIni.SetStringValue("SP_Conflict3", "Sound", "cc_conflict_" + indexArray[1]);
                    musicConfigIni.SetStringValue("SP_Conflict4", "Sound", "cc_conflict_" + indexArray[2]);
                    musicConfigIni.SetStringValue("SP_Conflict5", "Sound", "cc_conflict_" + indexArray[3]);
                }

                File.Delete(ProgramConstants.GamePath + SPMUSIC_SETTINGS);

                int[] startIndexArray = { 1, 2, 3 };
                List<int> tempStartArray = startIndexArray.ToList();
                tempStartArray.Remove(StartMusicIndex);
                startIndexArray = tempStartArray.ToArray();
                musicSettingsIni.SetIntValue("Settings", "NextStartMusicIndex", startIndexArray[random.Next(0, startIndexArray.Length)]);

                if (ConflictMusicIndex >= 4 || ConflictMusicIndex < 1)
                    musicSettingsIni.SetIntValue("Settings", "NextConflictMusicIndex", 1);
                else
                    musicSettingsIni.SetIntValue("Settings", "NextConflictMusicIndex", ConflictMusicIndex + 1);
            }
            musicConfigIni.WriteIniFile(ProgramConstants.GamePath + SPSOUND_INI);

            mapIni.WriteIniFile(ProgramConstants.GamePath + ProgramConstants.SPAWNMAP_INI);

            // Random skirmish loading screen
            LoadingScreenIndex = musicSettingsIni.GetIntValue("Settings", "NextLoadingIndex", 1);
            int[] loadingIndexArray = { 1, 2, 3, 4, 5, 6, 7 };
            List<int> tempLoadingArray = loadingIndexArray.ToList();
            tempLoadingArray.Remove(LoadingScreenIndex);
            loadingIndexArray = tempLoadingArray.ToArray();
            musicSettingsIni.SetIntValue("Settings", "NextLoadingIndex", loadingIndexArray[random.Next(0, loadingIndexArray.Length)]);

            if (LoadingScreenIndex >= 7 || LoadingScreenIndex < 1)
                musicSettingsIni.SetIntValue("Settings", "NextLoadingIndex", 1);
            else
                musicSettingsIni.SetIntValue("Settings", "NextLoadingIndex", LoadingScreenIndex + 1);

            musicSettingsIni.WriteIniFile();

            string loadingFilename = String.Empty;
            foreach (int resolution in MainClientConstants.ResolutionList)
            {
                if (UserINISettings.Instance.IngameScreenWidth >= resolution)
                {
                    loadingFilename += resolution + "skirmishloads" + LoadingScreenIndex + ".big";
                    break;
                }
            }

            if (String.IsNullOrEmpty(loadingFilename))
                loadingFilename += "1024skirmishloads" + LoadingScreenIndex + ".big";

            string bigPath = ProgramConstants.GetBaseSharedPath() + loadingFilename;
            if (File.Exists(bigPath))
                File.Copy(bigPath, ProgramConstants.GamePath + "tcextrab00.big", true);
            else
                Logger.Log("Cloud not find skirmish loading screen file: " + bigPath);
        }

        private void ManipulateStartingLocations(IniFile mapIni, PlayerHouseInfo[] houseInfos)
        {
            if (RemoveStartingLocations)
            {
                if (true/*Map.EnforceMaxPlayers*/)
                    return;

                // All random starting locations given by the game
                IniSection waypointSection = mapIni.GetSection("Waypoints");
                if (waypointSection == null)
                    return;

                // TODO implement IniSection.RemoveKey in Rampastring.Tools, then
                // remove implementation that depends on internal implementation
                // of IniSection
                for (int i = 0; i <= 7; i++)
                {
                    int index = waypointSection.Keys.FindIndex(k => !string.IsNullOrEmpty(k.Key) && k.Key == i.ToString());
                    if (index > -1)
                        waypointSection.Keys.RemoveAt(index);
                }
            }

            // Multiple players cannot properly share the same starting location
            // without breaking the SpawnX house logic that pre-placed objects depend on

            // To work around this, we add new starting locations that just point
            // to the same cell coordinates as existing stacked starting locations
            // and make additional players in the same start loc start from the new
            // starting locations instead.

            // As an additional restriction, players can only start from waypoints 0 to 7.
            // That means that if the map already has too many starting waypoints,
            // we need to move existing (but un-occupied) starting waypoints to point 
            // to the stacked locations so we can spawn the players there.


            // Check for stacked starting locations (locations with more than 1 player on it)
            bool[] startingLocationUsed = new bool[MAX_PLAYER_COUNT];
            bool stackedStartingLocations = false;
            foreach (PlayerHouseInfo houseInfo in houseInfos)
            {
                if (houseInfo.RealStartingWaypoint > -1)
                {
                    startingLocationUsed[houseInfo.RealStartingWaypoint] = true;

                    // If assigned starting waypoint is unknown while the real 
                    // starting location is known, it means that
                    // the location is shared with another player
                    if (houseInfo.StartingWaypoint == -1)
                    {
                        stackedStartingLocations = true;
                    }
                }
            }

            // If any starting location is stacked, re-arrange all starting locations
            // so that unused starting locations are removed and made to point at used
            // starting locations
            if (!stackedStartingLocations)
                return;

            // We also need to modify spawn.ini because WriteSpawnIni
            // doesn't handle stacked positions.
            // We could move this code there, but then we'd have to process
            // the stacked locations in two places (here and in WriteSpawnIni)
            // because we'd need to modify the map anyway.
            // Not sure whether having it like this or in WriteSpawnIni
            // is better, but this implementation is quicker to write for now.
            IniFile spawnIni = new IniFile(ProgramConstants.GamePath + ProgramConstants.SPAWNER_SETTINGS);

            // For each player, check if they're sharing the starting location
            // with someone else
            // If they are, find an unused waypoint and assign their 
            // starting location to match that
            for (int pId = 0; pId < houseInfos.Length; pId++)
            {
                PlayerHouseInfo houseInfo = houseInfos[pId];

                if (houseInfo.RealStartingWaypoint > -1 &&
                    houseInfo.StartingWaypoint == -1)
                {
                    // Find first unused starting location index
                    int unusedLocation = -1;
                    for (int i = 0; i < startingLocationUsed.Length; i++)
                    {
                        if (!startingLocationUsed[i])
                        {
                            unusedLocation = i;
                            startingLocationUsed[i] = true;
                            break;
                        }
                    }

                    houseInfo.StartingWaypoint = unusedLocation;
                    mapIni.SetIntValue("Waypoints", unusedLocation.ToString(),
                        mapIni.GetIntValue("Waypoints", houseInfo.RealStartingWaypoint.ToString(), 0));
                    spawnIni.SetIntValue("SpawnLocations", $"Multi{pId + 1}", unusedLocation);
                }
            }

            spawnIni.WriteIniFile();
        }

        /// <summary>
        /// Writes spawn.ini, writes the map file, initializes statistics and
        /// starts the game process.
        /// </summary>
        protected virtual void StartGame(bool bCanControlSpeed = true)
        {
            bool gsCustomizeRestriction = chkRandomOnly != null && chkRandomOnly.Checked;

            PlayerHouseInfo[] houseInfos = WriteSpawnIni(!bCanControlSpeed, gsCustomizeRestriction);
            InitializeMatchStatistics(houseInfos);
            WriteMap(houseInfos, !bCanControlSpeed);

            GameProcessLogic.GameProcessExited += GameProcessExited_Callback;

            GameProcessLogic.StartGameProcess(bCanControlSpeed);
            UpdateDiscordPresence(true);
        }

        private void GameProcessExited_Callback() => AddCallback(new Action(GameProcessExited), null);

        protected virtual void GameProcessExited()
        {
            GameProcessLogic.GameProcessExited -= GameProcessExited_Callback;

            Logger.Log("GameProcessExited: Parsing statistics.");

            matchStatistics.ParseStatistics(ProgramConstants.GamePath, ClientConfiguration.Instance.LocalGame, false);

            LogbuchParser.ParseForSkirmish(matchStatistics);

            LogbuchParser.ClearTrash();

            Logger.Log("GameProcessExited: Adding match to statistics.");

            StatisticsManager.Instance.AddMatchAndSaveDatabase(true, matchStatistics);

            ClearReadyStatuses();

            CopyPlayerDataToUI();

            UpdateDiscordPresence(true);
        }

        /// <summary>
        /// "Copies" player information from the UI to internal memory,
        /// applying users' player options changes.
        /// </summary>
        protected virtual void CopyPlayerDataFromUI(object sender, EventArgs e)
        {
            if (PlayerUpdatingInProgress)
                return;

            var senderDropDown = (XNADropDown)sender;
            if ((bool)senderDropDown.Tag)
                ClearReadyStatuses();

            var oldSideId = Players.Find(p => p.Name == ProgramConstants.PLAYERNAME)?.SideId;

            for (int pId = 0; pId < Players.Count; pId++)
            {
                PlayerInfo pInfo = Players[pId];

                pInfo.ColorId = ddPlayerColors[pId].SelectedIndex;
                pInfo.SideId = ddPlayerSides[pId].SelectedIndex;
                pInfo.StartingLocation = ddPlayerStarts[pId].SelectedIndex;
                pInfo.TeamId = ddPlayerTeams[pId].SelectedIndex;

                if (pInfo.SideId == SideCount + RandomSelectorCount)
                    pInfo.StartingLocation = 0;

                XNADropDown ddName = ddPlayerNames[pId];

                switch (ddName.SelectedIndex)
                {
                    case 0:
                        break;
                    case 1:
                        ddName.SelectedIndex = 0;
                        break;
                    case 2:
                        KickPlayer(pId);
                        break;
                    case 3:
                        BanPlayer(pId);
                        break;
                }
            }

            AIPlayers.Clear();
            for (int cmbId = Players.Count; cmbId < 8; cmbId++)
            {
                XNADropDown dd = ddPlayerNames[cmbId];
                dd.Items[0].Text = "-";

                if (dd.SelectedIndex < 1)
                    continue;

                PlayerInfo aiPlayer = new PlayerInfo
                {
                    Name = dd.Items[dd.SelectedIndex].Text,
                    AILevel = 2 - (dd.SelectedIndex - 1),
                    SideId = Math.Max(ddPlayerSides[cmbId].SelectedIndex, 0),
                    ColorId = Math.Max(ddPlayerColors[cmbId].SelectedIndex, 0),
                    StartingLocation = Math.Max(ddPlayerStarts[cmbId].SelectedIndex, 0),
                    TeamId = Map != null && Map.IsCoop ? 1 : Math.Max(ddPlayerTeams[cmbId].SelectedIndex, 0),
                    IsAI = true
                };

                AIPlayers.Add(aiPlayer);
            }

            CopyPlayerDataToUI();
            btnLaunchGame.SetRank(GetRank());

            if (oldSideId != Players.Find(p => p.Name == ProgramConstants.PLAYERNAME)?.SideId)
                UpdateDiscordPresence();
        }

        /// <summary>
        /// Sets the ready status of all non-host human players to false.
        /// </summary>
        /// <param name="resetAutoReady">If set, players with autoready enabled are reset as well.</param>
        protected void ClearReadyStatuses(bool resetAutoReady = false)
        {
            for (int i = 1; i < Players.Count; i++)
            {
                if (resetAutoReady || !Players[i].AutoReady || Players[i].IsInGame)
                    Players[i].Ready = false;
            }
        }

        private bool CanRightClickMultiplayer(XNADropDownItem selectedPlayer)
        {
            return selectedPlayer != null &&
                   selectedPlayer.Text != ProgramConstants.PLAYERNAME &&
                   !ProgramConstants.AI_PLAYER_NAMES.Contains(selectedPlayer.Text);
        }

        private void MultiplayerName_RightClick(object sender, EventArgs e)
        {
            var selectedPlayer = ((XNADropDown)sender).SelectedItem;
            if (!CanRightClickMultiplayer(selectedPlayer))
                return;

            if (selectedPlayer == null ||
                selectedPlayer.Text == ProgramConstants.PLAYERNAME)
            {
                return;
            }

            MultiplayerNameRightClicked?.Invoke(this, new MultiplayerNameRightClickedEventArgs(selectedPlayer.Text));
        }

        /// <summary>
        /// Applies player information changes done in memory to the UI.
        /// </summary>
        protected virtual void CopyPlayerDataToUI()
        {
            PlayerUpdatingInProgress = true;

            bool allowOptionsChange = AllowPlayerOptionsChange();
            var playerExtraOptions = GetPlayerExtraOptions();

            List<int> sideIndexList = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

            // Human players
            for (int pId = 0; pId < Players.Count; pId++)
            {
                PlayerInfo pInfo = Players[pId];

                pInfo.Index = pId;

                XNADropDown ddPlayerName = ddPlayerNames[pId];
                ddPlayerName.Items[0].Text = pInfo.Name;
                ddPlayerName.Items[1].Text = string.Empty;
                ddPlayerName.Items[2].Text = "Kick".L10N("UI:Main:Kick");
                ddPlayerName.Items[3].Text = "Ban".L10N("UI:Main:Ban");
                ddPlayerName.SelectedIndex = 0;
                ddPlayerName.AllowDropDown = false;

                bool allowPlayerOptionsChange = allowOptionsChange || pInfo.Name == ProgramConstants.PLAYERNAME;

                ddPlayerSides[pId].SelectedIndex = pInfo.SideId;
                ddPlayerSides[pId].AllowDropDown = !playerExtraOptions.IsForceRandomSides && allowPlayerOptionsChange;

                ddPlayerColors[pId].SelectedIndex = pInfo.ColorId;
                ddPlayerColors[pId].AllowDropDown = !playerExtraOptions.IsForceRandomColors && allowPlayerOptionsChange;

                ddPlayerStarts[pId].SelectedIndex = pInfo.StartingLocation;

                ddPlayerTeams[pId].SelectedIndex = pInfo.TeamId;
                if (Map != null && GameModeMap != null)
                {
                    ddPlayerTeams[pId].AllowDropDown = !playerExtraOptions.IsForceRandomTeams && allowPlayerOptionsChange && !Map.IsCoop && !Map.ForceNoTeams && !GameMode.ForceNoTeams;
                    ddPlayerStarts[pId].AllowDropDown = !playerExtraOptions.IsForceRandomStarts && allowPlayerOptionsChange && (Map.IsCoop || !Map.ForceRandomStartLocations && !GameMode.ForceRandomStartLocations);
                }

                // disallowed sides in slot0
                if (Map != null && GameMode != null)
                {
                    if (Map.CoopInfo != null)
                    {
                        ddPlayerStarts[pId].Items[0].Selectable = true;
                        for (int index = RandomSelectorCount; index < SideCount + RandomSelectorCount; index++) // enable all non-random
                            ddPlayerSides[pId].Items[index].Selectable = true;
                        if (Map.CoopInfo.DisallowedPlayerSides0.Count <= 0) // using default blocking
                        {
                            // default blocking
                            foreach (int disallowedSideIndex in Map.CoopInfo.DisallowedPlayerSides)
                            {
                                if (disallowedSideIndex > SideCount + RandomSelectorCount)
                                    continue;

                                ddPlayerSides[pId].Items[disallowedSideIndex + RandomSelectorCount].Selectable = false;
                            }
                        }
                        else // using custom blocking
                        {
                            // set default to slot1
                            if (pInfo.StartingLocation == 0)
                            {
                                pInfo.StartingLocation = 1;
                                ddPlayerStarts[pId].SelectedIndex = 1;
                            }

                            // disable random start and random selector
                            ddPlayerStarts[pId].Items[0].Selectable = false;
                            for (int index = 0; index < RandomSelectorCount; index++)
                                ddPlayerSides[pId].Items[index].Selectable = false;

                            // custom blocking
                            if (pInfo.StartingLocation == 1)
                            {
                                // set default to spectator
                                bool bShouldContinue = true;
                                foreach (int disallowedSideIndex0 in Map.CoopInfo.DisallowedPlayerSides0)
                                {
                                    if (disallowedSideIndex0 > SideCount + RandomSelectorCount)
                                        continue;

                                    if (bShouldContinue && pInfo.SideId < RandomSelectorCount ||
                                        pInfo.SideId == disallowedSideIndex0 + RandomSelectorCount)
                                    {
                                        pInfo.SideId = sideIndexList.Except(Map.CoopInfo.DisallowedPlayerSides0).First()
                                            + RandomSelectorCount;
                                        ddPlayerSides[pId].SelectedIndex = pInfo.SideId;
                                        bShouldContinue = false;
                                    }

                                    ddPlayerSides[pId].Items[disallowedSideIndex0 + RandomSelectorCount].Selectable = false;
                                }
                            }
                            else // use default
                            {
                                // set default to spectator
                                bool bShouldContinue = true;
                                foreach (int disallowedSideIndex in Map.CoopInfo.DisallowedPlayerSides)
                                {
                                    if (disallowedSideIndex > SideCount + RandomSelectorCount)
                                        continue;

                                    // set default to spectator
                                    if (bShouldContinue && pInfo.SideId < RandomSelectorCount ||
                                        pInfo.SideId == disallowedSideIndex + RandomSelectorCount)
                                    {
                                        pInfo.SideId = sideIndexList.Except(Map.CoopInfo.DisallowedPlayerSides).First()
                                            + RandomSelectorCount;
                                        ddPlayerSides[pId].SelectedIndex = pInfo.SideId;
                                        bShouldContinue = false;
                                    }

                                    ddPlayerSides[pId].Items[disallowedSideIndex + RandomSelectorCount].Selectable = false;
                                }
                            }
                        }
                    }

                    ddPlayerTeams[pId].AllowDropDown = allowPlayerOptionsChange && !Map.IsCoop && !Map.ForceNoTeams && !GameMode.ForceNoTeams;
                    ddPlayerStarts[pId].AllowDropDown = allowPlayerOptionsChange && (Map.IsCoop || !Map.ForceRandomStartLocations && !GameMode.ForceRandomStartLocations);
                }
            }

            // AI players
            for (int aiId = 0; aiId < AIPlayers.Count; aiId++)
            {
                PlayerInfo aiInfo = AIPlayers[aiId];

                int index = Players.Count + aiId;

                aiInfo.Index = index;

                XNADropDown ddPlayerName = ddPlayerNames[index];
                ddPlayerName.Items[0].Text = "-";
                if (GameMode != null && GameMode.Name == "Difficulty Tier")
                {
                    ddPlayerName.Items[1].Text = "Insane AI".L10N("UI:Main:InsaneAI");
                    ddPlayerName.Items[2].Text = "Brutal AI".L10N("UI:Main:BrutalAI");
                    ddPlayerName.Items[3].Text = "Abyss AI".L10N("UI:Main:AbyssAI");
                }
                else
                {
                    ddPlayerName.Items[1].Text = "Easy AI".L10N("UI:Main:EasyAI");
                    ddPlayerName.Items[2].Text = "Normal AI".L10N("UI:Main:NormalAI");
                    ddPlayerName.Items[3].Text = "Hard AI".L10N("UI:Main:HardAI");
                }
                ddPlayerName.SelectedIndex = 3 - aiInfo.AILevel;
                ddPlayerName.AllowDropDown = allowOptionsChange;

                ddPlayerSides[index].SelectedIndex = aiInfo.SideId;
                ddPlayerSides[index].AllowDropDown = !playerExtraOptions.IsForceRandomSides && allowOptionsChange;

                ddPlayerColors[index].SelectedIndex = aiInfo.ColorId;
                ddPlayerColors[index].AllowDropDown = !playerExtraOptions.IsForceRandomColors && allowOptionsChange;

                ddPlayerStarts[index].SelectedIndex = aiInfo.StartingLocation;

                ddPlayerTeams[index].SelectedIndex = aiInfo.TeamId;

                if (Map != null && GameMode != null)
                {
                    ddPlayerTeams[index].AllowDropDown = !playerExtraOptions.IsForceRandomTeams && allowOptionsChange && !Map.IsCoop && !Map.ForceNoTeams && !GameMode.ForceNoTeams;
                    ddPlayerStarts[index].AllowDropDown = !playerExtraOptions.IsForceRandomStarts && allowOptionsChange && (Map.IsCoop || !Map.ForceRandomStartLocations && !GameMode.ForceRandomStartLocations);
                }
            }

            // Unused player slots
            for (int ddIndex = Players.Count + AIPlayers.Count; ddIndex < MAX_PLAYER_COUNT; ddIndex++)
            {
                XNADropDown ddPlayerName = ddPlayerNames[ddIndex];
                ddPlayerName.AllowDropDown = false;
                ddPlayerName.Items[0].Text = string.Empty;
                if (GameMode != null && GameMode.Name == "Difficulty Tier")
                {
                    ddPlayerName.Items[1].Text = "Insane AI".L10N("UI:Main:InsaneAI");
                    ddPlayerName.Items[2].Text = "Brutal AI".L10N("UI:Main:BrutalAI");
                    ddPlayerName.Items[3].Text = "Abyss AI".L10N("UI:Main:AbyssAI");
                }
                else
                {
                    ddPlayerName.Items[1].Text = "Easy AI".L10N("UI:Main:EasyAI");
                    ddPlayerName.Items[2].Text = "Normal AI".L10N("UI:Main:NormalAI");
                    ddPlayerName.Items[3].Text = "Hard AI".L10N("UI:Main:HardAI");
                }
                ddPlayerName.SelectedIndex = 0;

                ddPlayerSides[ddIndex].SelectedIndex = -1;
                ddPlayerSides[ddIndex].AllowDropDown = false;

                ddPlayerColors[ddIndex].SelectedIndex = -1;
                ddPlayerColors[ddIndex].AllowDropDown = false;

                ddPlayerStarts[ddIndex].SelectedIndex = -1;
                ddPlayerStarts[ddIndex].AllowDropDown = false;

                ddPlayerTeams[ddIndex].SelectedIndex = -1;
                ddPlayerTeams[ddIndex].AllowDropDown = false;
            }

            CheckTeamLimit();

            if (allowOptionsChange && Players.Count + AIPlayers.Count < MAX_PLAYER_COUNT)
                ddPlayerNames[Players.Count + AIPlayers.Count].AllowDropDown = true;

            MapPreviewBox.UpdateStartingLocationTexts();
            UpdateMapPreviewBoxEnabledStatus();

            PlayerUpdatingInProgress = false;
        }

        /// <summary>
        /// Updates the enabled status of starting location selectors
        /// in the map preview box.
        /// </summary>
        protected abstract void UpdateMapPreviewBoxEnabledStatus();

        /// <summary>
        /// Override this in a derived class to kick players.
        /// </summary>
        /// <param name="playerIndex">The index of the player that should be kicked.</param>
        protected virtual void KickPlayer(int playerIndex)
        {
            // Do nothing by default
        }

        /// <summary>
        /// Override this in a derived class to ban players.
        /// </summary>
        /// <param name="playerIndex">The index of the player that should be banned.</param>
        protected virtual void BanPlayer(int playerIndex)
        {
            // Do nothing by default
        }

        /// <summary>
        /// Changes the current map and game mode.
        /// </summary>
        /// <param name="gameModeMap">The new game mode map.</param>
        protected virtual void ChangeMap(GameModeMap gameModeMap)
        {
            GameModeMap = gameModeMap;

            if (GameMode == null || Map == null)
            {
                lblMapName.Text = "Map: Unknown".L10N("UI:Main:MapUnknown");
                lblMapAuthor.Text = "By Unknown Author".L10N("UI:Main:AuthorByUnknown");
                lblGameMode.Text = "Game mode: Unknown".L10N("UI:Main:GameModeUnknown");
                lblMapSize.Text = "Size: Not available".L10N("UI:Main:MapSizeUnknown");

                lblMapAuthor.X = MapPreviewBox.Right - lblMapAuthor.Width;

                MapPreviewBox.GameModeMap = null;

                return;
            }

            lblMapName.Text = "Map:".L10N("UI:Main:Map") + " " + Renderer.GetSafeString(Map.Name, lblMapName.FontIndex);
            lblMapAuthor.Text = "By".L10N("UI:Main:AuthorBy") + " " + Renderer.GetSafeString(Map.Author, lblMapAuthor.FontIndex);
            lblGameMode.Text = "Game mode:".L10N("UI:Main:GameModeLabel") + " " + GameMode.UIName;
            lblMapSize.Text = "Size:".L10N("UI:Main:MapSize") + " " + Map.GetSizeString();

            disableGameOptionUpdateBroadcast = true;

            // Clear forced options
            foreach (var ddGameOption in DropDowns)
                ddGameOption.AllowDropDown = true;

            foreach (var checkBox in CheckBoxes)
                checkBox.AllowChecking = true;

            // We could either pass the CheckBoxes and DropDowns of this class
            // to the Map and GameMode instances and let them apply their forced
            // options, or we could do it in this class with helper functions.
            // The second approach is probably clearer.

            // We use these temp lists to determine which options WERE NOT forced
            // by the map. We then return these to user-defined settings.
            // This prevents forced options from one map getting carried
            // to other maps.

            var checkBoxListClone = new List<GameLobbyCheckBox>(CheckBoxes);
            var dropDownListClone = new List<GameLobbyDropDown>(DropDowns);

            ApplyForcedCheckBoxOptions(checkBoxListClone, GameMode.ForcedCheckBoxValues);
            ApplyForcedCheckBoxOptions(checkBoxListClone, Map.ForcedCheckBoxValues);

            ApplyForcedDropDownOptions(dropDownListClone, GameMode.ForcedDropDownValues);
            ApplyForcedDropDownOptions(dropDownListClone, Map.ForcedDropDownValues);

            foreach (var chkBox in checkBoxListClone)
                chkBox.Checked = chkBox.HostChecked;

            foreach (var dd in dropDownListClone)
                dd.SelectedIndex = dd.HostSelectedIndex;

            // Enable all sides by default
            foreach (var ddSide in ddPlayerSides)
            {
                ddSide.Items.ForEach(item => item.Selectable = true);
            }

            // Enable all colors by default
            foreach (var ddColor in ddPlayerColors)
            {
                ddColor.Items.ForEach(item => item.Selectable = true);
            }

            // Apply starting locations
            foreach (var ddStart in ddPlayerStarts)
            {
                ddStart.Items.Clear();

                ddStart.AddItem("-");

                for (int i = 1; i <= Map.MaxPlayers; i++)
                    ddStart.AddItem(i.ToString());
            }


            // Check if AI players allowed
            bool AIAllowed = !(Map.MultiplayerOnly || GameMode.MultiplayerOnly) ||
                             !(Map.HumanPlayersOnly || GameMode.HumanPlayersOnly);
            foreach (var ddName in ddPlayerNames)
            {
                if (ddName.Items.Count > 3)
                {
                    ddName.Items[1].Selectable = AIAllowed;
                    ddName.Items[2].Selectable = AIAllowed;
                    ddName.Items[3].Selectable = AIAllowed;
                }
            }

            if (!AIAllowed) AIPlayers.Clear();
            IEnumerable<PlayerInfo> concatPlayerList = Players.Concat(AIPlayers).ToList();

            foreach (PlayerInfo pInfo in concatPlayerList)
            {
                if (pInfo.StartingLocation > Map.MaxPlayers ||
                    (!Map.IsCoop && (Map.ForceRandomStartLocations || GameMode.ForceRandomStartLocations)))
                    pInfo.StartingLocation = 0;
                if (!Map.IsCoop && (Map.ForceNoTeams || GameMode.ForceNoTeams))
                    pInfo.TeamId = 0;
            }

            CheckDisallowedSides();

            if (Map.CoopInfo != null)
            {
                // Co-Op map disallowed color logic
                foreach (int disallowedColorIndex in Map.CoopInfo.DisallowedPlayerColors)
                {
                    if (disallowedColorIndex >= MPColors.Count)
                        continue;

                    foreach (XNADropDown ddColor in ddPlayerColors)
                        ddColor.Items[disallowedColorIndex + 1].Selectable = false;

                    foreach (PlayerInfo pInfo in concatPlayerList)
                    {
                        if (pInfo.ColorId == disallowedColorIndex + 1)
                            pInfo.ColorId = 0;
                    }
                }

                // Force teams
                foreach (PlayerInfo pInfo in concatPlayerList)
                    pInfo.TeamId = 1;
            }

            OnGameOptionChanged();

            MapPreviewBox.GameModeMap = GameModeMap;
            CopyPlayerDataToUI();

            disableGameOptionUpdateBroadcast = false;

            PlayerExtraOptionsPanel.UpdateForMap(Map);
        }

        private void ApplyForcedCheckBoxOptions(List<GameLobbyCheckBox> optionList,
            List<KeyValuePair<string, bool>> forcedOptions)
        {
            foreach (KeyValuePair<string, bool> option in forcedOptions)
            {
                GameLobbyCheckBox checkBox = CheckBoxes.Find(chk => chk.Name == option.Key);
                if (checkBox != null)
                {
                    checkBox.Checked = option.Value;
                    checkBox.AllowChecking = false;
                    optionList.Remove(checkBox);
                }
            }
        }

        private void ApplyForcedDropDownOptions(List<GameLobbyDropDown> optionList,
            List<KeyValuePair<string, int>> forcedOptions)
        {
            foreach (KeyValuePair<string, int> option in forcedOptions)
            {
                GameLobbyDropDown dropDown = DropDowns.Find(dd => dd.Name == option.Key);
                if (dropDown != null)
                {
                    dropDown.SelectedIndex = option.Value;
                    dropDown.AllowDropDown = false;
                    optionList.Remove(dropDown);
                }
            }
        }

        protected string AILevelToName(int aiLevel)
        {
            if (GameMode != null && GameMode.Name == "Difficulty Tier")
            {
                switch (aiLevel)
                {
                    case 0:
                        return "Abyss AI".L10N("UI:Main:AbyssAI");
                    case 1:
                        return "Brutal AI".L10N("UI:Main:BrutalAI");
                    case 2:
                        return "Insane AI".L10N("UI:Main:InsaneAI");
                }
            }
            else
            {
                switch (aiLevel)
                {
                    case 0:
                        return "Hard AI".L10N("UI:Main:HardAI");
                    case 1:
                        return "Normal AI".L10N("UI:Main:NormalAI");
                    case 2:
                        return "Easy AI".L10N("UI:Main:EasyAI");
                }
            }

            return string.Empty;
        }

        protected GameType GetGameType()
        {
            int teamCount = GetPvPTeamCount();

            if (teamCount == 0)
                return GameType.FFA;

            if (teamCount == 1)
                return GameType.Coop;

            return GameType.TeamGame;
        }

        protected int GetRank()
        {
            if (GameMode == null || Map == null)
                return RANK_NONE;

            foreach (GameLobbyCheckBox checkBox in CheckBoxes)
            {
                if ((checkBox.MapScoringMode == CheckBoxMapScoringMode.DenyWhenChecked && checkBox.Checked) ||
                    (checkBox.MapScoringMode == CheckBoxMapScoringMode.DenyWhenUnchecked && !checkBox.Checked))
                {
                    return RANK_NONE;
                }
            }

            PlayerInfo localPlayer = Players.Find(p => p.Name == ProgramConstants.PLAYERNAME);

            if (localPlayer == null)
                return RANK_NONE;

            if (IsPlayerSpectator(localPlayer))
                return RANK_NONE;

            // These variables are used by both the skirmish and multiplayer code paths
            int[] teamMemberCounts = new int[5];
            int lowestEnemyAILevel = 2;
            int highestAllyAILevel = 0;

            foreach (PlayerInfo aiPlayer in AIPlayers)
            {
                teamMemberCounts[aiPlayer.TeamId]++;

                if (aiPlayer.TeamId > 0 && aiPlayer.TeamId == localPlayer.TeamId)
                {
                    if (aiPlayer.ReversedAILevel > highestAllyAILevel)
                        highestAllyAILevel = aiPlayer.ReversedAILevel;
                }
                else
                {
                    if (aiPlayer.ReversedAILevel < lowestEnemyAILevel)
                        lowestEnemyAILevel = aiPlayer.ReversedAILevel;
                }
            }

            if (isMultiplayer)
            {
                if (Players.Count == 1)
                    return RANK_NONE;

                // PvP stars for 2-player and 3-player maps
                if (Map.MaxPlayers <= 3)
                {
                    List<PlayerInfo> filteredPlayers = Players.Where(p => !IsPlayerSpectator(p)).ToList();

                    if (AIPlayers.Count > 0)
                        return RANK_NONE;

                    if (filteredPlayers.Count != Map.MaxPlayers)
                        return RANK_NONE;

                    int localTeamIndex = localPlayer.TeamId;
                    if (localTeamIndex > 0 && filteredPlayers.Count(p => p.TeamId == localTeamIndex) > 1)
                        return RANK_NONE;

                    return RANK_HARD;
                }

                // Coop stars for maps with 4 or more players
                // See the code in StatisticsManager.GetRankForCoopMatch for the conditions

                if (Players.Find(p => IsPlayerSpectator(p)) != null)
                    return RANK_NONE;

                if (AIPlayers.Count == 0)
                    return RANK_NONE;

                if (Players.Find(p => p.TeamId != localPlayer.TeamId) != null)
                    return RANK_NONE;

                if (Players.Find(p => p.TeamId == 0) != null)
                    return RANK_NONE;

                if (AIPlayers.Find(p => p.TeamId == 0) != null)
                    return RANK_NONE;

                teamMemberCounts[localPlayer.TeamId] += Players.Count;

                if (lowestEnemyAILevel < highestAllyAILevel)
                {
                    // Check that the player's AI allies aren't stronger 
                    return RANK_NONE;
                }

                // Check that all teams have at least as many players
                // as the human players' team
                int allyCount = teamMemberCounts[localPlayer.TeamId];

                for (int i = 1; i < 5; i++)
                {
                    if (i == localPlayer.TeamId)
                        continue;

                    if (teamMemberCounts[i] > 0)
                    {
                        if (teamMemberCounts[i] < allyCount)
                            return RANK_NONE;
                    }
                }

                return lowestEnemyAILevel + 1;
            }

            // *********
            // Skirmish!
            // *********

            if (AIPlayers.Count != Map.MaxPlayers - 1)
                return RANK_NONE;

            teamMemberCounts[localPlayer.TeamId]++;

            if (lowestEnemyAILevel < highestAllyAILevel)
            {
                // Check that the player's AI allies aren't stronger 
                return RANK_NONE;
            }

            if (localPlayer.TeamId > 0)
            {
                // Check that all teams have at least as many players
                // as the local player's team
                int allyCount = teamMemberCounts[localPlayer.TeamId];

                for (int i = 1; i < 5; i++)
                {
                    if (i == localPlayer.TeamId)
                        continue;

                    if (teamMemberCounts[i] > 0)
                    {
                        if (teamMemberCounts[i] < allyCount)
                            return RANK_NONE;
                    }
                }

                // Check that there is a team other than the players' team that is at least as large
                bool pass = false;
                for (int i = 1; i < 5; i++)
                {
                    if (i == localPlayer.TeamId)
                        continue;

                    if (teamMemberCounts[i] >= allyCount)
                    {
                        pass = true;
                        break;
                    }
                }

                if (!pass)
                    return RANK_NONE;
            }

            return lowestEnemyAILevel + 1;
        }

        protected string AddGameOptionPreset(string name)
        {
            string error = GameOptionPreset.IsNameValid(name);
            if (!string.IsNullOrEmpty(error))
                return error;

            GameOptionPreset preset = new GameOptionPreset(name);
            foreach (GameLobbyCheckBox checkBox in CheckBoxes)
            {
                preset.AddCheckBoxValue(checkBox.Name, checkBox.Checked);
            }

            foreach (GameLobbyDropDown dropDown in DropDowns)
            {
                preset.AddDropDownValue(dropDown.Name, dropDown.SelectedIndex);
            }

            GameOptionPresets.Instance.AddPreset(preset);
            return null;
        }

        public bool LoadGameOptionPreset(string name)
        {
            GameOptionPreset preset = GameOptionPresets.Instance.GetPreset(name);
            if (preset == null)
                return false;

            disableGameOptionUpdateBroadcast = true;

            var checkBoxValues = preset.GetCheckBoxValues();
            foreach (var kvp in checkBoxValues)
            {
                GameLobbyCheckBox checkBox = CheckBoxes.Find(c => c.Name == kvp.Key);
                if (checkBox != null && checkBox.AllowChanges && checkBox.AllowChecking)
                    checkBox.Checked = kvp.Value;
            }

            var dropDownValues = preset.GetDropDownValues();
            foreach (var kvp in dropDownValues)
            {
                GameLobbyDropDown dropDown = DropDowns.Find(d => d.Name == kvp.Key);
                if (dropDown != null && dropDown.AllowDropDown)
                    dropDown.SelectedIndex = kvp.Value;
            }

            disableGameOptionUpdateBroadcast = false;
            OnGameOptionChanged();
            return true;
        }

        protected abstract bool AllowPlayerOptionsChange();
    }
}
