﻿using ClientCore;
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
        private const int DEFAULT_WIDTH = 650;
        private const int DEFAULT_HEIGHT = 600;
        private const string SPMUSIC_SETTINGS = "Client/MusicSettings.ini";
        private const string SPSOUND_INI = "spsound.ini";
        private const string CREDITS_TXT = "creditstc.txt";

        private static readonly string[] DifficultyNames = new string[]
        {
            "Easy".L10N("UI:Main:Easy"),
            "Normal".L10N("UI:Main:Normal"),
            "Hard".L10N("UI:Main:Hard"),
            "Abyss".L10N("UI:Main:Abyss")
        };

        private static string[] DifficultyIniPaths = new string[]
        {
            "INI/Map Code/Difficulty Easy.ini",
            "INI/Map Code/Difficulty Medium.ini",
            "INI/Map Code/Difficulty Hard.ini",
            "INI/Map Code/Difficulty Hell.ini"
        };

        protected int StartMusicIndex { get; set; }
        protected int ConflictMusicIndex { get; set; }

        private int FakeDifficultyLevel;

        public CampaignSelector(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.discordHandler = discordHandler;
        }

        private DiscordHandler discordHandler;

        static private readonly string[] filesHashArray =
        {
            "DAD4D370FCF48F08809058CB5300843442348B31", // Hell Settings
            "2D22C883B0C6FEB012811FA2D45A63127F7798F5", // Hard Settings
            "D63191F931C1FEA555E43B13131F2DFB6DE4C9FB", // Normal Settings
            "A0A68A2C59FE030DE427E701EC5811812DCA1A03", // Easy Settings
            "C71C34380B7AEC596B8EED889538C50B07FE6EED", // GlobalCode

            "8AAD50B140D2DDD4E1E49CA64492C3E4A7C0F6C3", // Train 01
            "tttttttttttttttttttttttttttttt", // GDO 01
            "tttttttttttttttttttttttttttttt", // GDO 02
            "tttttttttttttttttttttttttttttt", // GDO 03
            "tttttttttttttttttttttttttttttt", // GDO 04
            "tttttttttttttttttttttttttttttt", // GDO 05
            "tttttttttttttttttttttttttttttt", // GDO 06
            "tttttttttttttttttttttttttttttt", // GDO 07
            "tttttttttttttttttttttttttttttt", // GDO 08
            "tttttttttttttttttttttttttttttt", // GDO 09
            "tttttttttttttttttttttttttttttt", // GDO 10
            "tttttttttttttttttttttttttttttt", // GDO 11
            "tttttttttttttttttttttttttttttt", // GDO 12

            "115BF7C325662D2FAE9AAFFEF07B48FA2B0683BE", // GDI 01
            "4D9F17E4D3FE212469EF30681AED22B0A55D4CEA", // GDI 02
            "002FCB20E2E5A7B131B51835AE7CC8EA024B1AF2", // GDI 03
            "459E56532C271317C154C8FF1E8311465AACDE29", // GDI 04
            "B6DACE5C38801741DCD7419921F6B2531C1D487C", // GDI 05
            "A6C243453D43636C390388A62FB3A98ABD82195E", // GDI 06
            "E508FBB7838E39EC36DC490AF58105E9FF3D4F49", // GDI 07
            "34902BA603F3CE50D52FEEB0DB5465E788A81D9E", // GDI 08

            "0B14E96F9F38989EB804B123287961D162860710", // End 01
            "61FF1C4549223DA96ABBD4FD53CB5493C42061A8", // End 02
            "C0E64B2AF809E2B9ECDDBF886267E6C3B037E443", // End 03
            "F4A6A7BA08D3EE5F8DEB412F80DCD94416C270D0", // End 04
            "A8BC9E677A904434E966C6E60F44129EE4A59343", // End 05
            "2C888AC8ADDF6A5BD7D4CA7DCF11A32301914B53", // End 06
            "2C51E3829DCCBDF1BF48102AED65F641E6F7F215", // End 07
            "01694E76E5D2FDC284EB62C1FAE81A909E746129", // End 08
            "E6F96C43C5FF8870FB7057764DADCC99F4113F14", // End 08 Off
            "E49D70719290B484DE90B92EB9C2377F69D7849B", // End 08 Def
            "92620A88A5D197223254881B1F836ED282AC23CD", // End 08 Sup

            "B1CBE8F768F06E9A23F39637612DD2C2CAA78187", // Nod 01
            "33325CC089E85DAA9267D022506CC9A4C836CC9C", // Nod 02
            "387299AAC98EAE5D977BAF81CFD8B56E4FBEC9FF", // Nod 03
            "EA3EF0BEF7CAF5C0386C2ABB003E7D35FDF29F26", // Nod 04
            "00AE5B459D5159A472ED35A387F0BA132B7B0703", // Nod 05
            "0E67F4A3299E7BE88B94CD8F00168C8AEC504D94", // Nod 06
            "B04CE4D9C3C062CC0C16A93FAEBCD7985E06B0D0", // Nod 07
            "237717F0E6D3D4142D9170F61CD4FA14661DABAA", // Nod 08

            "F14CDD8372547122041C73BF81FB0F07E361003E", // Scrin 01
            "8624FDCE7BC10108761F660F46F7BEDB7EE8FB1B", // Scrin 02
            "6962F17CE16670482D5A1EBB0C866CBF9E528848", // Scrin 03
            "301488643E12C2B3203136B1FD2A1860D0B4E136"  // Scrin 04
        };

        static private readonly string[] filesToCheck =
        {
            "INI/Map Code/Difficulty Hell.ini",
            "INI/Map Code/Difficulty Hard.ini",
            "INI/Map Code/Difficulty Normal.ini",
            "INI/Map Code/Difficulty Easy.ini",
            "INI/Map Code/GlobalCode.ini",

            "MapsTC/Missions/tra01.map",
            "MapsTC/Missions/gdo01.map",
            "MapsTC/Missions/gdo02.map",
            "MapsTC/Missions/gdo03.map",
            "MapsTC/Missions/gdo04.map",
            "MapsTC/Missions/gdo05.map",
            "MapsTC/Missions/gdo06.map",
            "MapsTC/Missions/gdo07.map",
            "MapsTC/Missions/gdo08.map",
            "MapsTC/Missions/gdo09.map",
            "MapsTC/Missions/gdo10.map",
            "MapsTC/Missions/gdo11.map",
            "MapsTC/Missions/gdo12.map",

            "MapsTC/Missions/gdi01.map",
            "MapsTC/Missions/gdi02.map",
            "MapsTC/Missions/gdi03.map",
            "MapsTC/Missions/gdi04.map",
            "MapsTC/Missions/gdi05.map",
            "MapsTC/Missions/gdi06.map",
            "MapsTC/Missions/gdi07.map",
            "MapsTC/Missions/gdi08.map",

            "MapsTC/Missions/end01.map",
            "MapsTC/Missions/end02.map",
            "MapsTC/Missions/end03.map",
            "MapsTC/Missions/end04.map",
            "MapsTC/Missions/end05.map",
            "MapsTC/Missions/end06.map",
            "MapsTC/Missions/end07.map",
            "MapsTC/Missions/end08.map",
            "MapsTC/Missions/end08_off.map",
            "MapsTC/Missions/end08_def.map",
            "MapsTC/Missions/end08_sup.map",

            "MapsTC/Missions/nod01.map",
            "MapsTC/Missions/nod02.map",
            "MapsTC/Missions/nod03.map",
            "MapsTC/Missions/nod04.map",
            "MapsTC/Missions/nod05.map",
            "MapsTC/Missions/nod06.map",
            "MapsTC/Missions/nod07.map",
            "MapsTC/Missions/nod08.map",

            "MapsTC/Missions/scr01.map",
            "MapsTC/Missions/scr02.map",
            "MapsTC/Missions/scr03.map",
            "MapsTC/Missions/scr04.map"
        };

        private readonly string[] campaignList =
        {
            "tra01.map",
            "gdo01.map",
            "gdo02.map",
            "gdo03.map",
            "gdo04.map",
            "gdo05.map",
            "gdo06.map",
            "gdo07.map",
            "gdo08.map",
            "gdo09.map",
            "gdo10.map",
            "gdo11.map",
            "gdo12.map",

            "gdi01.map",
            "gdi02.map",
            "gdi03.map",
            "gdi04.map",
            "gdi05.map",
            "gdi06.map",
            "gdi07.map",
            "gdi08.map",

            "end01.map",
            "end02.map",
            "end03.map",
            "end04.map",
            "end05.map",
            "end06.map",
            "end07.map",
            "end08.map",

            "nod01.map",
            "nod02.map",
            "nod03.map",
            "nod04.map",
            "nod05.map",
            "nod06.map",
            "nod07.map",
            "nod08.map",

            "scr01.map",
            "scr02.map",
            "scr03.map",
            "scr04.map"
        };

        private readonly string[] LightNameArray =
        {
            "Red",
            "Blue",
            "Green",
            "Level",
            "Ground",
            "Ambient"
        };

        private List<Mission> Missions = new List<Mission>();
        private XNAListBox lbCampaignList;
        private XNAClientButton btnLaunch;
        private XNATextBlock tbMissionDescription;
        private XNATrackbar trbDifficultySelector;

        private CheaterWindow cheaterWindow;
        private XNAMessageBox TooHardMessageBox;

        private Mission missionToLaunch;

        public override void Initialize()
        {
            BackgroundTexture = AssetLoader.LoadTexture("missionselectorbg.png");
            ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
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

            var btnCancel = new XNAClientButton(WindowManager);
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

            ParseBattleIni("INI/" + ClientConfiguration.Instance.BattleFSFileName);

            cheaterWindow = new CheaterWindow(WindowManager);
            DarkeningPanel dp = new DarkeningPanel(WindowManager);
            dp.AddChild(cheaterWindow);
            AddChild(dp);
            dp.CenterOnParent();
            cheaterWindow.CenterOnParent();
            cheaterWindow.Disable();
        }

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
            Enabled = false;
        }

        private void BtnLaunch_LeftClick(object sender, EventArgs e)
        {
            if (trbDifficultySelector.Value > 2)
            {
                TooHardMessageBox = XNAMessageBox.ShowYesNoDialog(WindowManager,
                    "HELL Difficulty to Start".L10N("UI:Main:HellDifficultyStart"),
                    string.Format("Are you sure to play this mission at HELL difficulty?" + Environment.NewLine +
                    "High difficulty is named Hard.").L10N("UI:Main:HellDifficultyStart_Desc"));
                TooHardMessageBox.YesClickedAction = TooHardMessageBox_YesClicked;
            }
            else
            {
                PrepareToLaunch();
            }
        }

        private void TooHardMessageBox_YesClicked(XNAMessageBox messageBox)
        {
            PrepareToLaunch();
        }

        private void PrepareToLaunch()
        {
            int selectedMissionId = lbCampaignList.SelectedIndex;

            Mission mission = Missions[selectedMissionId];

            if (!ClientConfiguration.Instance.ModMode &&
                (!CUpdater.IsFileNonexistantOrOriginal(mission.Scenario) || AreFilesModified()))
            {
                // Confront the user by showing the cheater screen
                missionToLaunch = mission;
                cheaterWindow.Enable();
                return;
            }

            LaunchMission(mission);
        }

        private bool AreFilesModified()
        {
            foreach (string filePath in filesToCheck)
            {
                if (!CUpdater.IsFileNonexistantOrOriginal(filePath))
                    return true;
            }

            int iCount = 0;
            foreach (string filePath in filesToCheck)
            {
                if (!File.Exists(ProgramConstants.GamePath + filePath))
                {
                    cheaterWindow.SetCantFindText(filePath);
                    return false;
                }

                if (iCount >= filesHashArray.Length || Utilities.CalculateSHA1ForFile(filePath).ToUpper() != filesHashArray[iCount])
                {
                    cheaterWindow.SetDefaultText();
                    Logger.Log("File modified: " + filePath);
                    return false;
                }
                iCount++;
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
        /// Called when the user wants to proceed to the mission despite having
        /// being called a cheater.
        /// </summary>
        private void CheaterWindow_YesClicked(object sender, EventArgs e)
        {
            LaunchMission(missionToLaunch);
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

            IniFile difficultyIni = new IniFile(ProgramConstants.GamePath + DifficultyIniPaths[trbDifficultySelector.Value]);
            string difficultyName = DifficultyNames[trbDifficultySelector.Value];

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
                switch (UserINISettings.Instance.AntiAliasing)
                {
                    case 1:
                        strTechniques += ",SMAA";
                        break;
                    case 2:
                        strTechniques += ",FXAA";
                        break;
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
                if (mission.Scenario.ToUpper() == "END08.MAP")
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
                else
                    mapIni = new IniFile(ProgramConstants.GamePath + "MapsTC/Missions/" + mission.Scenario);

                // Map Settings
                foreach (string strName in LightNameArray)
                {
                    mapIni.SetDoubleValue("Lighting", "Ion" + strName, mapIni.GetDoubleValue("Lighting", strName, 0.35f));
                    mapIni.SetDoubleValue("Lighting", "Dominator" + strName, mapIni.GetDoubleValue("Lighting", strName, 0.35f));
                }
                mapIni.SetStringValue("Basic", "TiberiumDeathToVisceroid", "yes");
                if (!mapIni.GetBooleanValue("SpecialFlags", "FogOfWar", false))
                {
                    mapIni.SetBooleanValue("SpecialFlags", "FogOfWar", true);
                }

                if (CampaignIni.GetBooleanValue("BaseInfo", "Ambient.Wind.Cold", false))
                    mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_Cold");
                else if (CampaignIni.GetBooleanValue("BaseInfo", "Ambient.Wind.MountLow", false))
                    mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_MountLow");
                else if (CampaignIni.GetBooleanValue("BaseInfo", "Ambient.Wind.MountHigh", false))
                    mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "AmbS_Wind_MountHigh");
                else if (CampaignIni.GetBooleanValue("BaseInfo", "Ambient.Wind.Disable", false))
                    mapIni.SetStringValue("AmbSoundWPWH", "AnimList", "NULLQAQ");

                if (!File.Exists(ProgramConstants.GamePath + "tcextrab04.big"))
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

            string strDifficultyName = DifficultyNames[trbDifficultySelector.Value];
            discordHandler?.UpdatePresence(mission.GUIName, strDifficultyName, mission.IconPath, true);

            GameProcessLogic.GameProcessExited += GameProcessExited_Callback;
            GameProcessLogic.StartGameProcess(CampaignIni.GetBooleanValue("BaseInfo", "ControlSpeed", true), true);
        }

        private int GetComputerDifficulty() =>
            Math.Abs(trbDifficultySelector.Value - 2);

        private void GameProcessExited_Callback()
        {
            WindowManager.AddCallback(new Action(GameProcessExited), null);

            foreach (string strMapName in campaignList)
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
        private bool ParseBattleIni(string path)
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

            foreach (string battleEntry in battleKeys)
            {
                string battleSection = battleIni.GetStringValue("Battles", battleEntry, "NOT FOUND");

                if (!battleIni.SectionExists(battleSection))
                    continue;

                var mission = new Mission(battleIni, battleSection);

                // Skip TC2 missions
                if (mission.Hide)
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
