﻿using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glacc.UI.Components
{
    enum Orientation
    {
        Horizontal,
        Vertical,
    }

    class ScrollBar : Element
    {
        public int px;
        public int py;
        public int width;
        public int length;

        public Orientation orientation;

        float scrollPercentOld;
        public float scrollPercent;
        public float scrollSpeedPercent = 0.01f;
        public float scrollBgSpeedMultiplier = 30f;
        public float scrollerSizeScale = 0.3f;
        public int scrollerSizePixels
        {
            set => scrollerSizeScale = value / (float)(length - (width * 2));
        }
        int scrollerZone;
        int scrollerLength;

        bool m_mouseHover;
        public bool mouseHover
        {
            get => m_mouseHover;
        }
        bool mouseHoverUpLeft;
        bool mouseHoverDnRight;
        bool mouseHoverScroller;
        bool mouseHoverOnBg;

        bool mousePressUpLeft;
        bool mousePressDnRight;
        bool mouseDragScroller;
        public bool isDragging
        {
            get => mouseDragScroller;
        }

        bool scrollEnds;

        RectangleShape btnUpLeft;
        RectangleShape btnDnRight;
        RectangleShape bgRect;
        RectangleShape scroller;

        Drawable[] drawables;

        public EventHandler<EventArgs>? onPress = null;
        public EventHandler<EventArgs>? onMove = null;
        public EventHandler<EventArgs>? onScrollEnds = null;

        void UpdateDrawablePosition()
        {
            scrollerZone = length - (width * 2);
            scrollerLength = (int)(scrollerZone * scrollerSizeScale);

            bgRect.Position = new Vector2f(px, py);
            btnUpLeft.Position = new Vector2f(px, py);
            btnUpLeft.Size = btnDnRight.Size = new Vector2f(width, width);

            if (orientation == Orientation.Horizontal)
            {
                bgRect.Size = new Vector2f(length, width);
                btnDnRight.Position = new Vector2f(px + length - width, py);

                scroller.Size = new Vector2f(scrollerLength, width);
            }
            else
            {
                bgRect.Size = new Vector2f(width, length);
                btnDnRight.Position = new Vector2f(px, py + length - width);

                scroller.Size = new Vector2f(width, scrollerLength);
            }
        }

        void UpdateScrollAndBg()
        {
            int scrollerMoveRange = scrollerZone - scrollerLength;
            int scrollerPos = width + (int)(scrollerMoveRange * scrollPercent);

            if (orientation == Orientation.Horizontal)
                Utils.CheckMouseHover(px + scrollerPos, py, scrollerLength, width, out mouseHoverScroller, this);
            else
                Utils.CheckMouseHover(px, py + scrollerPos, width, scrollerLength, out mouseHoverScroller, this);

            mouseHoverOnBg = m_mouseHover && !(mouseHoverUpLeft || mouseHoverDnRight || mouseHoverScroller || mouseDragScroller);

            if (Event.mousePress)
            {
                if (mouseHoverScroller)
                    mouseDragScroller = true;

                if (mouseHoverUpLeft)
                    mousePressUpLeft = true;
                if (mouseHoverDnRight)
                    mousePressDnRight = true;

                if (mouseHoverOnBg)
                {
                    int scrollPosCentered = scrollerPos + (scrollerLength / 2);
                    float scrollSpeedMultiplied = scrollSpeedPercent * scrollBgSpeedMultiplier;
                    if (orientation == Orientation.Horizontal)
                    {
                        if (Event.mouseX > px + scrollPosCentered)
                            scrollPercent += scrollSpeedMultiplied;
                        else
                            scrollPercent -= scrollSpeedMultiplied;
                    }
                    else
                    {
                        if (Event.mouseY > py + scrollPosCentered)
                            scrollPercent += scrollSpeedMultiplied;
                        else
                            scrollPercent -= scrollSpeedMultiplied;
                    }

                    scrollEnds = true;
                }

                if (mouseHoverScroller || mouseHoverUpLeft || mouseHoverDnRight || mouseHoverOnBg)
                {
                    if (onPress != null)
                        onPress.Invoke(this, EventArgs.Empty);
                }

                if (mouseHoverUpLeft || mouseHoverDnRight || mouseHoverOnBg)
                {
                    if (onScrollEnds != null)
                        onScrollEnds.Invoke(this, EventArgs.Empty);
                }
            }
            if (!Event.mouseHold)
            {
                if (mouseDragScroller || mousePressUpLeft || mousePressDnRight)
                {
                    scrollEnds = true;
                }

                mousePressUpLeft = false;
                mousePressDnRight = false;
                mouseDragScroller = false;
            }

            float scrollVel = 0f;
            if (mouseDragScroller)
            { 
                if (orientation == Orientation.Horizontal)
                    scrollVel = Event.mouseXvel;
                else
                    scrollVel = Event.mouseYvel;

                scrollPercent += scrollVel / scrollerMoveRange;
            }

            if (mousePressUpLeft)
                scrollPercent -= scrollSpeedPercent;
            if (mousePressDnRight)
                scrollPercent += scrollSpeedPercent;

            scrollPercent = MathF.Max(0f, MathF.Min(scrollPercent, 1f));

            scrollerPos = width + (int)(scrollerMoveRange * scrollPercent);
            if (orientation == Orientation.Horizontal)
                scroller.Position = new Vector2f(px + scrollerPos, py);
            else
                scroller.Position = new Vector2f(px, py + scrollerPos);
        }

        public override void Update()
        {
            UpdateDrawablePosition();

            mouseHoverUpLeft = false;
            mouseHoverDnRight = false;
            mouseHoverScroller = false;

            Utils.CheckMouseHover(px, py, width, width, out mouseHoverUpLeft, this);

            if (orientation == Orientation.Horizontal)
            {
                Utils.CheckMouseHover(px, py, length, width, out m_mouseHover, this);
                Utils.CheckMouseHover(px + length - width, py, width, width, out mouseHoverDnRight, this);
            }
            else
            {
                Utils.CheckMouseHover(px, py, width, length, out m_mouseHover, this);
                Utils.CheckMouseHover(px, py + length - width, width, width, out mouseHoverDnRight, this);
            }

            UpdateScrollAndBg();

            if (scrollPercent != scrollPercentOld)
            {
                if (onMove != null)
                    onMove.Invoke(this, EventArgs.Empty);
            }
            if (scrollEnds)
            {
                if (onScrollEnds != null)
                    onScrollEnds.Invoke(this, EventArgs.Empty);
            }
            scrollEnds = false;

            scrollPercentOld = scrollPercent;
        }

        void UpdateDrawableColor()
        {
            bgRect.FillColor = mouseHoverOnBg ? Settings.elemBgColorDark : Settings.elemBgColor;

            btnUpLeft.FillColor = mouseHoverUpLeft || mousePressUpLeft ? Settings.elemBgColorDark : Settings.elemBgColor;
            btnDnRight.FillColor = mouseHoverDnRight || mousePressDnRight ? Settings.elemBgColorDark : Settings.elemBgColor;

            scroller.FillColor = mouseHoverScroller || mouseDragScroller ? Settings.elemBgColorDark : Settings.elemBgColor;
        }

        public override Drawable?[] Draw()
        {
            UpdateDrawableColor();

            return drawables;
        }

        public ScrollBar(int px, int py, int width, int length, Orientation orientation = Orientation.Vertical)
        {
            this.px = px;
            this.py = py;
            this.width = width;
            this.length = length;

            this.orientation = orientation;

            btnUpLeft = new RectangleShape();
            btnDnRight = new RectangleShape();
            bgRect = new RectangleShape();
            scroller = new RectangleShape();

            drawables = new Drawable[]
            {
                bgRect,
                btnUpLeft,
                btnDnRight,
                scroller
            };

            //UpdateDrawablePosition();
        }
    }
}
