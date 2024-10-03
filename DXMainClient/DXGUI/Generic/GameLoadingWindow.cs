using ClientCore;
using ClientGUI;
using DTAClient.Domain;
using Localization;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DTAClient.DXGUI.Generic
{
    /// <summary>
    /// A window for loading saved singleplayer games.
    /// </summary>
    public class GameLoadingWindow : XNAWindow
    {
        private const string SAVED_GAMES_DIRECTORY = "Saved Games";
        private const string SPMUSIC_SETTINGS = "Client/MusicSettings.ini";
        private const string SPSOUND_INI = "spsound.ini";
        private const string CREDITS_TXT = "creditstc.txt";
        private string loadedSide = null;
        private string loadedMisson = null;
        private bool IsCampaign = false;

        protected int StartMusicIndex { get; set; }
        protected int ConflictMusicIndex { get; set; }

        public GameLoadingWindow(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.discordHandler = discordHandler;
        }

        private DiscordHandler discordHandler;

        private XNAMultiColumnListBox lbSaveGameList;
        private XNAClientButton btnLaunch;
        private XNAClientButton btnDelete;
        private XNAClientButton btnCancel;

        private List<SavedGame> savedGames = new List<SavedGame>();

        public event EventHandler WindowExited;

        public override void Initialize()
        {
            Name = "GameLoadingWindow";
            BackgroundTexture = AssetLoader.LoadTexture("loadmissionbg.png");

            ClientRectangle = new Rectangle(0, 0, 1100, 618);
            CenterOnParent();

            lbSaveGameList = new XNAMultiColumnListBox(WindowManager);
            lbSaveGameList.Name = nameof(lbSaveGameList);
            lbSaveGameList.ClientRectangle = new Rectangle(13, 13, 1075, 555);
            lbSaveGameList.AddColumn("SAVED GAME NAME".L10N("UI:Main:SavedGameNameColumnHeader"), 400);
            lbSaveGameList.AddColumn("DATE / TIME".L10N("UI:Main:SavedGameDateTimeColumnHeader"), 174);
            //lbSaveGameList.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 1, 1);
            lbSaveGameList.BackgroundTexture = AssetLoader.LoadTexture("generalbglight.png");
            lbSaveGameList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbSaveGameList.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            lbSaveGameList.AllowKeyboardInput = true;

            btnLaunch = new XNAClientButton(WindowManager);
            btnLaunch.Name = nameof(btnLaunch);
            btnLaunch.ClientRectangle = new Rectangle(13, 582, 133, 23);
            btnLaunch.Text = "Load".L10N("UI:Main:ButtonLoad");
            btnLaunch.AllowClick = false;
            btnLaunch.LeftClick += BtnLaunch_LeftClick;

            btnDelete = new XNAClientButton(WindowManager);
            btnDelete.Name = nameof(btnDelete);
            btnDelete.ClientRectangle = new Rectangle(304, btnLaunch.ClientRectangle.Y, 133, 23);
            btnDelete.Text = "Delete".L10N("UI:Main:ButtonDelete");
            btnDelete.AllowClick = false;
            btnDelete.LeftClick += BtnDelete_LeftClick;

            btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = nameof(btnCancel);
            btnCancel.ClientRectangle = new Rectangle(304, btnLaunch.ClientRectangle.Y, 133, 23);
            btnCancel.Text = "Cancel".L10N("UI:Main:ButtonCancel");
            btnCancel.LeftClick += BtnCancel_LeftClick;

            AddChild(lbSaveGameList);
            AddChild(btnLaunch);
            AddChild(btnDelete);
            AddChild(btnCancel);

            base.Initialize();

            ListSaves();
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbSaveGameList.SelectedIndex == -1)
            {
                btnLaunch.AllowClick = false;
                btnDelete.AllowClick = false;
            }
            else
            {
                btnLaunch.AllowClick = true;
                btnDelete.AllowClick = true;
            }
        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            //Enabled = false;
            WindowExited?.Invoke(this, EventArgs.Empty);
        }

        private void BtnDelete_LeftClick(object sender, EventArgs e)
        {
            SavedGame sg = savedGames[lbSaveGameList.SelectedIndex];
            var msgBox = new XNAMessageBox(WindowManager, "Delete Confirmation".L10N("UI:Main:DeleteConfirmationTitle"),
                string.Format("The following saved game will be deleted permanently:".L10N("UI:Main:DeleteTitle") + Environment.NewLine +
                    Environment.NewLine +
                    "Filename".L10N("UI:Main:Filename") + "£º {0}" + Environment.NewLine +
                    "Saved game name".L10N("UI:Main:SavedGameName") + "£º {1}" + Environment.NewLine +
                    "Date and time".L10N("UI:Main:DateAndTime") + ": {2}" + Environment.NewLine +
                    Environment.NewLine +
                    "Are you sure you want to proceed?".L10N("UI:Main:DeleteConfirmationText"),
                    sg.FileName, Renderer.GetSafeString(sg.GUIName, lbSaveGameList.FontIndex), sg.LastModified.ToString()),
                XNAMessageBoxButtons.YesNo);
            msgBox.Show();
            msgBox.YesClickedAction = DeleteMsgBox_YesClicked;
        }

        private void DeleteMsgBox_YesClicked(XNAMessageBox obj)
        {
            SavedGame sg = savedGames[lbSaveGameList.SelectedIndex];

            Logger.Log("Deleting saved game " + sg.FileName);
            File.Delete(ProgramConstants.GamePath + SAVED_GAMES_DIRECTORY + Path.DirectorySeparatorChar + sg.FileName);
            ListSaves();
        }

        private string GetPlayerMusicSide(SavedGame sg)
        {
            if (sg.ParseMissionName() || sg.ParseSideName())
            {
                loadedSide = sg.SideName;
                loadedMisson = sg.MissionName;
                return sg.SideName;
            }

            Random random = new Random();
            if (Convert.ToBoolean(random.Next(0, 2)))
                return "GDI";
            else
                return "Nod";
        }

        private void BtnLaunch_LeftClick(object sender, EventArgs e)
        {
            //XNAMessageBox.Show(WindowManager, "Save / Load Unavailable".L10N("UI:Main:SLUnavailable"), "This feature is working in progress.".L10N("UI:Main:LoadGameUncompleted"));
            //return;

            SavedGame sg = savedGames[lbSaveGameList.SelectedIndex];
            Logger.Log("Loading saved game " + sg.FileName);

            File.Delete(ProgramConstants.GamePath + ProgramConstants.SPAWNER_SETTINGS);
            StreamWriter sw = new StreamWriter(ProgramConstants.GamePath + ProgramConstants.SPAWNER_SETTINGS);
            sw.WriteLine("; generated by DTA Client");
            sw.WriteLine("[Settings]");
            sw.WriteLine("Scenario=spawnmap.ini");
            sw.WriteLine("SaveGameName=" + sg.FileName);
            sw.WriteLine("LoadSaveGame=Yes");
            sw.WriteLine("SidebarHack=" + ClientConfiguration.Instance.SidebarHack);
            sw.WriteLine("CustomLoadScreen=" + LoadingScreenController.GetLoadScreenName("g"));
            sw.WriteLine("Firestorm=No");
            sw.WriteLine("GameSpeed=" + UserINISettings.Instance.GameSpeed);
            sw.WriteLine();
            sw.Close();

            File.Delete(ProgramConstants.GamePath + "spawnmap.ini");
            sw = new StreamWriter(ProgramConstants.GamePath + "spawnmap.ini");
            sw.WriteLine("[Map]");
            sw.WriteLine("Size=0,0,50,50");
            sw.WriteLine("LocalSize=0,0,50,50");
            sw.WriteLine();
            sw.Close();

            discordHandler?.UpdatePresence(sg.GUIName, true);

            // ReShade Settings
            IniFile CampaignIni = null;
            ProgramConstants.SetupPreset();
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

                if (sg.ParseMissionName()) // it is a campaign
                {
                    IsCampaign = true;
                    CampaignIni = new IniFile(ProgramConstants.GamePath + "GameShaders/CampaignINI/" +
                    sg.MissionName + ".ini");

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
                }
                else
                {
                    IsCampaign = false;

                    bool bLightClouds = false;
                    string strCloudsType = sg.LightClouds();

                    if (strCloudsType == "vanilla")
                        bLightClouds = false;
                    else if (strCloudsType == "special")
                        bLightClouds = true;

                    switch (sg.strShader())
                    {
                        case "A_Shader": // Snow Day
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
                            break;

                        case "S_Shader": // Snow Night
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
                            break;

                        case "M_Shader": // Night2
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
                            break;

                        case "N_Shader": // Night
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
                            break;

                        case "D_Shader": // Day
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
                            break;
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
            }

            shaderIniWriter.WriteLine(shaderIniWriter.NewLine);
            shaderIniWriter.Close();

            // Game Music Settings
            IniFile musicListIni = new IniFile(ProgramConstants.GamePath + "INI/MusicListTC.ini");
            IniFile musicConfigIni = new IniFile(ProgramConstants.GamePath + "INI/MusicConfigTC.ini");
            if (UserINISettings.Instance.SmartMusic || UserINISettings.Instance.MusicType < 2)
            {
                IniFile musicSettingsIni = new IniFile(ProgramConstants.GamePath + SPMUSIC_SETTINGS);
                StartMusicIndex = musicSettingsIni.GetIntValue("Settings", "NextStartMusicIndex", 1);
                ConflictMusicIndex = musicSettingsIni.GetIntValue("Settings", "NextConflictMusicIndex", 1);

                Random random = new Random();
                string sideName = GetPlayerMusicSide(sg);

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

                musicSettingsIni.WriteIniFile();
            }
            musicConfigIni.WriteIniFile(ProgramConstants.GamePath + SPSOUND_INI);

            if (CampaignIni != null && CampaignIni.GetBooleanValue("BaseInfo", "MusicFullControl", false))
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

            File.Delete(ProgramConstants.GamePath + CREDITS_TXT);

            Enabled = false;
            GameProcessLogic.GameProcessExited += GameProcessExited_Callback;

            if (CampaignIni == null)
            {
                GameProcessLogic.StartGameProcess();
            }
            else
            {
                GameProcessLogic.StartGameProcess(CampaignIni.GetBooleanValue("BaseInfo", "ControlSpeed", true), true);
            }
        }

        private void GameProcessExited_Callback()
        {
            WindowManager.AddCallback(new Action(GameProcessExited), null);
        }

        protected virtual void GameProcessExited()
        {
            GameProcessLogic.GameProcessExited -= GameProcessExited_Callback;
            if (!String.IsNullOrEmpty(loadedSide))
            {
                if (IsCampaign && !String.IsNullOrEmpty(loadedMisson))
                    LogbuchParser.ParseForCampaign(loadedSide, loadedMisson);
                else
                    LogbuchParser.ParseForLoadedSkirmish(loadedSide);

                LogbuchParser.ClearTrash();
            }
            discordHandler?.UpdatePresence();
        }

        public void ListSaves()
        {
            savedGames.Clear();
            lbSaveGameList.ClearItems();
            lbSaveGameList.SelectedIndex = -1;

            if (!Directory.Exists(ProgramConstants.GamePath + SAVED_GAMES_DIRECTORY))
            {
                Logger.Log("Saved Games directory not found!");
                return;
            }

            string[] files = Directory.GetFiles(ProgramConstants.GamePath +
                SAVED_GAMES_DIRECTORY + Path.DirectorySeparatorChar,
                "*.SAV", SearchOption.TopDirectoryOnly);

            foreach (string file in files)
            {
                ParseSaveGame(file);
            }

            savedGames = savedGames.OrderBy(sg => sg.LastModified.Ticks).ToList();
            savedGames.Reverse();

            foreach (SavedGame sg in savedGames)
            {
                string[] item = new string[] {
                    Renderer.GetSafeString(sg.GUIName, lbSaveGameList.FontIndex),
                    sg.LastModified.ToString() };
                lbSaveGameList.AddItem(item, true);
            }
        }

        private void ParseSaveGame(string fileName)
        {
            string shortName = Path.GetFileName(fileName);

            SavedGame sg = new SavedGame(shortName);
            if (sg.ParseInfo())
                savedGames.Add(sg);
        }
    }
}
