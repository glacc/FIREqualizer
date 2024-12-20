﻿using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacc.UI.Elements
{
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
}
