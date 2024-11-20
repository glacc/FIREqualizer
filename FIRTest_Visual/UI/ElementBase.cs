﻿using SFML.Graphics;

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
}
