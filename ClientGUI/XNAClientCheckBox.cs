using Rampastring.XNAUI.XNAControls;
using Rampastring.XNAUI;
using System;
using Rampastring.Tools;

namespace ClientGUI
{
    public class XNAClientCheckBox : XNACheckBox
    {
        public ToolTip ToolTip { get; set; }
        private EnhancedSoundEffect sndHoverSound = new EnhancedSoundEffect("button.wav");
        private bool bEnter = false;

        public XNAClientCheckBox(WindowManager windowManager) : base(windowManager)
        {
        }

        public override void Initialize()
        {
            CheckSoundEffect = new EnhancedSoundEffect("checkbox.wav");

            base.Initialize();

            ToolTip = new ToolTip(WindowManager, this);
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();

            if (AllowChecking && !bEnter && !Cursor.LeftDown)
            {
                sndHoverSound.Play();

                ClearTexture = ClientCore.ProgramConstants.CheckBoxClearHoverTexture;
                CheckedTexture = ClientCore.ProgramConstants.CheckBoxCheckedHoverTexture;

                bEnter = true;
            }
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();

            if (AllowChecking && bEnter)
            {
                ClearTexture = UISettings.ActiveSettings.CheckBoxClearTexture;
                CheckedTexture = UISettings.ActiveSettings.CheckBoxCheckedTexture;

                bEnter = false;
            }
        }

        public override void ParseAttributeFromINI(IniFile iniFile, string key, string value)
        {
            if (key == "ToolTip")
            {
                ToolTip.Text = value.Replace("@", Environment.NewLine);
                return;
            }

            base.ParseAttributeFromINI(iniFile, key, value);
        }
    }
}
