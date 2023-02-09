using ClientCore;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using ClientGUI;
using Rampastring.XNAUI.XNAControls;
using Rampastring.XNAUI;
using Rampastring.Tools;
using Localization;

namespace DTAClient.DXGUI.Generic
{
    public class DatabasePanel : XNAWindow
    {
        private const int DEFAULT_WIDTH = 650;
        private const int DEFAULT_HEIGHT = 600;
        private const int LISTBOX_X = 306;
        private const int LISTBOX_Y = 68;
        private const int LISTBOX_WIDTH = 259;
        private const int LISTBOX_HEIGHT = 649;
        private const string PROFILE_NAME = "Client/profile_data";

        public event EventHandler WindowExited;
        public DatabasePanel(WindowManager windowManager) : base(windowManager)
        {

        }

        //private EnhancedSoundEffect ClickSound = new EnhancedSoundEffect("checkbox.wav");

        private XNAClientButton btnFacGen;
        private XNAClientButton btnFacGDI;
        private XNAClientButton btnFacNod;
        private XNAClientButton btnFacScr;
        private XNAClientButton btnCancel;
        private XNAListBox lbGenDataList;
        private XNAListBox lbGDIDataList;
        private XNAListBox lbNodDataList;
        private XNAListBox lbScrDataList;
        private XNATextBlock tbDataText;
        private XNAExtraPanel epDataPic;
        private XNAExtraPanel epBackground;

        private IniFile dataOptionsIni;
        private IniFile profileIni;

        public override void Initialize()
        {
            BackgroundTexture = AssetLoader.LoadTexture("empty.png");
            ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            BorderColor = UISettings.ActiveSettings.PanelBorderColor;

            Name = "DatabasePanel";

            btnFacGen = new XNAClientButton(WindowManager);
            btnFacGen.Name = "btnFacGen";
            btnFacGen.ClientRectangle = new Rectangle(95, 52, 166, 135);
            btnFacGen.IdleTexture = AssetLoader.LoadTexture("Database/facgenbtn.png");
            btnFacGen.HoverTexture = AssetLoader.LoadTexture("Database/facgenbtn_c.png");
            btnFacGen.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnFacGen.LeftClick += BtnFacGen_LeftClick;
            btnFacGen.Tag = true;

            btnFacGDI = new XNAClientButton(WindowManager);
            btnFacGDI.Name = "btnFacGDI";
            btnFacGDI.ClientRectangle = new Rectangle(95, 230, 166, 135);
            btnFacGDI.IdleTexture = AssetLoader.LoadTexture("Database/facgdibtn.png");
            btnFacGDI.HoverTexture = AssetLoader.LoadTexture("Database/facgdibtn_c.png");
            btnFacGDI.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnFacGDI.LeftClick += BtnFacGDI_LeftClick;
            btnFacGDI.Tag = true;

            btnFacNod = new XNAClientButton(WindowManager);
            btnFacNod.Name = "btnFacNod";
            btnFacNod.ClientRectangle = new Rectangle(95, 406, 166, 135);
            btnFacNod.IdleTexture = AssetLoader.LoadTexture("Database/facnodbtn.png");
            btnFacNod.HoverTexture = AssetLoader.LoadTexture("Database/facnodbtn_c.png");
            btnFacNod.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnFacNod.LeftClick += BtnFacNod_LeftClick;
            btnFacNod.Tag = true;

            btnFacScr = new XNAClientButton(WindowManager);
            btnFacScr.Name = "btnFacScr";
            btnFacScr.ClientRectangle = new Rectangle(95, 582, 166, 135);
            btnFacScr.IdleTexture = AssetLoader.LoadTexture("Database/facscrbtn.png");
            btnFacScr.HoverTexture = AssetLoader.LoadTexture("Database/facscrbtn_c.png");
            btnFacScr.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnFacScr.LeftClick += BtnFacScr_LeftClick;
            btnFacScr.Tag = true;

            btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = "btnCancel";
            //if (ClientConfiguration.Instance.ClientLanguage == 0)
               // btnCancel.ClientRectangle = new Rectangle(1126, 725, 92, 31);
            //else
                btnCancel.ClientRectangle = new Rectangle(1126, 725, 66, 32);
            btnCancel.IdleTexture = AssetLoader.LoadTexture("Database/backbtn.png");
            btnCancel.HoverTexture = AssetLoader.LoadTexture("Database/backbtn_c.png");
            btnCancel.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnCancel.LeftClick += BtnCancel_LeftClick;

            lbGenDataList = new XNAListBox(WindowManager);
            lbGenDataList.Name = "lbGenDataList";
            lbGenDataList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbGenDataList.DrawBorders = false;
            lbGenDataList.ClientRectangle = new Rectangle(LISTBOX_X, LISTBOX_Y, LISTBOX_WIDTH, LISTBOX_HEIGHT);
            lbGenDataList.SelectedIndexChanged += LbGenDataList_SelectedIndexChanged;

            lbGDIDataList = new XNAListBox(WindowManager);
            lbGDIDataList.Name = "lbGDIDataList";
            lbGDIDataList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbGDIDataList.DrawBorders = false;
            lbGDIDataList.ClientRectangle = new Rectangle(LISTBOX_X, LISTBOX_Y, LISTBOX_WIDTH, LISTBOX_HEIGHT);
            lbGDIDataList.SelectedIndexChanged += LbGDIDataList_SelectedIndexChanged;

            lbNodDataList = new XNAListBox(WindowManager);
            lbNodDataList.Name = "lbNodDataList";
            lbNodDataList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbNodDataList.DrawBorders = false;
            lbNodDataList.ClientRectangle = new Rectangle(LISTBOX_X, LISTBOX_Y, LISTBOX_WIDTH, LISTBOX_HEIGHT);
            lbNodDataList.SelectedIndexChanged += LbNodDataList_SelectedIndexChanged;

            lbScrDataList = new XNAListBox(WindowManager);
            lbScrDataList.Name = "lbScrDataList";
            lbScrDataList.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            lbScrDataList.DrawBorders = false;
            lbScrDataList.ClientRectangle = new Rectangle(LISTBOX_X, LISTBOX_Y, LISTBOX_WIDTH, LISTBOX_HEIGHT);
            lbScrDataList.SelectedIndexChanged += LbScrDataList_SelectedIndexChanged;

            tbDataText = new XNATextBlock(WindowManager);
            tbDataText.Name = "tbDataText";
            tbDataText.ClientRectangle = new Rectangle(679, 45, 498, 395);
            // english:
            // tbDataText.ClientRectangle = new Rectangle(681, 59, 496, 375);
            tbDataText.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            tbDataText.DrawBorders = false;
            tbDataText.Alpha = 1.0f;

            epDataPic = new XNAExtraPanel(WindowManager);
            epDataPic.Name = "epDataPic";
            epDataPic.ClientRectangle = new Rectangle(673, 461, 512, 256);
            epDataPic.BackgroundTexture = AssetLoader.LoadTexture("empty.png");
            epDataPic.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            epDataPic.DrawBorders = false;

            epBackground = new XNAExtraPanel(WindowManager);
            epBackground.Name = "epBackground";
            epBackground.ClientRectangle = new Rectangle(0, 0, 1920, 1080);
            epBackground.BackgroundTexture = AssetLoader.LoadTexture("Database/gen_bg.png");
            epBackground.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            epBackground.DrawBorders = false;
            AddChild(epBackground);

            AddChild(btnFacGen);
            AddChild(btnFacGDI);
            AddChild(btnFacNod);
            AddChild(btnFacScr);
            AddChild(btnCancel);
            AddChild(lbGenDataList);
            AddChild(lbGDIDataList);
            AddChild(lbNodDataList);
            AddChild(lbScrDataList);
            AddChild(tbDataText);
            AddChild(epDataPic);

            dataOptionsIni = new IniFile(ProgramConstants.GetBaseResourcePath() + Name + ".ini");
            profileIni = new IniFile(ProgramConstants.GamePath + PROFILE_NAME);

            LoadDataList(lbGenDataList, "gen");
            LoadDataList(lbGDIDataList, "gdi");
            LoadDataList(lbNodDataList, "nod");
            LoadDataList(lbScrDataList, "scr");

            lbGenDataList.Enable();
            lbGenDataList.Visible = true;
            //btnFacGen.OnMouseEnter();
            btnFacGen.AllowClick = false;
            btnFacGen.IdleTexture = AssetLoader.LoadTexture("Database/facgenbtn_c.png");
            btnFacGen.bButtonOn = true;

            lbGDIDataList.Disable();
            lbGDIDataList.Visible = false;
            btnFacGDI.AllowClick = true;

            lbNodDataList.Disable();
            lbNodDataList.Visible = false;
            btnFacNod.AllowClick = true;

            lbScrDataList.Disable();
            lbScrDataList.Visible = false;
            btnFacScr.AllowClick = true;

            base.Initialize();

            epBackground.CenterOnParent();
            CenterOnParent();
        }

        public string GetSwitchName()
        {
            return "Database".L10N("UI:Main:Database");
        }

        public void UpdatePanel()
        {
            LoadDataList(lbGenDataList, "gen");
            LoadDataList(lbGDIDataList, "gdi");
            LoadDataList(lbNodDataList, "nod");
            LoadDataList(lbScrDataList, "scr");
        }

        public void ResetPanel(XNAListBox dataList = null)
        {
            if (dataList == null)
            {
                lbGenDataList.SelectedIndex = -1;
                lbGDIDataList.SelectedIndex = -1;
                lbNodDataList.SelectedIndex = -1;
                lbScrDataList.SelectedIndex = -1;
            }
            else
                dataList.SelectedIndex = -1;

            tbDataText.Text = String.Empty;
            epDataPic.BackgroundTexture = AssetLoader.LoadTexture("Database/empty.png");
        }

        private void LoadDataList(XNAListBox dataList, string strSide)
        {
            List<string> dataKeys = dataOptionsIni.GetSectionKeys("DataList");

            if (dataKeys == null)
                Logger.Log("Error: No sections in [DataList]!");

            foreach (string dataEntry in dataKeys)
            {
                string dataSection = dataOptionsIni.GetStringValue("DataList", dataEntry, "NOT FOUND");

                XNAListBoxItem item = new XNAListBoxItem();
                item.Text = dataOptionsIni.GetStringValue(dataSection, "Title", " ");
                item.Tag = dataSection;

                // match side
                if (!dataOptionsIni.SectionExists(dataSection) ||
                    dataOptionsIni.GetStringValue(dataSection, "Side", String.Empty).ToLower() != strSide)
                    continue;

                // check if it's a header
                if (dataOptionsIni.GetBooleanValue(dataSection, "IsHeader", false))
                {
                    item.TextColor = AssetLoader.GetColorFromString(ClientConfiguration.Instance.ListBoxHeaderColor);
                    item.IsHeader = true;
                    item.Selectable = false;

                    dataList.AddItem(item);
                    continue;
                }

                // check if player has achieved it
                if (dataOptionsIni.GetBooleanValue(dataSection, "Locked", false))
                {
                    if (profileIni.GetBooleanValue(dataSection, "Enable", false))
                    {
                        item.TextColor = dataList.DefaultItemColor;
                    }
                    else
                    {
                        continue;
                        //item.TextColor = UISettings.ActiveSettings.DisabledItemColor;
                        //item.Selectable = false;
                    }
                }
                else
                {
                    item.TextColor = dataList.DefaultItemColor;
                }

                // check if it's new
                if (profileIni.GetBooleanValue(dataSection, "New", false))
                {
                    switch (strSide)
                    {
                        case "gen":
                            if ((bool)btnFacGen.Tag)
                            {
                                btnFacGen.IdleTexture = btnFacGen.bButtonOn ? AssetLoader.LoadTexture("Database/facgenbtn_c_n.png") : AssetLoader.LoadTexture("Database/facgenbtn_n.png");
                                btnFacGen.HoverTexture = AssetLoader.LoadTexture("Database/facgenbtn_c_n.png");
                                btnFacGen.Tag = false;
                            }
                            break;
                        case "gdi":
                            if ((bool)btnFacGDI.Tag)
                            {
                                btnFacGDI.IdleTexture = btnFacGDI.bButtonOn ? AssetLoader.LoadTexture("Database/facgdibtn_c_n.png") : AssetLoader.LoadTexture("Database/facgdibtn_n.png");
                                btnFacGDI.HoverTexture = AssetLoader.LoadTexture("Database/facgdibtn_c_n.png");
                                btnFacGDI.Tag = false;
                            }
                            break;
                        case "nod":
                            if ((bool)btnFacNod.Tag)
                            {
                                btnFacNod.IdleTexture = btnFacNod.bButtonOn ? AssetLoader.LoadTexture("Database/facnodbtn_c_n.png") : AssetLoader.LoadTexture("Database/facnodbtn_n.png");
                                btnFacNod.HoverTexture = AssetLoader.LoadTexture("Database/facnodbtn_c_n.png");
                                btnFacNod.Tag = false;
                            }
                            break;
                        case "scr":
                            if ((bool)btnFacScr.Tag)
                            {
                                btnFacScr.IdleTexture = btnFacScr.bButtonOn ? AssetLoader.LoadTexture("Database/facscrbtn_c_n.png") : AssetLoader.LoadTexture("Database/facscrbtn_n.png");
                                btnFacScr.HoverTexture = AssetLoader.LoadTexture("Database/facscrbtn_c_n.png");
                                btnFacScr.Tag = false;
                            }
                            break;
                    }
                    item.Texture = AssetLoader.LoadTexture("Database/newdataicon.png");
                }
                else
                {
                    item.Texture = AssetLoader.LoadTexture("Database/dataicon.png");
                }

                dataList.AddItem(item);
            }
        }

        private void UpdateDisplayData(XNAListBox dataList)
        {
            if (dataList.SelectedIndex == -1)
            {
                tbDataText.Text = String.Empty;
                epDataPic.BackgroundTexture = AssetLoader.LoadTexture("Database/empty.png");
                return;
            }

            string dataName = (string)dataList.SelectedItem.Tag;
            if (profileIni.SectionExists(dataName) && profileIni.GetBooleanValue(dataName, "New", true))
            {
                profileIni.SetBooleanValue((string)dataList.SelectedItem.Tag, "New", false);
                profileIni.WriteIniFile();
                dataList.SelectedItem.Texture = AssetLoader.LoadTexture("Database/dataicon.png");
            }

            tbDataText.Text = ProgramConstants.SortText(dataOptionsIni.GetStringValue((string)dataList.SelectedItem.Tag, "Text", "null"), 94, dataOptionsIni.GetBooleanValue((string)dataList.SelectedItem.Tag, "Wordwrap", false));
            epDataPic.BackgroundTexture = AssetLoader.LoadTexture("Database/DataImages/" +
                dataOptionsIni.GetStringValue((string)dataList.SelectedItem.Tag, "Texture", "empty.png"));
        }

        private void LbGenDataList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplayData(lbGenDataList);
        }

        private void LbGDIDataList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplayData(lbGDIDataList);
        }

        private void LbNodDataList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplayData(lbNodDataList);
        }

        private void LbScrDataList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDisplayData(lbScrDataList);
        }

        private void BtnFacGen_LeftClick(object sender, EventArgs e)
        {
            //ClickSound.Play();

            if (!(bool)btnFacGen.Tag)
            {
                btnFacGen.IdleTexture = AssetLoader.LoadTexture("Database/facgenbtn.png");
                btnFacGen.HoverTexture = AssetLoader.LoadTexture("Database/facgenbtn_c.png");
                btnFacGen.Tag = true;
            }
            lbGenDataList.Enable();
            lbGenDataList.Visible = true;
            btnFacGen.AllowClick = false;
            btnFacGen.IdleTexture = AssetLoader.LoadTexture("Database/facgenbtn_c.png");
            btnFacGen.bButtonOn = true;

            lbGDIDataList.Disable();
            lbGDIDataList.Visible = false;
            btnFacGDI.AllowClick = true;
            if (btnFacGDI.bButtonOn)
            {
                //btnFacGDI.OnMouseLeave();
                btnFacGDI.IdleTexture = AssetLoader.LoadTexture("Database/facgdibtn.png");
                btnFacGDI.bButtonOn = false;
            }

            lbNodDataList.Disable();
            lbNodDataList.Visible = false;
            btnFacNod.AllowClick = true;
            if (btnFacNod.bButtonOn)
            {
                //btnFacNod.OnMouseLeave();
                btnFacNod.IdleTexture = AssetLoader.LoadTexture("Database/facnodbtn.png");
                btnFacNod.bButtonOn = false;
            }

            lbScrDataList.Disable();
            lbScrDataList.Visible = false;
            btnFacScr.AllowClick = true;
            if (btnFacScr.bButtonOn)
            {
                //btnFacScr.OnMouseLeave();
                btnFacScr.IdleTexture = AssetLoader.LoadTexture("Database/facscrbtn.png");
                btnFacScr.bButtonOn = false;
            }

            epBackground.BackgroundTexture = AssetLoader.LoadTexture("Database/gen_bg.png");
            UpdateDisplayData(lbGenDataList);
        }

        private void BtnFacGDI_LeftClick(object sender, EventArgs e)
        {
            //ClickSound.Play();

            lbGenDataList.Disable();
            lbGenDataList.Visible = false;
            btnFacGen.AllowClick = true;
            if (btnFacGen.bButtonOn)
            {
                //btnFacGen.OnMouseLeave();
                btnFacGen.IdleTexture = AssetLoader.LoadTexture("Database/facgenbtn.png");
                btnFacGen.bButtonOn = false;
            }

            if (!(bool)btnFacGDI.Tag)
            {
                btnFacGDI.IdleTexture = AssetLoader.LoadTexture("Database/facgdibtn.png");
                btnFacGDI.HoverTexture = AssetLoader.LoadTexture("Database/facgdibtn_c.png");
                btnFacGDI.Tag = true;
            }
            lbGDIDataList.Enable();
            lbGDIDataList.Visible = true;
            btnFacGDI.AllowClick = false;
            btnFacGDI.IdleTexture = AssetLoader.LoadTexture("Database/facgdibtn_c.png");
            btnFacGDI.bButtonOn = true;

            lbNodDataList.Disable();
            lbNodDataList.Visible = false;
            btnFacNod.AllowClick = true;
            if (btnFacNod.bButtonOn)
            {
                //btnFacNod.OnMouseLeave();
                btnFacNod.IdleTexture = AssetLoader.LoadTexture("Database/facnodbtn.png");
                btnFacNod.bButtonOn = false;
            }

            lbScrDataList.Disable();
            lbScrDataList.Visible = false;
            btnFacScr.AllowClick = true;
            if (btnFacScr.bButtonOn)
            {
                //btnFacScr.OnMouseLeave();
                btnFacScr.IdleTexture = AssetLoader.LoadTexture("Database/facscrbtn.png");
                btnFacScr.bButtonOn = false;
            }

            epBackground.BackgroundTexture = AssetLoader.LoadTexture("Database/gdi_bg.png");
            UpdateDisplayData(lbGDIDataList);
        }

        private void BtnFacNod_LeftClick(object sender, EventArgs e)
        {
            //ClickSound.Play();

            lbGenDataList.Disable();
            lbGenDataList.Visible = false;
            btnFacGen.AllowClick = true;
            if (btnFacGen.bButtonOn)
            {
                //btnFacGen.OnMouseLeave();
                btnFacGen.IdleTexture = AssetLoader.LoadTexture("Database/facgenbtn.png");
                btnFacGen.bButtonOn = false;
            }

            lbGDIDataList.Disable();
            lbGDIDataList.Visible = false;
            btnFacGDI.AllowClick = true;
            if (btnFacGDI.bButtonOn)
            {
                //btnFacGDI.OnMouseLeave();
                btnFacGDI.IdleTexture = AssetLoader.LoadTexture("Database/facgdibtn.png");
                btnFacGDI.bButtonOn = false;
            }

            if (!(bool)btnFacNod.Tag)
            {
                btnFacNod.IdleTexture = AssetLoader.LoadTexture("Database/facnodbtn.png");
                btnFacNod.HoverTexture = AssetLoader.LoadTexture("Database/facnodbtn_c.png");
                btnFacNod.Tag = true;
            }
            lbNodDataList.Enable();
            lbNodDataList.Visible = true;
            btnFacNod.AllowClick = false;
            btnFacNod.IdleTexture = AssetLoader.LoadTexture("Database/facnodbtn_c.png");
            btnFacNod.bButtonOn = true;

            lbScrDataList.Disable();
            lbScrDataList.Visible = false;
            btnFacScr.AllowClick = true;
            if (btnFacScr.bButtonOn)
            {
                //btnFacScr.OnMouseLeave();
                btnFacScr.IdleTexture = AssetLoader.LoadTexture("Database/facscrbtn.png");
                btnFacScr.bButtonOn = false;
            }

            epBackground.BackgroundTexture = AssetLoader.LoadTexture("Database/nod_bg.png");
            UpdateDisplayData(lbNodDataList);
        }

        private void BtnFacScr_LeftClick(object sender, EventArgs e)
        {
            //ClickSound.Play();

            lbGenDataList.Disable();
            lbGenDataList.Visible = false;
            btnFacGen.AllowClick = true;
            if (btnFacGen.bButtonOn)
            {
                //btnFacGen.OnMouseLeave();
                btnFacGen.IdleTexture = AssetLoader.LoadTexture("Database/facgenbtn.png");
                btnFacGen.bButtonOn = false;
            }

            lbGDIDataList.Disable();
            lbGDIDataList.Visible = false;
            btnFacGDI.AllowClick = true;
            if (btnFacGDI.bButtonOn)
            {
                //btnFacGDI.OnMouseLeave();
                btnFacGDI.IdleTexture = AssetLoader.LoadTexture("Database/facgdibtn.png");
                btnFacGDI.bButtonOn = false;
            }

            lbNodDataList.Disable();
            lbNodDataList.Visible = false;
            btnFacNod.AllowClick = true;
            if (btnFacNod.bButtonOn)
            {
                //btnFacNod.OnMouseLeave();
                btnFacNod.IdleTexture = AssetLoader.LoadTexture("Database/facnodbtn.png");
                btnFacNod.bButtonOn = false;
            }

            if (!(bool)btnFacScr.Tag)
            {
                btnFacScr.IdleTexture = AssetLoader.LoadTexture("Database/facscrbtn.png");
                btnFacScr.HoverTexture = AssetLoader.LoadTexture("Database/facscrbtn_c.png");
                btnFacScr.Tag = true;
            }
            lbScrDataList.Enable();
            lbScrDataList.Visible = true;
            btnFacScr.AllowClick = false;
            btnFacScr.IdleTexture = AssetLoader.LoadTexture("Database/facscrbtn_c.png");
            btnFacScr.bButtonOn = true;

            epBackground.BackgroundTexture = AssetLoader.LoadTexture("Database/scr_bg.png");
            UpdateDisplayData(lbScrDataList);
        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            WindowExited?.Invoke(this, EventArgs.Empty);
        }
    }
}
