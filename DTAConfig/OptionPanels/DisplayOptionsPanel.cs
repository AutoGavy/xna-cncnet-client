using ClientCore;
using ClientGUI;
using Localization;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace DTAConfig.OptionPanels
{
    class DisplayOptionsPanel : XNAOptionsPanel
    {
        private const int DRAG_DISTANCE_DEFAULT = 4;
        private const int ORIGINAL_RESOLUTION_WIDTH = 640;
        private const string RENDERERS_INI = "Renderers.ini";
        private const string SPSOUND_INI = "spsound.ini";
        private const string CREDITS_TXT = "creditstc.txt";

        public DisplayOptionsPanel(WindowManager windowManager, UserINISettings iniSettings)
            : base(windowManager, iniSettings)
        {
        }

        private XNAClientDropDown ddIngameResolution;
        private XNAClientDropDown ddDetailLevel;
        private XNAClientDropDown ddRenderer;
        private XNAClientCheckBox chkWindowedMode;
        private XNAClientCheckBox chkBorderlessWindowedMode;
        private XNAClientCheckBox chkBackBufferInVRAM;
        private XNAClientPreferredItemDropDown ddClientResolution;
        private XNAClientCheckBox chkBorderlessClient;
        private XNAClientDropDown ddClientTheme;

        private XNAClientDropDown ddHighDetail;
        private XNAClientDropDown ddCloudsEffect;
        private XNAClientDropDown ddAntiAliasing;
        private XNAClientDropDown ddEnhancedLaser;
        private XNAClientDropDown ddEnhancedLight;
        private XNAClientDropDown ddDisplacement;

        private XNAClientCheckBox chkAlphaLight;
        private XNAClientCheckBox chkAirflowEffect;
        private XNAClientCheckBox chkVideoMode;
		
        private XNAClientButton btnReadTutorial;
        private XNAClientButton btnTestGame;
        private XNALabel lblDetailTip;
        private XNAMessageBox SureToTextBox;

        private List<DirectDrawWrapper> renderers;

        private string defaultRenderer;
        private DirectDrawWrapper selectedRenderer = null;

#if TS
        private XNALabel lblCompatibilityFixes;
        private XNALabel lblGameCompatibilityFix;
        private XNALabel lblMapEditorCompatibilityFix;
        private XNAClientButton btnGameCompatibilityFix;
        private XNAClientButton btnMapEditorCompatibilityFix;

        private bool GameCompatFixInstalled = false;
        private bool FinalSunCompatFixInstalled = false;
        private bool GameCompatFixDeclined = false;
        //private bool FinalSunCompatFixDeclined = false;
#endif


        public override void Initialize()
        {
            base.Initialize();

            Name = "DisplayOptionsPanel";

            BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 200), 1, 1);

            var lblIngameResolution = new XNALabel(WindowManager);
            lblIngameResolution.Name = "lblIngameResolution";
            lblIngameResolution.ClientRectangle = new Rectangle(12, 14, 0, 0);
            lblIngameResolution.Text = "In-game Resolution:".L10N("UI:DTAConfig:InGameResolution");

            ddIngameResolution = new XNAClientDropDown(WindowManager);
            ddIngameResolution.Name = "ddIngameResolution";
            ddIngameResolution.ClientRectangle = new Rectangle(
                lblIngameResolution.Right + 12,
                lblIngameResolution.Y - 2, 120, 19);

            var clientConfig = ClientConfiguration.Instance;

            var resolutions = GetResolutions(clientConfig.MinimumIngameWidth,
                clientConfig.MinimumIngameHeight,
                1920, 1080);
                //clientConfig.MaximumIngameWidth, clientConfig.MaximumIngameHeight);

            resolutions.Sort();

            foreach (var res in resolutions)
                ddIngameResolution.AddItem(res.ToString());

            var lblDetailLevel = new XNALabel(WindowManager);
            lblDetailLevel.Name = "lblDetailLevel";
            lblDetailLevel.ClientRectangle = new Rectangle(lblIngameResolution.X,
                ddIngameResolution.Bottom + 16, 0, 0);
            lblDetailLevel.Text = "Detail Level:".L10N("UI:DTAConfig:DetailLevel");

            ddDetailLevel = new XNAClientDropDown(WindowManager);
            ddDetailLevel.Name = "ddDetailLevel";
            ddDetailLevel.ClientRectangle = new Rectangle(
                ddIngameResolution.X,
                lblDetailLevel.Y - 2,
                ddIngameResolution.Width,
                ddIngameResolution.Height);
            ddDetailLevel.AddItem("Low".L10N("UI:DTAConfig:DetailLevelLow"));
            ddDetailLevel.AddItem("Medium".L10N("UI:DTAConfig:DetailLevelMedium"));
            ddDetailLevel.AddItem("High".L10N("UI:DTAConfig:DetailLevelHigh"));

            var lblRenderer = new XNALabel(WindowManager);
            lblRenderer.Name = "lblRenderer";
            lblRenderer.ClientRectangle = new Rectangle(lblDetailLevel.X,
                ddDetailLevel.Bottom + 16, 0, 0);
            lblRenderer.Text = "Renderer:".L10N("UI:DTAConfig:Renderer");

            ddRenderer = new XNAClientDropDown(WindowManager);
            ddRenderer.Name = "ddRenderer";
            ddRenderer.ClientRectangle = new Rectangle(
                ddDetailLevel.X,
                lblRenderer.Y - 2,
                ddDetailLevel.Width,
                ddDetailLevel.Height);
            ddRenderer.SelectedIndexChanged += ddRenderer_Changed;

            GetRenderers();

            var localOS = ClientConfiguration.Instance.GetOperatingSystemVersion();

            foreach (var renderer in renderers)
            {
                if (renderer.IsCompatibleWithOS(localOS) && !renderer.Hidden)
                {
                    ddRenderer.AddItem(new XNADropDownItem()
                    {
                        Text = renderer.UIName,
                        Tag = renderer
                    });
                }
            }

            //ddRenderer.AddItem("Default");
            //ddRenderer.AddItem("IE-DDRAW");
            //ddRenderer.AddItem("TS-DDRAW");
            //ddRenderer.AddItem("DDWrapper");
            //ddRenderer.AddItem("DxWnd");
            //if (ClientConfiguration.Instance.GetOperatingSystemVersion() == OSVersion.WINXP)
            //    ddRenderer.AddItem("Software");

            chkWindowedMode = new XNAClientCheckBox(WindowManager);
            chkWindowedMode.Name = "chkWindowedMode";
            chkWindowedMode.ClientRectangle = new Rectangle(lblDetailLevel.X,
                ddRenderer.Bottom + 16, 0, 0);
            chkWindowedMode.Text = "Windowed Mode".L10N("UI:DTAConfig:WindowedMode");
            chkWindowedMode.CheckedChanged += ChkWindowedMode_CheckedChanged;

            chkBorderlessWindowedMode = new XNAClientCheckBox(WindowManager);
            chkBorderlessWindowedMode.Name = "chkBorderlessWindowedMode";
            chkBorderlessWindowedMode.ClientRectangle = new Rectangle(
                chkWindowedMode.X + 50,
                chkWindowedMode.Bottom + 24, 0, 0);
            chkBorderlessWindowedMode.Text = "Borderless Windowed Mode".L10N("UI:DTAConfig:BorderlessWindowedMode");
            chkBorderlessWindowedMode.AllowChecking = false;

            chkBackBufferInVRAM = new XNAClientCheckBox(WindowManager);
            chkBackBufferInVRAM.Name = "chkBackBufferInVRAM";
            chkBackBufferInVRAM.ClientRectangle = new Rectangle(
                lblDetailLevel.X,
                chkBorderlessWindowedMode.Bottom + 28, 0, 0);
            chkBackBufferInVRAM.Text = ("Back Buffer in Video Memory" + Environment.NewLine +
                "(lower performance, but is" + Environment.NewLine + "necessary on some systems)").L10N("UI:DTAConfig:BackBuffer");

            var lblClientResolution = new XNALabel(WindowManager);
            lblClientResolution.Name = "lblClientResolution";
            lblClientResolution.ClientRectangle = new Rectangle(
                285, 14, 0, 0);
            lblClientResolution.Text = "Client Resolution:".L10N("UI:DTAConfig:ClientResolution");

            ddClientResolution = new XNAClientPreferredItemDropDown(WindowManager);
            ddClientResolution.Name = "ddClientResolution";
            ddClientResolution.ClientRectangle = new Rectangle(
                lblClientResolution.Right + 12,
                lblClientResolution.Y - 2,
                Width - (lblClientResolution.Right + 24),
                ddIngameResolution.Height);
            ddClientResolution.AllowDropDown = false;
            ddClientResolution.PreferredItemLabel = "(recommended)".L10N("UI:DTAConfig:Recommended");

            var screenBounds = Screen.PrimaryScreen.Bounds;

            resolutions = GetResolutions(800, 600,
                screenBounds.Width, screenBounds.Height);

            // Add "optimal" client resolutions for windowed mode
            // if they're not supported in fullscreen mode

            var optimalResolutions = ClientConfiguration.Instance.OptimalRenderResolutions.Select(s => new ScreenResolution(s));
            foreach (var resolution in optimalResolutions)
            {
                AddResolutionIfFitting(resolution.Width, resolution.Height, resolutions);
            }

            resolutions.Sort();

            foreach (var res in resolutions)
            {
                var item = new XNADropDownItem();
                item.Text = res.ToString();
                item.Tag = res.ToString();
                ddClientResolution.AddItem(item);
            }

            // So we add the optimal resolutions to the list, sort it and then find
            // out the optimal resolution index - it's inefficient, but works

            var preferredResolutions = ClientConfiguration.Instance.PreferedRenderResolutions.Select(s => new ScreenResolution(s));
            int optimalWindowedResIndex = -1;
            foreach (var resolution in preferredResolutions)
            {
                optimalWindowedResIndex = resolutions.FindIndex(res => res == resolution);
                if (optimalWindowedResIndex > -1)
                    break;
            }

            if (optimalWindowedResIndex > -1)
                ddClientResolution.PreferredItemIndex = optimalWindowedResIndex;

            chkBorderlessClient = new XNAClientCheckBox(WindowManager);
            chkBorderlessClient.Name = "chkBorderlessClient";
            chkBorderlessClient.ClientRectangle = new Rectangle(
                lblClientResolution.X,
                lblDetailLevel.Y, 0, 0);
            chkBorderlessClient.Text = "Fullscreen Client".L10N("UI:DTAConfig:FullscreenClient");
            chkBorderlessClient.CheckedChanged += ChkBorderlessMenu_CheckedChanged;
            chkBorderlessClient.Checked = true;

            var lblClientTheme = new XNALabel(WindowManager);
            lblClientTheme.Name = "lblClientTheme";
            lblClientTheme.ClientRectangle = new Rectangle(
                lblClientResolution.X,
                lblRenderer.Y, 0, 0);
            lblClientTheme.Text = "Client Theme:".L10N("UI:DTAConfig:ClientTheme");

            ddClientTheme = new XNAClientDropDown(WindowManager);
            ddClientTheme.Name = "ddClientTheme";
            ddClientTheme.ClientRectangle = new Rectangle(
                ddClientResolution.X,
                ddRenderer.Y,
                ddClientResolution.Width,
                ddRenderer.Height);

            int themeCount = ClientConfiguration.Instance.ThemeCount;

            for (int i = 0; i < themeCount; i++)
                ddClientTheme.AddItem(ClientConfiguration.Instance.GetThemeInfoFromIndex(i)[0]);

#if TS
            lblCompatibilityFixes = new XNALabel(WindowManager);
            lblCompatibilityFixes.Name = "lblCompatibilityFixes";
            lblCompatibilityFixes.FontIndex = 1;
            lblCompatibilityFixes.Text = "Compatibility Fixes (advanced):".L10N("UI:DTAConfig:TSCompatibilityFixAdv");
            AddChild(lblCompatibilityFixes);
            lblCompatibilityFixes.CenterOnParent();
            lblCompatibilityFixes.Y = Height - 103;

            lblGameCompatibilityFix = new XNALabel(WindowManager);
            lblGameCompatibilityFix.Name = "lblGameCompatibilityFix";
            lblGameCompatibilityFix.ClientRectangle = new Rectangle(132,
                lblCompatibilityFixes.Bottom + 20, 0, 0);
            lblGameCompatibilityFix.Text = "DTA/TI/TS Compatibility Fix:".L10N("UI:DTAConfig:TSCompatibilityFix");

            btnGameCompatibilityFix = new XNAClientButton(WindowManager);
            btnGameCompatibilityFix.Name = "btnGameCompatibilityFix";
            btnGameCompatibilityFix.ClientRectangle = new Rectangle(
                lblGameCompatibilityFix.Right + 20,
                lblGameCompatibilityFix.Y - 4, UIDesignConstants.BUTTON_WIDTH_133, UIDesignConstants.BUTTON_HEIGHT);
            btnGameCompatibilityFix.FontIndex = 1;
            btnGameCompatibilityFix.Text = "Enable".L10N("UI:DTAConfig:Enable");
            btnGameCompatibilityFix.LeftClick += BtnGameCompatibilityFix_LeftClick;

            lblMapEditorCompatibilityFix = new XNALabel(WindowManager);
            lblMapEditorCompatibilityFix.Name = "lblMapEditorCompatibilityFix";
            lblMapEditorCompatibilityFix.ClientRectangle = new Rectangle(
                lblGameCompatibilityFix.X,
                lblGameCompatibilityFix.Bottom + 20, 0, 0);
            lblMapEditorCompatibilityFix.Text = "FinalSun Compatibility Fix:".L10N("UI:DTAConfig:TSFinalSunFix");

            btnMapEditorCompatibilityFix = new XNAClientButton(WindowManager);
            btnMapEditorCompatibilityFix.Name = "btnMapEditorCompatibilityFix";
            btnMapEditorCompatibilityFix.ClientRectangle = new Rectangle(
                btnGameCompatibilityFix.X,
                lblMapEditorCompatibilityFix.Y - 4,
                btnGameCompatibilityFix.Width,
                btnGameCompatibilityFix.Height);
            btnMapEditorCompatibilityFix.FontIndex = 1;
            btnMapEditorCompatibilityFix.Text = "Enable".L10N("UI:DTAConfig:TSButtonEnable");
            btnMapEditorCompatibilityFix.LeftClick += BtnMapEditorCompatibilityFix_LeftClick;

            AddChild(lblGameCompatibilityFix);
            AddChild(btnGameCompatibilityFix);
            AddChild(lblMapEditorCompatibilityFix);
            AddChild(btnMapEditorCompatibilityFix);
#endif

            var lblHighDetail = new XNALabel(WindowManager);
            lblHighDetail.Name = "lblHighDetail";
            lblHighDetail.ClientRectangle = new Rectangle(lblIngameResolution.ClientRectangle.X,
                ddIngameResolution.ClientRectangle.Bottom + 16, 0, 0);
            lblHighDetail.Text = "Shader Level:".L10N("UI:DTAConfig:ShaderLevel");
            ddHighDetail = new XNAClientDropDown(WindowManager);
            ddHighDetail.Name = "ddHighDetail";
            ddHighDetail.ClientRectangle = new Rectangle(
                ddIngameResolution.ClientRectangle.X,
                lblHighDetail.ClientRectangle.Y - 2,
                ddIngameResolution.ClientRectangle.Width,
                ddIngameResolution.ClientRectangle.Height);
                ddHighDetail.AddItem("Low".L10N("UI:DTAConfig:Low"));
                ddHighDetail.AddItem("Medium".L10N("UI:DTAConfig:Medium"));
                ddHighDetail.AddItem("High".L10N("UI:DTAConfig:High"));
                ddHighDetail.AddItem("Ultra".L10N("UI:DTAConfig:Ultra"));
            ddHighDetail.AllowDropDown = true;

            var lblCloudsEffect = new XNALabel(WindowManager);
            lblCloudsEffect.Name = "lblCloudsEffect";
            lblCloudsEffect.ClientRectangle = new Rectangle(lblIngameResolution.ClientRectangle.X,
                ddIngameResolution.ClientRectangle.Bottom + 16, 0, 0);
            lblCloudsEffect.Text = "Ambient Effect:".L10N("UI:DTAConfig:AmbientEffect");
            ddCloudsEffect = new XNAClientDropDown(WindowManager);
            ddCloudsEffect.Name = "ddCloudsEffect";
            ddCloudsEffect.ClientRectangle = new Rectangle(
                ddIngameResolution.ClientRectangle.X,
                lblCloudsEffect.ClientRectangle.Y - 2,
                ddIngameResolution.ClientRectangle.Width,
                ddIngameResolution.ClientRectangle.Height);
                ddCloudsEffect.AddItem("Disable".L10N("UI:DTAConfig:Disable"));
                ddCloudsEffect.AddItem("Enable".L10N("UI:DTAConfig:Enable"));
            ddCloudsEffect.AllowDropDown = true;

            var lblEnhancedLaser = new XNALabel(WindowManager);
            lblEnhancedLaser.Name = "lblEnhancedLaser";
            lblEnhancedLaser.ClientRectangle = new Rectangle(lblIngameResolution.ClientRectangle.X,
                ddIngameResolution.ClientRectangle.Bottom + 16, 0, 0);
            lblEnhancedLaser.Text = "Tracer Detail:".L10N("UI:DTAConfig:TracerDetail");
            ddEnhancedLaser = new XNAClientDropDown(WindowManager);
            ddEnhancedLaser.Name = "ddEnhancedLaser";
            ddEnhancedLaser.ClientRectangle = new Rectangle(
                ddIngameResolution.ClientRectangle.X,
                lblCloudsEffect.ClientRectangle.Y - 2,
                ddIngameResolution.ClientRectangle.Width,
                ddIngameResolution.ClientRectangle.Height);
                ddEnhancedLaser.AddItem("Low".L10N("UI:DTAConfig:Low"));
                ddEnhancedLaser.AddItem("High".L10N("UI:DTAConfig:High"));
            ddEnhancedLaser.AllowDropDown = true;

            var lblEnhancedLight = new XNALabel(WindowManager);
            lblEnhancedLight.Name = "lblEnhancedLight";
            lblEnhancedLight.ClientRectangle = new Rectangle(lblIngameResolution.ClientRectangle.X,
                ddIngameResolution.ClientRectangle.Bottom + 16, 0, 0);
            lblEnhancedLight.Text = "VFX Detail:".L10N("UI:DTAConfig:VFXDetail");
            ddEnhancedLight = new XNAClientDropDown(WindowManager);
            ddEnhancedLight.Name = "ddEnhancedLight";
            ddEnhancedLight.ClientRectangle = new Rectangle(
                ddIngameResolution.ClientRectangle.X,
                lblCloudsEffect.ClientRectangle.Y - 2,
                ddIngameResolution.ClientRectangle.Width,
                ddIngameResolution.ClientRectangle.Height);
                ddEnhancedLight.AddItem("Low".L10N("UI:DTAConfig:Low"));
                ddEnhancedLight.AddItem("High".L10N("UI:DTAConfig:High"));
            ddEnhancedLight.AllowDropDown = true;

            var lblDisplacement = new XNALabel(WindowManager);
            lblDisplacement.Name = "lblDisplacement";
            lblDisplacement.ClientRectangle = new Rectangle(lblIngameResolution.ClientRectangle.X,
                ddIngameResolution.ClientRectangle.Bottom + 16, 0, 0);
            lblDisplacement.Text = "Fog Detail:".L10N("UI:DTAConfig:FogDetail");
            ddDisplacement = new XNAClientDropDown(WindowManager);
            ddDisplacement.Name = "ddDisplacement";
            ddDisplacement.ClientRectangle = new Rectangle(
                ddIngameResolution.ClientRectangle.X,
                lblCloudsEffect.ClientRectangle.Y - 2,
                ddIngameResolution.ClientRectangle.Width,
                ddIngameResolution.ClientRectangle.Height);
                ddDisplacement.AddItem("Low".L10N("UI:DTAConfig:Low"));
                ddDisplacement.AddItem("High".L10N("UI:DTAConfig:High"));
            ddDisplacement.AllowDropDown = true;

            var lblAntiAliasing = new XNALabel(WindowManager);
            lblAntiAliasing.Name = "lblAntiAliasing";
            lblAntiAliasing.ClientRectangle = new Rectangle(lblIngameResolution.ClientRectangle.X,
                ddIngameResolution.ClientRectangle.Bottom + 16, 0, 0);
            lblAntiAliasing.Text = "Anti-Aliasing:".L10N("UI:DTAConfig:AntiAliasing");
            ddAntiAliasing = new XNAClientDropDown(WindowManager);
            ddAntiAliasing.Name = "ddAntiAliasing";
            ddAntiAliasing.ClientRectangle = new Rectangle(
                ddIngameResolution.ClientRectangle.X,
                lblCloudsEffect.ClientRectangle.Y - 2,
                ddIngameResolution.ClientRectangle.Width,
                ddIngameResolution.ClientRectangle.Height);
                //ddAntiAliasing.AddItem("Invalid Setting".L10N("UI:DTAConfig:Invalid"));
                ddAntiAliasing.AddItem("Disable".L10N("UI:DTAConfig:Disable"));
                //ddAntiAliasing.AddItem("Subpixel Morphological (SMAA)".L10N("UI:DTAConfig:SMAA"));
                ddAntiAliasing.AddItem("FXAA (recommended for large screen)".L10N("UI:DTAConfig:FXAA"));
            ddAntiAliasing.AllowDropDown = true;

            lblDetailTip = new XNALabel(WindowManager);
            lblDetailTip.Name = "lblDetailTip";
            lblDetailTip.ClientRectangle = new Rectangle(0, 0, 0, 0);
            lblDetailTip.Visible = false;
            lblDetailTip.Text = " ";

            chkAlphaLight = new XNAClientCheckBox(WindowManager);
            chkAlphaLight.Name = "chkAlphaLight";
            chkAlphaLight.ClientRectangle = new Rectangle(0, 0, 0, 0);
            chkAlphaLight.Text = "Muzzle Light Effect".L10N("UI:DTAConfig:MuzzleLightEffect");

            chkAirflowEffect = new XNAClientCheckBox(WindowManager);
            chkAirflowEffect.Name = "chkAirflowEffect";
            chkAirflowEffect.ClientRectangle = new Rectangle(0, 0, 0, 0);
            chkAirflowEffect.Text = "Dust Particle Effect".L10N("UI:DTAConfig:DustParticleEffect");

            chkVideoMode = new XNAClientCheckBox(WindowManager);
            chkVideoMode.Name = "chkVideoMode";
            chkVideoMode.ClientRectangle = new Rectangle(0, 0, 0, 0);
            chkVideoMode.Text = "Youtube/Twitch Record Mode".L10N("UI:DTAConfig:RecordMode");
			
            btnReadTutorial = new XNAClientButton(WindowManager);
            btnReadTutorial.Name = "btnReadTutorial";
            btnReadTutorial.ClientRectangle = new Rectangle(0, 0, 160, 23);
            btnReadTutorial.Text = "Quality Tutorial".L10N("UI:DTAConfig:QualityTutorial");
            btnReadTutorial.LeftClick += btnReadTutorial_LeftClick;

            btnTestGame = new XNAClientButton(WindowManager);
            btnTestGame.Name = "btnTestGame";
            btnTestGame.ClientRectangle = new Rectangle(0, 0, 160, 23);
            btnTestGame.Text = "Test Quality".L10N("UI:DTAConfig:TestQuality");
            btnTestGame.LeftClick += BtnTestGame_LeftClick;
            btnTestGame.MouseEnter += BtnTestGame_MouseEnter;
            btnTestGame.MouseLeave += BtnTestGame_MouseLeave;

            AddChild(chkWindowedMode);
            AddChild(chkBorderlessWindowedMode);
            AddChild(chkBackBufferInVRAM);
            AddChild(chkBorderlessClient);
            AddChild(lblClientTheme);
            AddChild(ddClientTheme);
            AddChild(lblClientResolution);
            AddChild(ddClientResolution);
            AddChild(lblRenderer);
            AddChild(ddRenderer);
            AddChild(lblDetailLevel);
            AddChild(ddDetailLevel);
            AddChild(lblIngameResolution);
            AddChild(ddIngameResolution);

            AddChild(lblHighDetail);
            AddChild(ddHighDetail);
            AddChild(lblCloudsEffect);
            AddChild(ddCloudsEffect);
            AddChild(lblEnhancedLaser);
            AddChild(ddEnhancedLaser);
            AddChild(lblEnhancedLight);
            AddChild(ddEnhancedLight);
            AddChild(lblDisplacement);
            AddChild(ddDisplacement);
            AddChild(lblAntiAliasing);
            AddChild(ddAntiAliasing);

            AddChild(chkAlphaLight);
            AddChild(chkAirflowEffect);
            AddChild(chkVideoMode);

            AddChild(btnReadTutorial);
            AddChild(btnTestGame);
            AddChild(lblDetailTip);
        }

        private void btnReadTutorial_LeftClick(object sender, EventArgs e)
        {
            /*if (ClientConfiguration.Instance.ClientLanguage == 0)
                Process.Start(ProgramConstants.GetBaseSharedPath() + "ENHANCED_QUALITY_HELP_ENG.doc");
            else
                Process.Start(ProgramConstants.GetBaseSharedPath() + "ENHANCED_QUALITY_HELP_CHS.doc");*/
            Process.Start(ProgramConstants.GetBaseSharedPath() + "ENHANCED_QUALITY_HELP_CHS.doc");
        }

        private void BtnTestGame_LeftClick(object sender, EventArgs e)
        {
           SureToTextBox = XNAMessageBox.ShowYesNoDialog(WindowManager, "Test Game Quality".L10N("UI:DTAConfig:TestGame"),
               string.Format("Are you sure to test right now?").L10N("UI:DTAConfig:TestGame_Desc"));

            SureToTextBox.YesClickedAction = SureToTextBox_YesClicked;
        }

        private void BtnTestGame_MouseEnter(object sender, EventArgs e)
        {
            lblDetailTip.Visible = true;
        }

        private void BtnTestGame_MouseLeave(object sender, EventArgs e)
        {
            lblDetailTip.Visible = false;
        }

        private void SureToTextBox_YesClicked(XNAMessageBox messageBox)
        {
            LaunchTestMap();
        }

        /// <summary>
        /// Adds a screen resolution to a list of resolutions if it fits on the screen.
        /// Checks if the resolution already exists before adding it.
        /// </summary>
        /// <param name="width">The width of the new resolution.</param>
        /// <param name="height">The height of the new resolution.</param>
        /// <param name="resolutions">A list of screen resolutions.</param>
        private void AddResolutionIfFitting(int width, int height, List<ScreenResolution> resolutions)
        {
            if (resolutions.Find(res => res.Width == width && res.Height == height) != null)
                return;

            var screenBounds = Screen.PrimaryScreen.Bounds;

            if (screenBounds.Width >= width && screenBounds.Height >= height)
            {
                resolutions.Add(new ScreenResolution(width, height));
            }
        }

        private void GetRenderers()
        {
            renderers = new List<DirectDrawWrapper>();

            var renderersIni = new IniFile(ProgramConstants.GetBaseResourcePath() + RENDERERS_INI);

            var keys = renderersIni.GetSectionKeys("Renderers");
            if (keys == null)
                throw new ClientConfigurationException("[Renderers] not found from Renderers.ini!");

            foreach (string key in keys)
            {
                string internalName = renderersIni.GetStringValue("Renderers", key, string.Empty);

                var ddWrapper = new DirectDrawWrapper(internalName, renderersIni);
                renderers.Add(ddWrapper);
            }

            OSVersion osVersion = ClientConfiguration.Instance.GetOperatingSystemVersion();

            defaultRenderer = renderersIni.GetStringValue("DefaultRenderer", osVersion.ToString(), string.Empty);

            if (defaultRenderer == null)
                throw new ClientConfigurationException("Invalid or missing default renderer for operating system: " + osVersion);


            string renderer = UserINISettings.Instance.Renderer;

            selectedRenderer = renderers.Find(r => r.InternalName == renderer);

            if (selectedRenderer == null)
                selectedRenderer = renderers.Find(r => r.InternalName == defaultRenderer);

            if (selectedRenderer == null)
                throw new ClientConfigurationException("Missing renderer: " + renderer);

            GameProcessLogic.UseQres = selectedRenderer.UseQres;
            GameProcessLogic.SingleCoreAffinity = selectedRenderer.SingleCoreAffinity;
        }

#if TS

        /// <summary>
        /// Asks the user whether they want to install the DTA/TI/TS compatibility fix.
        /// </summary>
        public void PostInit()
        {
            Load();

            if (!GameCompatFixInstalled && !GameCompatFixDeclined)
            {
                string defaultGame = ClientConfiguration.Instance.LocalGame;

                var messageBox = XNAMessageBox.ShowYesNoDialog(WindowManager, "New Compatibility Fix".L10N("UI:DTAConfig:TSFixTitle"),
                    string.Format("A performance-enhancing compatibility fix for modern Windows versions" + Environment.NewLine +
                        "has been included in this version of {0}. Enabling it requires" + Environment.NewLine +
                        "administrative priveleges. Would you like to install the compatibility fix?" + Environment.NewLine + Environment.NewLine +
                        "You'll always be able to install or uninstall the compatibility fix later from the options menu.", defaultGame
                    ).L10N("UI:DTAConfig:TSFixText"));
                messageBox.YesClickedAction = MessageBox_YesClicked;
                messageBox.NoClickedAction = MessageBox_NoClicked;
            }
        }

        private void MessageBox_NoClicked(XNAMessageBox messageBox)
        {
            // Set compatibility fix declined flag in registry
            try
            {
                RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Tiberian Sun Client");

                try
                {
                    regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
                    regKey = regKey.CreateSubKey("Tiberian Sun Client");
                    regKey.SetValue("TSCompatFixDeclined", "Yes");
                }
                catch (Exception ex)
                {
                    Logger.Log("Setting TSCompatFixDeclined failed! Returned error: " + ex.Message);
                }
            }
            catch { }
        }

        private void MessageBox_YesClicked(XNAMessageBox messageBox)
        {
            BtnGameCompatibilityFix_LeftClick(messageBox, EventArgs.Empty);
        }

        private void BtnGameCompatibilityFix_LeftClick(object sender, EventArgs e)
        {
            if (GameCompatFixInstalled)
            {
                try
                {
                    Process sdbinst = Process.Start("sdbinst.exe", "-q -n \"TS Compatibility Fix\"");

                    sdbinst.WaitForExit();

                    Logger.Log("DTA/TI/TS Compatibility Fix succesfully uninstalled.");
                    XNAMessageBox.Show(WindowManager, "Compatibility Fix Uninstalled".L10N("UI:DTAConfig:TSFixUninstallTitle"),
                        "The DTA/TI/TS Compatibility Fix has been succesfully uninstalled.".L10N("UI:DTAConfig:TSFixUninstallText"));

                    RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
                    regKey = regKey.CreateSubKey("Tiberian Sun Client");
                    regKey.SetValue("TSCompatFixInstalled", "No");

                    btnGameCompatibilityFix.Text = "Enable";

                    GameCompatFixInstalled = false;
                }
                catch (Exception ex)
                {
                    Logger.Log("Uninstalling DTA/TI/TS Compatibility Fix failed. Error message: " + ex.Message);
                    XNAMessageBox.Show(WindowManager, "Uninstalling Compatibility Fix Failed".L10N("UI:DTAConfig:TSFixUninstallFailTitle"),
                        "Uninstalling DTA/TI/TS Compatibility Fix failed. Returned error:".L10N("UI:DTAConfig:TSFixUninstallFailText") + " " + ex.Message);
                }

                return;
            }

            try
            {
                Process sdbinst = Process.Start("sdbinst.exe", "-q \"" + ProgramConstants.GamePath + "Resources/compatfix.sdb\"");

                sdbinst.WaitForExit();

                Logger.Log("DTA/TI/TS Compatibility Fix succesfully installed.");
                XNAMessageBox.Show(WindowManager, "Compatibility Fix Installed".L10N("UI:DTAConfig:TSFixInstallSuccessTitle"),
                    "The DTA/TI/TS Compatibility Fix has been succesfully installed.".L10N("UI:DTAConfig:TSFixInstallSuccessText"));

                RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
                regKey = regKey.CreateSubKey("Tiberian Sun Client");
                regKey.SetValue("TSCompatFixInstalled", "Yes");

                btnGameCompatibilityFix.Text = "Disable";

                GameCompatFixInstalled = true;
            }
            catch (Exception ex)
            {
                Logger.Log("Installing DTA/TI/TS Compatibility Fix failed. Error message: " + ex.Message);
                XNAMessageBox.Show(WindowManager, "Installing Compatibility Fix Failed".L10N("UI:DTAConfig:TSFixInstallFailTitle"),
                    "Installing DTA/TI/TS Compatibility Fix failed. Error message:".L10N("UI:DTAConfig:TSFixInstallFailText") + " " + ex.Message);
            }
        }

        private void BtnMapEditorCompatibilityFix_LeftClick(object sender, EventArgs e)
        {
            if (FinalSunCompatFixInstalled)
            {
                try
                {
                    Process sdbinst = Process.Start("sdbinst.exe", "-q -n \"Final Sun Compatibility Fix\"");

                    sdbinst.WaitForExit();

                    RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
                    regKey = regKey.CreateSubKey("Tiberian Sun Client");
                    regKey.SetValue("FSCompatFixInstalled", "No");

                    btnMapEditorCompatibilityFix.Text = "Enable".L10N("UI:DTAConfig:TSFEnable");

                    Logger.Log("FinalSun Compatibility Fix succesfully uninstalled.");
                    XNAMessageBox.Show(WindowManager, "Compatibility Fix Uninstalled".L10N("UI:DTAConfig:TSFinalSunFixUninstallTitle"),
                        "The FinalSun Compatibility Fix has been succesfully uninstalled.".L10N("UI:DTAConfig:TSFinalSunFixUninstallText"));

                    FinalSunCompatFixInstalled = false;
                }
                catch (Exception ex)
                {
                    Logger.Log("Uninstalling FinalSun Compatibility Fix failed. Error message: " + ex.Message);
                    XNAMessageBox.Show(WindowManager, "Uninstalling Compatibility Fix Failed".L10N("UI:DTAConfig:TSFinalSunFixUninstallFailedTitle"),
                        "Uninstalling FinalSun Compatibility Fix failed. Error message:".L10N("UI:DTAConfig:TSFinalSunFixUninstallFailedText") +" "+ ex.Message);
                }

                return;
            }


            try
            {
                Process sdbinst = Process.Start("sdbinst.exe", "-q \"" + ProgramConstants.GamePath + "Resources/FSCompatFix.sdb\"");

                sdbinst.WaitForExit();

                RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
                regKey = regKey.CreateSubKey("Tiberian Sun Client");
                regKey.SetValue("FSCompatFixInstalled", "Yes");

                btnMapEditorCompatibilityFix.Text = "Disable".L10N("UI:DTAConfig:TSDisable");

                Logger.Log("FinalSun Compatibility Fix succesfully installed.");
                XNAMessageBox.Show(WindowManager, "Compatibility Fix Installed".L10N("UI:DTAConfig:TSFinalSunCompatibilityFixInstalledTitle"),
                    "The FinalSun Compatibility Fix has been succesfully installed.".L10N("UI:DTAConfig:TSFinalSunCompatibilityFixInstalledText"));

                FinalSunCompatFixInstalled = true;
            }
            catch (Exception ex)
            {
                Logger.Log("Installing FinalSun Compatibility Fix failed. Error message: " + ex.Message);
                XNAMessageBox.Show(WindowManager, "Installing Compatibility Fix Failed".L10N("UI:DTAConfig:TSFinalSunCompatibilityFixInstalledFailedTitle"),
                    "Installing FinalSun Compatibility Fix failed. Error message:".L10N("UI:DTAConfig:TSFinalSunCompatibilityFixInstalledFailedText") + " " + ex.Message);
            }
        }

#endif

        private void ChkBorderlessMenu_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBorderlessClient.Checked)
            {
                ddClientResolution.AllowDropDown = false;
                string nativeRes = Screen.PrimaryScreen.Bounds.Width +
                    "x" + Screen.PrimaryScreen.Bounds.Height;

                int nativeResIndex = ddClientResolution.Items.FindIndex(i => (string)i.Tag == nativeRes);
                if (nativeResIndex > -1)
                    ddClientResolution.SelectedIndex = nativeResIndex;
            }
            else
            {
                ddClientResolution.AllowDropDown = true;

                var preferredResolutions = ClientConfiguration.Instance.PreferedRenderResolutions.Select(s => new ScreenResolution(s));
                int optimalWindowedResIndex = -1;
                foreach (var resolution in preferredResolutions)
                {
                    optimalWindowedResIndex = ddClientResolution.Items.FindIndex(i => (string)i.Tag == resolution.ToString());
                    if (optimalWindowedResIndex > -1) break;
                }

                if (optimalWindowedResIndex > -1)
                {
                    ddClientResolution.SelectedIndex = optimalWindowedResIndex;
                }
            }
        }

        private void ChkWindowedMode_CheckedChanged(object sender, EventArgs e)
        {
            if (chkWindowedMode.Checked)
            {
                chkBorderlessWindowedMode.AllowChecking = true;
                return;
            }

            chkBorderlessWindowedMode.AllowChecking = false;
            chkBorderlessWindowedMode.Checked = false;
        }

        private void ddRenderer_Changed(object sender, EventArgs e)
        {
            var renderer = (DirectDrawWrapper)ddRenderer.SelectedItem.Tag;

            chkWindowedMode.Checked = UserINISettings.Instance.WindowedMode;
            chkBorderlessWindowedMode.Checked = UserINISettings.Instance.BorderlessWindowedMode;
            chkWindowedMode.AllowChecking = true;

            chkBorderlessWindowedMode.AllowChecking = chkWindowedMode.Checked;

            if (renderer.NoReShade)
            {
                ddHighDetail.SelectedIndex = 0;
                ddCloudsEffect.SelectedIndex = 0;
                ddEnhancedLaser.SelectedIndex = 0;
                ddEnhancedLight.SelectedIndex = 0;
                ddDisplacement.SelectedIndex = 0;
                ddAntiAliasing.SelectedIndex = 0;

                ddHighDetail.AllowDropDown = false;
                ddCloudsEffect.AllowDropDown = false;
                ddEnhancedLaser.AllowDropDown = false;
                ddEnhancedLight.AllowDropDown = false;
                ddDisplacement.AllowDropDown = false;
                ddAntiAliasing.AllowDropDown = false;
            }
            else
            {
                ddHighDetail.SelectedIndex = UserINISettings.Instance.HighDetail;
                ddCloudsEffect.SelectedIndex = UserINISettings.Instance.CloudsEffect;
                ddEnhancedLaser.SelectedIndex = UserINISettings.Instance.EnhancedLaser;
                ddEnhancedLight.SelectedIndex = UserINISettings.Instance.EnhancedLight;
                ddDisplacement.SelectedIndex = UserINISettings.Instance.Displacement;
                ddAntiAliasing.SelectedIndex = UserINISettings.Instance.AntiAliasing;

                ddHighDetail.AllowDropDown = true;
                ddCloudsEffect.AllowDropDown = true;
                ddEnhancedLaser.AllowDropDown = true;
                ddEnhancedLight.AllowDropDown = true;
                ddDisplacement.AllowDropDown = true;
                ddAntiAliasing.AllowDropDown = true;
            }
        }

        /// <summary>
        /// Loads the user's preferred renderer.
        /// </summary>
        private void LoadRenderer()
        {
            int index = ddRenderer.Items.FindIndex(
                           r => ((DirectDrawWrapper)r.Tag).InternalName == selectedRenderer.InternalName);

            if (index < 0 && selectedRenderer.Hidden)
            {
                ddRenderer.AddItem(new XNADropDownItem()
                {
                    Text = selectedRenderer.UIName,
                    Tag = selectedRenderer
                });
                index = ddRenderer.Items.Count - 1;
            }

            ddRenderer.SelectedIndex = index;
        }

        public override void Load()
        {
            base.Load();

            LoadRenderer();
            //ddDetailLevel.SelectedIndex = UserINISettings.Instance.DetailLevel;
            ddDetailLevel.SelectedIndex = 2;

            string currentRes = UserINISettings.Instance.IngameScreenWidth.Value +
                "x" + UserINISettings.Instance.IngameScreenHeight.Value;

            int index = ddIngameResolution.Items.FindIndex(i => i.Text == currentRes);

            ddIngameResolution.SelectedIndex = index > -1 ? index : 0;

            // Wonder what this "Win8CompatMode" actually does..
            // Disabling it used to be TS-DDRAW only, but it was never enabled after 
            // you had tried TS-DDRAW once, so most players probably have it always
            // disabled anyway
            IniSettings.Win8CompatMode.Value = "No";

            var renderer = (DirectDrawWrapper)ddRenderer.SelectedItem.Tag;

            if (renderer.UsesCustomWindowedOption())
            {
                // For renderers that have their own windowed mode implementation
                // enabled through their own config INI file
                // (for example DxWnd and CnC-DDRAW)

                IniFile rendererSettingsIni = new IniFile(ProgramConstants.GamePath + renderer.ConfigFileName);

                chkWindowedMode.Checked = rendererSettingsIni.GetBooleanValue(renderer.WindowedModeSection,
                    renderer.WindowedModeKey, false);

                if (!string.IsNullOrEmpty(renderer.BorderlessWindowedModeKey))
                {
                    bool setting = rendererSettingsIni.GetBooleanValue(renderer.WindowedModeSection,
                        renderer.BorderlessWindowedModeKey, false);
                    chkBorderlessWindowedMode.Checked = renderer.IsBorderlessWindowedModeKeyReversed ? !setting : setting;
                }
                else
                {
                    chkBorderlessWindowedMode.Checked = UserINISettings.Instance.BorderlessWindowedMode;
                }
            }
            else
            {
                chkWindowedMode.Checked = UserINISettings.Instance.WindowedMode;
                chkBorderlessWindowedMode.Checked = UserINISettings.Instance.BorderlessWindowedMode;
            }

            string currentClientRes = IniSettings.ClientResolutionX.Value + "x" + IniSettings.ClientResolutionY.Value;

            int clientResIndex = ddClientResolution.Items.FindIndex(i => (string)i.Tag == currentClientRes);

            ddClientResolution.SelectedIndex = clientResIndex > -1 ? clientResIndex : 0;

            chkBorderlessClient.Checked = UserINISettings.Instance.BorderlessWindowedClient;

            int selectedThemeIndex = ddClientTheme.Items.FindIndex(
                ddi => ddi.Text == UserINISettings.Instance.ClientTheme);
            ddClientTheme.SelectedIndex = selectedThemeIndex > -1 ? selectedThemeIndex : 0;

            ddHighDetail.SelectedIndex = UserINISettings.Instance.HighDetail;
            ddCloudsEffect.SelectedIndex = UserINISettings.Instance.CloudsEffect;
            ddEnhancedLaser.SelectedIndex = UserINISettings.Instance.EnhancedLaser;
            ddEnhancedLight.SelectedIndex = UserINISettings.Instance.EnhancedLight;
            ddDisplacement.SelectedIndex = UserINISettings.Instance.Displacement;
            ddAntiAliasing.SelectedIndex = UserINISettings.Instance.AntiAliasing;
            //ddAntiAliasing.SelectedIndex = 0;

            chkBackBufferInVRAM.Checked = false;

            chkAlphaLight.Checked = UserINISettings.Instance.AlphaLight;

            chkAirflowEffect.Checked = UserINISettings.Instance.AirflowEffect;

            chkVideoMode.Checked = UserINISettings.Instance.VideoMode;

#if YR
            chkBackBufferInVRAM.Checked = false;
#else
            chkBackBufferInVRAM.Checked = false;

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Tiberian Sun Client");

            if (regKey == null)
                return;

            object tsCompatFixValue = regKey.GetValue("TSCompatFixInstalled", "No");
            string tsCompatFixString = (string)tsCompatFixValue;

            if (tsCompatFixString == "Yes")
            {
                GameCompatFixInstalled = true;
                btnGameCompatibilityFix.Text = "Disable".L10N("UI:DTAConfig:TSDisable");
            }

            object fsCompatFixValue = regKey.GetValue("FSCompatFixInstalled", "No");
            string fsCompatFixString = (string)fsCompatFixValue;

            if (fsCompatFixString == "Yes")
            {
                FinalSunCompatFixInstalled = true;
                btnMapEditorCompatibilityFix.Text = "Disable".L10N("UI:DTAConfig:TSDisable");
            }

            object tsCompatFixDeclinedValue = regKey.GetValue("TSCompatFixDeclined", "No");

            if (((string)tsCompatFixDeclinedValue) == "Yes")
            {
                GameCompatFixDeclined = true;
            }

            //object fsCompatFixDeclinedValue = regKey.GetValue("FSCompatFixDeclined", "No");

            //if (((string)fsCompatFixDeclinedValue) == "Yes")
            //{
            //    FinalSunCompatFixDeclined = true;
            //}
#endif
        }

        public override bool Save()
        {
            bool restartRequired = base.Save();

            //IniSettings.DetailLevel.Value = ddDetailLevel.SelectedIndex;
            IniSettings.DetailLevel.Value = 2;

            string[] resolution = ddIngameResolution.SelectedItem.Text.Split('x');

            int[] ingameRes = new int[2] { int.Parse(resolution[0]), int.Parse(resolution[1]) };

            IniSettings.IngameScreenWidth.Value = ingameRes[0];
            IniSettings.IngameScreenHeight.Value = ingameRes[1];

            // Calculate drag selection distance, scale it with resolution width
            int dragDistance = ingameRes[0] / ORIGINAL_RESOLUTION_WIDTH * DRAG_DISTANCE_DEFAULT;
            IniSettings.DragDistance.Value = dragDistance;

            DirectDrawWrapper originalRenderer = selectedRenderer;
            selectedRenderer = (DirectDrawWrapper)ddRenderer.SelectedItem.Tag;

            IniSettings.WindowedMode.Value = chkWindowedMode.Checked &&
                !selectedRenderer.UsesCustomWindowedOption();

            IniSettings.BorderlessWindowedMode.Value = chkBorderlessWindowedMode.Checked &&
                string.IsNullOrEmpty(selectedRenderer.BorderlessWindowedModeKey);

            // force fullscreen
            string nativeRes = Screen.PrimaryScreen.Bounds.Width + "x" + Screen.PrimaryScreen.Bounds.Height;

            int nativeResIndex = ddClientResolution.Items.FindIndex(i => (string)i.Tag == nativeRes);
            if (ddClientResolution.SelectedIndex == nativeResIndex)
            {
                chkBorderlessClient.Checked = true;
            }

            // force fullscreen end

            string[] clientResolution = ((string)ddClientResolution.SelectedItem.Tag).Split('x');

            int[] clientRes = new int[2] { int.Parse(clientResolution[0]), int.Parse(clientResolution[1]) };

            if (clientRes[0] != IniSettings.ClientResolutionX.Value ||
                clientRes[1] != IniSettings.ClientResolutionY.Value)
                restartRequired = true;

            IniSettings.ClientResolutionX.Value = clientRes[0];
            IniSettings.ClientResolutionY.Value = clientRes[1];

            if (IniSettings.BorderlessWindowedClient.Value != chkBorderlessClient.Checked)
                restartRequired = true;

            IniSettings.BorderlessWindowedClient.Value = chkBorderlessClient.Checked;

            if (IniSettings.ClientTheme != ddClientTheme.SelectedItem.Text)
                restartRequired = true;

            IniSettings.ClientTheme.Value = ddClientTheme.SelectedItem.Text;

            IniSettings.NoReShade.Value = selectedRenderer.NoReShade;
            IniSettings.HighDetail.Value = ddHighDetail.SelectedIndex;
            IniSettings.CloudsEffect.Value = ddCloudsEffect.SelectedIndex;
            IniSettings.EnhancedLaser.Value = ddEnhancedLaser.SelectedIndex;
            IniSettings.EnhancedLight.Value = ddEnhancedLight.SelectedIndex;
            IniSettings.Displacement.Value = ddDisplacement.SelectedIndex;
            IniSettings.AntiAliasing.Value = ddAntiAliasing.SelectedIndex;
            //IniSettings.AntiAliasing.Value = 0;

            IniSettings.AlphaLight.Value = chkAlphaLight.Checked;

            IniSettings.AirflowEffect.Value = chkAirflowEffect.Checked;

            IniSettings.VideoMode.Value = chkVideoMode.Checked;

#if YR
            IniSettings.BackBufferInVRAM.Value = false;
#else
            IniSettings.BackBufferInVRAM.Value = false;
#endif

            if (selectedRenderer != originalRenderer || 
                !File.Exists(ProgramConstants.GamePath + selectedRenderer.ConfigFileName))
            {
                foreach (var renderer in renderers)
                {
                    if (renderer != selectedRenderer)
                        renderer.Clean();
                }
            }
            
            selectedRenderer.Apply();

            GameProcessLogic.UseQres = selectedRenderer.UseQres;
            GameProcessLogic.SingleCoreAffinity = selectedRenderer.SingleCoreAffinity;

            if (selectedRenderer.UsesCustomWindowedOption())
            {
                IniFile rendererSettingsIni = new IniFile(
                    ProgramConstants.GamePath + selectedRenderer.ConfigFileName);

                rendererSettingsIni.SetBooleanValue(selectedRenderer.WindowedModeSection,
                    selectedRenderer.WindowedModeKey, chkWindowedMode.Checked);

                if (!string.IsNullOrEmpty(selectedRenderer.BorderlessWindowedModeKey))
                {
                    bool borderlessModeIniValue = chkBorderlessWindowedMode.Checked;
                    if (selectedRenderer.IsBorderlessWindowedModeKeyReversed)
                        borderlessModeIniValue = !borderlessModeIniValue;

                    rendererSettingsIni.SetBooleanValue(selectedRenderer.WindowedModeSection,
                        selectedRenderer.BorderlessWindowedModeKey, borderlessModeIniValue);
                }
                
                rendererSettingsIni.WriteIniFile();
            }

            IniSettings.Renderer.Value = selectedRenderer.InternalName;

#if TS
            File.Delete(ProgramConstants.GamePath + "Language.dll");

            if (ingameRes[0] >= 1024 && ingameRes[1] >= 720)
                File.Copy(ProgramConstants.GamePath + "Resources/language_1024x720.dll", ProgramConstants.GamePath + "Language.dll");
            else if (ingameRes[0] >= 800 && ingameRes[1] >= 600)
                File.Copy(ProgramConstants.GamePath + "Resources/language_800x600.dll", ProgramConstants.GamePath + "Language.dll");
            else
                File.Copy(ProgramConstants.GamePath + "Resources/language_640x480.dll", ProgramConstants.GamePath + "Language.dll");
#endif

            ExtraSave();

            return restartRequired;
        }

        private void ExtraSave()
        {
            // 10.big
            string strBigPath = ProgramConstants.GamePath + "tcextrab10.big";
            if (File.Exists(strBigPath))
            {
                if (Utilities.CalculateSHA1ForFile(strBigPath).ToUpper() != "0B0766448139D48C4A2FB531966165BFA226DB48")
                {
                    File.Delete(strBigPath);
                }
            }

            // Airflow Effect
            strBigPath = ProgramConstants.GamePath + "tcextrab11.big";
            if (!chkAirflowEffect.Checked)
            {
                File.Copy(ProgramConstants.GetBaseSharedPath() + "airflow.big", strBigPath, true);
            }
            else if (File.Exists(strBigPath))
            {
                File.Delete(strBigPath);
            } 

            // Alpha Light
            strBigPath = ProgramConstants.GamePath + "tcextrab12.big";
            if (!chkAlphaLight.Checked)
            {
                File.Copy(ProgramConstants.GetBaseSharedPath() + "alphalight.big", strBigPath, true);
            }
            else if (File.Exists(strBigPath))
            {
                File.Delete(strBigPath);
            }

            // Airflow Effect
            strBigPath = ProgramConstants.GamePath + "tcextrab13.big";
            string strMusicPath = ProgramConstants.GamePath + "music.big";
            if (chkVideoMode.Checked)
            {
                // move out big
                if (File.Exists(strBigPath))
                    File.Move(strBigPath, strMusicPath);
            }
            else if (!File.Exists(strBigPath) && File.Exists(strMusicPath))
            {
                // move in big
                File.Move(strMusicPath, strBigPath);
            }
        }

        private List<ScreenResolution> GetResolutions(int minWidth, int minHeight, int maxWidth, int maxHeight)
        {
            var screenResolutions = new List<ScreenResolution>();

            foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (dm.Width < minWidth || dm.Height < minHeight || dm.Width > maxWidth || dm.Height > maxHeight)
                    continue;

                var resolution = new ScreenResolution(dm.Width, dm.Height);

                // SupportedDisplayModes can include the same resolution multiple times
                // because it takes the refresh rate into consideration.
                // Which means that we have to check if the resolution is already listed
                if (screenResolutions.Find(res => res.Equals(resolution)) != null)
                    continue;

                screenResolutions.Add(resolution);
            }

            return screenResolutions;
        }

        private void LaunchTestMap()
        {
            // Save Start

            base.Save();

            //IniSettings.DetailLevel.Value = ddDetailLevel.SelectedIndex;
            IniSettings.DetailLevel.Value = 2;

            string[] resolution = ddIngameResolution.SelectedItem.Text.Split('x');

            int[] ingameRes = new int[2] { int.Parse(resolution[0]), int.Parse(resolution[1]) };

            IniSettings.IngameScreenWidth.Value = ingameRes[0];
            IniSettings.IngameScreenHeight.Value = ingameRes[1];

            // Calculate drag selection distance, scale it with resolution width
            int dragDistance = ingameRes[0] / ORIGINAL_RESOLUTION_WIDTH * DRAG_DISTANCE_DEFAULT;
            IniSettings.DragDistance.Value = dragDistance;

            DirectDrawWrapper originalRenderer = selectedRenderer;
            selectedRenderer = (DirectDrawWrapper)ddRenderer.SelectedItem.Tag;

            IniSettings.WindowedMode.Value = chkWindowedMode.Checked &&
                !selectedRenderer.UsesCustomWindowedOption();

            IniSettings.BorderlessWindowedMode.Value = chkBorderlessWindowedMode.Checked &&
                string.IsNullOrEmpty(selectedRenderer.BorderlessWindowedModeKey);

            string[] clientResolution = ((string)ddClientResolution.SelectedItem.Tag).Split('x');

            int[] clientRes = new int[2] { int.Parse(clientResolution[0]), int.Parse(clientResolution[1]) };

            IniSettings.ClientResolutionX.Value = clientRes[0];
            IniSettings.ClientResolutionY.Value = clientRes[1];

            IniSettings.BorderlessWindowedClient.Value = chkBorderlessClient.Checked;

            IniSettings.ClientTheme.Value = ddClientTheme.SelectedItem.Text;

            IniSettings.NoReShade.Value = selectedRenderer.NoReShade;
            IniSettings.HighDetail.Value = ddHighDetail.SelectedIndex;
            IniSettings.CloudsEffect.Value = ddCloudsEffect.SelectedIndex;
            IniSettings.EnhancedLaser.Value = ddEnhancedLaser.SelectedIndex;
            IniSettings.EnhancedLight.Value = ddEnhancedLight.SelectedIndex;
            IniSettings.Displacement.Value = ddDisplacement.SelectedIndex;
            IniSettings.AntiAliasing.Value = ddAntiAliasing.SelectedIndex;

            IniSettings.AlphaLight.Value = chkAlphaLight.Checked;

            IniSettings.AirflowEffect.Value = chkAirflowEffect.Checked;

            IniSettings.VideoMode.Value = chkVideoMode.Checked;

#if YR
            IniSettings.BackBufferInVRAM.Value = false;
#else
            IniSettings.BackBufferInVRAM.Value = false;
#endif

            if (selectedRenderer != originalRenderer ||
                !File.Exists(ProgramConstants.GamePath + selectedRenderer.ConfigFileName))
            {
                foreach (var renderer in renderers)
                {
                    if (renderer != selectedRenderer)
                        renderer.Clean();
                }
            }

            selectedRenderer.Apply();

            GameProcessLogic.UseQres = selectedRenderer.UseQres;
            GameProcessLogic.SingleCoreAffinity = selectedRenderer.SingleCoreAffinity;

            if (selectedRenderer.UsesCustomWindowedOption())
            {
                IniFile rendererSettingsIni = new IniFile(
                    ProgramConstants.GamePath + selectedRenderer.ConfigFileName);

                rendererSettingsIni.SetBooleanValue(selectedRenderer.WindowedModeSection,
                    selectedRenderer.WindowedModeKey, chkWindowedMode.Checked);

                if (!string.IsNullOrEmpty(selectedRenderer.BorderlessWindowedModeKey))
                {
                    bool borderlessModeIniValue = chkBorderlessWindowedMode.Checked;
                    if (selectedRenderer.IsBorderlessWindowedModeKeyReversed)
                        borderlessModeIniValue = !borderlessModeIniValue;

                    rendererSettingsIni.SetBooleanValue(selectedRenderer.WindowedModeSection,
                        selectedRenderer.BorderlessWindowedModeKey, borderlessModeIniValue);
                }

                rendererSettingsIni.WriteIniFile();
            }

            IniSettings.Renderer.Value = selectedRenderer.InternalName;

#if TS
            File.Delete(ProgramConstants.GamePath + "Language.dll");

            if (ingameRes[0] >= 1024 && ingameRes[1] >= 720)
                File.Copy(ProgramConstants.GamePath + "Resources/language_1024x720.dll", ProgramConstants.GamePath + "Language.dll");
            else if (ingameRes[0] >= 800 && ingameRes[1] >= 600)
                File.Copy(ProgramConstants.GamePath + "Resources/language_800x600.dll", ProgramConstants.GamePath + "Language.dll");
            else
                File.Copy(ProgramConstants.GamePath + "Resources/language_640x480.dll", ProgramConstants.GamePath + "Language.dll");
#endif

            ExtraSave();

            // End Save

            StreamWriter swriter = new StreamWriter(ProgramConstants.GamePath + "spawn.ini");
            swriter.WriteLine("; Generated by DTA Client");
            swriter.WriteLine("[Settings]");
            swriter.WriteLine("Scenario=" + "TESTQ.MAP");

            if (UserINISettings.Instance.GameSpeed != 2)
                UserINISettings.Instance.GameSpeed.Value = 2;

            if (UserINISettings.Instance.Difficulty < 0 || UserINISettings.Instance.Difficulty > 2)
                UserINISettings.Instance.Difficulty.Value = 0;

            UserINISettings.Instance.SaveSettings();

            swriter.WriteLine("GameSpeed=" + UserINISettings.Instance.GameSpeed);
            swriter.WriteLine("Firestorm=False");
            swriter.WriteLine("CustomLoadScreen=Resources/l600s02.pcx");
            swriter.WriteLine("IsSinglePlayer=Yes");
            swriter.WriteLine("SidebarHack=" + ClientConfiguration.Instance.SidebarHack);
            swriter.WriteLine("Side=0");
            swriter.WriteLine("BuildOffAlly=True");
            swriter.WriteLine("DifficultyModeHuman=0");
            swriter.WriteLine("DifficultyModeComputer=2");
            swriter.WriteLine();
            swriter.WriteLine();
            swriter.WriteLine();
            swriter.Close();

            ProgramConstants.SetupPreset();
            IniFile CampaignIni = new IniFile(ProgramConstants.GamePath + "GameShaders/CampaignINI/testq.ini");
            StreamWriter shaderIniWriter = new StreamWriter(ProgramConstants.GamePath + "GameShaders/TCMainShader.ini");
            if (!UserINISettings.Instance.NoReShade)
            {
                string strTechniques = "UI_Before,Colourfulness";
                string strExtraLines = String.Empty;

                if (UserINISettings.Instance.EnhancedLaser > 0)
                {
                    strTechniques += ",BlitLaser";
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
                        if (UserINISettings.Instance.EnhancedLight > 0)
                        {
                            strTechniques += ",AnimMask";
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
                        if (UserINISettings.Instance.EnhancedLight > 0)
                        {
                            strTechniques += ",AnimMask";
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
                    if (UserINISettings.Instance.EnhancedLight > 0)
                    {
                        strTechniques += ",AnimMask";
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
                    if (UserINISettings.Instance.EnhancedLight > 0)
                    {
                        strTechniques += ",AnimMask";
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
                    if (UserINISettings.Instance.EnhancedLight > 0)
                    {
                        strTechniques += ",AnimMask";
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

                strTechniques += ",Tint";
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

                strTechniques += ",UI_After";
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
            IniFile musicConfigIni = new IniFile(ProgramConstants.GamePath + "INI/MusicConfigTC.ini");
            List<string> sections = musicConfigIni.GetSections();
            foreach (string sectionName in sections)
            {
                if (musicConfigIni.GetStringValue(sectionName, "Normal", null) == "yes")
                {
                    musicConfigIni.SetStringValue(sectionName, "Normal", "no");
                }

                if (!String.IsNullOrEmpty(musicConfigIni.GetStringValue(sectionName, "Side", null)))
                {
                    musicConfigIni.SetStringValue(sectionName, "Side", "none");
                }
            }
            musicConfigIni.WriteIniFile(ProgramConstants.GamePath + SPSOUND_INI);

            File.Delete(ProgramConstants.GamePath + CREDITS_TXT);

            GameProcessLogic.GameProcessExited += GameProcessExited_Callback;
            GameProcessLogic.StartGameProcess(CampaignIni.GetBooleanValue("BaseInfo", "ControlSpeed", true), true);
        }

        private void GameProcessExited_Callback()
        {
            WindowManager.AddCallback(new Action(GameProcessExited), null);
        }

        protected virtual void GameProcessExited()
        {
            GameProcessLogic.GameProcessExited -= GameProcessExited_Callback;
            if (Directory.Exists(ProgramConstants.GamePath + "debug"))
            {
                List<string> files = Directory.GetFiles(ProgramConstants.GamePath + "debug", "debug.*.log", SearchOption.TopDirectoryOnly).ToList();
                files.Sort();
                foreach (string logFile in files)
                    File.Delete(logFile);
                File.Delete(ProgramConstants.GamePath + ClientConfiguration.Instance.StatisticsLogFileName);
            }
            string filePath = ProgramConstants.GamePath + "Saved Games/TESTQ.SAV";
            if (Directory.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// A single screen resolution.
        /// </summary>
        sealed class ScreenResolution : IComparable<ScreenResolution>
        {
            public ScreenResolution(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public ScreenResolution(string resolution)
            {
                var resolutionAxes = resolution.Split('x');
                if (resolutionAxes.Length != 2) throw new InvalidDataException(string.Format("Invalid resolution {0}".L10N("UI:DTAConfig:InvalidResolution"), resolution));
                Width = int.Parse(resolutionAxes[0], CultureInfo.InvariantCulture);
                Height = int.Parse(resolutionAxes[1], CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// The width of the resolution in pixels.
            /// </summary>
            public int Width { get; set; }

            /// <summary>
            /// The height of the resolution in pixels.
            /// </summary>
            public int Height { get; set; }

            public override string ToString()
            {
                return Width + "x" + Height;
            }

            public int CompareTo(ScreenResolution res2)
            {
                if (this.Width < res2.Width)
                    return -1;
                else if (this.Width > res2.Width)
                    return 1;
                else // equal
                {
                    if (this.Height < res2.Height)
                        return -1;
                    else if (this.Height > res2.Height)
                        return 1;
                    else return 0;
                }
            }

            public override bool Equals(object obj)
            {
                var resolution = obj as ScreenResolution;

                if (resolution == null)
                    return false;

                return CompareTo(resolution) == 0;
            }

            public override int GetHashCode()
            {
                return new { Width, Height }.GetHashCode();
            }

            public static bool operator ==(ScreenResolution res1, ScreenResolution res2)
            {
                if (res1 is null || res2 is null)
                    return Object.ReferenceEquals(res1, res2);
                else
                    return res1.Equals(res2);
            }

            public static bool operator !=(ScreenResolution res1, ScreenResolution res2)
            {
                return !(res1 == res2);
            }
        }
    }
}
