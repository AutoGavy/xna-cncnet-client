using Microsoft.Xna.Framework;
using System;
using DTAClient.Domain;
using ClientGUI;
using Rampastring.XNAUI;

namespace DTAClient.DXGUI.Generic
{
    public class CreditsPanel : XNAWindow
    {
        // offset is 320, 156
        private const int DEFAULT_WIDTH = 1280;
        private const int DEFAULT_HEIGHT = 768;
        public event EventHandler WindowExited;

        private DiscordHandler discordHandler;

        private XNAClientButton btnBack;

        public CreditsPanel(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.discordHandler = discordHandler;
        }

        public override void Initialize()
        {
            ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            BackgroundTexture = AssetLoader.LoadTexture("creditspanelbg.png");
            DrawBorders = false;

            Name = "CreditsPanel";

            btnBack = new XNAClientButton(WindowManager);
            btnBack.Name = "btnBack";
            btnBack.ClientRectangle = new Rectangle(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT);
            btnBack.IdleTexture = AssetLoader.LoadTexture("empty.png");
            btnBack.HoverTexture = AssetLoader.LoadTexture("empty.png");
            btnBack.LeftClick += BtnBack_LeftClick;
            //btnBack.MuteSound();

            AddChild(btnBack);

            base.Initialize();

            CenterOnParent();
        }


        private void BtnBack_LeftClick(object sender, EventArgs e)
        {
            // 退外层菜单
            Enabled = false;
            WindowExited?.Invoke(this, EventArgs.Empty);
        }
    }
}
