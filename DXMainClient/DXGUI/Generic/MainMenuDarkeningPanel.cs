using ClientGUI;
using DTAClient.Domain;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;

namespace DTAClient.DXGUI.Generic
{
    /// <summary>
    /// TODO Replace this class with DarkeningPanels.
    /// Handles transitions between the main menu and its sub-menus.
    /// </summary>
    public class MainMenuDarkeningPanel : XNAPanel
    {
        public MainMenuDarkeningPanel(WindowManager windowManager, DiscordHandler discordHandler) : base(windowManager)
        {
            this.discordHandler = discordHandler;
            DrawBorders = false;
            DrawMode = ControlDrawMode.UNIQUE_RENDER_TARGET;
            AlphaRate = 0.0f;
        }

        private DiscordHandler discordHandler;

        public CampaignPanel CampaignPanel;
        public CampaignSelect CampaignSelect;
        public CampaignSelector CampaignSelector;
        public GameLoadingWindow GameLoadingWindow;
        public StatisticsWindow StatisticsWindow;
        public UpdateQueryWindow UpdateQueryWindow;
        public UpdateWindow UpdateWindow;
        public ExtrasWindow ExtrasWindow;
        public DatabasePanel DatabasePanel;
        public CreditsPanel CreditsPanel;

        public override void Initialize()
        {
            base.Initialize();

            Name = "DarkeningPanel";
            BorderColor = UISettings.ActiveSettings.PanelBorderColor;
            //BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 1, 1);
            BackgroundTexture = AssetLoader.LoadTexture("generalbglight.png");
            //ClientRectangle = new Rectangle(0, 0, 1, 1);
            PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            Alpha = 1.0f;

            // Note: this is actually a child of CampaignSelect
            CampaignPanel = new CampaignPanel(WindowManager, discordHandler);
            AddChild(CampaignPanel);

            CampaignSelect = new CampaignSelect(WindowManager, discordHandler);
            CampaignSelect.WindowExited += (sender, arg) =>
            {
                Hide();
            };
            AddChild(CampaignSelect);

            CreditsPanel = new CreditsPanel(WindowManager, discordHandler);
            CreditsPanel.WindowExited += (sender, arg) =>
            {
                Hide();
            };
            AddChild(CreditsPanel);

            CampaignSelector = new CampaignSelector(WindowManager, discordHandler);
            AddChild(CampaignSelector);

            GameLoadingWindow = new GameLoadingWindow(WindowManager, discordHandler);
            GameLoadingWindow.WindowExited += (sender, arg) =>
            {
                Hide();
            };
            AddChild(GameLoadingWindow);

            StatisticsWindow = new StatisticsWindow(WindowManager);
            StatisticsWindow.WindowExited += (sender, arg) =>
            {
                Hide();
            };
            AddChild(StatisticsWindow);

            UpdateQueryWindow = new UpdateQueryWindow(WindowManager);
            AddChild(UpdateQueryWindow);

            UpdateWindow = new UpdateWindow(WindowManager);
            AddChild(UpdateWindow);

            ExtrasWindow = new ExtrasWindow(WindowManager);
            AddChild(ExtrasWindow);

            DatabasePanel = new DatabasePanel(WindowManager);
            DatabasePanel.WindowExited += (sender, arg) =>
            {
                Hide();
            };
            AddChild(DatabasePanel);

            foreach (XNAControl child in Children)
            {
                child.Visible = false;
                child.Enabled = false;
            }
        }

        public void Show(XNAControl control)
        {
            foreach (XNAControl child in Children)
            {
                child.Enabled = false;
                child.Visible = false;
            }

            Enabled = true;
            Visible = true;

            AlphaRate = DarkeningPanel.ALPHA_RATE;

            if (control != null)
            {
                control.Enabled = true;
                control.Visible = true;
                control.IgnoreInputOnFrame = true;
            }
        }

        public void ShowSubControl(XNAControl control)
        {
            if (control != null)
            {
                control.Enabled = true;
                control.Visible = true;
                control.IgnoreInputOnFrame = true;
            }
        }

        public void Hide()
        {
            AlphaRate = -DarkeningPanel.ALPHA_RATE;
            Enabled = false;
            Visible = false;
            foreach (XNAControl child in Children)
            {
                child.Enabled = false;
                child.Visible = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Alpha <= 0f)
            {
                Enabled = false;
                Visible = false;

                foreach (XNAControl child in Children)
                {
                    child.Visible = false;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            DrawTexture(BackgroundTexture, Point.Zero, Color.White);
            base.Draw(gameTime);
        }
    }
}
