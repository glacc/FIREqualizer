using SFML.Graphics;
using System.Diagnostics;

namespace Glacc.UI
{
    class AppWindow
    {
        string m_title = string.Empty;
        public string title
        {
            get => m_title;

            set
            {
                renderWindow?.SetTitle(value);
                m_title = value;
            }
        }

        int m_width;
        public int width
        {
            get => m_width;

            set
            {
                m_width = value;
            }
        }
        int m_height;
        public int height
        {
            get => m_height;

            set
            {
                m_height = value;
            }
        }

        float m_updateTickrate = 60f;
        float timeEachUpdate;
        public float updateTickrate
        {
            get => m_updateTickrate;
            set
            {
                m_updateTickrate = value;
                timeEachUpdate = 1000f / value;
            }
        }
        public int maxUpdateEachDraw = 10;

        public RenderWindow? renderWindow = null;

        public EventHandler<EventArgs>? userInit = null;
        public EventHandler<EventArgs>? userUpdate = null;
        public EventHandler<EventArgs>? userDraw = null;
        public EventHandler<EventArgs>? afterClosing = null;

        public static RenderTarget? GetRenderTarget(AppWindow? appWindow)
        {
            if (appWindow == null)
                return null;
            if (appWindow.renderWindow == null)
                return null;

            RenderTarget? renderTarget = appWindow.renderWindow;

            return renderTarget;
        }

        void OnClose(object? sender, EventArgs e)
        {
            if (sender == null)
                return;

            renderWindow?.Close();
        }

        public void Run()
        {
            if (renderWindow != null)
                return;

            Settings.InitSettings();

            renderWindow = new RenderWindow
            (
                new SFML.Window.VideoMode
                (
                    (uint)m_width,
                    (uint)m_height
                ),
                m_title,
                SFML.Window.Styles.Titlebar | SFML.Window.Styles.Close
            );
            // renderWindow.SetFramerateLimit(60);
            renderWindow.SetVerticalSyncEnabled(true);
            renderWindow.Closed += OnClose;

            Event.ApplyEventHandlers(renderWindow);

            userInit?.Invoke(this, EventArgs.Empty);

            bool resetState = true;

            timeEachUpdate = 1000f / m_updateTickrate;
            float timeAfterLastUpdate = timeEachUpdate;
            Stopwatch stopwatch = new Stopwatch();

            while (renderWindow.IsOpen)
            {
                Event.Update(renderWindow, resetState, true);
                resetState = false;

                renderWindow.Clear(Settings.bgColor);

                stopwatch.Stop();
                double elaspedMs = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Restart();
                if (elaspedMs != double.NaN)
                    timeAfterLastUpdate += (float)elaspedMs;

                int updateCount = 0;
                while (timeAfterLastUpdate >= timeEachUpdate)
                {
                    Event.ResetPossiblyRepeatedState();
                    Event.UpdateState();

                    userUpdate?.Invoke(this, EventArgs.Empty);

                    resetState = true;

                    timeAfterLastUpdate -= timeEachUpdate;

                    updateCount++;
                    if (updateCount >= maxUpdateEachDraw)
                    {
                        timeAfterLastUpdate %= timeEachUpdate;
                        break;
                    }
                }

                userDraw?.Invoke(this, EventArgs.Empty);

                renderWindow.Display();
            }

            afterClosing?.Invoke(this, EventArgs.Empty);
        }

        public AppWindow(string title, int width, int height)
        {
            m_title = title;
            m_width = width;
            m_height = height;
        }
    }
}
