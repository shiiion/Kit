using System;
using System.Windows.Media;
using System.Runtime.InteropServices;
using Kit.Graphics.Types;
using Kit.Graphics.Drawing;
using Kit.Core;

namespace Kit.Graphics.Components
{
    class KitTitleBar : KitComponent
    {
        public Color BarColor { get; set; }

        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
                Redraw = true;
            }
        }

        private double windowHoverTime;
        private double windowReleaseTime;
        private TopLevelComponent topParent;

        private const double FADE_TIME = 350;

        private KitText titleComponent;

        public KitTitleBar(Color barColor, TopLevelComponent topParent, string title = "", Vector2 location = default(Vector2))
            : base(location)
        {
            BarColor = barColor;
            Title = title;
            windowHoverTime = -1;
            ComponentDepth = double.MaxValue;
            this.topParent = topParent;
            Anchor = KitAnchoring.TopCenter;
            Origin = KitAnchoring.TopCenter;
            Size = new Vector2(topParent.Size.X, 22);

            topParent.Resize += () =>
            {
                Size = new Vector2(topParent.Size.X, 22);
            };

            titleComponent = new KitText(title, "Consolas", 13, new Vector2(5, 5))
            {
                Origin = KitAnchoring.LeftCenter,
                Anchor = KitAnchoring.LeftCenter,
                ShouldDraw = false,
                TextColor = Color.FromArgb(barColor.A, 255, 255, 255)
            };

        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Vector2(POINT point)
            {
                return new Vector2(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static Vector2 GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);

            return lpPoint;
        }

        protected override void OnUpdate()
        {
            Vector2 cursorLoc = GetCursorPosition() - topParent.WindowLocation;
            if(topParent.Contains(cursorLoc))
            {
                if (windowHoverTime == -1)
                {
                    windowHoverTime = time;
                }
                if(time - windowHoverTime < FADE_TIME)
                {
                    Redraw = true;
                }
            }
            else
            {
                if(windowHoverTime != -1)
                {
                    Redraw = true;
                    windowReleaseTime = time;
                }
                windowHoverTime = -1;
                if(time - windowReleaseTime < FADE_TIME)
                {
                    Redraw = true;
                }
                else
                {
                    windowReleaseTime = -1;
                }
            }
            base.OnUpdate();
        }

        protected override bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            if(state == MouseState.Left)
            {
                if((Contains(start) || Contains(end)) && Focused)
                {
                    return true;
                }
            }
            return base.OnMouseMove(state, start, end);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            //REFACTOR TO ANIMATION CLASS
            if(windowHoverTime != -1)
            {
                double alphaFade = (time - windowHoverTime) / FADE_TIME;

                byte newAlpha;

                if (alphaFade > 1.0)
                {
                    newAlpha = BarColor.A;
                    Redraw = false;
                }
                else
                {
                    newAlpha = (byte)(BarColor.A * alphaFade);
                }

                Color faded = Color.FromArgb(newAlpha, BarColor.R, BarColor.G, BarColor.B);

                brush.DrawRoundedRectangle(new Box(topParent.GetLocation(), Size), true, faded, 5, 5);

                titleComponent.TextColor = Color.FromArgb(newAlpha, titleComponent.TextColor.R, titleComponent.TextColor.G, titleComponent.TextColor.B);
                titleComponent._DrawComponent(brush);
                return;
            }
            if(windowReleaseTime != -1)
            {
                double alphaFade = 1 - ((time - windowReleaseTime) / FADE_TIME);

                byte newAlpha;

                if (alphaFade > 1.0)
                {
                    newAlpha = BarColor.A;
                    Redraw = false;
                }
                else
                {
                    newAlpha = (byte)(BarColor.A * alphaFade);
                }

                Color faded = Color.FromArgb(newAlpha, BarColor.R, BarColor.G, BarColor.B);

                brush.DrawRoundedRectangle(new Box(topParent.GetLocation(), Size), true, faded, 5, 5);

                titleComponent.TextColor = Color.FromArgb(newAlpha, titleComponent.TextColor.R, titleComponent.TextColor.G, titleComponent.TextColor.B);


                titleComponent._DrawComponent(brush);
                return;
            }
            redraw = false;
        }
    }
}
