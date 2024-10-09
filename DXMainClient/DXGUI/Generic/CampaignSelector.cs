using ClientCore;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using DTAClient.Domain;
using System.IO;
using ClientGUI;
using Rampastring.XNAUI.XNAControls;
using Rampastring.XNAUI;
using Rampastring.Tools;
using Updater;
using Localization;
using System.Linq;

namespace DTAClient.DXGUI.Generic
{
    public class CampaignSelector : XNAWindow
    {
        private const int DEFAULT_WIDTH = 1280;
        private const int DEFAULT_HEIGHT = 768;
        private const string SPMUSIC_SETTINGS = "Client/MusicSettings.ini";
        private const string SPSOUND_INI = "spsound.ini";
        private const string CREDITS_TXT = "creditstc.txt";

        protected int StartMusicIndex { get; set; }
        protected int ConflictMusicIndex { get; set; }

        private int FakeDifficultyLevel;

        public CampaignSelector(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.discordHandler = discordHandler;
        }

        private DiscordHandler discordHandler;

        private List<Mission> Missions = new List<Mission>();
        private XNAListBox lbCampaignList;
        private XNAClientButton btnLaunch;
        private XNAClientButton btnCancel;
        private XNATextBlock tbMissionDescription;
        private XNATrackbar trbDifficultySelector;

        private CheaterWindow cheaterWindow;

        public override void Initialize()
        {
            ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            BackgroundTexture = AssetLoader.LoadTexture("missionselectorbg.png");
            BorderColor = UISettings.ActiveSettings.PanelBorderColor;

            Name = "CampaignSelector";

            var lblSelectCampaign = new XNALabel(WindowManager);
            lblSelectCampaign.Name = "lblSelectCampaign";
            lblSelectCampaign.FontIndex = 1;
            lblSelectCampaign.ClientRectangle = new Rectangle(12, 12, 0, 0);
            lblSelectCampaign.Text = "MISSIONS:".L10N("UI:Main:Missions");

            lbCampaignList = new XNAListBox(WindowManager);
            lbCampaignList.Name = "lbCampaignList";
            // lbCampaignList.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 2, 2);
            lbCampaignList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbCampaignList.ClientRectangle = new Rectangle(12,
                lblSelectCampaign.Bottom + 6, 300, 516);
            lbCampaignList.SelectedIndexChanged += LbCampaignList_SelectedIndexChanged;

            var lblMissionDescriptionHeader = new XNALabel(WindowManager);
            lblMissionDescriptionHeader.Name = "lblMissionDescriptionHeader";
            lblMissionDescriptionHeader.FontIndex = 1;
            lblMissionDescriptionHeader.ClientRectangle = new Rectangle(
                lbCampaignList.Right + 12,
                lblSelectCampaign.Y, 0, 0);
            lblMissionDescriptionHeader.Text = "MISSION DESCRIPTION:".L10N("UI:Main:MissionDescription");

            tbMissionDescription = new XNATextBlock(WindowManager);
            tbMissionDescription.Name = "tbMissionDescription";
            tbMissionDescription.ClientRectangle = new Rectangle(
                lblMissionDescriptionHeader.X,
                lblMissionDescriptionHeader.Bottom + 6,
                Width - 24 - lbCampaignList.Right, 430);
            tbMissionDescription.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            tbMissionDescription.Alpha = 1.0f;

            tbMissionDescription.BackgroundTexture = AssetLoader.CreateTexture(AssetLoader.GetColorFromString(ClientConfiguration.Instance.AltUIBackgroundColor),
                tbMissionDescription.Width, tbMissionDescription.Height);

            var lblDifficultyLevel = new XNALabel(WindowManager);
            lblDifficultyLevel.Name = "lblDifficultyLevel";
            lblDifficultyLevel.Text = "DIFFICULTY LEVEL".L10N("UI:Main:DifficultyLevel");
            lblDifficultyLevel.FontIndex = 1;
            Vector2 textSize = Renderer.GetTextDimensions(lblDifficultyLevel.Text, lblDifficultyLevel.FontIndex);
            lblDifficultyLevel.ClientRectangle = new Rectangle(
                tbMissionDescription.X + (tbMissionDescription.Width - (int)textSize.X) / 2,
                tbMissionDescription.Bottom + 12, (int)textSize.X, (int)textSize.Y);

            trbDifficultySelector = new XNATrackbar(WindowManager);
            trbDifficultySelector.Name = "trbDifficultySelector";
            trbDifficultySelector.ClientRectangle = new Rectangle(
                tbMissionDescription.X, lblDifficultyLevel.Bottom + 6,
                tbMissionDescription.Width, 30);
            trbDifficultySelector.MinValue = 0;
            trbDifficultySelector.MaxValue = 3;
            trbDifficultySelector.BackgroundTexture = AssetLoader.CreateTexture(
                new Color(0, 0, 0, 128), 2, 2);
            trbDifficultySelector.ButtonTexture = AssetLoader.LoadTextureUncached(
                "trackbarButton_difficulty.png");

            var lblEasy = new XNALabel(WindowManager);
            lblEasy.Name = "lblEasy";
            lblEasy.FontIndex = 1;
            lblEasy.Text = "EASY".L10N("UI:Main:DifficultyEasy");
            lblEasy.ClientRectangle = new Rectangle(trbDifficultySelector.X,
                trbDifficultySelector.Bottom + 6, 1, 1);

            var lblNormal = new XNALabel(WindowManager);
            lblNormal.Name = "lblNormal";
            lblNormal.FontIndex = 1;
            lblNormal.Text = "NORMAL".L10N("UI:Main:DifficultyNormal");
            textSize = Renderer.GetTextDimensions(lblNormal.Text, lblNormal.FontIndex);
            lblNormal.ClientRectangle = new Rectangle(
                tbMissionDescription.X + (tbMissionDescription.Width - (int)textSize.X) / 2,
                lblEasy.Y, (int)textSize.X, (int)textSize.Y);

            var lblHard = new XNALabel(WindowManager);
            lblHard.Name = "lblHard";
            lblHard.FontIndex = 1;
            lblHard.Text = "HARD".L10N("UI:Main:DifficultyHard");
            lblHard.ClientRectangle = new Rectangle(
                tbMissionDescription.Right - lblHard.Width,
                lblEasy.Y, 1, 1);

            var lblHELL = new XNALabel(WindowManager);
            lblHELL.Name = "lblHELL";
            lblHELL.FontIndex = 1;
            lblHELL.Text = "Abyss".L10N("UI:Main:DifficultyAbyss");
            lblHELL.ClientRectangle = new Rectangle(
                tbMissionDescription.ClientRectangle.Right - lblHard.ClientRectangle.Width,
                lblEasy.ClientRectangle.Y, 1, 1);

            btnLaunch = new XNAClientButton(WindowManager);
            btnLaunch.Name = "btnLaunch";
            btnLaunch.ClientRectangle = new Rectangle(12, Height - 35, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
            btnLaunch.Text = "Launch".L10N("UI:Main:ButtonLaunch");
            btnLaunch.AllowClick = false;
            btnLaunch.LeftClick += BtnLaunch_LeftClick;

            btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = "btnCancel";
            btnCancel.ClientRectangle = new Rectangle(Width - 145,
                btnLaunch.Y, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
            btnCancel.Text = "Cancel".L10N("UI:Main:ButtonCancel");
            btnCancel.LeftClick += BtnCancel_LeftClick;

            AddChild(lblSelectCampaign);
            AddChild(lblMissionDescriptionHeader);
            AddChild(lbCampaignList);
            AddChild(tbMissionDescription);
            AddChild(lblDifficultyLevel);
            AddChild(btnLaunch);
            AddChild(btnCancel);
            AddChild(trbDifficultySelector);
            AddChild(lblEasy);
            AddChild(lblNormal);
            AddChild(lblHard);
            AddChild(lblHELL);

            // Set control attributes from INI file
            base.Initialize();

            // Center on screen
            CenterOnParent();

            trbDifficultySelector.Value = UserINISettings.Instance.FakeDifficulty;

            ReloadBattleIni(false);

            cheaterWindow = new CheaterWindow(WindowManager);
            DarkeningPanel dp = new DarkeningPanel(WindowManager);
            dp.AddChild(cheaterWindow);
            AddChild(dp);
            dp.CenterOnParent();
            cheaterWindow.CenterOnParent();
            cheaterWindow.Disable();
        }

        public void ReloadBattleIni(bool ShowTutorialOnly) => ParseBattleIni("INI/" + ClientConfiguration.Instance.BattleFSFileName, ShowTutorialOnly);

        private void LbCampaignList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbCampaignList.SelectedIndex == -1)
            {
                tbMissionDescription.Text = string.Empty;
                btnLaunch.AllowClick = false;
                return;
            }

            Mission mission = Missions[lbCampaignList.SelectedIndex];

            if (string.IsNullOrEmpty(mission.Scenario))
            {
                tbMissionDescription.Text = string.Empty;
                btnLaunch.AllowClick = false;
                return;
            }

            tbMissionDescription.Text = mission.GUIDescription;

            if (!mission.Enabled)
            {
                btnLaunch.AllowClick = false;
                return;
            }

            btnLaunch.AllowClick = true;
        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            // 退内层菜单
            Enabled = false;
            Visible = false;
        }

        private void BtnLaunch_LeftClick(object sender, EventArgs e)
        {
            PrepareToLaunch();
        }

        private void PrepareToLaunch()
        {
            if (!ClientConfiguration.Instance.ModMode && !AreFilesModified())
            {
                // Confront the user by showing the cheater screen
                cheaterWindow.Enable();
                return;
            }

            int selectedMissionId = lbCampaignList.SelectedIndex;

            Mission mission = Missions[selectedMissionId];

            // use vanilla campaign loading screen
            File.Delete(ProgramConstants.GamePath + "tcextrab15.big");

            LaunchMission(mission);
        }

        private bool AreFilesModified()
        {
            int iCount = 0;
            foreach (string filePath in InfoShared.filesToCheckCamp)
            {
                if (!File.Exists(ProgramConstants.GamePath + filePath))
                {
                    cheaterWindow.SetCantFindText(filePath);
                    return false;
                }

                if (iCount >= InfoShared.filesHashArrayCamp.Length || Utilities.CalculateSHA1ForFile(filePath).ToUpper() != InfoShared.filesHashArrayCamp[iCount].ToUpper())
                {
                    cheaterWindow.SetDefaultText(filePath);
                    return false;
                }
                ++iCount;
            }
            return true;
        }

        private string GetPlayerMusicSide(string strSide)
        {
            switch (strSide.ToLower())
            {
                case "gdi":
                    return "GDI";
                case "nod":
                    return "Nod";
                case "scr":
                    return "Scrin";
                default:
                    return "GDI";
            }
        }

        /// <summary>
        /// Starts a singleplayer mission.
        /// </summary>
        /// <param name="scenario">The internal name of the scenario.</param>
        /// <param name="requiresAddon">True if the mission is for Firestorm / Enhanced Mode.</param>
        private void LaunchMission(Mission mission)
        {
            bool copyMapsToSpawnmapINI = ClientConfiguration.Instance.CopyMissionsToSpawnmapINI;

            Logger.Log("About to write spawn.ini.");
            StreamWriter swriter = new StreamWriter(ProgramConstants.GamePath + "spawn.ini");
            swriter.WriteLine("; Generated by DTA Client");
            swriter.WriteLine("[Settings]");
            if (!copyMapsToSpawnmapINI)
                swriter.WriteLine("Scenario=spawnmap.ini");
            else
                swriter.WriteLine("Scenario=" + mission.Scenario);

            if (UserINISettings.Instance.GameSpeed != 2)
                UserINISettings.Instance.GameSpeed.Value = 2;

            swriter.WriteLine("GameSpeed=" + UserINISettings.Instance.GameSpeed);
            swriter.WriteLine("Firestorm=" + mission.RequiredAddon);
            swriter.WriteLine("CustomLoadScreen=" + LoadingScreenController.GetLoadScreenName(mission.Side.ToString()));
            swriter.WriteLine("IsSinglePlayer=Yes");
            swriter.WriteLine("SidebarHack=" + ClientConfiguration.Instance.SidebarHack);
            swriter.WriteLine("Side=" + mission.Side);
            swriter.WriteLine("BuildOffAlly=" + mission.BuildOffAlly);

            swriter.WriteLine("DifficultyModeHuman=" + (mission.PlayerAlwaysOnNormalDifficulty ? "1" : trbDifficultySelector.Value.ToString()));
            swriter.WriteLine("DifficultyModeComputer=" + GetComputerDifficulty());

            IniFile difficultyIni = new IniFile(ProgramConstants.GamePath + InfoShared.DifficultyIniPaths[trbDifficultySelector.Value]);
            string difficultyName = InfoShared.DifficultyNames[trbDifficultySelector.Value];

            swriter.WriteLine();
            swriter.WriteLine();
            swriter.WriteLine();
            swriter.Close();

            // ReShade Settings
            ProgramConstants.SetupPreset();
            IniFile CampaignIni = new IniFile(ProgramConstants.GamePath + "GameShaders/CampaignINI/" +
                mission.Scenario.Substring(0, 5).ToLower() + ".ini");
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

                // Clouds
                bool bLightClouds = CampaignIni.GetBooleanValue("BaseInfo", "RedAmbEffect", false);
                string strLightClouds = CampaignIni.GetStringValue("BaseInfo", "RedAmbEffect", "none").ToLower();
                if (strLightClouds == "true")
                    bLightClouds = true;
                else if (strLightClouds == "false")
                    bLightClouds = false;

                if (CampaignIni.GetBooleanValue("BaseInfo", "IsSnow", false)) // Snow
                {
                    if (CampaignIni.GetBooleanValue("BaseInfo", "IsSnowNight", false))
                    {
                        // shaderIniWriter.WriteLine(ClientConfiguration.TC_SHADER_SNOWNIGHT);
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

                        // Tint
                        if (bLightClouds)
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

                        if (UserINISettings.Instance.HighDetail >= 2)
                        {
                            strTechniques += ",AmbientLight";
                        }
                        if (UserINISettings.Instance.HighDetail >= 1)
                        {
                            strTechniques += ",Levels";
                        }


                        // Tint
                        if (bLightClouds)
                            strExtraLines += ClientConfiguration.SHADER_TINT_SNOWDAY_VANILLA;
                        else
                            strExtraLines += ClientConfiguration.SHADER_TINT_SNOW_LIGHT;
                    }
                }
                else if (CampaignIni.GetBooleanValue("BaseInfo", "IsNight2", false)) // Night2
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

                    // Tint
                    if (bLightClouds)
                        strExtraLines += ClientConfiguration.SHADER_TINT_NIGHT_VANILLA;
                    else
                        strExtraLines += ClientConfiguration.SHADER_TINT_NONE;
                }
                else if (CampaignIni.GetBooleanValue("BaseInfo", "IsNight", false)) // Night
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

                    // Tint
                    if (bLightClouds)
                        strExtraLines += ClientConfiguration.SHADER_TINT_NIGHT_VANILLA;
                    else
                        strExtraLines += ClientConfiguration.SHADER_TINT_NONE;
                }
                else // Day
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

                    // Tint
                    if (bLightClouds)
                        strExtraLines += ClientConfiguration.SHADER_TINT_DAY;
                    else
                        strExtraLines += ClientConfiguration.SHADER_TINT_DAY;
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
            }

            shaderIniWriter.WriteLine(shaderIniWriter.NewLine);
            shaderIniWriter.Close();

            // Game Music Settings
            IniFile musicListIni = new IniFile(ProgramConstants.GamePath + "INI/MusicListTC.ini");
            IniFile musicConfigIni = new IniFile(ProgramConstants.GamePath + "INI/MusicConfigTC.ini");
            if (UserINISettings.Instance.SmartMusic && UserINISettings.Instance.MusicType < 2)
            {
                IniFile musicSettingsIni = new IniFile(ProgramConstants.GamePath + SPMUSIC_SETTINGS);
                StartMusicIndex = musicSettingsIni.GetIntValue("Settings", "NextStartMusicIndex", 1);
                ConflictMusicIndex = musicSettingsIni.GetIntValue("Settings", "NextConflictMusicIndex", 1);

                Random random = new Random();
                string sideName = GetPlayerMusicSide(CampaignIni.GetStringValue("BaseInfo", "PlayerSide", mission.Scenario.Substring(0, 3)));

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
                        musicListIni.GetStringValue("GameConflict", sideName + indexArray[3], "gdi_conflict_5"));
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

                musicSettingsIni.WriteIniFile();
            }
            musicConfigIni.WriteIniFile(ProgramConstants.GamePath + SPSOUND_INI);

            if (copyMapsToSpawnmapINI)
            {
                IniFile mapIni;
                /*if (mission.Scenario.ToUpper() == "END08.MAP")
                {
                    string[] mapNameArray =
                    {
                        "END08.MAP",
                        "END08_OFF.MAP",
                        "END08_DEF.MAP",
                        "END08_SUP.MAP"
                    };
                    Random random = new Random();
                    mapIni = new IniFile(ProgramConstants.GamePath + "MapsTC/Missions/" + mapNameArray[random.Next(0, 4)]);
                }
                else*/
                    mapIni = new IniFile(ProgramConstants.GamePath + "MapsTC/Missions/" + mission.Scenario);

                // Map Settings
                foreach (string strName in InfoShared.LightNameArray)
                {
                    mapIni.SetDoubleValue("Lighting", "Ion" + strName, mapIni.GetDoubleValue("Lighting", strName, 0.35f));
                    mapIni.SetDoubleValue("Lighting", "Dominator" + strName, mapIni.GetDoubleValue("Lighting", strName, 0.35f));
                }
                mapIni.SetStringValue("Basic", "TiberiumDeathToVisceroid", "yes");

                if (CampaignIni.GetBooleanValue("BaseInfo", "Ambient.Wind.Cold", false))
                    mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_Cold");
                else if (CampaignIni.GetBooleanValue("BaseInfo", "Ambient.Wind.MountLow", false))
                    mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_MountLow");
                else if (CampaignIni.GetBooleanValue("BaseInfo", "Ambient.Wind.MountHigh", false))
                    mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_MountHigh");
                else if (CampaignIni.GetBooleanValue("BaseInfo", "Ambient.Wind.Disable", false))
                    mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "NULLQAQ");

                string moviePath = ProgramConstants.GamePath + "tcextrab04.big";
                if (!File.Exists(moviePath) || Utilities.CalculateSHA1ForFile(moviePath).ToUpper() != "F432ACC0E48675CF2545EB8DC777E5EE7138247C")
                    mapIni.SetStringValue("Basic", "Win", "dummymovie_win");

                IniFile globalCodeIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/GlobalCode.ini");

                // other settings in BaseInfo
                if (CampaignIni.GetBooleanValue("BaseInfo", "DifficultyAdjust", true))
                {
                    IniFile.ConsolidateIniFiles(mapIni, difficultyIni);
                }
                if (CampaignIni.GetBooleanValue("BaseInfo", "MusicFullControl", false))
                {
                    IniFile themeIni = new IniFile(ProgramConstants.GamePath + SPSOUND_INI);

                    List<string> sections = themeIni.GetSections();
                    foreach (string sectionName in sections)
                    {
                        if (themeIni.GetStringValue(sectionName, "Normal", null) == "yes")
                        {
                            themeIni.SetStringValue(sectionName, "Normal", "no");
                        }

                        if (!String.IsNullOrEmpty(themeIni.GetStringValue(sectionName, "Side", null)))
                        {
                            themeIni.SetStringValue(sectionName, "Side", "none");
                        }
                    }
                    themeIni.WriteIniFile();
                }
                else if (CampaignIni.GetBooleanValue("BaseInfo", "SmartMusic", true))
                {
                    IniFile.ConsolidateIniFiles(mapIni, globalCodeIni);
                }

                mapIni.WriteIniFile(ProgramConstants.GamePath + mission.Scenario.ToLower());
            }

            if (mission.Scenario.ToLower() == "gdi08.map" || mission.Scenario.ToLower() == "nod08.map" || mission.Scenario.ToLower() == "scr04.map" || mission.Scenario.ToLower() == "end08.map")
                File.Copy(ProgramConstants.GetBaseResourcePath() + CREDITS_TXT, ProgramConstants.GamePath + CREDITS_TXT, true);
            else
                File.Delete(ProgramConstants.GamePath + CREDITS_TXT);

            FakeDifficultyLevel = trbDifficultySelector.Value;
            if (trbDifficultySelector.Value < 0)
                UserINISettings.Instance.Difficulty.Value = 0;
            else
                UserINISettings.Instance.Difficulty.Value = trbDifficultySelector.Value - 1;
            UserINISettings.Instance.FakeDifficulty.Value = FakeDifficultyLevel;
            UserINISettings.Instance.SaveSettings();

            ((MainMenuDarkeningPanel)Parent).Hide();

            string strDifficultyName = InfoShared.DifficultyNames[trbDifficultySelector.Value];
            discordHandler?.UpdatePresence(mission.GUIName, strDifficultyName, mission.IconPath, true);

            GameProcessLogic.GameProcessExited += GameProcessExited_Callback;
            GameProcessLogic.StartGameProcess(CampaignIni.GetBooleanValue("BaseInfo", "ControlSpeed", true), true);
        }

        private int GetComputerDifficulty() =>
            Math.Abs(trbDifficultySelector.Value - 2);

        private void GameProcessExited_Callback()
        {
            WindowManager.AddCallback(new Action(GameProcessExited), null);

            foreach (string strMapName in InfoShared.campaignList)
                File.Delete(ProgramConstants.GamePath + strMapName);
        }

        protected virtual void GameProcessExited()
        {
            GameProcessLogic.GameProcessExited -= GameProcessExited_Callback;
            LogbuchParser.ParseForCampaign();
            LogbuchParser.ClearTrash();
            discordHandler?.UpdatePresence();
        }

        /// <summary>
        /// Parses a Battle(E).ini file. Returns true if succesful (file found), otherwise false.
        /// </summary>
        /// <param name="path">The path of the file, relative to the game directory.</param>
        /// <returns>True if succesful, otherwise false.</returns>
        private bool ParseBattleIni(string path, bool bShowTutorialOnly)
        {
            Logger.Log("Attempting to parse " + path + " to populate mission list.");

            string battleIniPath = ProgramConstants.GamePath + path;
            if (!File.Exists(battleIniPath))
            {
                Logger.Log("File " + path + " not found. Ignoring.");
                return false;
            }

            IniFile battleIni = new IniFile(battleIniPath);

            List<string> battleKeys = battleIni.GetSectionKeys("Battles");

            if (battleKeys == null)
                return false; // File exists but [Battles] doesn't

            // Clear up stuff
            lbCampaignList.SelectedIndex = -1;
            tbMissionDescription.Text = string.Empty;
            btnLaunch.AllowClick = false;
            Missions.Clear();
            lbCampaignList.Clear();

            foreach (string battleEntry in battleKeys)
            {
                string battleSection = battleIni.GetStringValue("Battles", battleEntry, "NOT FOUND");

                if (!battleIni.SectionExists(battleSection))
                    continue;

                var mission = new Mission(battleIni, battleSection);

                // Skip Hidden missions
                if (bShowTutorialOnly)
                {
                    if (!mission.IsTutorial)
                        continue;
                }
                else if (mission.Hide)
                    continue;

                Missions.Add(mission);

                XNAListBoxItem item = new XNAListBoxItem();
                item.Text = mission.GUIName;
                if (!mission.Enabled)
                {
                    item.TextColor = UISettings.ActiveSettings.DisabledItemColor;
                }
                else if (string.IsNullOrEmpty(mission.Scenario))
                {
                    item.TextColor = AssetLoader.GetColorFromString(
                        ClientConfiguration.Instance.ListBoxHeaderColor);
                    item.IsHeader = true;
                    item.Selectable = false;
                }
                else
                {
                    item.TextColor = lbCampaignList.DefaultItemColor;
                }

                if (!string.IsNullOrEmpty(mission.IconPath))
                    item.Texture = AssetLoader.LoadTexture(mission.IconPath + "icon.png");

                lbCampaignList.AddItem(item);
            }

            Logger.Log("Finished parsing " + path + ".");
            return true;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
