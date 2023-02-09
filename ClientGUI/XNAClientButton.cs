using Microsoft.Xna.Framework;
using Rampastring.XNAUI.XNAControls;
using Rampastring.XNAUI;
using Rampastring.Tools;
using ClientCore;
using System;

namespace ClientGUI
{
    public class XNAClientButton : XNAButton
    {
        private string _toolTipText { get; set; }

        public ToolTip _toolTip { get; set; }

        private bool ignoreBorderCheck;
        private bool isNgon;
        private bool enter;
        private byte alphaCheckVal;

        public int borderWidth;
        public int borderHeight;
        public int textureEndX;
        public int textureEndY;

        public bool AutoPos;
        public Point LocationENG;
        public Point LocationCHS;

        public bool bButtonHover = false;
        public bool bButtonOn = false;

        public XNAClientButton(WindowManager windowManager) : base(windowManager)
        {
            FontIndex = 1;
            Height = UIDesignConstants.BUTTON_HEIGHT;
            borderWidth = 0;
            borderHeight = 0;
            alphaCheckVal = 0;
            ignoreBorderCheck = true;
            isNgon = false;
            AutoPos = false;
            LocationENG = Point.Zero;
            LocationCHS = Point.Zero;
        }

        public override void Initialize()
        {
            int width = Width;

            if (IdleTexture == null)
                IdleTexture = AssetLoader.LoadTexture(width + "pxbtn.png");

            if (HoverTexture == null)
                HoverTexture = AssetLoader.LoadTexture(width + "pxbtn_c.png");

            if (HoverSoundEffect == null)
                HoverSoundEffect = new EnhancedSoundEffect("button.wav");

            if (ClickSoundEffect == null)
                ClickSoundEffect = new EnhancedSoundEffect("checkbox.wav");

            base.Initialize();

            if (Width == 0)
                Width = IdleTexture.Width;

            _toolTip = new ToolTip(WindowManager, this);
        }

        private bool IsCursorAboveTexture()
        {
            if (borderWidth > 0 && borderHeight > 0)
            {
                return GetCursorPoint().X > borderWidth &&
                    GetCursorPoint().X < textureEndX &&
                    GetCursorPoint().Y > borderHeight &&
                    GetCursorPoint().Y < textureEndY && IsCursorInNgonButton();
            }
            else if (borderWidth > 0)
            {
                return GetCursorPoint().X > borderWidth &&
                    GetCursorPoint().X < textureEndX && IsCursorInNgonButton();
            }
            else if (borderHeight > 0)
            {
                return GetCursorPoint().Y > borderHeight &&
                    GetCursorPoint().Y < textureEndY && IsCursorInNgonButton();
            }
            return IsCursorInNgonButton();
        }

        private bool IsCursorInNgonButton()
        {
            if (isNgon)
            {
                Color[] array = new Color[1];
                IdleTexture.GetData(0, new Rectangle?(new Rectangle(GetCursorPoint().X, GetCursorPoint().Y, 1, 1)), array, 0, 1);
                return array[0].A > alphaCheckVal;
            }
            return true;
        }

        public override void OnMouseEnter()
        {
            if (ignoreBorderCheck || (!enter && (enter = IsCursorAboveTexture())))
            {
                bButtonHover = true;
                base.OnMouseEnter();
            }
        }
        public override void OnLeftClick()
        {
            if (ignoreBorderCheck || IsCursorAboveTexture())
            {
                base.OnLeftClick();
            }
        }

        public override void OnMouseMove()
        {
            bool flag = IsCursorAboveTexture();
            if (ignoreBorderCheck | flag)
            {
                base.OnMouseMove();
            }
            if (ignoreBorderCheck)
            {
                return;
            }
            if (enter && !(enter = flag))
            {
                base.OnMouseLeave();
                return;
            }
            if (!enter && (enter = flag))
            {
                base.OnMouseEnter();
            }
        }

        public override void OnMouseLeave()
        {
            if (ignoreBorderCheck || enter)
            {
                enter = false;
                bButtonHover = false;
                base.OnMouseLeave();
            }
        }

        public void SetBorderWidth(int value)
        {
            borderWidth = value;
            textureEndX = Width - borderWidth;
            if (ignoreBorderCheck)
                ignoreBorderCheck = false;
        }

        public void SetBorderHeight(int value)
        {
            borderHeight = value;
            textureEndY = Height - borderHeight;
            if (ignoreBorderCheck)
                ignoreBorderCheck = false;
        }

        public void SetAlphaCheckVal(int value)
        {
            alphaCheckVal = (byte)value;
            if (isNgon)
                isNgon = false;
        }

        public override void ParseAttributeFromINI(IniFile iniFile, string key, string value)
        {
            if (key == "MatchTextureSize" && Conversions.BooleanFromString(key, true))
            {
                Width = IdleTexture.Width;
                Height = IdleTexture.Height;
                return;
            }

            if (key == "AutoPos" && Conversions.BooleanFromString(key, true))
            {
                AutoPos = true;
                return;
            }

            if (key == "Location")
            {
                string[] strPoint = value.Split(',');
                LocationENG.X = Conversions.IntFromString(strPoint[0], 0);
                LocationENG.Y = Conversions.IntFromString(strPoint[1], 0);
                //return;
            }
            if (key == "LocationCHS")
            {
                string[] strPoint = value.Split(',');
                LocationCHS.X = Conversions.IntFromString(strPoint[0], 0);
                LocationCHS.Y = Conversions.IntFromString(strPoint[1], 0);
                return;
            }

            if (key == "BorderWidth")
            {
                borderWidth = Conversions.IntFromString(value, 0);
                textureEndX = Width - borderWidth;
                ignoreBorderCheck = false;
                return;
            }

            if (key == "BorderHeight")
            {
                borderHeight = Conversions.IntFromString(value, 0);
                textureEndY = Height - borderHeight;
                ignoreBorderCheck = false;
                return;
            }

            if (key == "AlphaCheckVal")
            {
                alphaCheckVal = (byte)Conversions.IntFromString(value, 0);
                ignoreBorderCheck = false;
                isNgon = true;
                return;
            }

            if (key == "ToolTip")
            {
                SetToolTipText(value);
                return;
            }
            if (key == "ToolTip.Delay")
            {
                _toolTip.SetToolTipDelay(Conversions.FloatFromString(value, ClientConfiguration.Instance.ToolTipDelay));
                return;
            }
            if (key == "ToolTip.Offset")
            {
                string[] strOffset = value.Split(',');
                _toolTip.SetOffset(new Point(Conversions.IntFromString(strOffset[0], 0), Conversions.IntFromString(strOffset[1], 0)));
                return;
            }

            base.ParseAttributeFromINI(iniFile, key, value);
        }

        public void SetToolTipText(string text)
        {
            if (_toolTip != null)
                _toolTip.Text = text.Replace("@", Environment.NewLine);
        }

        public void MuteSound()
        {
            if (HoverSoundEffect != null)
            {
                HoverSoundEffect.Dispose();
                HoverSoundEffect = null;
            }
            if (ClickSoundEffect != null)
            {
                ClickSoundEffect.Dispose();
                ClickSoundEffect = null;
            }
        }
    }
}
