using ClientCore;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using DTAClient.Domain;
using System.IO;
using ClientGUI;
using Rampastring.XNAUI;
using Rampastring.Tools;
using System.Linq;
using Localization;

namespace DTAClient.DXGUI.Generic
{
    public class CampaignPanel : XNAWindow
    {
        private const int DEFAULT_WIDTH = 650;
        private const int DEFAULT_HEIGHT = 600;
        private const int MEDAL_COUNT = 8;
        private const string DEFAULT_POS = "0,0";
        private const string DEFAULT_SIZE = "1,1";
        private const string SPMUSIC_SETTINGS = "Client/MusicSettings.ini";
        private const string SPSOUND_INI = "spsound.ini";
        private const string CREDITS_TXT = "creditstc.txt";
        private const string PROFILE_NAME = "Client/profile_data";
        private const string RESOURCE_PATH = "CampaignRes/";
        private const string MSMEDAL_PREFIX = "MSMEDAL_";

        private static readonly string[] DifficultyNames = new string[]
        {
            "Easy".L10N("UI:Main:Easy"),
            "Normal".L10N("UI:Main:Normal"),
            "Hard".L10N("UI:Main:Hard"),
            "Abyss".L10N("UI:Main:Abyss")
        };

        protected int StartMusicIndex { get; set; }
        protected int ConflictMusicIndex { get; set; }

        public CampaignPanel(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
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

        private readonly string[] DifficultyList =
        {
            "btnEasy",
            "btnNormal",
            "btnHard",
            "btnAbyss"
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
        private EnhancedSoundEffect ClickSoundHeavy = new EnhancedSoundEffect("checkbox.wav");

        private delegate void gsDelegate(XNAClientButton button);
        private readonly gsDelegate[] SetAsButton = new gsDelegate[4];

        private List<Mission> Missions = new List<Mission>();
        private List<XNAClientButton> DifficultyButtons = new List<XNAClientButton>();
        private List<XNAClientButton> MissionButtons = new List<XNAClientButton>();
        private List<XNAClientButton> UnusedButtons = new List<XNAClientButton>();
        private List<int> UnusedButtonsIndex = new List<int>();

        private List<CMedal> Medals = new List<CMedal>(8);
        private CMedal DifficultyMedal;

        private CheaterWindow cheaterWindow;
        private XNAMessageBox TooHardMessageBox;

        private XNAClientButton btnLaunch;
        private XNAClientButton btnCancel;
        private XNAClientButton btnSlideUp;
        private XNAClientButton btnSlideDown;
        private XNAClientButton btnOldCampaign;

        private IniFile campaignOptionsIni;
        private IniFile profileIni;

        private string SIDE_ABBR;
        private int curDifficultyIndex;
        private int curMissionIndex;

        public override void Initialize()
        {
            ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            BorderColor = UISettings.ActiveSettings.PanelBorderColor;

            Name = "CampaignPanel";

            SIDE_ABBR = "GDO";
            MissionList = GDOMissionList;

            campaignOptionsIni = new IniFile(ProgramConstants.GetBaseResourcePath() + Name + ".ini");
            profileIni = new IniFile(ProgramConstants.GamePath + PROFILE_NAME);

            SetAsButton[0] = new gsDelegate(SetAsButton1);
            SetAsButton[1] = new gsDelegate(SetAsButton2);
            SetAsButton[2] = new gsDelegate(SetAsButton3);
            SetAsButton[3] = new gsDelegate(SetAsButton4);

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
            for (int i = 1; i <= MissionList.Length; i++)
            {
                XNAClientButton xNAClientButton = new XNAClientButton(WindowManager);
                xNAClientButton.Name = "btn" + SIDE_ABBR + i;
                xNAClientButton.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
                xNAClientButton.Disable();
                xNAClientButton.Visible = false;
                xNAClientButton.LeftClick += MissionButton_LeftClick;

                MissionButtons.Add(xNAClientButton);
                AddChild(xNAClientButton);
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
            btnOldCampaign.ClientRectangle = new Rectangle(5, 0, 86, 33);
            // english:
            // btnOldCampaign.ClientRectangle = new Rectangle(5, 0, 112, 33);
            btnOldCampaign.SetBorderHeight(10);
            btnOldCampaign.SetBorderWidth(11);
            btnOldCampaign.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "oldcampaignbtn.png");
            btnOldCampaign.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "oldcampaignbtn_c.png");
            btnOldCampaign.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            if (true) //(UserINISettings.Instance.TC2Completed)
            {
                btnOldCampaign.Visible = true;
                btnOldCampaign.AllowClick = true;
            }
            else
            {
                btnOldCampaign.Visible = false;
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

            if (curMissionIndex < 8 && campaignOptionsIni.GetBooleanValue("General", SIDE_ABBR + '5', false))
            {
                btnSlideDown.AllowClick = true;
            }

            base.Initialize();

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

        public void UpdateMissionMedals()
        {
            profileIni.Reload();

            foreach (CMedal medal in Medals)
            {
                string optionName = SIDE_ABBR + (curMissionIndex + 1).ToString() + "_" + medal.Name;

                medal.SetDisplayText(profileIni.GetBooleanValue(optionName, "Enable", false)
                    ? medal.unlockedText
                    : medal.lockedText);

                medal.DisabledClearTexture = profileIni.GetBooleanValue(optionName, "Enable", false)
                    ? AssetLoader.LoadTexture(RESOURCE_PATH + SIDE_ABBR + (curMissionIndex + 1).ToString() + "/" + medal.Name + "_c.png")
                    : AssetLoader.LoadTexture(RESOURCE_PATH + SIDE_ABBR + (curMissionIndex + 1).ToString() + "/" + medal.Name + ".png");
            }

            // Difficulty Medal
            string medalName = profileIni.GetStringValue(SIDE_ABBR + (curMissionIndex + 1).ToString(), "DifficultyMedal", "none");
            if (!MedalNameList.ContainsKey(medalName))
                medalName = "none";

            DifficultyMedal.SetDisplayText(campaignOptionsIni.GetStringValue(MSMEDAL_PREFIX + medalName.ToUpper(), "Text", String.Empty));
            DifficultyMedal.DisabledClearTexture = AssetLoader.LoadTexture(RESOURCE_PATH + SIDE_ABBR.ToLower() + medalName + ".png");
            DifficultyMedal.Enable();
        }

        private void GetMissionMedals(int index)
        {
            string missionIndex = (index + 1).ToString();
            string[] medalArray = campaignOptionsIni.GetStringValue(SIDE_ABBR + missionIndex, "Medals", String.Empty).Split(',');
            if (medalArray.Length > MEDAL_COUNT)
            {
                Logger.Log("Error: Mission " + SIDE_ABBR + missionIndex + " exceeds 8 medals!");
                return;
            }

            // Disable all medals
            foreach (CMedal medal in Medals)
            {
                medal.Checked = false;
                medal.Disable();
                medal.Visible = false;
            }

            for (int i = 0; i < medalArray.Length; i++)
            {
                string optionName = SIDE_ABBR + missionIndex + "_" + medalArray[i];
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
                    ? AssetLoader.LoadTexture(RESOURCE_PATH + SIDE_ABBR + missionIndex + "/" + medalArray[i] + "_c.png")
                    : AssetLoader.LoadTexture(RESOURCE_PATH + SIDE_ABBR + missionIndex + "/" + medalArray[i] + ".png");

                Medals[i].RefreshSize();
                Medals[i].Enable();
                Medals[i].Visible = true;
            }

            // Difficulty Medal
            string medalName = profileIni.GetStringValue(SIDE_ABBR + missionIndex, "DifficultyMedal", "none");
            if (!MedalNameList.ContainsKey(medalName))
                medalName = "none";

            string[] rectArray = MedalNameList[medalName].Split(',');
            DifficultyMedal.ClientRectangle = new Rectangle(Convert.ToInt32(rectArray[0]), Convert.ToInt32(rectArray[1]),
                Convert.ToInt32(rectArray[2]), Convert.ToInt32(rectArray[3]));

            DifficultyMedal.SetDisplayText(campaignOptionsIni.GetStringValue(MSMEDAL_PREFIX + medalName.ToUpper(), "Text", String.Empty));
            DifficultyMedal.DisabledClearTexture = AssetLoader.LoadTexture(RESOURCE_PATH + SIDE_ABBR.ToLower() + medalName + ".png");
            DifficultyMedal.RefreshSize();
            DifficultyMedal.Enable();
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
                MissionButtons[currentIndex].AllowClick = true;
                //MissionButtons[currentIndex].OnMouseLeave();
                MissionButtons[currentIndex].IdleTexture = AssetLoader.LoadTexture(MissionButtons[currentIndex].Tag.ToString() + ".png");
                MissionButtons[index].AllowClick = true;
            }

            if (index >= MissionList.Length)
            {
                index = 0;
            }

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
            MissionButtons[index].bButtonHover = false;
            MissionButtons[index].AllowClick = false;
            MissionButtons[index].IdleTexture = AssetLoader.LoadTexture(MissionButtons[index].Tag.ToString() + "_c.png");
            BackgroundTexture = AssetLoader.LoadTexture(RESOURCE_PATH + MissionList[index] + "bg.png");

            btnLaunch.AllowClick = campaignOptionsIni.GetBooleanValue(SIDE_ABBR + (currentIndex).ToString(), "Enable", true);

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
            ClickSoundHeavy.Play();
            btnSlideUp.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slideupbtn_c.png");

            UnusedButtons = MissionButtons;

            foreach (XNAClientButton button in MissionButtons)
            {
                if (button.Tag != null && (int)button.Tag == 1)
                {
                    int topIndex = Convert.ToInt32(button.Name.Substring(6)) - 1;

                    for (int i = 0; i < 4; i++)
                    {
                        SetAsButton[i](MissionButtons[topIndex + i - 1]);
                        CheckMission(MissionButtons[topIndex + i - 1]);

                        UnusedButtons.RemoveAt(topIndex + i - 1);
                    }

                    foreach (XNAClientButton unusedButton in UnusedButtons)
                    {
                        UnusedButtonsIndex.Add(Convert.ToInt32(unusedButton.Name.Substring(6)) - 1);
                    }

                    ClearUnusedButtonsTag();

                    return;
                }
            }
        }

        private void BtnSlideDown_LeftClick(object sender, EventArgs e)
        {
            ClickSoundHeavy.Play();
            btnSlideDown.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "slidedownbtn_c.png");

            UnusedButtons = MissionButtons;

            foreach (XNAClientButton button in MissionButtons)
            {
                if (button.Tag != null && (int)button.Tag == 4)
                {
                    int bottomIndex = Convert.ToInt32(button.Name.Substring(6)) - 1;

                    for (int i = 0; i < 4; i++)
                    {
                        SetAsButton[i](MissionButtons[bottomIndex + i - 2]);
                        CheckMission(MissionButtons[bottomIndex + i - 2]);

                        UnusedButtons.RemoveAt(bottomIndex + i - 2);
                    }

                    foreach (XNAClientButton unusedButton in UnusedButtons)
                    {
                        UnusedButtonsIndex.Add(Convert.ToInt32(unusedButton.Name.Substring(6)) - 1);
                    }

                    ClearUnusedButtonsTag();

                    return;
                }
            }
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
                    ClickSoundHeavy.Play();
                    SwitchMission(Convert.ToInt32(button.Name.Substring(6)) - 1, curMissionIndex);
                    return;
                }
            }
            Logger.Log("Error: Could not find hovered mission button.");
        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            Enabled = false;
        }

        private void BtnLaunch_LeftClick(object sender, EventArgs e)
        {
            if (curDifficultyIndex != 0)
            {
                TooHardMessageBox = XNAMessageBox.ShowYesNoDialog(WindowManager, "以非简单难度进行",
                string.Format("您确定不从简单难度开始任务吗?" + Environment.NewLine +
                "moreblabla"));
                TooHardMessageBox.YesClickedAction = TooHardMessageBox_YesClicked;
            }
            else if (curDifficultyIndex > 2)
            {
                TooHardMessageBox = XNAMessageBox.ShowYesNoDialog(WindowManager, "以地狱难度进行",
                string.Format("您确定要以地狱难度进行此任务吗?" + Environment.NewLine +
                "高难度为困难难度。"));
                TooHardMessageBox.YesClickedAction = TooHardMessageBox_YesClicked;
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
            parent.Show(parent.CampaignSelector);
        }

        private void TooHardMessageBox_YesClicked(XNAMessageBox messageBox)
        {
            PrepareToLaunch();
        }

        private void PrepareToLaunch()
        {
            Mission mission = Missions[curMissionIndex + 3];

            if (!ClientConfiguration.Instance.ModMode && !AreFilesModified())
            {
                // Confront the user by showing the cheater screen
                cheaterWindow.Enable();
                return;
            }

            LaunchMission(mission);
        }

        private bool AreFilesModified()
        {
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

            if (curDifficultyIndex <= 0)
                UserINISettings.Instance.Difficulty.Value = 0;
            else
                UserINISettings.Instance.Difficulty.Value = curDifficultyIndex - 1;
            UserINISettings.Instance.SaveSettings();

            ((MainMenuDarkeningPanel)Parent).Hide();

            string strDifficultyName = DifficultyNames[curDifficultyIndex];
            discordHandler?.UpdatePresence(mission.GUIName, strDifficultyName, mission.IconPath, true);

            GameProcessLogic.GameProcessExited += GameProcessExited_Callback;
            GameProcessLogic.StartGameProcess(CampaignIni.GetBooleanValue("BaseInfo", "ControlSpeed", true), true);
        }

        private void GameProcessExited_Callback()
        {
            WindowManager.AddCallback(new Action(GameProcessExited), null);

            foreach (string strMapName in campaignList)
                File.Delete(ProgramConstants.GamePath + strMapName);
        }

        protected virtual void GameProcessExited()
        {
            GameProcessLogic.GameProcessExited -= GameProcessExited_Callback;
            LogbuchParser.ParseForCampaign(null, null, curDifficultyIndex);
            LogbuchParser.ClearTrash();
            UpdateMissionMedals();
            if (true) //(UserINISettings.Instance.TC2Completed)
            {
                btnOldCampaign.Enable();
                btnOldCampaign.Visible = true;
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

        private void CheckMission(XNAClientButton button)
        {
            if ((button.Name.Length == 6 && button.Name[button.Name.Length - 1] == '1') ||
                profileIni.GetBooleanValue("General", button.Name.Substring(3), false))
            {
                button.Enable();
                button.Visible = true;
            }
            else
            {
                button.Disable();
                button.Visible = false;
            }
        }

        private void ClearUnusedButtonsTag()
        {
            foreach (int unusedInex in UnusedButtonsIndex)
            {
                MissionButtons[unusedInex].Tag = null;
                MissionButtons[unusedInex].Disable();
                MissionButtons[unusedInex].AllowClick = false;
            }
        }

        private void SetAsButton1(XNAClientButton button)
        {
            button.Tag = 1;
            button.X = 894;
            button.Y = 89;
            button.Width = 308;
            button.Height = 133;
            button.SetAlphaCheckVal(30);

            string strPath = RESOURCE_PATH + button.Name.ToLower() + "a";
            button.IdleTexture = AssetLoader.LoadTexture(strPath + ".png");
            button.HoverTexture = AssetLoader.LoadTexture(strPath + "_c.png");
            button.Tag = strPath;
            button.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            button.RefreshSize();
        }

        private void SetAsButton2(XNAClientButton button)
        {
            button.Tag = 2;
            button.X = 893;
            button.Y = 240;
            button.Width = 306;
            button.Height = 127;

            string strPath = RESOURCE_PATH + button.Name.ToLower() + "b";
            button.IdleTexture = AssetLoader.LoadTexture(strPath + ".png");
            button.HoverTexture = AssetLoader.LoadTexture(strPath + "_c.png");
            button.Tag = strPath;
            button.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            button.RefreshSize();
        }

        private void SetAsButton3(XNAClientButton button)
        {
            button.Tag = 3;
            button.X = 893;
            button.Y = 388;
            button.Width = 306;
            button.Height = 128;

            string strPath = RESOURCE_PATH + button.Name.ToLower() + "c";
            button.IdleTexture = AssetLoader.LoadTexture(strPath + ".png");
            button.HoverTexture = AssetLoader.LoadTexture(strPath + "_c.png");
            button.Tag = strPath;
            button.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            button.RefreshSize();
        }

        private void SetAsButton4(XNAClientButton button)
        {
            button.Tag = 4;
            button.X = 0;
            button.Y = 0;
            button.Width = 0;
            button.Height = 0;

            string strPath = RESOURCE_PATH + button.Name.ToLower() + "d";
            button.IdleTexture = AssetLoader.LoadTexture(strPath + ".png");
            button.HoverTexture = AssetLoader.LoadTexture(strPath + "_c.png");
            button.Tag = strPath + "d";
            button.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            button.RefreshSize();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
