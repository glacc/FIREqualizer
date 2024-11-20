using SFML.Graphics;

namespace Glacc.UI.Elements
{
    class Viewport : Element
    {
        public int px;
        public int py;

        int m_width;
        int m_height;
        int m_width_buf;
        int m_height_buf;
        public int width
        {
            get => m_width;
            set => m_width_buf = value;
        }
        public int height
        {
            get => m_height;
            set => m_height_buf = value;
        }

        public List<Element?> elements = new List<Element?>();

        public Color bgColor = Settings.bgColor;

        RenderTexture texture;
        Sprite sprite;

        Drawable[] drawables;

        protected virtual void UpdateSizeCustom(bool diff) { }

        public void UpdateSize()
        {
            bool isDiff = (
                m_width != m_width_buf ||
                m_height != m_height_buf
            );

            if (isDiff)
            {
                m_width = m_width_buf;
                m_height = m_height_buf;

                texture = new RenderTexture((uint)m_width, (uint)m_height);

                sprite.Texture = texture.Texture;
                sprite.TextureRect = new IntRect(0, 0, m_width, m_height);
            }

            UpdateSizeCustom(isDiff);
        }

        protected virtual void UpdateCustom() { }

        protected virtual Drawable?[] DrawCustom()
            => Array.Empty<Drawable>();

        public override void Update()
        {
            Event.PushState(px, py);

            Utils.CheckMouseHover(0, 0, width, height, out Event.mouseAvailable, this);

            Event.currentViewport = this;
            if (Event.mouseAvailable)
                Event.focusingViewport = this;

            Utils.UpdateElements(elements);

            UpdateCustom();

            Event.PopState();

            sprite.Position = new SFML.System.Vector2f(px, py);
        }

        public override Drawable?[] Draw()
        {
            texture.Clear(bgColor);

            Utils.DrawElements(elements, texture);

            Utils.DrawArray(DrawCustom(), texture);

            texture.Display();

            return drawables;
        }

        public Viewport(int px, int py, int width, int height)
        {
            this.px = px;
            this.py = py;

            m_width = width;
            m_height = height;

            texture = new RenderTexture((uint)m_width, (uint)m_height);
            sprite = new Sprite(texture.Texture);

            drawables = new Drawable[] { sprite };
        }
    }
}
