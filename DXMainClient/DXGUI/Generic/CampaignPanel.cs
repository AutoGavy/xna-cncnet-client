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
using System.Linq;
using Localization;

namespace DTAClient.DXGUI.Generic
{
    public class CampaignPanel : XNAWindow
    {
        private const int DEFAULT_WIDTH = 1280;
        private const int DEFAULT_HEIGHT = 768;
        private const int MEDAL_COUNT = 8;
        private const string DEFAULT_POS = "0,0";
        private const string DEFAULT_SIZE = "1,1";
        private const string SPMUSIC_SETTINGS = "Client/MusicSettings.ini";
        private const string SPSOUND_INI = "spsound.ini";
        private const string CREDITS_TXT = "creditstc.txt";
        private const string PROFILE_NAME = "Client/profile_data";
        private const string RESOURCE_PATH = "CampaignRes/";
        private const string MSMEDAL_PREFIX = "MSMEDAL_";

        public event EventHandler WindowExited;
        protected int StartMusicIndex { get; set; }
        protected int ConflictMusicIndex { get; set; }

        public CampaignPanel(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.discordHandler = discordHandler;
        }

        private DiscordHandler discordHandler;

        private readonly string[] DifficultyList =
        {
            "btnEasy",
            "btnNormal",
            "btnHard",
            "btnAbyss"
        };

        private readonly string[] PRLMissionList =
        {
            "prl1",
            "prl2"
        };

        private readonly string[] GDOMissionList =
        {
            "gdo1",
            "gdo2",
            "gdo3",
            "gdo4",
            "gdo5",
            "gdo6",
            "gdo7",
            "gdo8",
            "gdo9",
            "gdo10",
            "gdo11",
            "gdo12"
        };

        private readonly Dictionary<string, string> MedalNameList = new Dictionary<string, string>
        {
            {"none",   "362,355,123,107"},
            {"easy",   "380,356,97,106"},
            {"normal", "377,358,103,104"},
            {"hard",   "380,351,97,111"},
            {"abyss",  "378,350,102,112"},
        };

        private string[] MissionList;

        //private EnhancedSoundEffect ClickSoundLight = new EnhancedSoundEffect("button.wav");
        //private EnhancedSoundEffect ClickSoundHeavy = new EnhancedSoundEffect("checkbox.wav");

        private delegate void gsDelegate(XNAClientButton button);
        private readonly gsDelegate[] SetAsButton = new gsDelegate[4];

        private List<Mission> Missions = new List<Mission>();
        private List<XNAClientButton> DifficultyButtons = new List<XNAClientButton>();
        private List<XNAClientButton> MissionButtons = new List<XNAClientButton>();
        private List<XNAClientButton> UnusedButtons = new List<XNAClientButton>();

        private List<CMedal> Medals = new List<CMedal>(8);
        private CMedal DifficultyMedal;

        private CheaterWindow cheaterWindow;
        private XNAMessageBox TooHardMessageBox;

        private XNAClientButton btnLaunch;
        private XNAClientButton btnCancel;
        private XNAClientButton btnSlideUp;
        private XNAClientButton btnSlideDown;
        private XNAClientButton btnOldCampaign;

        private XNAExtraPanel epBackground;

        private IniFile campaignOptionsIni;

        private string SIDE_ABBR;
        private int curDifficultyIndex;
        private int curMissionIndex;

        public override void Initialize()
        {
            ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            BackgroundTexture = AssetLoader.LoadTexture("empty.png");
            BorderColor = UISettings.ActiveSettings.PanelBorderColor;

            Name = "CampaignPanel";

            SIDE_ABBR = "GDO";
            MissionList = PRLMissionList.Concat(GDOMissionList).ToArray();

            campaignOptionsIni = new IniFile(ProgramConstants.GetBaseResourcePath() + Name + ".ini");
            IniFile profileIni = new IniFile(ProgramConstants.GamePath + PROFILE_NAME);

            SetAsButton[0] = new gsDelegate(SetAsButton1);
            SetAsButton[1] = new gsDelegate(SetAsButton2);
            SetAsButton[2] = new gsDelegate(SetAsButton3);
            SetAsButton[3] = new gsDelegate(SetAsButton4);

            epBackground = new XNAExtraPanel(WindowManager);
            epBackground.Name = "epBackground";
            epBackground.ClientRectangle = new Rectangle(0, 0, 1920, 1080);
            epBackground.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            epBackground.DrawBorders = false;
            AddChild(epBackground);

            // Difficulty buttons
            foreach (string difficultyName in DifficultyList)
            {
                string[] posArray = campaignOptionsIni.GetStringValue(difficultyName, "Location", DEFAULT_POS).Split(',');
                string[] sizeArray = campaignOptionsIni.GetStringValue(difficultyName, "Size", DEFAULT_SIZE).Split(',');

                XNAClientButton xNAClientButton = new XNAClientButton(WindowManager);
                xNAClientButton.Name = difficultyName;
                xNAClientButton.ClientRectangle = new Rectangle(Convert.ToInt32(posArray[0]), Convert.ToInt32(posArray[1]),
                    Convert.ToInt32(sizeArray[0]), Convert.ToInt32(sizeArray[1]));
                xNAClientButton.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + difficultyName.ToLower() + ".png");
                xNAClientButton.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + difficultyName.ToLower() + "_c.png");
                xNAClientButton.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
                xNAClientButton.Disable();
                xNAClientButton.Visible = false;
                xNAClientButton.LeftClick += DifficultyButton_LeftClick;

                DifficultyButtons.Add(xNAClientButton);
                AddChild(xNAClientButton);
            }

            // Mission buttons
            if (SIDE_ABBR == "GDO")
            {
                for (int i = 1; i <= PRLMissionList.Length; ++i)
                {
                    XNAClientButton xNAClientButton = new XNAClientButton(WindowManager);
                    xNAClientButton.Name = "btnprl" + i;
                    xNAClientButton.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
                    xNAClientButton.Disable();
                    xNAClientButton.Visible = false;
                    xNAClientButton.LeftClick += MissionButton_LeftClick;

                    MissionButtons.Add(xNAClientButton);
                    AddChild(xNAClientButton);
                }
                for (int i = 1; i <= GDOMissionList.Length; ++i)
                {
                    XNAClientButton xNAClientButton = new XNAClientButton(WindowManager);
                    xNAClientButton.Name = "btn" + SIDE_ABBR.ToLower() + i;
                    xNAClientButton.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
                    xNAClientButton.Disable();
                    xNAClientButton.Visible = false;
                    xNAClientButton.LeftClick += MissionButton_LeftClick;

                    MissionButtons.Add(xNAClientButton);
                    AddChild(xNAClientButton);
                }
            }

            // Medals
            for (int i = 0; i < MEDAL_COUNT; i++)
            {
                CMedal cMedal = new CMedal(WindowManager);
                cMedal.Checked = false;
                cMedal.Disable();
                cMedal.Visible = false;

                Medals.Add(cMedal);
                AddChild(cMedal);
            }
            DifficultyMedal = new CMedal(WindowManager);
            DifficultyMedal.Name = "DifficultyMedal";
            DifficultyMedal.Visible = true;
            AddChild(DifficultyMedal);

            btnLaunch = new XNAClientButton(WindowManager);
            btnLaunch.Name = "btnLaunch";
            btnLaunch.ClientRectangle = new Rectangle(85, 655, 429, 45);
            btnLaunch.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "launchbtn.png");
            btnLaunch.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "launchbtn_c.png");
            btnLaunch.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnLaunch.SetBorderWidth(101);
            btnLaunch.SetAlphaCheckVal(0);
            btnLaunch.LeftClick += BtnLaunch_LeftClick;

            btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = "btnCancel";
            btnCancel.ClientRectangle = new Rectangle(1184, 725, 66, 32);
            // english:
            // btnCancel.ClientRectangle = new Rectangle(1161, 725, 92, 31);
            btnCancel.IdleTexture = AssetLoader.LoadTexture("Database/backbtn.png");
            btnCancel.HoverTexture = AssetLoader.LoadTexture("Database/backbtn_c.png");
            btnCancel.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnCancel.LeftClick += BtnCancel_LeftClick;

            btnSlideUp = new XNAClientButton(WindowManager);
            btnSlideUp.Name = "btnSlideUp";
            btnSlideUp.ClientRectangle = new Rectangle(892, 51, 313, 39);
            btnSlideUp.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slideupbtn.png");
            btnSlideUp.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slideupbtn_c.png");
            btnSlideUp.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnSlideUp.ClickSoundEffect = new EnhancedSoundEffect("checkbox.wav");
            btnSlideUp.AllowClick = false;
            btnSlideUp.MouseLeftDown += BtnSlideUp_MouseLeftDown;
            btnSlideUp.MouseLeave += BtnSlideUp_MouseLeave;
            btnSlideUp.LeftClick += BtnSlideUp_LeftClick;

            btnSlideDown = new XNAClientButton(WindowManager);
            btnSlideDown.Name = "btnSlideDown";
            btnSlideDown.ClientRectangle = new Rectangle(892, 666, 314, 39);
            btnSlideDown.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slidedownbtn.png");
            btnSlideDown.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slidedownbtn_c.png");
            btnSlideDown.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnSlideDown.ClickSoundEffect = new EnhancedSoundEffect("checkbox.wav");
            btnSlideDown.AllowClick = false;
            btnSlideDown.MouseLeftDown += BtnSlideDown_MouseLeftDown;
            btnSlideDown.MouseLeave += BtnSlideDown_MouseLeave;
            btnSlideDown.LeftClick += BtnSlideDown_LeftClick;

            btnOldCampaign = new XNAClientButton(WindowManager);
            btnOldCampaign.Name = "btnOldCampaign";
            //btnOldCampaign.ClientRectangle = new Rectangle(25, 24, 248, 41);
            btnOldCampaign.ClientRectangle = new Rectangle(65, 60, 248, 41);
            btnOldCampaign.SetBorderHeight(10);
            btnOldCampaign.SetBorderWidth(11);
            btnOldCampaign.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "oldcampaignbtn.png");
            btnOldCampaign.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "oldcampaignbtn_c.png");
            btnOldCampaign.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnOldCampaign.Visible = true;
            if (UserINISettings.Instance.TC2Completed)
            {
                btnOldCampaign.AllowClick = true;
            }
            else
            {
                btnOldCampaign.AllowClick = false;
            }
            btnOldCampaign.LeftClick += BtnOldCampaign_LeftClick;

            AddChild(btnLaunch);
            AddChild(btnCancel);
            AddChild(btnSlideUp);
            AddChild(btnSlideDown);
            AddChild(btnOldCampaign);

            curMissionIndex = UserINISettings.Instance.SelectedMissionIndex;
            curDifficultyIndex = UserINISettings.Instance.FakeDifficulty;

            // Sort button list
            if (curMissionIndex > 3)
            {
                btnSlideUp.AllowClick = true;

                for (int i = 0; i < 4; i++)
                {
                    SetAsButton[i](MissionButtons[curMissionIndex + i - 3]);
                    CheckMission(MissionButtons[curMissionIndex + i - 3]);
                    MissionButtons[curMissionIndex - i].Enable();
                    MissionButtons[curMissionIndex - i].Visible = true;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    SetAsButton[i](MissionButtons[i]);
                    CheckMission(MissionButtons[i]);
                }
            }

            SwitchMission(curMissionIndex, -1, true);
            SwitchDifficulty(curDifficultyIndex, -1, true);

            if (curMissionIndex < 8 && profileIni.GetBooleanValue("General", MissionList[4].ToUpper(), false))
                btnSlideDown.AllowClick = true;

            base.Initialize();

            epBackground.CenterOnParent();
            CenterOnParent();

            ParseBattleIni("INI/" + ClientConfiguration.Instance.BattleFSFileName);

            cheaterWindow = new CheaterWindow(WindowManager);

            DarkeningPanel dp = new DarkeningPanel(WindowManager);
            dp.AddChild(cheaterWindow);

            AddChild(dp);
            dp.CenterOnParent();

            cheaterWindow.CenterOnParent();
            cheaterWindow.Disable();
        }

        public void UpdateMissionButtons()
        {
            IniFile profileIni = new IniFile(ProgramConstants.GamePath + PROFILE_NAME);

            if (profileIni.GetBooleanValue("General", "GDO3", false))
            {
                if (curMissionIndex == 3)
                {
                    btnSlideDown.AllowClick = true;
                }
            }

            if (profileIni.GetBooleanValue("General", "GDO5", false))
            {
                if (curMissionIndex == 5)
                {
                    btnSlideDown.AllowClick = true;
                }
            }
        }

        public void UpdateMissionMedals()
        {
            IniFile profileIni = new IniFile(ProgramConstants.GamePath + PROFILE_NAME);

            string missionName = MissionList[curMissionIndex].ToString().ToUpper();

            // 8 Medals
            if (!string.IsNullOrEmpty(campaignOptionsIni.GetStringValue(missionName, "Medals", String.Empty)))
            {
                foreach (CMedal medal in Medals)
                {
                    string optionName = missionName + "_" + medal.Name;

                    medal.SetDisplayText(profileIni.GetBooleanValue(optionName, "Enable", false)
                        ? medal.unlockedText
                        : medal.lockedText);

                    medal.DisabledClearTexture = profileIni.GetBooleanValue(optionName, "Enable", false) ?
                        AssetLoader.LoadTexture(RESOURCE_PATH + missionName + "/" + medal.Name + "_c.png") :
                        AssetLoader.LoadTexture(RESOURCE_PATH + missionName + "/" + medal.Name + ".png");
                }
            }

            // Difficulty Medal
            string medalName = profileIni.GetStringValue(missionName, "DifficultyMedal", "none");
            if (!MedalNameList.ContainsKey(medalName))
                medalName = "none";

            DifficultyMedal.SetDisplayText(campaignOptionsIni.GetStringValue(MSMEDAL_PREFIX + medalName.ToUpper(), "Text", String.Empty));
            DifficultyMedal.DisabledClearTexture = AssetLoader.LoadTexture(RESOURCE_PATH + SIDE_ABBR.ToLower() + medalName + ".png");
            DifficultyMedal.Enable();
        }

        private void GetMissionMedals(int index)
        {
            IniFile profileIni = new IniFile(ProgramConstants.GamePath + PROFILE_NAME);

            string missionName = MissionList[index].ToUpper();

            // Difficulty Medal
            string medalName = profileIni.GetStringValue(missionName, "DifficultyMedal", "none");
            if (!MedalNameList.ContainsKey(medalName))
                medalName = "none";

            string[] rectArray = MedalNameList[medalName].Split(',');
            DifficultyMedal.ClientRectangle = new Rectangle(Convert.ToInt32(rectArray[0]), Convert.ToInt32(rectArray[1]),
                Convert.ToInt32(rectArray[2]), Convert.ToInt32(rectArray[3]));

            DifficultyMedal.SetDisplayText(campaignOptionsIni.GetStringValue(MSMEDAL_PREFIX + medalName.ToUpper(), "Text", String.Empty));
            DifficultyMedal.DisabledClearTexture = AssetLoader.LoadTexture(RESOURCE_PATH + SIDE_ABBR.ToLower() + medalName + ".png");
            DifficultyMedal.RefreshSize();
            DifficultyMedal.Enable();

            // Disable all medals
            foreach (CMedal medal in Medals)
            {
                medal.Checked = false;
                medal.Disable();
                medal.Visible = false;
            }

            // Check if this mission has 8 medals
            string[] medalArray = campaignOptionsIni.GetStringValue(missionName, "Medals", String.Empty).Split(',');
            if (medalArray.Length != MEDAL_COUNT)
            {
                Logger.Log("Error: Mission " + missionName + " does not reach 8 medals!");
                return;
            }

            // Init medals
            for (int i = 0; i < medalArray.Length; i++)
            {
                string optionName = missionName + "_" + medalArray[i];
                string[] posArray = campaignOptionsIni.GetStringValue(optionName, "Location", DEFAULT_POS).Split(',');
                string[] sizeArray = campaignOptionsIni.GetStringValue(optionName, "Size", DEFAULT_SIZE).Split(',');

                Medals[i].Name = medalArray[i];
                Medals[i].ClientRectangle = new Rectangle(Convert.ToInt32(posArray[0]), Convert.ToInt32(posArray[1]),
                    Convert.ToInt32(sizeArray[0]), Convert.ToInt32(sizeArray[1]));

                Medals[i].lockedText = campaignOptionsIni.GetStringValue(optionName, "TipLocked", String.Empty);
                Medals[i].unlockedText = campaignOptionsIni.GetStringValue(optionName, "TipUnlocked", String.Empty);
                Medals[i].SetDisplayText(profileIni.GetBooleanValue(optionName, "Enable", false)
                    ? Medals[i].unlockedText
                    : Medals[i].lockedText);

                Medals[i].DisabledClearTexture = profileIni.GetBooleanValue(optionName, "Enable", false)
                    ? AssetLoader.LoadTexture(RESOURCE_PATH + missionName + "/" + medalArray[i] + "_c.png")
                    : AssetLoader.LoadTexture(RESOURCE_PATH + missionName + "/" + medalArray[i] + ".png");

                Medals[i].RefreshSize();
                Medals[i].Enable();
                Medals[i].Visible = true;
            }
        }

        private void SwitchDifficulty(int index, int currentIndex = -1, bool firstRun = false)
        {
            if (currentIndex >= 0)
            {
                DifficultyButtons[currentIndex].Disable();
                DifficultyButtons[currentIndex].Visible = false;
                DifficultyButtons[currentIndex].OnMouseLeave();
            }

            if (index >= DifficultyList.Length)
            {
                index = 0;
            }

            if (firstRun)
            {
                curDifficultyIndex = UserINISettings.Instance.FakeDifficulty.Value;
                index = curDifficultyIndex;
            }
            else
            {
                UserINISettings.Instance.FakeDifficulty.Value = index;
                curDifficultyIndex = index;
            }

            if (!firstRun)
                DifficultyButtons[index].OnMouseEnter();

            DifficultyButtons[index].Enable();
            DifficultyButtons[index].Visible = true;
        }

        private void SwitchMission(int index, int currentIndex = -1, bool firstRun = false)
        {
            if (currentIndex >= 0)
            {
                MissionButtons[currentIndex].bButtonOn = false;
                MissionButtons[currentIndex].AllowClick = true;
                //MissionButtons[currentIndex].OnMouseLeave();

                if (MissionButtons[currentIndex].Tag != null)
                    MissionButtons[currentIndex].IdleTexture = AssetLoader.LoadTexture(
                        RESOURCE_PATH + MissionButtons[currentIndex].Name + MissionButtons[currentIndex].Tag.ToString() + ".png");
            }

            if (index >= MissionList.Length)
                index = 0;

            if (firstRun)
            {
                curMissionIndex = UserINISettings.Instance.SelectedMissionIndex.Value;
                index = curMissionIndex;
            }
            else
            {
                UserINISettings.Instance.SelectedMissionIndex.Value = index;
                curMissionIndex = index;
            }

            //MissionButtons[index].OnMouseEnter();
            MissionButtons[index].bButtonOn = true;
            MissionButtons[index].bButtonHover = false;
            MissionButtons[index].AllowClick = false;
            MissionButtons[index].IdleTexture = AssetLoader.LoadTexture(
                RESOURCE_PATH + MissionButtons[index].Name + MissionButtons[index].Tag.ToString() + "_c.png");
            epBackground.BackgroundTexture = AssetLoader.LoadTexture(RESOURCE_PATH + MissionList[index] + "bg.png");

            btnLaunch.AllowClick = currentIndex > 0 ?
                campaignOptionsIni.GetBooleanValue(MissionList[currentIndex - 1].ToUpper(), "Enable", true) : true;

            GetMissionMedals(curMissionIndex);
        }

        private void BtnSlideUp_MouseLeftDown(object sender, EventArgs e)
        {
            btnSlideUp.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slideupbtn.png");
        }

        private void BtnSlideDown_MouseLeftDown(object sender, EventArgs e)
        {
            btnSlideDown.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slidedownbtn.png");
        }

        private void BtnSlideUp_MouseLeave(object sender, EventArgs e)
        {
            btnSlideUp.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slideupbtn_c.png");
        }

        private void BtnSlideDown_MouseLeave(object sender, EventArgs e)
        {
            btnSlideDown.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slidedownbtn_c.png");
        }

        private void BtnSlideUp_LeftClick(object sender, EventArgs e)
        {
            btnSlideUp.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slideupbtn_c.png");

            foreach (XNAClientButton button in MissionButtons)
            {
                if (button.Tag != null && button.Tag.ToString() == "a")
                {
                    int topIndex = Convert.ToInt32(button.Name.Substring(6)) - 1;
                    if (button.Name != "btnprl1" && button.Name != "btnprl2")
                        topIndex += 2;

                    var readyDeleteButtons = new List<XNAClientButton>();
                    for (int i = 0; i < 4; ++i)
                    {
                        int listIndex = topIndex + i - 1;
                        if (listIndex >= MissionButtons.Count())
                            break;

                        SetAsButton[i](MissionButtons[listIndex]);
                        CheckMission(MissionButtons[listIndex]);

                        readyDeleteButtons.Add(MissionButtons[listIndex]);
                    }
                    UnusedButtons = MissionButtons.Except(readyDeleteButtons).ToList();

                    int checkIndex = topIndex + 3;
                    if (checkIndex < MissionButtons.Count() && CheckMission(MissionButtons[checkIndex]))
                        btnSlideDown.AllowClick = true;

                    ClearUnusedButtonsTag();
                    break;
                }
            }

            if (MissionButtons[0].Tag != null && MissionButtons[0].Tag.ToString() == "a")
                btnSlideUp.AllowClick = false;
        }

        private void BtnSlideDown_LeftClick(object sender, EventArgs e)
        {
            btnSlideDown.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slidedownbtn_c.png");

            foreach (XNAClientButton button in MissionButtons)
            {
                if (button.Tag != null && button.Tag.ToString() == "d")
                {
                    int bottomIndex = Convert.ToInt32(button.Name.Substring(6)) - 1;
                    if (button.Name != "btnprl1" && button.Name != "btnprl2")
                        bottomIndex += 2;

                    var readyDeleteButtons = new List<XNAClientButton>();
                    for (int i = 0; i < 4; ++i)
                    {
                        int listIndex = bottomIndex + i - 2;
                        if (listIndex >= MissionButtons.Count())
                            break;

                        SetAsButton[i](MissionButtons[listIndex]);
                        CheckMission(MissionButtons[listIndex]);

                        readyDeleteButtons.Add(MissionButtons[listIndex]);
                    }
                    UnusedButtons = MissionButtons.Except(readyDeleteButtons).ToList();

                    int checkIndex = bottomIndex + 2;
                    if (checkIndex < MissionButtons.Count() && !CheckMission(MissionButtons[checkIndex]))
                        btnSlideDown.AllowClick = false;

                    ClearUnusedButtonsTag();
                    break;
                }
            }

            if (MissionButtons[0].Tag == null || MissionButtons[0].Tag.ToString() != "a")
                btnSlideUp.AllowClick = true;
        }

        private void DifficultyButton_LeftClick(object sender, EventArgs e)
        {
            //ClickSoundLight.Play();
            SwitchDifficulty(curDifficultyIndex + 1, curDifficultyIndex);
        }

        private void MissionButton_LeftClick(object sender, EventArgs e)
        {
            foreach (XNAClientButton button in MissionButtons)
            {
                if (button.bButtonHover)
                {
                    //ClickSoundHeavy.Play();

                    int oriIndex = Convert.ToInt32(button.Name.Substring(6));
                    if (button.Name != "btnprl1" && button.Name != "btnprl2")
                    {
                        oriIndex += 2;
                    }
                    int btnIndex = oriIndex - 1;

                    SwitchMission(btnIndex, curMissionIndex);
                    return;
                }
            }
            Logger.Log("Error: Could not find hovered mission button.");
        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            // 退内层菜单
            WindowExited?.Invoke(this, EventArgs.Empty);
        }

        private void BtnLaunch_LeftClick(object sender, EventArgs e)
        {
            string missionName = MissionList[curMissionIndex].ToUpper();
            if (!campaignOptionsIni.GetBooleanValue(missionName, "IsValid", true))
            {
                XNAMessageBox.Show(WindowManager,
                    "Cannot Start Mission".L10N("UI:Main:CantStartMission"),
                    string.Format("Mission is not completed yet,\nplease select next mission.".L10N("UI:Main:InvalidMission")));
            }
            else if (UserINISettings.Instance.TooHardHint)
            {
                TooHardMessageBox = XNAMessageBox.ShowYesNoDialog(WindowManager,
                    "Start With This Difficulty".L10N("UI:Main:StartWithThisDiff"),
                    string.Format("Are you sure to start with this difficulty?\nIf you've played Command & Conquer before, you can start on normal difficulty.\nSave / Load currently is not available, please read game text and dialogue at any difficulty,\nand be careful, otherwise you may lose in a unexpected plot.\n*Abyss difficulty is a hardcore plot background mode!! This difficulty is not recommended for the first time to play!".L10N("UI:Main:StartWithThisDiffDesc").Replace("@", Environment.NewLine)));
                TooHardMessageBox.YesClickedAction = TooHardMessageBox_YesClicked;
                UserINISettings.Instance.TooHardHint.Value = false;
            }
            else
            {
                //ClickSoundLight.Play();
                PrepareToLaunch();
            }
        }

        private void BtnOldCampaign_LeftClick(object sender, EventArgs e)
        {
            //ClickSoundLight.Play();
            MainMenuDarkeningPanel parent = (MainMenuDarkeningPanel)Parent;
            parent.CampaignSelector.ReloadBattleIni(false);
            parent.ShowSubControl(parent.CampaignSelector);
        }

        private void TooHardMessageBox_YesClicked(XNAMessageBox messageBox)
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

            Mission mission = Missions[curMissionIndex + 3];

            // New Campaign loading screen
            string loadingFilename = String.Empty;

            foreach (int resolution in MainClientConstants.ResolutionList)
            {
                if (UserINISettings.Instance.IngameScreenWidth >= resolution)
                {
                    loadingFilename += resolution + "campload" + MissionList[curMissionIndex] + ".big";
                    break;
                }
            }

            if (String.IsNullOrEmpty(loadingFilename))
                loadingFilename += "1024skirmishloads" + MissionList[curMissionIndex] + ".big";

            string bigPath = ProgramConstants.GetBaseSharedPath() + loadingFilename;
            if (File.Exists(bigPath))
                File.Copy(bigPath, ProgramConstants.GamePath + "tcextrab15.big", true);
            else
                Logger.Log("Cloud not find campaign loading screen file: " + bigPath);

            // main launch
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

            IniFile difficultyIni;
            IniFile globalCodeIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/GlobalCode.ini");

            if (curDifficultyIndex == 0) // Easy
            {
                swriter.WriteLine("DifficultyModeHuman=0");
                swriter.WriteLine("DifficultyModeComputer=2");
                difficultyIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/Difficulty Easy.ini");
            }
            else if (curDifficultyIndex == 1) // Normal
            {
                swriter.WriteLine("DifficultyModeHuman=0");
                swriter.WriteLine("DifficultyModeComputer=2");
                difficultyIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/Difficulty Normal.ini");
            }
            else if (curDifficultyIndex == 2) // Hard
            {
                swriter.WriteLine("DifficultyModeHuman=1");
                swriter.WriteLine("DifficultyModeComputer=1");
                difficultyIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/Difficulty Hard.ini");
            }
            else // Abyss
            {
                swriter.WriteLine("DifficultyModeHuman=2");
                swriter.WriteLine("DifficultyModeComputer=0");
                difficultyIni = new IniFile(ProgramConstants.GamePath + "INI/Map Code/Difficulty Hell.ini");
            }
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
                    if (CampaignIni.GetBooleanValue("BaseInfo", "IsSnowNight", false)) // Snow Night
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
                    else // Snow Day
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

            if (ClientConfiguration.Instance.ModMode && copyMapsToSpawnmapINI)
            {
                IniFile mapIni;
                mapIni = new IniFile(ProgramConstants.GamePath + "MapsTC/Missions/" + mission.Scenario);

                mapIni.WriteIniFile(ProgramConstants.GamePath + mission.Scenario.ToLower());
            }

            File.Delete(ProgramConstants.GamePath + CREDITS_TXT);

            if (curDifficultyIndex <= 0)
                UserINISettings.Instance.Difficulty.Value = 0;
            else
                UserINISettings.Instance.Difficulty.Value = curDifficultyIndex - 1;
            UserINISettings.Instance.SaveSettings();

            //((MainMenuDarkeningPanel)Parent).Hide();

            string strDifficultyName = InfoShared.DifficultyNames[curDifficultyIndex];
            discordHandler?.UpdatePresence(mission.GUIName, strDifficultyName, mission.IconPath, true);

            GameProcessLogic.GameProcessExited += GameProcessExited_Callback;
            GameProcessLogic.StartGameProcess(CampaignIni.GetBooleanValue("BaseInfo", "ControlSpeed", true), true);
        }

        private void GameProcessExited_Callback()
        {
            WindowManager.AddCallback(new Action(GameProcessExited), null);

            foreach (string strMapName in InfoShared.campaignList)
                File.Delete(ProgramConstants.GamePath + strMapName);
        }

        protected virtual void GameProcessExited()
        {
            GameProcessLogic.GameProcessExited -= GameProcessExited_Callback;
            LogbuchParser.ParseForCampaign(null, null, curDifficultyIndex);
            LogbuchParser.ClearTrash();
            UpdateMissionMedals();
            UpdateMissionButtons();
            if (UserINISettings.Instance.TC2Completed)
            {
                btnOldCampaign.AllowClick = true;
            }
            discordHandler?.UpdatePresence();
        }

        private bool ParseBattleIni(string path)
        {
            Logger.Log("Attempting to parse " + path + " to populate mission list.");

            string battleIniPath = ProgramConstants.GamePath + path;
            if (!File.Exists(battleIniPath))
            {
                Logger.Log("File " + path + " not found. Ignoring.");
                return false;
            }

            IniFile battle_ini = new IniFile(battleIniPath);

            List<string> battleKeys = battle_ini.GetSectionKeys("Battles");

            if (battleKeys == null)
                return false; // File exists but [Battles] doesn't

            foreach (string battleEntry in battleKeys)
            {
                string battleSection = battle_ini.GetStringValue("Battles", battleEntry, "NOT FOUND");

                if (!battle_ini.SectionExists(battleSection))
                    continue;

                var mission = new Mission(battle_ini, battleSection);

                Missions.Add(mission);
            }

            Logger.Log("Finished parsing " + path + ".");
            return true;
        }

        private bool CheckMission(XNAClientButton button)
        {
            IniFile profileIni = new IniFile(ProgramConstants.GamePath + PROFILE_NAME);
            if (button.Name == "btnprl1" || profileIni.GetBooleanValue("General", button.Name.Substring(3).ToUpper(), false))
            {
                button.Enable();
                button.Visible = true;
            }
            else
            {
                button.Disable();
                button.Visible = false;
            }
            return button.Visible;
        }

        private void ClearUnusedButtonsTag()
        {
            foreach (var unusedButton in UnusedButtons)
            {
                unusedButton.Tag = null;
                unusedButton.Disable();
                unusedButton.AllowClick = false;
            }
            UnusedButtons.Clear();
        }

        private void SetAsButton1(XNAClientButton button)
        {
            button.Tag = "a";
            button.X = 894;
            button.Y = 89;
            button.Width = 308;
            button.Height = 133;

            //button.SetAlphaCheckVal(30);
            _SetUpButton(button);
        }

        private void SetAsButton2(XNAClientButton button)
        {
            button.Tag = "b";
            button.X = 893;
            button.Y = 240;
            button.Width = 306;
            button.Height = 127;

            _SetUpButton(button);
        }

        private void SetAsButton3(XNAClientButton button)
        {
            button.Tag = "c";
            button.X = 893;
            button.Y = 388;
            button.Width = 306;
            button.Height = 128;

            _SetUpButton(button);
        }

        private void SetAsButton4(XNAClientButton button)
        {
            button.Tag = "d";
            button.X = 894;
            button.Y = 534;
            button.Width = 310;
            button.Height = 134;
            _SetUpButton(button);
        }

        private void _SetUpButton(XNAClientButton button)
        {
            string strPath = RESOURCE_PATH + button.Name + button.Tag.ToString();
            if (button.bButtonOn)
            {
                button.IdleTexture = AssetLoader.LoadTexture(strPath + "_c.png");
                button.HoverTexture = AssetLoader.LoadTexture(strPath + "_c.png");
            }
            else
            {
                button.IdleTexture = AssetLoader.LoadTexture(strPath + ".png");
                button.HoverTexture = AssetLoader.LoadTexture(strPath + "_c.png");
                button.AllowClick = true;
            }
            button.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            button.RefreshSize();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
