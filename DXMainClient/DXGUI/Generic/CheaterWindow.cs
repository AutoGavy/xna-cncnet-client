using ClientGUI;
using ClientCore;
using System;
using System.IO;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using Microsoft.Xna.Framework;
using Localization;

namespace DTAClient.DXGUI.Generic
{
    public class CheaterWindow : XNAWindow
    {
        private XNALabel lblCheater;
        private XNALabel lblDescription;
        private XNAPanel imagePanel;
        private XNAClientButton btnCancel;
        private XNAClientButton btnYes;
        private string strFilePath;

        public CheaterWindow(WindowManager windowManager) : base(windowManager)
        {
        }

        public event EventHandler YesClicked;

        public override void Initialize()
        {
            Name = "CheaterScreen";
            ClientRectangle = new Rectangle(0, 0, 334, 453);
            BackgroundTexture = AssetLoader.LoadTexture("cheaterbg.png");
            strFilePath = String.Empty;

            lblCheater = new XNALabel(WindowManager);
            lblCheater.Name = "lblCheater";
            lblCheater.ClientRectangle = new Rectangle(0, 0, 0, 0);
            lblCheater.FontIndex = 1;
            lblCheater.Text = "CHEATER!".L10N("UI:Main:Cheater");

            lblDescription = new XNALabel(WindowManager);
            lblDescription.Name = "lblDescription";
            lblDescription.ClientRectangle = new Rectangle(12, 40, 0, 0);
            lblDescription.Text = ("Modified game files have been detected. They could" + Environment.NewLine +
                "affect the game experience." +
                Environment.NewLine + Environment.NewLine +
                "Do you really lack the skill for winning the mission" + Environment.NewLine + "without cheating?").
                L10N("UI:Main:CheaterText");

            imagePanel = new XNAPanel(WindowManager);
            imagePanel.Name = "imagePanel";
            imagePanel.PanelBackgroundDrawMode = PanelBackgroundImageDrawMode.STRETCHED;
            imagePanel.ClientRectangle = new Rectangle(lblDescription.ClientRectangle.X,
                lblDescription.ClientRectangle.Bottom + 12, ClientRectangle.Width - 24,
                ClientRectangle.Height - (lblDescription.ClientRectangle.Bottom + 59));
            imagePanel.BackgroundTexture = AssetLoader.LoadTextureUncached("tds" + Convert.ToString(new Random().Next(1, 5)) + ".ctcb");

            btnCancel = new XNAClientButton(WindowManager);
            btnCancel.Name = "btnCancel";
            btnCancel.ClientRectangle = new Rectangle(Width - 104,
                Height - 35, 92, 23);
            btnCancel.Text = "OK".L10N("UI:Main:OK");
            btnCancel.LeftClick += BtnCancel_LeftClick;

            btnYes = new XNAClientButton(WindowManager);
            btnYes.Name = "btnYes";
            btnYes.ClientRectangle = new Rectangle(12, btnCancel.Y,
                btnCancel.Width, btnCancel.Height);
            btnYes.Text = "No".L10N("UI:Main:No");
            btnYes.LeftClick += BtnYes_LeftClick;

            AddChild(lblCheater);
            AddChild(lblDescription);
            AddChild(imagePanel);
            AddChild(btnCancel);
            AddChild(btnYes);

            lblCheater.CenterOnParent();
            lblCheater.ClientRectangle = new Rectangle(lblCheater.X, 12,
                lblCheater.Width, lblCheater.Height);

            base.Initialize();
        }

        private void BtnCancel_LeftClick(object sender, EventArgs e)
        {
            Disable();
        }

        private void BtnYes_LeftClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(strFilePath))
            {
                string backupFilePath = ProgramConstants.GetBaseSharedPath() + "BackupFiles/" + strFilePath;
                if (File.Exists(backupFilePath))
                {
                    File.Copy(backupFilePath, ProgramConstants.GamePath + strFilePath);
                }
            }

            Disable();
            YesClicked?.Invoke(this, EventArgs.Empty);
        }

        public void SetDefaultText(string filePath)
        {
            imagePanel.BackgroundTexture = AssetLoader.LoadTextureUncached("tds" + Convert.ToString(new Random().Next(1, 5)) + ".ctcb");
            lblCheater.ClientRectangle = new Rectangle(lblCheater.ClientRectangle.X, 12,
                lblCheater.ClientRectangle.Width, lblCheater.ClientRectangle.Height);
            lblCheater.Text = "File Modified".L10N("UI:Main:FileModified");
            lblDescription.Text = ("Game file " + filePath + "has been modified," + Environment.NewLine +
                  "it can affect the game experience." + Environment.NewLine + Environment.NewLine +
                  "Do you want to restore this file?")
                  .L10N("UI:Main:FileModifiedText");
            strFilePath = filePath;
        }

        public void SetCantFindText(string filePath)
        {
            imagePanel.BackgroundTexture = AssetLoader.LoadTextureUncached("tds" + Convert.ToString(new Random().Next(1, 5)) + ".ctcb");
            lblCheater.ClientRectangle = new Rectangle(lblCheater.ClientRectangle.X, 6,
                lblCheater.ClientRectangle.Width, lblCheater.ClientRectangle.Height);
            lblCheater.Text = "Game File Not Found".L10N("UI:Main:GameFileNotFound");
            lblDescription.Text =
                ("Cannot find file\"" + filePath + "\"" +
                Environment.NewLine + "Failed to start game.").
                L10N("UI:Main:GameNotFound_Desc");
            strFilePath = String.Empty;
        }
    }
}
