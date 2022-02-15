using Rampastring.XNAUI.XNAControls;
using Rampastring.XNAUI;
using System;

namespace ClientGUI
{
    public class CMedal : XNACheckBox
    {
        public string lockedText;
        public string unlockedText;

        private ToolTip toolTip;

        public CMedal(WindowManager windowManager) : base(windowManager)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            AllowChecking = false;
            Enabled = false;
            lockedText = String.Empty;
            unlockedText = String.Empty;

            toolTip = new ToolTip(WindowManager, this);
        }

        public void SetDisplayText(string text)
        {
            toolTip.Text = text.Replace("@", Environment.NewLine);
        }
    }
}
