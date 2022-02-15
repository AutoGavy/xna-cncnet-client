using ClientCore.Settings;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ClientCore.Enums;

namespace ClientCore
{
    public class UserINISettings
    {
        private static UserINISettings _instance;

        private const string VIDEO = "Video";
        private const string MULTIPLAYER = "MultiPlayer";
        private const string OPTIONS = "Options";
        private const string AUDIO = "Audio";
        private const string CUSTOM_SETTINGS = "CustomSettings";
        private const string COMPATIBILITY = "Compatibility";
        private const string GAME_FILTERS = "GameFilters";

        private const bool DEFAULT_SHOW_FRIENDS_ONLY_GAMES = false;
        private const bool DEFAULT_HIDE_LOCKED_GAMES = false;
        private const bool DEFAULT_HIDE_PASSWORDED_GAMES = false;
        private const bool DEFAULT_HIDE_INCOMPATIBLE_GAMES = false;
        private const int DEFAULT_MAX_PLAYER_COUNT = 8;

        public static UserINISettings Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("UserINISettings not initialized!");

                return _instance;
            }
        }

        public static void Initialize(string iniFileName)
        {
            if (_instance != null)
                throw new InvalidOperationException("UserINISettings has already been initialized!");

            var iniFile = new IniFile(ProgramConstants.GamePath + iniFileName);

            _instance = new UserINISettings(iniFile);
        }

        protected UserINISettings(IniFile iniFile)
        {
            SettingsIni = iniFile;

#if YR || ARES
            const string WINDOWED_MODE_KEY = "Video.Windowed";
            BackBufferInVRAM = new BoolSetting(iniFile, VIDEO, "VideoBackBuffer", false);
#else
            const string WINDOWED_MODE_KEY = "Video.Windowed";
            BackBufferInVRAM = new BoolSetting(iniFile, VIDEO, "UseGraphicsPatch", true);
#endif
            int IngameScreenX = 1024;
            int IngameScreenY = 768;
            if (Screen.PrimaryScreen.Bounds.Width >= 1920 || Screen.PrimaryScreen.Bounds.Height >= 1080)
            {
                IngameScreenX = 1920;
                IngameScreenY = 1080;
            }
            else if (Screen.PrimaryScreen.Bounds.Width >= 1600 || Screen.PrimaryScreen.Bounds.Height >= 900)
            {
                IngameScreenX = 1600;
                IngameScreenY = 900;
            }
            IngameScreenWidth = new IntSetting(iniFile, VIDEO, "ScreenWidth", IngameScreenX);
            IngameScreenHeight = new IntSetting(iniFile, VIDEO, "ScreenHeight", IngameScreenY);

            ClientTheme = new StringSetting(iniFile, MULTIPLAYER, "Theme", string.Empty);
            DetailLevel = new IntSetting(iniFile, OPTIONS, "DetailLevel", 2);
            Renderer = new StringSetting(iniFile, COMPATIBILITY, "Renderer", string.Empty);
            WindowedMode = new BoolSetting(iniFile, VIDEO, WINDOWED_MODE_KEY, true);
            BorderlessWindowedMode = new BoolSetting(iniFile, VIDEO, "NoWindowFrame", true);

            int ScreenX = 1280;
            int ScreenY = 800;
            if (Screen.PrimaryScreen.Bounds.Width < 1280 || Screen.PrimaryScreen.Bounds.Height < 800)
            {
                ScreenX = Screen.PrimaryScreen.Bounds.Width;
                ScreenY = Screen.PrimaryScreen.Bounds.Height;
            }
            ClientResolutionX = new IntSetting(iniFile, VIDEO, "ClientResolutionX", ScreenX);
            ClientResolutionY = new IntSetting(iniFile, VIDEO, "ClientResolutionY", ScreenY);

            BorderlessWindowedClient = new BoolSetting(iniFile, VIDEO, "BorderlessWindowedClient", false);
            DebugReShade = new BoolSetting(iniFile, OPTIONS, "DebugReShade", false);
            ClientFPS = new IntSetting(iniFile, VIDEO, "ClientFPS", 60);

            System.Management.ManagementObjectSearcher objvide = new System.Management.ManagementObjectSearcher("select * from Win32_VideoController");
            foreach (System.Management.ManagementObject obj in objvide.Get())
            {
                if (obj["VideoProcessor"] == null)
                    continue;

                string strName = obj["VideoProcessor"].ToString().ToUpper();
                if (strName.Contains("RTX") || strName.Contains("TITAN") || strName.Contains("GTX"))
                    GoodGPU = 3;
                else if (strName.Contains("AMD"))
                {
                    if (strName.Contains("RX") || strName.Contains("R9") || strName.Contains("PRO")
                        || strName.Contains("HD7990") || strName.Contains("HD7970") || strName.Contains("HD6990")
                        || strName.Contains("HD7950") || strName.Contains("HD7870"))
                        GoodGPU = 3;
                }
            }

            NoReShade = new BoolSetting(iniFile, VIDEO, "NoReShade", false);
            HighDetail = new IntSetting(iniFile, VIDEO, "HighDetail", GoodGPU);
            AntiAliasing = new IntSetting(iniFile, VIDEO, "AntiAliasing", GoodGPU == 3 ? 1 : 0);
            EnhancedLaser = new IntSetting(iniFile, VIDEO, "EnhancedLaser", 1);
            EnhancedLight = new IntSetting(iniFile, VIDEO, "EnhancedLight", 1);
            Displacement = new IntSetting(iniFile, VIDEO, "DisplaceEffect", GoodGPU == 3 ? 1 : 0);
            CloudsEffect = new IntSetting(iniFile, VIDEO, "CloudsEffect", GoodGPU == 3 ? 1 : 0);

            AlphaLight = new BoolSetting(iniFile, VIDEO, "AlphaLight", GoodGPU == 3 ? true : false);
            AirflowEffect = new BoolSetting(iniFile, VIDEO, "AirflowEffect", GoodGPU == 3 ? true : false);
            VideoMode = new BoolSetting(iniFile, VIDEO, "VideoMode", false);
            MultiCPU = new BoolSetting(iniFile, VIDEO, "MultiCPU", true);

            ScoreVolume = new DoubleSetting(iniFile, AUDIO, "ScoreVolume", 0.7);
            SoundVolume = new DoubleSetting(iniFile, AUDIO, "SoundVolume", 0.7);
            VoiceVolume = new DoubleSetting(iniFile, AUDIO, "VoiceVolume", 0.7);
            IsScoreShuffle = new BoolSetting(iniFile, AUDIO, "IsScoreShuffle", true);
            ClientVolume = new DoubleSetting(iniFile, AUDIO, "ClientVolume", 1.0);
            PlayMainMenuMusic = new BoolSetting(iniFile, AUDIO, "PlayMainMenuMusic", true);
            StopMusicOnMenu = new BoolSetting(iniFile, AUDIO, "StopMusicOnMenu", true);
            MessageSound = new BoolSetting(iniFile, AUDIO, "ChatMessageSound", true);
            SmartMusic = new BoolSetting(iniFile, AUDIO, "SmartMusic", true);
            MusicType = new IntSetting(iniFile, AUDIO, "MusicType", 0);

            ScrollRate = new IntSetting(iniFile, OPTIONS, "ScrollRate", 3);
            TargetLines = new BoolSetting(iniFile, OPTIONS, "UnitActionLines", false);
            ScrollCoasting = new IntSetting(iniFile, OPTIONS, "ScrollMethod", 0);
            Tooltips = new BoolSetting(iniFile, OPTIONS, "ToolTips", true);
            ClassicRallyPoint = new BoolSetting(iniFile, OPTIONS, "ClassicRallyPoint", false);
            ClassicDoubleClick = new BoolSetting(iniFile, OPTIONS, "ClassicDoubleClick", false);
            WheelZoom = new BoolSetting(iniFile, OPTIONS, "WheelZoom", true);
            bDisableWin = new BoolSetting(iniFile, OPTIONS, "bDisableWin", false);
            ShowHiddenObjects = new BoolSetting(iniFile, OPTIONS, "ShowHidden", true);
            MoveToUndeploy = new BoolSetting(iniFile, OPTIONS, "MoveToUndeploy", true);
            TextBackgroundColor = new IntSetting(iniFile, OPTIONS, "TextBackgroundColor", 0);
            DragDistance = new IntSetting(iniFile, OPTIONS, "DragDistance", 4);
            DoubleTapInterval = new IntSetting(iniFile, OPTIONS, "DoubleTapInterval", 30);
            Win8CompatMode = new StringSetting(iniFile, OPTIONS, "Win8Compat", "No");

            PlayerName = new StringSetting(iniFile, MULTIPLAYER, "Handle", string.Empty);

            ChatColor = new IntSetting(iniFile, MULTIPLAYER, "ChatColor", -1);
            LANChatColor = new IntSetting(iniFile, MULTIPLAYER, "LANChatColor", -1);
            PingUnofficialCnCNetTunnels = new BoolSetting(iniFile, MULTIPLAYER, "PingCustomTunnels", true);
            WritePathToRegistry = new BoolSetting(iniFile, OPTIONS, "WriteInstallationPathToRegistry", true);
            PlaySoundOnGameHosted = new BoolSetting(iniFile, MULTIPLAYER, "PlaySoundOnGameHosted", true);
            SkipConnectDialog = new BoolSetting(iniFile, MULTIPLAYER, "SkipConnectDialog", false);
            PersistentMode = new BoolSetting(iniFile, MULTIPLAYER, "PersistentMode", false);
            AutomaticCnCNetLogin = new BoolSetting(iniFile, MULTIPLAYER, "AutomaticCnCNetLogin", false);
            DiscordIntegration = new BoolSetting(iniFile, MULTIPLAYER, "DiscordIntegration", true);
            AllowGameInvitesFromFriendsOnly = new BoolSetting(iniFile, MULTIPLAYER, "AllowGameInvitesFromFriendsOnly", false);
            NotifyOnUserListChange = new BoolSetting(iniFile, MULTIPLAYER, "NotifyOnUserListChange", true);
            DisablePrivateMessagePopups = new BoolSetting(iniFile, MULTIPLAYER, "DisablePrivateMessagePopups", false);
            AllowPrivateMessagesFromState = new IntSetting(iniFile, MULTIPLAYER, "AllowPrivateMessagesFromState", (int)AllowPrivateMessagesFromEnum.All);
            EnableMapSharing = new BoolSetting(iniFile, MULTIPLAYER, "EnableMapSharing", true);
            AlwaysDisplayTunnelList = new BoolSetting(iniFile, MULTIPLAYER, "AlwaysDisplayTunnelList", false);

            CheckForUpdates = new BoolSetting(iniFile, OPTIONS, "CheckforUpdates", true);

            PrivacyPolicyAccepted = new BoolSetting(iniFile, OPTIONS, "PrivacyPolicyAccepted", false);
            IsFirstRun = new BoolSetting(iniFile, OPTIONS, "IsFirstRun", true);
            CustomComponentsDenied = new BoolSetting(iniFile, OPTIONS, "CustomComponentsDenied", false);
            Difficulty = new IntSetting(iniFile, OPTIONS, "Difficulty", 1);
            ScrollDelay = new IntSetting(iniFile, OPTIONS, "ScrollDelay", 4);
            FakeDifficulty = new IntSetting(iniFile, OPTIONS, "FakeDifficulty", 0);
            SelectedMissionIndex = new IntSetting(iniFile, OPTIONS, "SelectedMissionIndex", 0);
            GameSpeed = new IntSetting(iniFile, OPTIONS, "GameSpeed", 1);
            PreloadMapPreviews = new BoolSetting(iniFile, VIDEO, "PreloadMapPreviews", false);
            ForceLowestDetailLevel = new BoolSetting(iniFile, VIDEO, "ForceLowestDetailLevel", false);
            MinimizeWindowsOnGameStart = new BoolSetting(iniFile, OPTIONS, "MinimizeWindowsOnGameStart", true);
            AutoRemoveUnderscoresFromName = new BoolSetting(iniFile, OPTIONS, "AutoRemoveUnderscoresFromName", true);

            CanReShade = new BoolSetting(iniFile, OPTIONS, "CanReShade", false);

            TC2Completed = new BoolSetting(iniFile, "Network", "OTStuID5", false);
            EggSide1 = new BoolSetting(iniFile, "Network", "OTStuID1", false);
            EggSide2 = new BoolSetting(iniFile, "Network", "OTStuID2", false);
            EggSide3 = new BoolSetting(iniFile, "Network", "OTStuID3", false);
            EggSide4 = new BoolSetting(iniFile, "Network", "OTStuID4", false);

            SortState = new IntSetting(iniFile, GAME_FILTERS, "SortState", (int)SortDirection.None);
            ShowFriendGamesOnly = new BoolSetting(iniFile, GAME_FILTERS, "ShowFriendGamesOnly", DEFAULT_SHOW_FRIENDS_ONLY_GAMES);
            HideLockedGames = new BoolSetting(iniFile, GAME_FILTERS, "HideLockedGames", DEFAULT_HIDE_LOCKED_GAMES);
            HidePasswordedGames = new BoolSetting(iniFile, GAME_FILTERS, "HidePasswordedGames", DEFAULT_HIDE_PASSWORDED_GAMES);
            HideIncompatibleGames = new BoolSetting(iniFile, GAME_FILTERS, "HideIncompatibleGames", DEFAULT_HIDE_INCOMPATIBLE_GAMES);
            MaxPlayerCount = new IntRangeSetting(iniFile, GAME_FILTERS, "MaxPlayerCount", DEFAULT_MAX_PLAYER_COUNT, 2, 8);

            FavoriteMaps = new StringListSetting(iniFile, OPTIONS, "FavoriteMaps", new List<string>());
        }

        public IniFile SettingsIni { get; private set; }

        public event EventHandler SettingsSaved;

        /*********/
        /* VIDEO */
        /*********/

        public IntSetting IngameScreenWidth { get; private set; }
        public IntSetting IngameScreenHeight { get; private set; }
        public StringSetting ClientTheme { get; private set; }
        public IntSetting DetailLevel { get; private set; }
        public StringSetting Renderer { get; private set; }
        public BoolSetting WindowedMode { get; private set; }
        public BoolSetting BorderlessWindowedMode { get; private set; }
        public BoolSetting BackBufferInVRAM { get; private set; }
        public IntSetting ClientResolutionX { get; private set; }
        public IntSetting ClientResolutionY { get; private set; }
        public BoolSetting BorderlessWindowedClient { get; private set; }

        public BoolSetting AlphaLight { get; private set; }
        public BoolSetting AirflowEffect { get; private set; }
        public BoolSetting VideoMode { get; private set; }
        public BoolSetting DebugReShade { get; private set; }
        public BoolSetting MultiCPU { get; private set; }

        public BoolSetting NoReShade { get; private set; }
        public IntSetting HighDetail { get; private set; }
        public IntSetting CloudsEffect { get; private set; }
        public IntSetting AntiAliasing { get; private set; }
        public IntSetting EnhancedLaser { get; private set; }
        public IntSetting EnhancedLight { get; private set; }
        public IntSetting Displacement { get; private set; }

        public IntSetting ClientFPS { get; private set; }

        /*********/
        /* AUDIO */
        /*********/

        public DoubleSetting ScoreVolume { get; private set; }
        public DoubleSetting SoundVolume { get; private set; }
        public DoubleSetting VoiceVolume { get; private set; }
        public BoolSetting IsScoreShuffle { get; private set; }
        public DoubleSetting ClientVolume { get; private set; }
        public BoolSetting PlayMainMenuMusic { get; private set; }
        public BoolSetting StopMusicOnMenu { get; private set; }
        public BoolSetting MessageSound { get; private set; }
        public BoolSetting SmartMusic { get; private set; }
        public IntSetting MusicType { get; private set; }

        /********/
        /* GAME */
        /********/

        public IntSetting ScrollRate { get; private set; }
        public BoolSetting TargetLines { get; private set; }
        public IntSetting ScrollCoasting { get; private set; }
        public BoolSetting Tooltips { get; private set; }
        public BoolSetting ShowHiddenObjects { get; private set; }
        public BoolSetting MoveToUndeploy { get; private set; }
        public IntSetting TextBackgroundColor { get; private set; }
        public IntSetting DragDistance { get; private set; }
        public IntSetting DoubleTapInterval { get; private set; }
        public StringSetting Win8CompatMode { get; private set; }
        public BoolSetting ClassicRallyPoint { get; private set; }
        public BoolSetting ClassicDoubleClick { get; private set; }
        public BoolSetting WheelZoom { get; private set; }
        public BoolSetting bDisableWin { get; private set; }

        /************************/
        /* MULTIPLAYER (CnCNet) */
        /************************/

        public StringSetting PlayerName { get; private set; }

        public IntSetting ChatColor { get; private set; }
        public IntSetting LANChatColor { get; private set; }
        public BoolSetting PingUnofficialCnCNetTunnels { get; private set; }
        public BoolSetting WritePathToRegistry { get; private set; }
        public BoolSetting PlaySoundOnGameHosted { get; private set; }

        public BoolSetting SkipConnectDialog { get; private set; }
        public BoolSetting PersistentMode { get; private set; }
        public BoolSetting AutomaticCnCNetLogin { get; private set; }
        public BoolSetting DiscordIntegration { get; private set; }
        public BoolSetting AllowGameInvitesFromFriendsOnly { get; private set; }

        public BoolSetting NotifyOnUserListChange { get; private set; }

        public BoolSetting DisablePrivateMessagePopups { get; private set; }

        public IntSetting AllowPrivateMessagesFromState { get; private set; }

        public BoolSetting EnableMapSharing { get; private set; }

        public BoolSetting AlwaysDisplayTunnelList { get; private set; }

        /*********************/
        /* GAME LIST FILTERS */
        /*********************/

        public IntSetting SortState { get; private set; }

        public BoolSetting ShowFriendGamesOnly { get; private set; }

        public BoolSetting HideLockedGames { get; private set; }

        public BoolSetting HidePasswordedGames { get; private set; }

        public BoolSetting HideIncompatibleGames { get; private set; }

        public IntRangeSetting MaxPlayerCount { get; private set; }

        /********/
        /* MISC */
        /********/

        public BoolSetting CheckForUpdates { get; private set; }

        public BoolSetting PrivacyPolicyAccepted { get; private set; }
        public BoolSetting IsFirstRun { get; private set; }
        public BoolSetting CustomComponentsDenied { get; private set; }

        public IntSetting FakeDifficulty { get; private set; }

        public IntSetting SelectedMissionIndex { get; private set; }

        public IntSetting Difficulty { get; private set; }

        public IntSetting GameSpeed { get; private set; }

        public IntSetting ScrollDelay { get; private set; }

        public BoolSetting PreloadMapPreviews { get; private set; }

        public BoolSetting ForceLowestDetailLevel { get; private set; }

        public BoolSetting MinimizeWindowsOnGameStart { get; private set; }

        public BoolSetting AutoRemoveUnderscoresFromName { get; private set; }

        public StringListSetting FavoriteMaps { get; private set; }

        public BoolSetting CanReShade { get; private set; }

        public BoolSetting NewUpdate { get; private set; }

        public BoolSetting TC2Completed { get; private set; }
        public BoolSetting EggSide1 { get; private set; }
        public BoolSetting EggSide2 { get; private set; }
        public BoolSetting EggSide3 { get; private set; }
        public BoolSetting EggSide4 { get; private set; }

        public int GoodGPU { get; set; } = 0;

        public bool IsGameFollowed(string gameName)
        {
            return SettingsIni.GetBooleanValue("Channels", gameName, false);
        }

        public bool ToggleFavoriteMap(string mapName, string gameModeName, bool isFavorite)
        {
            if (string.IsNullOrEmpty(mapName))
                return isFavorite;

            var favoriteMapKey = FavoriteMapKey(mapName, gameModeName);
            isFavorite = IsFavoriteMap(mapName, gameModeName);
            if (isFavorite)
                FavoriteMaps.Remove(favoriteMapKey);
            else
                FavoriteMaps.Add(favoriteMapKey);

            Instance.SaveSettings();

            return !isFavorite;
        }

        /// <summary>
        /// Checks if a specified map name and game mode name belongs to the favorite map list.
        /// </summary>
        /// <param name="nameName">The name of the map.</param>
        /// <param name="gameModeName">The name of the game mode</param>
        public bool IsFavoriteMap(string nameName, string gameModeName) => FavoriteMaps.Value.Contains(FavoriteMapKey(nameName, gameModeName));

        private string FavoriteMapKey(string nameName, string gameModeName) => $"{nameName}:{gameModeName}";

        public void ReloadSettings()
        {
            SettingsIni.Reload();
        }

        public void ApplyDefaults()
        {
            ForceLowestDetailLevel.SetDefaultIfNonexistent();
            DoubleTapInterval.SetDefaultIfNonexistent();
            ScrollDelay.SetDefaultIfNonexistent();
        }

        #region Custom settings

        public bool CustomSettingCheckBoxValueExists(string name)
            => SettingsIni.KeyExists(CUSTOM_SETTINGS, $"{name}_Checked");

        public bool GetCustomSettingValue(string name, bool defaultValue)
            => SettingsIni.GetBooleanValue(CUSTOM_SETTINGS, $"{name}_Checked", defaultValue);

        public void SetCustomSettingValue(string name, bool value)
            => SettingsIni.SetBooleanValue(CUSTOM_SETTINGS, $"{name}_Checked", value);

        public bool CustomSettingDropDownValueExists(string name)
            => SettingsIni.KeyExists(CUSTOM_SETTINGS, $"{name}_SelectedIndex");

        public int GetCustomSettingValue(string name, int defaultValue)
            => SettingsIni.GetIntValue(CUSTOM_SETTINGS, $"{name}_SelectedIndex", defaultValue);

        public void SetCustomSettingValue(string name, int value)
            => SettingsIni.SetIntValue(CUSTOM_SETTINGS, $"{name}_SelectedIndex", value);

        #endregion

        public void SaveSettings()
        {
            Logger.Log("Writing settings INI.");

            ApplyDefaults();
            // CleanUpLegacySettings();

            SettingsIni.WriteIniFile();

            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        public bool IsGameFiltersApplied()
        {
            return ShowFriendGamesOnly.Value != DEFAULT_SHOW_FRIENDS_ONLY_GAMES ||
                   HideLockedGames.Value != DEFAULT_HIDE_LOCKED_GAMES ||
                   HidePasswordedGames.Value != DEFAULT_HIDE_PASSWORDED_GAMES ||
                   HideIncompatibleGames.Value != DEFAULT_HIDE_INCOMPATIBLE_GAMES ||
                   MaxPlayerCount.Value != DEFAULT_MAX_PLAYER_COUNT;
        }

        public void ResetGameFilters()
        {
            ShowFriendGamesOnly.Value = DEFAULT_SHOW_FRIENDS_ONLY_GAMES;
            HideLockedGames.Value = DEFAULT_HIDE_LOCKED_GAMES;
            HideIncompatibleGames.Value = DEFAULT_HIDE_INCOMPATIBLE_GAMES;
            HidePasswordedGames.Value = DEFAULT_HIDE_PASSWORDED_GAMES;
            MaxPlayerCount.Value = DEFAULT_MAX_PLAYER_COUNT;
        }

        /// <summary>
        /// Used to remove old sections/keys to avoid confusion when viewing the ini file directly.
        /// </summary>
        private void CleanUpLegacySettings()
        {
            SettingsIni.GetSection(GAME_FILTERS).RemoveKey("SortAlpha");
        }
    }
}
