using SFML.Graphics;

namespace Glacc.UI
{
    class Element
    {
        public bool visable = true;

        public string customData = "";

        public virtual void Update() { }

        public virtual Drawable?[] Draw() { return Array.Empty<Drawable>(); }
    }
}
