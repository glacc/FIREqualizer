using SFML.Graphics;

namespace Glacc.UI
{
    public enum TextAlign
    {
        TopLeft		= 0x00,
        Top			= 0x01,
        TopRight	= 0x02,
        Left		= 0x10,
        Center		= 0x11,
        Right		= 0x12,
        BottomLeft	= 0x20,
        Bottom		= 0x21,
        BottomRight	= 0x22,
    };

    class Element
    {
        public bool visable = true;

        public string customData = "";

        public virtual void Update() { }

        public virtual Drawable?[] Draw() { return Array.Empty<Drawable>(); }
    }

    namespace Elements
    {
        class Button : Element
        {
            public string label;

            public int px;
            public int py;
            public int width;
            public int height;

            public Font? font;
            public int fontSize = Settings.defaultFontSize;

            public bool enabled = true;

            bool m_isHolding = false;
            public bool isHolding
            {
                get { return m_isHolding; }
            }
            bool m_mouseHover = false;
            public bool mouseHover
            {
                get { return m_mouseHover; }
            }

            RectangleShape bgRect;
            Text txt;

            public TextAlign textAlign = TextAlign.Center;

            Drawable?[] drawables;

            public EventHandler<EventArgs>? onClick = null;

            public override void Update()
            {
                if (enabled)
                    Utils.CheckMouseHover(px, py, width, height, out m_mouseHover, this);
                else
                {
                    m_isHolding = false;
                    m_mouseHover = false;
                }

                if (m_mouseHover && Event.mousePress)
                    m_isHolding = true;

                if (m_mouseHover && m_isHolding && Event.mouseRelease && !Event.mouseDragRelease)
                    onClick?.Invoke(this, EventArgs.Empty);

                if (!Event.mouseHold)
                    m_isHolding = false;
            }

            public override Drawable?[] Draw()
            {
                bgRect.Position = new SFML.System.Vector2f(px, py);
                bgRect.Size = new SFML.System.Vector2f(width, height);
                bgRect.FillColor = (m_mouseHover || m_isHolding) ? Settings.elemBgColorDark : (enabled ? Settings.elemBgColor : Settings.elemBgColorLight);
                drawables[0] = bgRect;

                if (font != null)
                {
                    txt.Font = font;
                    txt.DisplayedString = label;
                    txt.CharacterSize = (uint)fontSize;

                    float origy = txt.GetLocalBounds().Top + txt.GetLocalBounds().Height / 2f;
                    float origx = 0f;
                    int txtPy = py + height / 2;
                    int txtPx = 0;
                    switch (textAlign)
                    {
                        case TextAlign.TopLeft:
                        case TextAlign.Left:
                        case TextAlign.BottomLeft:
                            origx = txt.GetLocalBounds().Left;
                            txtPx = px + fontSize / 2;
                            break;
                        case TextAlign.Top:
                        case TextAlign.Center:
                        case TextAlign.Bottom:
                            origx = txt.GetLocalBounds().Left + txt.GetLocalBounds().Width / 2f;
                            txtPx = px + width / 2;
                            break;
                        case TextAlign.TopRight:
                        case TextAlign.Right:
                        case TextAlign.BottomRight:
                            origx = txt.GetLocalBounds().Left + txt.GetLocalBounds().Width;
                            txtPx = px + width - fontSize / 2;
                            break;
                    }

                    txt.Origin = new SFML.System.Vector2f((int)origx, (int)origy);
                    txt.Position = new SFML.System.Vector2f(txtPx, txtPy);
                    txt.FillColor = enabled ? Settings.txtColor : Settings.txtColorLight;
                }

                return drawables;
            }

            public Button(string label, int px, int py, int width, int height)
            {
                this.label = label;
                this.px = px;
                this.py = py;
                this.width = width;
                this.height = height;

                bgRect = new RectangleShape();
                txt = new Text();

                drawables = new Drawable[2] { bgRect, txt };

                if (Settings.font != null)
                    font = Settings.font;
            }
        }

        class Label : Element
        {
            public string text;

            public int px;
            public int py;

            public Font? font;
            public int fontSize = Settings.defaultFontSize;

            public TextAlign textAlign = TextAlign.TopLeft;

            Drawable?[] drawables;

            Text txt;

            public override Drawable?[] Draw()
            {
                if (font != null)
                {
                    txt.Font = font;
                    txt.DisplayedString = text;
                    txt.CharacterSize = (uint)fontSize;
                    Utils.UpdateTextOrigins(txt, textAlign);
                    txt.Position = new SFML.System.Vector2f(px, py);
                    txt.FillColor = Color.Black;
                }

                return drawables;
            }

            public Label(string label, int px, int py, int fontSize, Font? font = null)
            {
                this.text = label;
                this.px = px;
                this.py = py;
                this.fontSize = fontSize;

                txt = new Text();

                drawables = new Drawable[1] { txt };

                if (font == null)
                {
                    if (Settings.font != null)
                        this.font = Settings.font;
                }
                else
                    this.font = font;
            }
        }

        class InputBox : Element
        {
            public string text;
            public string textWhenEmpty = "";

            public int px;
            public int py;
            public int width;
            public int height;

            public bool lostFocusAfterEnter = false;

            public Font? font;
            public int fontSize = Settings.defaultFontSize;

            bool m_mouseHover = false;
            public bool mouseHover
            {
                get { return m_mouseHover; }
            }

            public bool hasFocus = false;

            public TextAlign textAlign = TextAlign.Left;

            Drawable?[] drawables = new Drawable?[2];

            RectangleShape bgRect;
            Text txt;

            int blinkTimer = 0;

            public EventHandler<EventArgs>? onFocus = null;
            public EventHandler<EventArgs>? onTextChange = null;
            public EventHandler<EventArgs>? onEnterPressed = null;
            public EventHandler<EventArgs>? onLostFocus = null;

            public override void Update()
            {
                Utils.CheckMouseHover(px, py, width, height, out m_mouseHover, this);

                if (Event.mousePress)
                {
                    if (hasFocus && !m_mouseHover)
                    {
                        if (onLostFocus != null)
                            onLostFocus.Invoke(this, EventArgs.Empty);
                    }

                    bool hadFocusBefore = hasFocus;
                    hasFocus = m_mouseHover;

                    if (!hadFocusBefore && hasFocus)
                    {
                        if (onFocus != null)
                            onFocus.Invoke(this, EventArgs.Empty);
                    }
                }

                if (hasFocus)
                {
                    if (Event.textEntered)
                    {
                        blinkTimer = 0;

                        if (Event.textUnicode[0] == '\u0008')
                        {
                            if (text.Length > 0)
                                text = text.Remove(text.Length - 1);
                        }
                        else
                            text += Event.textUnicode.Replace("\r", string.Empty);

                        if (onTextChange != null)
                            onTextChange.Invoke(this, EventArgs.Empty);
                    }

                    if (Event.IsKeyPressed(SFML.Window.Keyboard.Scancode.Enter))
                    {
                        if (onEnterPressed != null)
                            onEnterPressed.Invoke(this, EventArgs.Empty);

                        if (lostFocusAfterEnter)
                            hasFocus = false;
                    }

                    blinkTimer++;
                    if ((blinkTimer >> 1) >= Settings.inputBoxBlinkTime)
                        blinkTimer = 0;
                }
                else
                    blinkTimer = 0;
            }

            public override Drawable?[] Draw()
            {
                bgRect.Position = new SFML.System.Vector2f(px, py);
                bgRect.Size = new SFML.System.Vector2f(width, height);
                bgRect.FillColor = (m_mouseHover || hasFocus) ? Settings.elemBgColorDark : Settings.elemBgColor;
                drawables[0] = bgRect;

                if (font != null)
                {
                    txt.Font = font;
                    if (text != "" || hasFocus)
                    {
                        txt.DisplayedString = text + ((hasFocus && blinkTimer < Settings.inputBoxBlinkTime) ? Settings.inputBoxCursor : "");
                        txt.FillColor = Settings.txtColor;
                    }
                    else
                    {
                        txt.DisplayedString = textWhenEmpty;
                        txt.FillColor = Settings.txtColorLight;
                    }
                    txt.CharacterSize = (uint)fontSize;
                    float origy = txt.GetLocalBounds().Top + txt.GetLocalBounds().Height / 2f;
                    float origx = 0f;
                    int txtPy = py + height / 2;
                    int txtPx = 0;
                    switch (textAlign)
                    {
                        case TextAlign.TopLeft:
                        case TextAlign.Left:
                        case TextAlign.BottomLeft:
                            origx = txt.GetLocalBounds().Left;
                            txtPx = px + fontSize / 2;
                            break;
                        case TextAlign.Top:
                        case TextAlign.Center:
                        case TextAlign.Bottom:
                            origx = txt.GetLocalBounds().Left + txt.GetLocalBounds().Width / 2f;
                            txtPx = px + width / 2;
                            break;
                        case TextAlign.TopRight:
                        case TextAlign.Right:
                        case TextAlign.BottomRight:
                            origx = txt.GetLocalBounds().Left + txt.GetLocalBounds().Width;
                            txtPx = px + width - fontSize / 2;
                            break;
                    }
                    txt.Origin = new SFML.System.Vector2f((int)origx, (int)origy);
                    txt.Position = new SFML.System.Vector2f(txtPx, txtPy);
                }

                return drawables;
            }

            public InputBox(int px, int py, int width, int height, string str = "")
            {
                this.px = px;
                this.py = py;
                this.width = width;
                this.height = height;

                text = str;

                bgRect = new RectangleShape();
                txt = new Text();

                drawables = new Drawable[2] { bgRect, txt };

                if (Settings.font != null)
                    font = Settings.font;
            }
        }
    }
}
