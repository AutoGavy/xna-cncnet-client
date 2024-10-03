using ClientCore;
using ClientGUI;
using Localization;
using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;

namespace DTAConfig.OptionPanels
{
    class AudioOptionsPanel : XNAOptionsPanel
    {
        public AudioOptionsPanel(WindowManager windowManager, UserINISettings iniSettings)
            : base(windowManager, iniSettings)
        {
        }

        private XNATrackbar trbScoreVolume;
        private XNATrackbar trbSoundVolume;
        private XNATrackbar trbVoiceVolume;

        private XNALabel lblScoreVolumeValue;
        private XNALabel lblSoundVolumeValue;
        private XNALabel lblVoiceVolumeValue;
        private XNAClientDropDown ddMusicType;

        private XNAClientCheckBox chkScoreShuffle;
        private XNAClientCheckBox chkSmartMusic;

        private XNALabel lblClientVolumeValue;
        private XNATrackbar trbClientVolume;

        private XNAClientCheckBox chkMainMenuMusic;
        private XNAClientCheckBox chkStopMusicOnMenu;

        private XNAMessageBox NoMusicPackMessageBox;

        public override void Initialize()
        {
            base.Initialize();

            Name = "AudioOptionsPanel";

            BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 200), 1, 1);

            var lblScoreVolume = new XNALabel(WindowManager);
            lblScoreVolume.Name = "lblScoreVolume";
            lblScoreVolume.ClientRectangle = new Rectangle(12, 14, 0, 0);
            lblScoreVolume.Text = "Music Volume:".L10N("UI:DTAConfig:MusicVolume");

            lblScoreVolumeValue = new XNALabel(WindowManager);
            lblScoreVolumeValue.Name = "lblScoreVolumeValue";
            lblScoreVolumeValue.FontIndex = 1;
            lblScoreVolumeValue.Text = "0";
            lblScoreVolumeValue.ClientRectangle = new Rectangle(
                Width - lblScoreVolumeValue.Width - 12,
                lblScoreVolume.Y, 0, 0);

            trbScoreVolume = new XNATrackbar(WindowManager);
            trbScoreVolume.Name = "trbScoreVolume";
            trbScoreVolume.ClientRectangle = new Rectangle(
                lblScoreVolume.Right + 16,
                lblScoreVolume.Y - 2,
                lblScoreVolumeValue.X - lblScoreVolume.Right - 31,
                22);
            trbScoreVolume.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 128), 2, 2);
            trbScoreVolume.MinValue = 0;
            trbScoreVolume.MaxValue = 10;
            trbScoreVolume.ValueChanged += TrbScoreVolume_ValueChanged;

            var lblSoundVolume = new XNALabel(WindowManager);
            lblSoundVolume.Name = "lblSoundVolume";
            lblSoundVolume.ClientRectangle = new Rectangle(lblScoreVolume.X,
                lblScoreVolume.Bottom + 20, 0, 0);
            lblSoundVolume.Text = "Sound Volume:".L10N("UI:DTAConfig:SoundVolume");

            lblSoundVolumeValue = new XNALabel(WindowManager);
            lblSoundVolumeValue.Name = "lblSoundVolumeValue";
            lblSoundVolumeValue.FontIndex = 1;
            lblSoundVolumeValue.Text = "0";
            lblSoundVolumeValue.ClientRectangle = new Rectangle(
                lblScoreVolumeValue.X,
                lblSoundVolume.Y, 0, 0);

            trbSoundVolume = new XNATrackbar(WindowManager);
            trbSoundVolume.Name = "trbSoundVolume";
            trbSoundVolume.ClientRectangle = new Rectangle(
                trbScoreVolume.X,
                lblSoundVolume.Y - 2,
                trbScoreVolume.Width,
                trbScoreVolume.Height);
            trbSoundVolume.BackgroundTexture = trbScoreVolume.BackgroundTexture;
            trbSoundVolume.MinValue = 0;
            trbSoundVolume.MaxValue = 10;
            trbSoundVolume.ValueChanged += TrbSoundVolume_ValueChanged;

            var lblVoiceVolume = new XNALabel(WindowManager);
            lblVoiceVolume.Name = "lblVoiceVolume";
            lblVoiceVolume.ClientRectangle = new Rectangle(lblScoreVolume.X,
                lblSoundVolume.Bottom + 20, 0, 0);
            lblVoiceVolume.Text = "Voice Volume:".L10N("UI:DTAConfig:VoiceVolume");

            lblVoiceVolumeValue = new XNALabel(WindowManager);
            lblVoiceVolumeValue.Name = "lblVoiceVolumeValue";
            lblVoiceVolumeValue.FontIndex = 1;
            lblVoiceVolumeValue.Text = "0";
            lblVoiceVolumeValue.ClientRectangle = new Rectangle(
                lblScoreVolumeValue.X,
                lblVoiceVolume.Y, 0, 0);

            trbVoiceVolume = new XNATrackbar(WindowManager);
            trbVoiceVolume.Name = "trbVoiceVolume";
            trbVoiceVolume.ClientRectangle = new Rectangle(
                trbScoreVolume.X,
                lblVoiceVolume.Y - 2,
                trbScoreVolume.Width,
                trbScoreVolume.Height);
            trbVoiceVolume.BackgroundTexture = trbScoreVolume.BackgroundTexture;
            trbVoiceVolume.MinValue = 0;
            trbVoiceVolume.MaxValue = 10;
            trbVoiceVolume.ValueChanged += TrbVoiceVolume_ValueChanged;

            chkScoreShuffle = new XNAClientCheckBox(WindowManager);
            chkScoreShuffle.Name = "chkScoreShuffle";
            chkScoreShuffle.ClientRectangle = new Rectangle(
                lblScoreVolume.X,
                trbVoiceVolume.Bottom + 12, 0, 0);
            chkScoreShuffle.Text = "Shuffle Music".L10N("UI:DTAConfig:ShuffleMusic");

            var lblClientVolume = new XNALabel(WindowManager);
            lblClientVolume.Name = "lblClientVolume";
            lblClientVolume.ClientRectangle = new Rectangle(lblScoreVolume.X,
                chkScoreShuffle.Bottom + 40, 0, 0);
            lblClientVolume.Text = "Client Volume:".L10N("UI:DTAConfig:ClientVolume");

            lblClientVolumeValue = new XNALabel(WindowManager);
            lblClientVolumeValue.Name = "lblClientVolumeValue";
            lblClientVolumeValue.FontIndex = 1;
            lblClientVolumeValue.Text = "0";
            lblClientVolumeValue.ClientRectangle = new Rectangle(
                lblScoreVolumeValue.X,
                lblClientVolume.Y, 0, 0);

            trbClientVolume = new XNATrackbar(WindowManager);
            trbClientVolume.Name = "trbClientVolume";
            trbClientVolume.ClientRectangle = new Rectangle(
                trbScoreVolume.X,
                lblClientVolume.Y - 2,
                trbScoreVolume.Width,
                trbScoreVolume.Height);
            trbClientVolume.BackgroundTexture = trbScoreVolume.BackgroundTexture;
            trbClientVolume.MinValue = 0;
            trbClientVolume.MaxValue = 10;
            trbClientVolume.ValueChanged += TrbClientVolume_ValueChanged;

            chkMainMenuMusic = new XNAClientCheckBox(WindowManager);
            chkMainMenuMusic.Name = "chkMainMenuMusic";
            chkMainMenuMusic.ClientRectangle = new Rectangle(
                lblScoreVolume.X,
                trbClientVolume.Bottom + 12, 0, 0);
            chkMainMenuMusic.Text = "Main menu music".L10N("UI:DTAConfig:MainMenuMusic");
            chkMainMenuMusic.CheckedChanged += ChkMainMenuMusic_CheckedChanged;

            chkStopMusicOnMenu = new XNAClientCheckBox(WindowManager);
            chkStopMusicOnMenu.Name = "chkStopMusicOnMenu";
            chkStopMusicOnMenu.ClientRectangle = new Rectangle(
                lblScoreVolume.X, chkMainMenuMusic.Bottom + 24, 0, 0);
            chkStopMusicOnMenu.Text = "Don't play main menu music in lobbies".L10N("UI:DTAConfig:NoLobbiesMusic");

            chkSmartMusic = new XNAClientCheckBox(WindowManager);
            chkSmartMusic.Name = "chkSmartMusic";
            chkSmartMusic.ClientRectangle = new Rectangle(
                lblScoreVolume.ClientRectangle.X, chkMainMenuMusic.ClientRectangle.Bottom + 24, 0, 0);
            chkSmartMusic.Text = "Situational Music".L10N("UI:DTAConfig:SmartMusic");
            chkSmartMusic.CheckedChanged += ChkSmartMusic_CheckedChanged;

            var lblMusicType = new XNALabel(WindowManager);
            lblMusicType.Name = "lblMusicType";
            lblMusicType.ClientRectangle = new Rectangle(12, 14, 0, 0);
            lblMusicType.Text = "Music Type:".L10N("UI:DTAConfig:MusicType");

            ddMusicType = new XNAClientDropDown(WindowManager);
            ddMusicType.Name = "ddMusicType";
            ddMusicType.ClientRectangle = new Rectangle(12, 14, 0, 0);
            ddMusicType.AddItem("CNC4 (Default)".L10N("UI:DTAConfig:MusicType_1"));
            ddMusicType.AddItem("CNC Classic (Need to download)".L10N("UI:DTAConfig:MusicType_2"));
            ddMusicType.AddItem("None (Using vanilla)".L10N("UI:DTAConfig:MusicType_3"));
            ddMusicType.Items[0].Selectable = chkSmartMusic.Checked;
            ddMusicType.Items[1].Selectable = chkSmartMusic.Checked;
            ddMusicType.SelectedIndexChanged += ddMusicType_SelectedIndexChanged;

            AddChild(lblScoreVolume);
            AddChild(lblScoreVolumeValue);
            AddChild(trbScoreVolume);
            AddChild(lblSoundVolume);
            AddChild(lblSoundVolumeValue);
            AddChild(trbSoundVolume);
            AddChild(lblVoiceVolume);
            AddChild(lblVoiceVolumeValue);
            AddChild(trbVoiceVolume);

            AddChild(chkScoreShuffle);
            AddChild(chkSmartMusic);
            AddChild(lblMusicType);
            AddChild(ddMusicType);

            AddChild(lblClientVolume);
            AddChild(lblClientVolumeValue);
            AddChild(trbClientVolume);

            AddChild(chkMainMenuMusic);
            AddChild(chkStopMusicOnMenu);

            WindowManager.SoundPlayer.SetVolume(trbClientVolume.Value / 10.0f);
        }

        private void ChkSmartMusic_CheckedChanged(object sender, EventArgs e)
        {
            ddMusicType.Items[0].Selectable = chkSmartMusic.Checked;
            ddMusicType.Items[1].Selectable = chkSmartMusic.Checked;
            if (!chkSmartMusic.Checked)
                ddMusicType.SelectedIndex = 2;
        }

        private void ddMusicType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddMusicType.SelectedIndex == 1)
            {
                string strFilePath = ProgramConstants.GamePath + "tcextrab10.big";
                if (!System.IO.File.Exists(strFilePath) || Rampastring.Tools.Utilities.CalculateSHA1ForFile(strFilePath).ToUpper() != "0B0766448139D48C4A2FB531966165BFA226DB48")
                {
                    NoMusicPackMessageBox = XNAMessageBox.ShowYesNoDialog(WindowManager,
                        "CNC Music Pack Not Found".L10N("UI:DTAConfig:CNCMusicPackNotFound"),
                    string.Format("Cannot find CNC Music Pack." + Environment.NewLine +
                    "You need to download it before using this type of music." + Environment.NewLine +
                    "Do you want to download now?").
                    L10N("UI:DTAConfig:CNCMusicPackNotFound_Desc"));
                    NoMusicPackMessageBox.YesClickedAction = NoMusicPackMessageBox_YesClicked;
                    NoMusicPackMessageBox.NoClickedAction = NoMusicPackMessageBox_NoClicked;
                }
            }
        }

        private void NoMusicPackMessageBox_YesClicked(XNAMessageBox messageBox)
        {
            try
            {
                System.Diagnostics.Process.Start("http://jq.qq.com/?_wv=1027&k=5cRWqHk");
            }
            catch (Exception)
            {
                try
                {
                    System.Diagnostics.Process.Start("iexplore.exe", "http://jq.qq.com/?_wv=1027&k=5cRWqHk");
                }
                catch (Exception ex)
                {
                    Rampastring.Tools.Logger.Log("Error opening qq website, message: " + ex.Message);
                }
            }
            ddMusicType.SelectedIndex = 0;
        }

        private void NoMusicPackMessageBox_NoClicked(XNAMessageBox messageBox)
        {
            ddMusicType.SelectedIndex = 0;
        }

        private void ChkMainMenuMusic_CheckedChanged(object sender, EventArgs e)
        {
            chkStopMusicOnMenu.AllowChecking = chkMainMenuMusic.Checked;
            chkStopMusicOnMenu.Checked = chkMainMenuMusic.Checked;
        }

        private void TrbScoreVolume_ValueChanged(object sender, EventArgs e)
        {
            lblScoreVolumeValue.Text = trbScoreVolume.Value.ToString();
        }

        private void TrbSoundVolume_ValueChanged(object sender, EventArgs e)
        {
            lblSoundVolumeValue.Text = trbSoundVolume.Value.ToString();
        }

        private void TrbVoiceVolume_ValueChanged(object sender, EventArgs e)
        {
            lblVoiceVolumeValue.Text = trbVoiceVolume.Value.ToString();
        }

        private void TrbClientVolume_ValueChanged(object sender, EventArgs e)
        {
            lblClientVolumeValue.Text = trbClientVolume.Value.ToString();
            WindowManager.SoundPlayer.SetVolume(trbClientVolume.Value / 10.0f);
        }

        public override void Load()
        {
            base.Load();

            trbScoreVolume.Value = (int)(IniSettings.ScoreVolume * 10);
            trbSoundVolume.Value = (int)(IniSettings.SoundVolume * 10);
            trbVoiceVolume.Value = (int)(IniSettings.VoiceVolume * 10);

            chkScoreShuffle.Checked = IniSettings.IsScoreShuffle;

            trbClientVolume.Value = (int)(IniSettings.ClientVolume * 10);

            chkMainMenuMusic.Checked = IniSettings.PlayMainMenuMusic;
            chkStopMusicOnMenu.Checked = IniSettings.StopMusicOnMenu;
            chkSmartMusic.Checked = IniSettings.SmartMusic;
            ddMusicType.SelectedIndex = IniSettings.MusicType;
        }

        public override bool Save()
        {
            bool restartRequired = base.Save();

            IniSettings.ScoreVolume.Value = trbScoreVolume.Value / 10.0;
            IniSettings.SoundVolume.Value = trbSoundVolume.Value / 10.0;
            IniSettings.VoiceVolume.Value = trbVoiceVolume.Value / 10.0;

            IniSettings.IsScoreShuffle.Value = chkScoreShuffle.Checked;
            IniSettings.SmartMusic.Value = chkSmartMusic.Checked;
            IniSettings.MusicType.Value = ddMusicType.SelectedIndex;

            IniSettings.ClientVolume.Value = trbClientVolume.Value / 10.0;

            IniSettings.PlayMainMenuMusic.Value = chkMainMenuMusic.Checked;
            IniSettings.StopMusicOnMenu.Value = chkStopMusicOnMenu.Checked;

            return restartRequired;
        }
    }
}
