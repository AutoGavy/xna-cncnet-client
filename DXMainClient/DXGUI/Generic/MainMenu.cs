using ClientCore;
using ClientGUI;
using DTAClient.Domain;
using DTAClient.Domain.Multiplayer.CnCNet;
using DTAClient.DXGUI.Multiplayer;
using DTAClient.DXGUI.Multiplayer.CnCNet;
using DTAClient.DXGUI.Multiplayer.GameLobby;
using DTAClient.Online;
using DTAConfig;
using Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Updater;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace DTAClient.DXGUI.Generic
{
    /// <summary>
    /// The main menu of the client.
    /// </summary>
    class MainMenu : XNAWindow, ISwitchable
    {
        private const float MEDIA_PLAYER_VOLUME_FADE_STEP = 0.01f;
        private const float MEDIA_PLAYER_VOLUME_EXIT_FADE_STEP = 0.025f;
        private const double UPDATE_RE_CHECK_THRESHOLD = 30.0;

        /// <summary>
        /// Creates a new instance of the main menu.
        /// </summary>
        public MainMenu(WindowManager windowManager, SkirmishLobby skirmishLobby,
            LANLobby lanLobby, TopBar topBar, OptionsWindow optionsWindow,
            CnCNetLobby cncnetLobby,
            CnCNetManager connectionManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.skirmishLobby = skirmishLobby;
            this.lanLobby = lanLobby;
            this.topBar = topBar;
            this.connectionManager = connectionManager;
            this.optionsWindow = optionsWindow;
            this.cncnetLobby = cncnetLobby;
            this.discordHandler = discordHandler;
            cncnetLobby.UpdateCheck += CncnetLobby_UpdateCheck;
            isMediaPlayerAvailable = IsMediaPlayerAvailable();
        }

        private MainMenuDarkeningPanel innerPanel;

        private XNALabel lblCnCNetPlayerCount;
        private XNALinkLabel lblUpdateStatus;
        private XNALinkLabel lblVersion;
        private XNALinkLabel lblSite;
        private XNALinkLabel lblLanguage;
        private XNAExtraPanel versionIcon;
        private XNAExtraPanel updateIcon;
        private XNAExtraPanel webIcon;
        private XNAExtraPanel langIcon;

        private CnCNetLobby cncnetLobby;

        private SkirmishLobby skirmishLobby;

        private LANLobby lanLobby;

        private CnCNetManager connectionManager;

        private OptionsWindow optionsWindow;

        private DiscordHandler discordHandler;

        private TopBar topBar;

        private XNAMessageBox firstRunMessageBox;

        private bool _updateInProgress;
        private bool UpdateInProgress
        {
            get { return _updateInProgress; }
            set
            {
                _updateInProgress = value;
                topBar.SetSwitchButtonsClickable(!_updateInProgress);
                topBar.SetOptionsButtonClickable(!_updateInProgress);
                SetButtonHotkeys(!_updateInProgress);
            }
        }

        private bool customComponentDialogQueued = false;

        private DateTime lastUpdateCheckTime;

        private Song themeSong;

        private static readonly object locker = new object();

        private bool isMusicFading = false;

        private readonly bool isMediaPlayerAvailable;

        private bool isWindowIdle = true;

        private bool isGameExited = false;

        private float musicVolume = 1.0f;

        private CancellationTokenSource cncnetPlayerCountCancellationSource;

        // Main Menu Buttons
        private XNAClientButton btnNewCampaign;
        private XNAClientButton btnLoadGame;
        private XNAClientButton btnSkirmish;
        private XNAClientButton btnCnCNet;
        private XNAClientButton btnLan;
        private XNAClientButton btnOptions;
        private XNAClientButton btnMapEditor;
        private XNAClientButton btnStatistics;
        private XNAClientButton btnCredits;
        private XNAClientButton btnExtras;
        private XNAClientButton btnExit;

        /// <summary>
        /// Initializes the main menu's controls.
        /// </summary>
        public override void Initialize()
        {
            Width = UserINISettings.Instance.ClientResolutionX;
            Height = UserINISettings.Instance.ClientResolutionY;

            GameProcessLogic.GameProcessExited += SharedUILogic_GameProcessExited;
            LogbuchParser.ScoreSongStateChanged += StateScoreSongChanged;

            Name = nameof(MainMenu);
            BackgroundTexture = AssetLoader.LoadTexture("MainMenu/mainmenubg.png");
            ClientRectangle = new Rectangle(0, 0, Width, Height);
            WindowManager.CenterControlOnScreen(this);

            btnNewCampaign = new XNAClientButton(WindowManager);
            btnNewCampaign.Name = nameof(btnNewCampaign);
            btnNewCampaign.IdleTexture = AssetLoader.LoadTexture("MainMenu/campaign.png");
            btnNewCampaign.HoverTexture = AssetLoader.LoadTexture("MainMenu/campaign_c.png");
            btnNewCampaign.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnNewCampaign.LeftClick += BtnNewCampaign_LeftClick;

            btnLoadGame = new XNAClientButton(WindowManager);
            btnLoadGame.Name = nameof(btnLoadGame);
            btnLoadGame.IdleTexture = AssetLoader.LoadTexture("MainMenu/loadmission.png");
            btnLoadGame.HoverTexture = AssetLoader.LoadTexture("MainMenu/loadmission_c.png");
            btnLoadGame.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnLoadGame.LeftClick += BtnLoadGame_LeftClick;

            btnSkirmish = new XNAClientButton(WindowManager);
            btnSkirmish.Name = nameof(btnSkirmish);
            btnSkirmish.IdleTexture = AssetLoader.LoadTexture("MainMenu/skirmish.png");
            btnSkirmish.HoverTexture = AssetLoader.LoadTexture("MainMenu/skirmish_c.png");
            btnSkirmish.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnSkirmish.LeftClick += BtnSkirmish_LeftClick;

            btnCnCNet = new XNAClientButton(WindowManager);
            btnCnCNet.Name = nameof(btnCnCNet);
            btnCnCNet.IdleTexture = AssetLoader.LoadTexture("MainMenu/cncnet.png");
            btnCnCNet.HoverTexture = AssetLoader.LoadTexture("MainMenu/cncnet_c.png");
            btnCnCNet.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnCnCNet.LeftClick += BtnCnCNet_LeftClick;

            btnLan = new XNAClientButton(WindowManager);
            btnLan.Name = nameof(btnLan);
            btnLan.IdleTexture = AssetLoader.LoadTexture("MainMenu/lan.png");
            btnLan.HoverTexture = AssetLoader.LoadTexture("MainMenu/lan_c.png");
            btnLan.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnLan.LeftClick += BtnLan_LeftClick;

            btnOptions = new XNAClientButton(WindowManager);
            btnOptions.Name = nameof(btnOptions);
            btnOptions.IdleTexture = AssetLoader.LoadTexture("MainMenu/options.png");
            btnOptions.HoverTexture = AssetLoader.LoadTexture("MainMenu/options_c.png");
            btnOptions.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnOptions.LeftClick += BtnOptions_LeftClick;

            btnMapEditor = new XNAClientButton(WindowManager);
            btnMapEditor.Name = nameof(btnMapEditor);
            btnMapEditor.IdleTexture = AssetLoader.LoadTexture("MainMenu/mapeditor.png");
            btnMapEditor.HoverTexture = AssetLoader.LoadTexture("MainMenu/mapeditor_c.png");
            btnMapEditor.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnMapEditor.LeftClick += BtnMapEditor_LeftClick;

            btnStatistics = new XNAClientButton(WindowManager);
            btnStatistics.Name = nameof(btnStatistics);
            btnStatistics.IdleTexture = AssetLoader.LoadTexture("MainMenu/statistics.png");
            btnStatistics.HoverTexture = AssetLoader.LoadTexture("MainMenu/statistics_c.png");
            btnStatistics.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnStatistics.LeftClick += BtnStatistics_LeftClick;

            btnCredits = new XNAClientButton(WindowManager);
            btnCredits.Name = nameof(btnCredits);
            btnCredits.IdleTexture = AssetLoader.LoadTexture("MainMenu/credits.png");
            btnCredits.HoverTexture = AssetLoader.LoadTexture("MainMenu/credits_c.png");
            btnCredits.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnCredits.LeftClick += BtnCredits_LeftClick;

            btnExtras = new XNAClientButton(WindowManager);
            btnExtras.Name = nameof(btnExtras);
            btnExtras.IdleTexture = AssetLoader.LoadTexture("MainMenu/extras.png");
            btnExtras.HoverTexture = AssetLoader.LoadTexture("MainMenu/extras_c.png");
            btnExtras.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnExtras.LeftClick += BtnExtras_LeftClick;

            btnExit = new XNAClientButton(WindowManager);
            btnExit.Name = nameof(btnExit);
            btnExit.IdleTexture = AssetLoader.LoadTexture("MainMenu/exitgame.png");
            btnExit.HoverTexture = AssetLoader.LoadTexture("MainMenu/exitgame_c.png");
            btnExit.HoverSoundEffect = new EnhancedSoundEffect("MainMenu/button.wav");
            btnExit.LeftClick += BtnExit_LeftClick;

            /*XNALabel lblCnCNetStatus = new XNALabel(WindowManager);
            lblCnCNetStatus.Name = nameof(lblCnCNetStatus);
            lblCnCNetStatus.Text = "DTA players on CnCNet:".L10N("UI:Main:CnCNetOnlinePlayersCountText");
            lblCnCNetStatus.ClientRectangle = new Rectangle(12, 9, 0, 0);*/

            lblCnCNetPlayerCount = new XNALabel(WindowManager);
            lblCnCNetPlayerCount.Name = nameof(lblCnCNetPlayerCount);
            lblCnCNetPlayerCount.Text = "-";
            lblCnCNetPlayerCount.Visible = false;
            lblCnCNetPlayerCount.Disable();

            Color idleColor = new Color(190, 199, 207, 0.8f);
            Color hoverColor = new Color(255, 255, 255, 1.0f);

            lblVersion = new XNALinkLabel(WindowManager);
            lblVersion.Name = nameof(lblVersion);
            lblVersion.LeftClick += LblVersion_LeftClick;
            lblVersion.IdleColor = idleColor;
            lblVersion.HoverColor = hoverColor;
            lblVersion.Tag = 0;

            lblUpdateStatus = new XNALinkLabel(WindowManager);
            lblUpdateStatus.Name = nameof(lblUpdateStatus);
            lblUpdateStatus.LeftClick += LblUpdateStatus_LeftClick;
            lblUpdateStatus.IdleColor = idleColor;
            lblUpdateStatus.HoverColor = hoverColor;
            lblUpdateStatus.Tag = 0;

            lblSite = new XNALinkLabel(WindowManager);
            lblSite.Name = nameof(lblSite);
            lblSite.LeftClick += lblSite_LeftClick;
            lblSite.Text = "Official Website".L10N("UI:Menu:Website");
            lblSite.IdleColor = idleColor;
            lblSite.HoverColor = hoverColor;
            lblSite.Tag = 0;

            lblLanguage = new XNALinkLabel(WindowManager);
            lblLanguage.Name = nameof(lblLanguage);
            lblLanguage.LeftClick += lblLanguage_LeftClick;
            lblLanguage.Text = "Menu Language: < English >".L10N("UI:Menu:English");
            lblLanguage.IdleColor = idleColor;
            lblLanguage.HoverColor = hoverColor;
            lblLanguage.Tag = 0;

            versionIcon = new XNAExtraPanel(WindowManager);
            versionIcon.Name = nameof(versionIcon);
            versionIcon.BackgroundTexture = AssetLoader.LoadTexture("MainMenu/versionicon.png");
            versionIcon.DrawBorders = false;
            versionIcon.Tag = 0;

            updateIcon = new XNAExtraPanel(WindowManager);
            updateIcon.Name = nameof(updateIcon);
            updateIcon.BackgroundTexture = AssetLoader.LoadTexture("MainMenu/updateicon.png");
            updateIcon.DrawBorders = false;
            updateIcon.Tag = 0;

            webIcon = new XNAExtraPanel(WindowManager);
            webIcon.Name = nameof(webIcon);
            webIcon.BackgroundTexture = AssetLoader.LoadTexture("MainMenu/webicon.png");
            webIcon.DrawBorders = false;
            webIcon.Tag = 0;

            langIcon = new XNAExtraPanel(WindowManager);
            langIcon.Name = nameof(langIcon);
            langIcon.BackgroundTexture = AssetLoader.LoadTexture("MainMenu/langicon.png");
            langIcon.DrawBorders = false;
            langIcon.Tag = 0;

            AddChild(btnNewCampaign);
            AddChild(btnLoadGame);
            AddChild(btnSkirmish);
            AddChild(btnCnCNet);
            AddChild(btnLan);
            AddChild(btnOptions);
            AddChild(btnMapEditor);
            AddChild(btnStatistics);
            AddChild(btnCredits);
            AddChild(btnExtras);
            AddChild(btnExit);

            AddChild(lblVersion);
            AddChild(lblUpdateStatus);
            AddChild(lblSite);
            AddChild(lblLanguage);

            AddChild(versionIcon);
            AddChild(updateIcon);
            AddChild(webIcon);
            AddChild(langIcon);

            CUpdater.FileIdentifiersUpdated += CUpdater_FileIdentifiersUpdated;
            CUpdater.OnCustomComponentsOutdated += CUpdater_OnCustomComponentsOutdated;

            base.Initialize(); // Read control attributes from INI

            innerPanel = new MainMenuDarkeningPanel(WindowManager, discordHandler);
            innerPanel.ClientRectangle = new Rectangle(0, 0,
                Width,
                Height);
            innerPanel.DrawOrder = int.MaxValue;
            innerPanel.UpdateOrder = int.MaxValue;
            AddChild(innerPanel);
            innerPanel.Hide();

            lblVersion.Text = CUpdater.GameVersion;

            innerPanel.UpdateQueryWindow.UpdateDeclined += UpdateQueryWindow_UpdateDeclined;
            innerPanel.UpdateQueryWindow.UpdateAccepted += UpdateQueryWindow_UpdateAccepted;

            innerPanel.UpdateWindow.UpdateCompleted += UpdateWindow_UpdateCompleted;
            innerPanel.UpdateWindow.UpdateCancelled += UpdateWindow_UpdateCancelled;
            innerPanel.UpdateWindow.UpdateFailed += UpdateWindow_UpdateFailed;

            this.ClientRectangle = new Rectangle((WindowManager.RenderResolutionX - Width) / 2,
                (WindowManager.RenderResolutionY - Height) / 2,
                Width, Height);
            innerPanel.ClientRectangle = new Rectangle(0, 0,
                Math.Max(WindowManager.RenderResolutionX, Width),
                Math.Max(WindowManager.RenderResolutionY, Height));

            CnCNetPlayerCountTask.CnCNetGameCountUpdated += CnCNetInfoController_CnCNetGameCountUpdated;
            cncnetPlayerCountCancellationSource = new CancellationTokenSource();
            CnCNetPlayerCountTask.InitializeService(cncnetPlayerCountCancellationSource);

            WindowManager.GameClosing += WindowManager_GameClosing;

            skirmishLobby.Exited += SkirmishLobby_Exited;
            lanLobby.Exited += LanLobby_Exited;
            optionsWindow.EnabledChanged += OptionsWindow_EnabledChanged;

            optionsWindow.OnForceUpdate += (s, e) => ForceUpdate();

            GameProcessLogic.GameProcessStarted += SharedUILogic_GameProcessStarted;
            GameProcessLogic.GameProcessStarting += SharedUILogic_GameProcessStarting;

            UserINISettings.Instance.SettingsSaved += SettingsSaved;

            CUpdater.Restart += CUpdater_Restart;

            SetButtonHotkeys(true);

            if (!UserINISettings.Instance.MenuTextIsEnglish)
                ChangeToChineseBtn();
        }

        private void ChangeToChineseBtn()
        {
            Texture2D texture;
            btnNewCampaign.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/campaign.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/campaign_c.png");
            btnNewCampaign.HoverTexture = texture;
            btnNewCampaign.ClientRectangle = new Rectangle(btnNewCampaign.LocationCHS.X, btnNewCampaign.LocationCHS.Y, texture.Width, texture.Height);
            btnNewCampaign.textureEndX = btnNewCampaign.Width - 4;
            btnNewCampaign.textureEndY = btnNewCampaign.Height - 4;

            btnLoadGame.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/loadmission.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/loadmission_c.png");
            btnLoadGame.HoverTexture = texture;
            btnLoadGame.ClientRectangle = new Rectangle(btnLoadGame.LocationCHS.X, btnLoadGame.LocationCHS.Y, texture.Width, texture.Height);
            btnLoadGame.textureEndX = btnLoadGame.Width - 4;
            btnLoadGame.textureEndY = btnLoadGame.Height - 4;

            btnSkirmish.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/skirmish.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/skirmish_c.png");
            btnSkirmish.HoverTexture = texture;
            btnSkirmish.ClientRectangle = new Rectangle(btnSkirmish.LocationCHS.X, btnSkirmish.LocationCHS.Y, texture.Width, texture.Height);
            btnSkirmish.textureEndX = btnSkirmish.Width - 4;
            btnSkirmish.textureEndY = btnSkirmish.Height - 4;

            btnCnCNet.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/cncnet.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/cncnet_c.png");
            btnCnCNet.HoverTexture = texture;
            btnCnCNet.ClientRectangle = new Rectangle(btnCnCNet.LocationCHS.X, btnCnCNet.LocationCHS.Y, texture.Width, texture.Height);
            btnCnCNet.textureEndX = btnCnCNet.Width - 4;
            btnCnCNet.textureEndY = btnCnCNet.Height - 4;

            btnLan.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/lan.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/lan_c.png");
            btnLan.HoverTexture = texture;
            btnLan.ClientRectangle = new Rectangle(btnLan.LocationCHS.X, btnLan.LocationCHS.Y, texture.Width, texture.Height);
            btnLan.textureEndX = btnLan.Width - 4;
            btnLan.textureEndY = btnLan.Height - 4;

            btnOptions.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/options.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/options_c.png");
            btnOptions.HoverTexture = texture;
            btnOptions.ClientRectangle = new Rectangle(btnOptions.LocationCHS.X, btnOptions.LocationCHS.Y, texture.Width, texture.Height);
            btnOptions.textureEndX = btnOptions.Width - 4;
            btnOptions.textureEndY = btnOptions.Height - 4;

            btnMapEditor.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/mapeditor.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/mapeditor_c.png");
            btnMapEditor.HoverTexture = texture;
            btnMapEditor.ClientRectangle = new Rectangle(btnMapEditor.LocationCHS.X, btnMapEditor.LocationCHS.Y, texture.Width, texture.Height);
            btnMapEditor.textureEndX = btnMapEditor.Width - 4;
            btnMapEditor.textureEndY = btnMapEditor.Height - 4;

            btnStatistics.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/statistics.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/statistics_c.png");
            btnStatistics.HoverTexture = texture;
            btnStatistics.ClientRectangle = new Rectangle(btnStatistics.LocationCHS.X, btnStatistics.LocationCHS.Y, texture.Width, texture.Height);
            btnStatistics.textureEndX = btnStatistics.Width - 4;
            btnStatistics.textureEndY = btnStatistics.Height - 4;

            btnCredits.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/credits.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/credits_c.png");
            btnCredits.HoverTexture = texture;
            btnCredits.ClientRectangle = new Rectangle(btnCredits.LocationCHS.X, btnCredits.LocationCHS.Y, texture.Width, texture.Height);
            btnCredits.textureEndX = btnCredits.Width - 4;
            btnCredits.textureEndY = btnCredits.Height - 4;

            btnExtras.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/extras.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/extras_c.png");
            btnExtras.HoverTexture = texture;
            btnExtras.ClientRectangle = new Rectangle(btnExtras.LocationCHS.X, btnExtras.LocationCHS.Y, texture.Width, texture.Height);
            btnExtras.textureEndX = btnExtras.Width - 4;
            btnExtras.textureEndY = btnExtras.Height - 4;

            btnExit.IdleTexture = AssetLoader.LoadTexture("MainMenu/CHS/exitgame.png");
            texture = AssetLoader.LoadTexture("MainMenu/CHS/exitgame_c.png");
            btnExit.HoverTexture = texture;
            btnExit.ClientRectangle = new Rectangle(btnExit.LocationCHS.X, btnExit.LocationCHS.Y, texture.Width, texture.Height);
            btnExit.textureEndX = btnExit.Width - 4;
            btnExit.textureEndY = btnExit.Height - 4;

            btnNewCampaign._toolTip.Disable();
            btnLoadGame._toolTip.Disable();
            btnSkirmish._toolTip.Disable();
            btnCnCNet._toolTip.Disable();
            btnLan._toolTip.Disable();
            btnOptions._toolTip.Disable();
            btnMapEditor._toolTip.Disable();
            btnStatistics._toolTip.Disable();
            btnCredits._toolTip.Disable();
            btnExtras._toolTip.Disable();
            btnExit._toolTip.Disable();
            lblLanguage.Text = "Menu Language: < Chinese >".L10N("UI:Menu:Chinese");
        }

        private void ChangeToEnglishBtn()
        {
            Texture2D texture;
            btnNewCampaign.IdleTexture = AssetLoader.LoadTexture("MainMenu/campaign.png");
            texture = AssetLoader.LoadTexture("MainMenu/campaign_c.png");
            btnNewCampaign.HoverTexture = texture;
            btnNewCampaign.ClientRectangle = new Rectangle(btnNewCampaign.LocationENG.X, btnNewCampaign.LocationENG.Y, texture.Width, texture.Height);
            btnNewCampaign.textureEndX = btnNewCampaign.Width - btnNewCampaign.borderWidth;
            btnNewCampaign.textureEndY = btnNewCampaign.Height - btnNewCampaign.borderHeight;

            btnLoadGame.IdleTexture = AssetLoader.LoadTexture("MainMenu/loadmission.png");
            texture = AssetLoader.LoadTexture("MainMenu/loadmission_c.png");
            btnLoadGame.HoverTexture = texture;
            btnLoadGame.ClientRectangle = new Rectangle(btnLoadGame.LocationENG.X, btnLoadGame.LocationENG.Y, texture.Width, texture.Height);
            btnLoadGame.textureEndX = btnLoadGame.Width - btnLoadGame.borderWidth;
            btnLoadGame.textureEndY = btnLoadGame.Height - btnLoadGame.borderHeight;

            btnSkirmish.IdleTexture = AssetLoader.LoadTexture("MainMenu/skirmish.png");
            texture = AssetLoader.LoadTexture("MainMenu/skirmish_c.png");
            btnSkirmish.HoverTexture = texture;
            btnSkirmish.ClientRectangle = new Rectangle(btnSkirmish.LocationENG.X, btnSkirmish.LocationENG.Y, texture.Width, texture.Height);
            btnSkirmish.textureEndX = btnSkirmish.Width - btnSkirmish.borderWidth;
            btnSkirmish.textureEndY = btnSkirmish.Height - btnSkirmish.borderHeight;

            btnCnCNet.IdleTexture = AssetLoader.LoadTexture("MainMenu/cncnet.png");
            texture = AssetLoader.LoadTexture("MainMenu/cncnet_c.png");
            btnCnCNet.HoverTexture = texture;
            btnCnCNet.ClientRectangle = new Rectangle(btnCnCNet.LocationENG.X, btnCnCNet.LocationENG.Y, texture.Width, texture.Height);
            btnCnCNet.textureEndX = btnCnCNet.Width - btnCnCNet.borderWidth;
            btnCnCNet.textureEndY = btnCnCNet.Height - btnCnCNet.borderHeight;

            btnLan.IdleTexture = AssetLoader.LoadTexture("MainMenu/lan.png");
            texture = AssetLoader.LoadTexture("MainMenu/lan_c.png");
            btnLan.HoverTexture = texture;
            btnLan.ClientRectangle = new Rectangle(btnLan.LocationENG.X, btnLan.LocationENG.Y, texture.Width, texture.Height);
            btnLan.textureEndX = btnLan.Width - btnLan.borderWidth;
            btnLan.textureEndY = btnLan.Height - btnLan.borderHeight;

            btnOptions.IdleTexture = AssetLoader.LoadTexture("MainMenu/options.png");
            texture = AssetLoader.LoadTexture("MainMenu/options_c.png");
            btnOptions.HoverTexture = texture;
            btnOptions.ClientRectangle = new Rectangle(btnOptions.LocationENG.X, btnOptions.LocationENG.Y, texture.Width, texture.Height);
            btnOptions.textureEndX = btnOptions.Width - btnOptions.borderWidth;
            btnOptions.textureEndY = btnOptions.Height - btnOptions.borderHeight;

            btnMapEditor.IdleTexture = AssetLoader.LoadTexture("MainMenu/mapeditor.png");
            texture = AssetLoader.LoadTexture("MainMenu/mapeditor_c.png");
            btnMapEditor.HoverTexture = texture;
            btnMapEditor.ClientRectangle = new Rectangle(btnMapEditor.LocationENG.X, btnMapEditor.LocationENG.Y, texture.Width, texture.Height);
            btnMapEditor.textureEndX = btnMapEditor.Width - btnMapEditor.borderWidth;
            btnMapEditor.textureEndY = btnMapEditor.Height - btnMapEditor.borderHeight;

            btnStatistics.IdleTexture = AssetLoader.LoadTexture("MainMenu/statistics.png");
            texture = AssetLoader.LoadTexture("MainMenu/statistics_c.png");
            btnStatistics.HoverTexture = texture;
            btnStatistics.ClientRectangle = new Rectangle(btnStatistics.LocationENG.X, btnStatistics.LocationENG.Y, texture.Width, texture.Height);
            btnStatistics.textureEndX = btnStatistics.Width - btnStatistics.borderWidth;
            btnStatistics.textureEndY = btnStatistics.Height - btnStatistics.borderHeight;

            btnCredits.IdleTexture = AssetLoader.LoadTexture("MainMenu/credits.png");
            texture = AssetLoader.LoadTexture("MainMenu/credits_c.png");
            btnCredits.HoverTexture = texture;
            btnCredits.ClientRectangle = new Rectangle(btnCredits.LocationENG.X, btnCredits.LocationENG.Y, texture.Width, texture.Height);
            btnCredits.textureEndX = btnCredits.Width - btnCredits.borderWidth;
            btnCredits.textureEndY = btnCredits.Height - btnCredits.borderHeight;

            btnExtras.IdleTexture = AssetLoader.LoadTexture("MainMenu/extras.png");
            texture = AssetLoader.LoadTexture("MainMenu/extras_c.png");
            btnExtras.HoverTexture = texture;
            btnExtras.ClientRectangle = new Rectangle(btnExtras.LocationENG.X, btnExtras.LocationENG.Y, texture.Width, texture.Height);
            btnExtras.textureEndX = btnExtras.Width - btnExtras.borderWidth;
            btnExtras.textureEndY = btnExtras.Height - btnExtras.borderHeight;

            btnExit.IdleTexture = AssetLoader.LoadTexture("MainMenu/exitgame.png");
            texture = AssetLoader.LoadTexture("MainMenu/exitgame_c.png");
            btnExit.HoverTexture = texture;
            btnExit.ClientRectangle = new Rectangle(btnExit.LocationENG.X, btnExit.LocationENG.Y, texture.Width, texture.Height);
            btnExit.textureEndX = btnExit.Width - btnExit.borderWidth;
            btnExit.textureEndY = btnExit.Height - btnExit.borderHeight;

            btnNewCampaign._toolTip.Enable();
            btnLoadGame._toolTip.Enable();
            btnSkirmish._toolTip.Enable();
            btnCnCNet._toolTip.Enable();
            btnLan._toolTip.Enable();
            btnOptions._toolTip.Enable();
            btnMapEditor._toolTip.Enable();
            btnStatistics._toolTip.Enable();
            btnCredits._toolTip.Enable();
            btnExtras._toolTip.Enable();
            btnExit._toolTip.Enable();
            lblLanguage.Text = "Menu Language: < English >".L10N("UI:Menu:English");
        }

        private void lblLanguage_LeftClick(object sender, EventArgs e)
        {
            if (UserINISettings.Instance.MenuTextIsEnglish) // english -> chinese
            {
                ChangeToChineseBtn();
                UserINISettings.Instance.ReloadSettings();
                UserINISettings.Instance.MenuTextIsEnglish.Value = false;
                UserINISettings.Instance.SaveSettings();
            }
            else // chinese -> english
            {
                ChangeToEnglishBtn();
                UserINISettings.Instance.ReloadSettings();
                UserINISettings.Instance.MenuTextIsEnglish.Value = true;
                UserINISettings.Instance.SaveSettings();
            }
        }

        private void SetButtonHotkeys(bool enableHotkeys)
        {
            if (!Initialized)
                return;

            if (enableHotkeys)
            {
                btnNewCampaign.HotKey = Keys.C;
                btnLoadGame.HotKey = Keys.L;
                btnSkirmish.HotKey = Keys.S;
                btnCnCNet.HotKey = Keys.M;
                btnLan.HotKey = Keys.N;
                btnOptions.HotKey = Keys.O;
                btnMapEditor.HotKey = Keys.E;
                btnStatistics.HotKey = Keys.T;
                btnCredits.HotKey = Keys.R;
                btnExtras.HotKey = Keys.X;
            }
            else
            {
                btnNewCampaign.HotKey = Keys.None;
                btnLoadGame.HotKey = Keys.None;
                btnSkirmish.HotKey = Keys.None;
                btnCnCNet.HotKey = Keys.None;
                btnLan.HotKey = Keys.None;
                btnOptions.HotKey = Keys.None;
                btnMapEditor.HotKey = Keys.None;
                btnStatistics.HotKey = Keys.None;
                btnCredits.HotKey = Keys.None;
                btnExtras.HotKey = Keys.None;
            }
        }

        private void OptionsWindow_EnabledChanged(object sender, EventArgs e)
        {
            if (!optionsWindow.Enabled)
            {
                if (customComponentDialogQueued)
                    CUpdater_OnCustomComponentsOutdated();
            }
        }

        /// <summary>
        /// Refreshes settings. Called when the game process is starting.
        /// </summary>
        private void SharedUILogic_GameProcessStarting()
        {
            UserINISettings.Instance.ReloadSettings();

            try
            {
                optionsWindow.RefreshSettings();
            }
            catch (Exception ex)
            {
                Logger.Log("Refreshing settings failed! Exception message: " + ex.Message);
                // We don't want to show the dialog when starting a game
                //XNAMessageBox.Show(WindowManager, "Saving settings failed",
                //    "Saving settings failed! Error message: " + ex.Message);
            }
        }

        private void CUpdater_Restart(object sender, EventArgs e) =>
            WindowManager.AddCallback(new Action(ExitClient), null);

        /// <summary>
        /// Applies configuration changes (music playback and volume)
        /// when settings are saved.
        /// </summary>
        private void SettingsSaved(object sender, EventArgs e)
        {
            musicVolume = (float)UserINISettings.Instance.ClientVolume;

            if (isMediaPlayerAvailable)
            {
                if (MediaPlayer.State == MediaState.Playing)
                {
                    if (!UserINISettings.Instance.PlayMainMenuMusic)
                        isMusicFading = true;
                }
                else if (LogbuchParser.SongEnded && topBar.GetTopMostPrimarySwitchable() == this &&
                    topBar.LastSwitchType == SwitchType.PRIMARY)
                {
                    PlayMusic();
                }
            }

            if (!connectionManager.IsConnected)
                ProgramConstants.PLAYERNAME = UserINISettings.Instance.PlayerName;

            if (UserINISettings.Instance.DiscordIntegration)
                discordHandler?.Connect();
            else
                discordHandler?.Disconnect();

            isWindowIdle = true;
        }

        /// <summary>
        /// Checks files which are required for the mod to function
        /// but not distributed with the mod (usually base game files
        /// for YR mods which can't be standalone).
        /// </summary>
        private void CheckRequiredFiles()
        {
            List<string> absentFiles = ClientConfiguration.Instance.RequiredFiles.ToList()
                .FindAll(f => !string.IsNullOrWhiteSpace(f) && !File.Exists(ProgramConstants.GamePath + f));

            if (absentFiles.Count > 0)
                XNAMessageBox.Show(WindowManager, "Missing Files".L10N("UI:Main:MissingFilesTitle"),
#if ARES
                    ("You are missing Yuri's Revenge files that are required" + Environment.NewLine +
                    "to play this mod! Yuri's Revenge mods are not standalone," + Environment.NewLine +
                    "so you need a copy of following Yuri's Revenge (v. 1.001)" + Environment.NewLine +
                    "files placed in the mod folder to play the mod:").L10N("UI:Main:MissingFilesText1Ares") +
#else
                    "The following required files are missing:".L10N("UI:Main:MissingFilesText1NonAres") +
#endif
                    Environment.NewLine + Environment.NewLine +
                    String.Join(Environment.NewLine, absentFiles) +
                    Environment.NewLine + Environment.NewLine +
                    "You won't be able to play without those files.".L10N("UI:Main:MissingFilesText2"));
        }

        private void CheckForbiddenFiles()
        {
            List<string> presentFiles = ClientConfiguration.Instance.ForbiddenFiles.ToList()
                .FindAll(f => !string.IsNullOrWhiteSpace(f) && File.Exists(ProgramConstants.GamePath + f));

            if (presentFiles.Count > 0)
                XNAMessageBox.Show(WindowManager, "Interfering Files Detected".L10N("UI:Main:InterferingFilesDetectedTitle"),
#if TS
                    ("You have installed the mod on top of a Tiberian Sun" + Environment.NewLine +
                    "copy! This mod is standalone, therefore you have to" + Environment.NewLine +
                    "install it in an empty folder. Otherwise the mod won't" + Environment.NewLine +
                    "function correctly." +
                    Environment.NewLine + Environment.NewLine +
                    "Please reinstall the mod into an empty folder to play.").L10N("UI:Main:InterferingFilesDetectedTextTS")
#else
                    "The following interfering files are present:".L10N("UI:Main:InterferingFilesDetectedTextNonTS1") +
                    Environment.NewLine + Environment.NewLine +
                    String.Join(Environment.NewLine, presentFiles) +
                    Environment.NewLine + Environment.NewLine +
                    "The mod won't work correctly without those files removed.".L10N("UI:Main:InterferingFilesDetectedTextNonTS2")
#endif
                    );
        }

        /// <summary>
        /// Checks whether the client is running for the first time.
        /// If it is, displays a dialog asking the user if they'd like
        /// to configure settings.
        /// </summary>
        private void CheckIfFirstRun()
        {
            if (UserINISettings.Instance.IsFirstRun)
            {
                UserINISettings.Instance.IsFirstRun.Value = false;
                UserINISettings.Instance.WindowedMode.Value = true;
                UserINISettings.Instance.BorderlessWindowedMode.Value = true;
                UserINISettings.Instance.SaveSettings();

                firstRunMessageBox = XNAMessageBox.ShowYesNoDialog(WindowManager, "Initial Installation".L10N("UI:Main:InitialInstallationTitle"),
                    string.Format(("You have just installed {0}." + Environment.NewLine +
                    "It's highly recommended that you configure your settings before playing." +
                    Environment.NewLine + "Do you want to configure them now?").L10N("UI:Main:InitialInstallationText"), ClientConfiguration.Instance.LocalGame));
                firstRunMessageBox.YesClickedAction = FirstRunMessageBox_YesClicked;
                firstRunMessageBox.NoClickedAction = FirstRunMessageBox_NoClicked;
            }

            optionsWindow.PostInit();
        }

        private void FirstRunMessageBox_NoClicked(XNAMessageBox messageBox)
        {
            if (customComponentDialogQueued)
                CUpdater_OnCustomComponentsOutdated();
        }

        private void FirstRunMessageBox_YesClicked(XNAMessageBox messageBox) => optionsWindow.Open(UserINISettings.Instance.IsFirstRun);

        private void SharedUILogic_GameProcessStarted()
        {
            isWindowIdle = false;
            isGameExited = false;
            MusicOff();
        }

        private void WindowManager_GameClosing(object sender, EventArgs e) => Clean();

        private void SkirmishLobby_Exited(object sender, EventArgs e)
        {
            if (LogbuchParser.SongEnded && UserINISettings.Instance.StopMusicOnMenu)
                PlayMusic();

            isWindowIdle = true;
        }

        private void LanLobby_Exited(object sender, EventArgs e)
        {
            topBar.SetLanMode(false);

            if (UserINISettings.Instance.AutomaticCnCNetLogin)
                connectionManager.Connect();

            if (LogbuchParser.SongEnded && UserINISettings.Instance.StopMusicOnMenu)
                PlayMusic();

            isWindowIdle = true;
        }

        private void CnCNetInfoController_CnCNetGameCountUpdated(object sender, PlayerCountEventArgs e)
        {
            lock (locker)
            {
                if (e.PlayerCount == -1)
                    lblCnCNetPlayerCount.Text = "N/A".L10N("UI:Main:N/A");
                else
                    lblCnCNetPlayerCount.Text = e.PlayerCount.ToString();
            }
        }

        /// <summary>
        /// Attemps to "clean" the client session in a nice way if the user closes the game.
        /// </summary>
        private void Clean()
        {
            CUpdater.FileIdentifiersUpdated -= CUpdater_FileIdentifiersUpdated;

            if (cncnetPlayerCountCancellationSource != null) cncnetPlayerCountCancellationSource.Cancel();
            topBar.Clean();
            if (UpdateInProgress)
                CUpdater.TerminateUpdate = true;

            if (connectionManager.IsConnected)
                connectionManager.Disconnect();
        }

        /// <summary>
        /// Starts playing music, initiates an update check if automatic updates
        /// are enabled and checks whether the client is run for the first time.
        /// Called after all internal client UI logic has been initialized.
        /// </summary>
        public void PostInit()
        {
            if (LogbuchParser.SongEnded)
            {
                themeSong = AssetLoader.LoadSong(ClientConfiguration.Instance.MainMenuMusicName);
                PlayMusic();
            }

            isWindowIdle = true;

            if (UserINISettings.Instance.CheckForUpdates)
                CheckForUpdates();
            else
                lblUpdateStatus.Text = "Click to check for updates.".L10N("UI:Main:ClickToCheckUpdate");

            CheckRequiredFiles();
            CheckForbiddenFiles();
            CheckIfFirstRun();
        }

        #region Updating / versioning system

        private void UpdateWindow_UpdateFailed(object sender, UpdateFailureEventArgs e)
        {
            innerPanel.Hide();
            lblUpdateStatus.Text = "Updating failed! Click to retry.".L10N("UI:Main:UpdateFailedClickToRetry");
            lblUpdateStatus.DrawUnderline = true;
            lblUpdateStatus.Enabled = true;
            UpdateInProgress = false;

            innerPanel.Show(null); // Darkening
            XNAMessageBox msgBox = new XNAMessageBox(WindowManager, "Update failed".L10N("UI:Main:UpdateFailedTitle"),
                string.Format(("An error occured while updating. Returned error was: {0}" +
                Environment.NewLine + Environment.NewLine +
                "If you are connected to the Internet and your firewall isn't blocking" + Environment.NewLine +
                "{1}, and the issue is reproducible, contact us at " + Environment.NewLine +
                "{2} for support.").L10N("UI:Main:UpdateFailedText"),
                e.Reason, CUpdater.CURRENT_LAUNCHER_NAME, MainClientConstants.SUPPORT_URL_SHORT), XNAMessageBoxButtons.OK);
            msgBox.OKClickedAction = MsgBox_OKClicked;
            msgBox.Show();
        }

        private void MsgBox_OKClicked(XNAMessageBox messageBox)
        {
            innerPanel.Hide();
        }

        private void UpdateWindow_UpdateCancelled(object sender, EventArgs e)
        {
            innerPanel.Hide();
            lblUpdateStatus.Text = "The update was cancelled. Click to retry.".L10N("UI:Main:UpdateCancelledClickToRetry");
            lblUpdateStatus.DrawUnderline = true;
            lblUpdateStatus.Enabled = true;
            UpdateInProgress = false;
        }

        private void UpdateWindow_UpdateCompleted(object sender, EventArgs e)
        {
            innerPanel.Hide();
            lblUpdateStatus.Text = string.Format("{0} was succesfully updated to v.{1}".L10N("UI:Main:UpdateSuccess"),
                MainClientConstants.GAME_NAME_SHORT, CUpdater.GameVersion);
            lblVersion.Text = CUpdater.GameVersion;
            UpdateInProgress = false;
            lblUpdateStatus.Enabled = true;
            lblUpdateStatus.DrawUnderline = false;
        }

        private void LblUpdateStatus_LeftClick(object sender, EventArgs e)
        {
            Logger.Log(CUpdater.DTAVersionState.ToString());

            if (CUpdater.DTAVersionState == VersionState.OUTDATED ||
                CUpdater.DTAVersionState == VersionState.MISMATCHED ||
                CUpdater.DTAVersionState == VersionState.UNKNOWN ||
                CUpdater.DTAVersionState == VersionState.UPTODATE)
            {
                CheckForUpdates();
            }
        }

        private void LblVersion_LeftClick(object sender, EventArgs e)
        {
            Process.Start(ClientConfiguration.Instance.ChangelogURL);
        }

        private void lblSite_LeftClick(object sender, EventArgs e)
        {
            Process.Start(ClientConfiguration.Instance.LongSupportURL);
        }

        private void ForceUpdate()
        {
            UpdateInProgress = true;
            innerPanel.Hide();
            innerPanel.UpdateWindow.ForceUpdate();
            innerPanel.Show(innerPanel.UpdateWindow);
            lblUpdateStatus.Text = "Force updating...".L10N("UI:Main:ForceUpdating");
        }

        /// <summary>
        /// Starts a check for updates.
        /// </summary>
        private void CheckForUpdates()
        {
            CUpdater.CheckForUpdates();
            lblUpdateStatus.Enabled = false;
            lblUpdateStatus.Text = "Checking for updates...".L10N("UI:Main:CheckingForUpdate");
            try
            {
                StatisticsSender.Instance.SendUpdate();
            }
            catch { }
            lastUpdateCheckTime = DateTime.Now;
        }

        private void CUpdater_FileIdentifiersUpdated()
        {
            WindowManager.AddCallback(new Action(HandleFileIdentifierUpdate), null);
        }

        /// <summary>
        /// Used for displaying the result of an update check in the UI.
        /// </summary>
        private void HandleFileIdentifierUpdate()
        {
            if (UpdateInProgress)
            {
                return;
            }

            if (CUpdater.DTAVersionState == VersionState.UPTODATE)
            {
                lblUpdateStatus.Text = string.Format("{0} is up to date.".L10N("UI:Main:GameUpToDate"), MainClientConstants.GAME_NAME_SHORT);
                lblUpdateStatus.Enabled = true;
                lblUpdateStatus.DrawUnderline = false;
            }
            else if (CUpdater.DTAVersionState == VersionState.OUTDATED)
            {
                lblUpdateStatus.Text = "An update is available.".L10N("UI:Main:UpdateAvailable");
                innerPanel.UpdateQueryWindow.SetInfo(CUpdater.ServerGameVersion, CUpdater.UpdateSizeInKb);
                innerPanel.Show(innerPanel.UpdateQueryWindow);
            }
            else if (CUpdater.DTAVersionState == VersionState.UNKNOWN)
            {
                lblUpdateStatus.Text = "Checking for updates failed! Click to retry.".L10N("UI:Main:CheckUpdateFailedClickToRetry");
                lblUpdateStatus.Enabled = true;
                lblUpdateStatus.DrawUnderline = true;
            }
        }

        /// <summary>
        /// Asks the user if they'd like to update their custom components.
        /// Handles an event raised by the updater when it has detected
        /// that the custom components are out of date.
        /// </summary>
        private void CUpdater_OnCustomComponentsOutdated()
        {
            if (innerPanel.UpdateQueryWindow.Visible)
                return;

            if (UpdateInProgress)
                return;

            if ((firstRunMessageBox != null && firstRunMessageBox.Visible) || optionsWindow.Enabled)
            {
                // If the custom components are out of date on the first run
                // or the options window is already open, don't show the dialog
                customComponentDialogQueued = true;
                return;
            }

            customComponentDialogQueued = false;

            XNAMessageBox ccMsgBox = XNAMessageBox.ShowYesNoDialog(WindowManager,
                "Custom Component Updates Available".L10N("UI:Main:CustomUpdateAvailableTitle"),
                ("Updates for custom components are available. Do you want to open" + Environment.NewLine +
                "the Options menu where you can update the custom components?").L10N("UI:Main:CustomUpdateAvailableText"));
            ccMsgBox.YesClickedAction = CCMsgBox_YesClicked;
        }

        private void CCMsgBox_YesClicked(XNAMessageBox messageBox)
        {
            optionsWindow.Open();
            optionsWindow.SwitchToCustomComponentsPanel();
        }

        /// <summary>
        /// Called when the user has declined an update.
        /// </summary>
        private void UpdateQueryWindow_UpdateDeclined(object sender, EventArgs e)
        {
            UpdateQueryWindow uqw = (UpdateQueryWindow)sender;
            innerPanel.Hide();
            lblUpdateStatus.Text = "An update is available, click to install.".L10N("UI:Main:UpdateAvailableClickToInstall");
            lblUpdateStatus.Enabled = true;
            lblUpdateStatus.DrawUnderline = true;
        }

        /// <summary>
        /// Called when the user has accepted an update.
        /// </summary>
        private void UpdateQueryWindow_UpdateAccepted(object sender, EventArgs e)
        {
            innerPanel.Hide();
            innerPanel.UpdateWindow.SetData(CUpdater.ServerGameVersion);
            innerPanel.Show(innerPanel.UpdateWindow);
            lblUpdateStatus.Text = "Updating...".L10N("UI:Main:Updating");
            UpdateInProgress = true;
            CUpdater.StartAsyncUpdate();
        }

        #endregion

        private void BtnOptions_LeftClick(object sender, EventArgs e) => optionsWindow.Open();

        private void BtnNewCampaign_LeftClick(object sender, EventArgs e) =>
            innerPanel.Show(innerPanel.CampaignSelect);

        private void BtnLoadGame_LeftClick(object sender, EventArgs e) =>
            innerPanel.Show(innerPanel.GameLoadingWindow);

        private void BtnLan_LeftClick(object sender, EventArgs e)
        {
            lanLobby.Open();

            if (UserINISettings.Instance.StopMusicOnMenu)
                MusicOff();

            if (connectionManager.IsConnected)
                connectionManager.Disconnect();

            topBar.SetLanMode(true);
        }

        private void BtnCnCNet_LeftClick(object sender, EventArgs e) => topBar.SwitchToSecondary();

        private void BtnSkirmish_LeftClick(object sender, EventArgs e)
        {
            skirmishLobby.Open();

            if (UserINISettings.Instance.StopMusicOnMenu)
                MusicOff();
        }

        private void BtnMapEditor_LeftClick(object sender, EventArgs e) => LaunchMapEditor();

        private void BtnStatistics_LeftClick(object sender, EventArgs e) =>
            innerPanel.Show(innerPanel.StatisticsWindow);

        private void BtnCredits_LeftClick(object sender, EventArgs e)
        {
            if (UserINISettings.Instance.TC2Completed)
                Process.Start("http://www.bilibili.com/video/BV1cJ411X7pi?p=3");
            else
                innerPanel.Show(innerPanel.CreditsPanel);
        }

        private void BtnExtras_LeftClick(object sender, EventArgs e) =>
            innerPanel.Show(innerPanel.DatabasePanel); //innerPanel.Show(innerPanel.ExtrasWindow);

        private void BtnExit_LeftClick(object sender, EventArgs e)
        {
            isWindowIdle = false;
            WindowManager.HideWindow();
            FadeMusicExit();
        }

        private void SharedUILogic_GameProcessExited() =>
            AddCallback(new Action(HandleGameProcessExited), null);

        private void HandleGameProcessExited()
        {
            innerPanel.GameLoadingWindow.ListSaves();
            innerPanel.Hide();

            // If music is disabled on menus, check if the main menu is the top-most
            // window of the top bar and only play music if it is
            // LAN has the top bar disabled, so to detect the LAN game lobby
            // we'll check whether the top bar is enabled
            if (!UserINISettings.Instance.StopMusicOnMenu ||
                (topBar.Enabled && topBar.LastSwitchType == SwitchType.PRIMARY &&
                topBar.GetTopMostPrimarySwitchable() == this))
                isGameExited = true;
        }

        private void StateScoreSongChanged()
        {
            if (!LogbuchParser.SongEnded)
                PlayMusic();
            else if (isGameExited)
            {
                themeSong = AssetLoader.LoadSong(ClientConfiguration.Instance.MainMenuMusicName);
                PlayMusic();
            }
        }

        /// <summary>
        /// Switches to the main menu and performs a check for updates.
        /// </summary>
        private void CncnetLobby_UpdateCheck(object sender, EventArgs e)
        {
            CheckForUpdates();
            topBar.SwitchToPrimary();
        }

        public override void Update(GameTime gameTime)
        {
            if (isMusicFading)
                FadeMusic(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            lock (locker)
            {
                base.Draw(gameTime);
            }
        }

        /// <summary>
        /// Attempts to start playing the menu music.
        /// </summary>
        private void PlayMusic()
        {
            if (!isMediaPlayerAvailable)
                return; // SharpDX fails at music playback on Vista

            if (themeSong != null && UserINISettings.Instance.PlayMainMenuMusic)
            {
                musicVolume = 1.0f;
                isMusicFading = false;
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = (float)UserINISettings.Instance.ClientVolume;

                try
                {
                    if (!LogbuchParser.SongEnded)
                    {
                        Logger.Log("Attempting to play score music: " + LogbuchParser.scoreSong);
                        themeSong = AssetLoader.LoadSong(LogbuchParser.scoreSong);
                        MediaPlayer.MediaStateChanged += StateSongChanged;
                        MediaPlayer.IsRepeating = false;
                    }
                    MediaPlayer.Play(themeSong);
                }
                catch (InvalidOperationException ex)
                {
                    Logger.Log("Playing main menu music failed! " + ex.Message);
                }
            }
        }

        protected virtual void StateSongChanged(object sender, EventArgs e)
        {
            if (MediaPlayer.State == MediaState.Stopped)
            {
                MediaPlayer.MediaStateChanged -= StateSongChanged;
                themeSong = AssetLoader.LoadSong(ClientConfiguration.Instance.MainMenuMusicName);
                LogbuchParser.SongEnded = true;
                if (isWindowIdle)
                    PlayMusic();
            }
        }

        /// <summary>
        /// Lowers the volume of the menu music, or stops playing it if the
        /// volume is unaudibly low.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void FadeMusic(GameTime gameTime)
        {
            if (!isMediaPlayerAvailable || !isMusicFading || themeSong == null)
                return;

            // Fade during 1 second
            float step = SoundPlayer.Volume * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (MediaPlayer.Volume > step)
                MediaPlayer.Volume -= step;
            else
            {
                MediaPlayer.Stop();
                isMusicFading = false;
            }
        }

        /// <summary>
        /// Exits the client. Quickly fades the music if it's playing.
        /// </summary>
        private void FadeMusicExit()
        {
            if (!isMediaPlayerAvailable || themeSong == null)
            {
                ExitClient();
                return;
            }

            float step = MEDIA_PLAYER_VOLUME_EXIT_FADE_STEP * (float)UserINISettings.Instance.ClientVolume;

            if (MediaPlayer.Volume > step)
            {
                MediaPlayer.Volume -= step;
                AddCallback(new Action(FadeMusicExit), null);
            }
            else
            {
                MediaPlayer.Stop();
                ExitClient();
            }
        }

        private void ExitClient()
        {
            Logger.Log("Exiting.");
            WindowManager.CloseGame();
#if !XNA
            Thread.Sleep(1000);
            Environment.Exit(0);
#endif
        }

        public void SwitchOn()
        {
            if (LogbuchParser.SongEnded && UserINISettings.Instance.StopMusicOnMenu)
                PlayMusic();

            /*if (!ClientConfiguration.Instance.ModMode && UserINISettings.Instance.CheckForUpdates)
            {
                // Re-check for updates

                if ((DateTime.Now - lastUpdateCheckTime) > TimeSpan.FromSeconds(UPDATE_RE_CHECK_THRESHOLD))
                    CheckForUpdates();
            }*/
        }

        public void SwitchOff()
        {
            isWindowIdle = false;
            if (LogbuchParser.SongEnded && UserINISettings.Instance.StopMusicOnMenu)
                MusicOff();
        }

        private void MusicOff()
        {
            try
            {
                if (isMediaPlayerAvailable &&
                    MediaPlayer.State == MediaState.Playing)
                {
                    isMusicFading = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Turning music off failed! Message: " + ex.Message);
            }
        }

        /// <summary>
        /// Checks if media player is available currently.
        /// It is not available on Windows Vista or other systems without the appropriate media player components.
        /// </summary>
        /// <returns>True if media player is available, false otherwise.</returns>
        private bool IsMediaPlayerAvailable()
        {
            if (MainClientConstants.OSId == OSVersion.WINVISTA)
                return false;

            try
            {
                MediaState state = MediaPlayer.State;
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Error encountered when checking media player availability. Error message: " + e.Message);
                return false;
            }
        }

        private void LaunchMapEditor()
        {
            /*OSVersion osVersion = ClientConfiguration.Instance.GetOperatingSystemVersion();
            Process mapEditorProcess = new Process();

            if (osVersion != OSVersion.UNIX)
            {
                mapEditorProcess.StartInfo.FileName = ProgramConstants.GamePath + ClientConfiguration.Instance.MapEditorExePath;
            }
            else
            {
                mapEditorProcess.StartInfo.FileName = ProgramConstants.GamePath + ClientConfiguration.Instance.UnixMapEditorExePath;
                mapEditorProcess.StartInfo.UseShellExecute = false;
            }

            mapEditorProcess.Start();*/
        }

        public string GetSwitchName() => "Main Menu".L10N("UI:Main:MainMenu");
    }
}
