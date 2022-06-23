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
    public class CampaignSelect : XNAWindow
    {
        // offset is 320, 156
        private const int DEFAULT_WIDTH = 1280;
        private const int DEFAULT_HEIGHT = 768;
        private const string RESOURCE_PATH = "CampaignSelect/";
        public event EventHandler WindowExited;
        public CampaignSelect(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.discordHandler = discordHandler;
        }

        private DiscordHandler discordHandler;

        private XNAClientButton btnTutorial;
        private XNAClientButton btnGDI;
        private XNAClientButton btnBack;

        private XNAExtraPanel epBackground;

        public override void Initialize()
        {
            ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            BackgroundTexture = AssetLoader.LoadTexture("empty.png");
            BorderColor = UISettings.ActiveSettings.PanelBorderColor;

            Name = "CampaignSelect";

            epBackground = new XNAExtraPanel(WindowManager);
            epBackground.Name = "epBackground";
            epBackground.ClientRectangle = new Rectangle(0, 0, 1920, 1080);
            epBackground.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            epBackground.DrawBorders = false;
            AddChild(epBackground);

            btnTutorial = new XNAClientButton(WindowManager);
            btnTutorial.Name = "btnTutorial";
            btnTutorial.ClientRectangle = new Rectangle(57, 86, 554, 586);
            btnTutorial.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "btntutorial.png");
            btnTutorial.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "btntutorial_c.png");
            btnTutorial.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnTutorial.LeftClick += BtnTutorial_LeftClick;

            btnGDI = new XNAClientButton(WindowManager);
            btnGDI.Name = "btnGDI";
            btnGDI.ClientRectangle = new Rectangle(633, 48, 654, 667);
            btnGDI.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "btngdi.png");
            btnGDI.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "btngdi_c.png");
            btnGDI.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnGDI.LeftClick += BtnGDI_LeftClick;

            btnBack = new XNAClientButton(WindowManager);
            btnBack.Name = "btnBack";
            btnBack.ClientRectangle = new Rectangle(400, 735, 480, 41);
            btnBack.IdleTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "btnback.png");
            btnBack.HoverTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "btnback_c.png");
            btnBack.HoverSoundEffect = new EnhancedSoundEffect("button.wav");
            btnBack.LeftClick += BtnBack_LeftClick;

            AddChild(btnTutorial);
            AddChild(btnGDI);
            AddChild(btnBack);

            base.Initialize();

            epBackground.CenterOnParent();
            CenterOnParent();
            epBackground.BackgroundTexture = AssetLoader.LoadTexture(RESOURCE_PATH + "background.png");;
        }

        private void BtnTutorial_LeftClick(object sender, EventArgs e)
        {
            MainMenuDarkeningPanel parent = (MainMenuDarkeningPanel)Parent;
            parent.CampaignSelector.ReloadBattleIni(true);
            parent.ShowSubControl(parent.CampaignSelector);
        }

        private void BtnGDI_LeftClick(object sender, EventArgs e)
        {
            if (UserINISettings.Instance.TutorialCompleted)
            {
                Disable();
                MainMenuDarkeningPanel parent = (MainMenuDarkeningPanel)Parent;
                parent.Show(parent.CampaignPanel);
            }
            else
            {
                XNAMessageBox.Show(WindowManager, "Tutorial Not Completed".L10N("UI:Main:TutorialNotCompleted"),
                    string.Format("You need to completed at least one mission of tutorial\nto start main campaign.".L10N("UI:Main:TutorialNotCompletedDesc")));
            }
        }

        private void BtnBack_LeftClick(object sender, EventArgs e)
        {
            // 退外层菜单
            Enabled = false;
            WindowExited?.Invoke(this,EventArgs.Empty);
        }
    }
}
