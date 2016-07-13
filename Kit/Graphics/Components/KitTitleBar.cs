using System;
using System.Windows.Media;
using System.Runtime.InteropServices;
using Kit.Graphics.Types;
using Kit.Graphics.Drawing;
using Kit.Core;
using Kit.Core.Delegates;

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
        private TopLevelComponent topParent;

        private KitText titleComponent;
        private KitButton closeButton;
        private AnimationControl fadingAnimation;

        public KitTitleBar(Color barColor, TopLevelComponent topParent, VoidDelegate onClose, string title = "", Vector2 location = default(Vector2))
            : base(location)
        {
            ComponentDepth = double.MaxValue;
            ShouldDraw = false;
            BarColor = barColor;
            Title = title;

            this.topParent = topParent;
            Anchor = KitAnchoring.TopLeft;
            Origin = KitAnchoring.TopLeft;
            Size = new Vector2(topParent.Size.X, 22);

            Opacity = 0;

            fadingAnimation = new AnimationControl(350, 1);

            topParent.Resize += () =>
            {
                Size = new Vector2(topParent.Size.X, 22);
            };

            titleComponent = new KitText(title, "Consolas", 13, new Vector2(5))
            {
                Origin = KitAnchoring.LeftCenter,
                Anchor = KitAnchoring.LeftCenter,
                TextColor = Color.FromArgb(barColor.A, 255, 255, 255),
                ComponentDepth = double.MaxValue,
                Opacity = 0
            };

            closeButton = new KitButton(@"D:\JPEG\XButton.png", @"D:\JPEG\XButtonDown.png", new Vector2(16, 16))
            {
                Origin = KitAnchoring.RightCenter,
                Anchor = KitAnchoring.RightCenter,
                Location = new Vector2(-2, 0),
                ComponentDepth = double.MaxValue,
                Opacity = 0
            };

            closeButton.Released += onClose;

            AddChild(titleComponent);
            AddChild(closeButton);
        }

        #region Interop GetCursorPos
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

        #endregion

        protected override void OnMouseInput(Vector2 clickLocation, MouseState mouseFlags)
        {
            if (closeButton.Contains(clickLocation))
            {
                Focused = false;
            }
            base.OnMouseInput(clickLocation, mouseFlags);
        }

        private Vector2 hover;
        private double hoverTime;

        protected override void OnUpdate()
        {
            lock (redrawLock)
            {
                Vector2 cursorLoc = GetCursorPosition() - topParent.WindowLocation;
                if (topParent.Contains(cursorLoc))
                {
                    if (!fadingAnimation.Animating || fadingAnimation.AnimationTag.Equals("fadeout"))
                    {
                        hoverTime = time;
                        hover = cursorLoc;
                        fadingAnimation.BeginAnimation(time, "fadein");
                    }
                    if(!cursorLoc.Equals(hover) || closeButton.IsPressed)
                    {
                        hoverTime = time;
                    }
                    if(time - hoverTime >= 3000 && !fadingAnimation.AnimationTag.Equals("fadeouthover"))
                    {
                        fadingAnimation.BeginAnimation(time, "fadeouthover");
                    }
                    if(fadingAnimation.AnimationTag.Equals("fadeouthover") && (time - hoverTime < 3000))
                    {
                        fadingAnimation.BeginAnimation(time, "fadeinhover");
                    }
                }
                else
                {
                    if (fadingAnimation.AnimationTag.Contains("fadein"))
                    {
                        fadingAnimation.BeginAnimation(time, "fadeout");
                    }
                }
                fadingAnimation.StepAnimation(time);

                if (!fadingAnimation.AnimationOver() && fadingAnimation.Animating)
                {
                    redraw = true;
                }
                hover = cursorLoc;
            }
            base.OnUpdate();
        }

        protected override bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            if (state == MouseState.Left)
            {
                if ((Contains(start) || Contains(end)) && Focused)
                {
                    return true;
                }
            }
            return base.OnMouseMove(state, start, end);
        }

        public override void PreDrawComponent(KitBrush brush)
        {
            if (fadingAnimation.AnimationOver())
            {
                Redraw = false;
            }
            if (fadingAnimation.AnimationTag.Contains("fadein"))
            {
                Opacity = fadingAnimation.GetGradient();
            }
            else if (fadingAnimation.AnimationTag.Contains("fadeout"))
            {
                Opacity = 1 - fadingAnimation.GetGradient();
            }
            titleComponent.Opacity = Opacity;
            closeButton.Opacity = Opacity;
            base.PreDrawComponent(brush);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            //REFACTOR TO ANIMATION CLASS
            if (fadingAnimation.Animating)
            {
                brush.DrawRoundedRectangle(new Box(topParent.GetLocation(), Size), true, BarColor, 5, 5);
                return;
            }
            Redraw = false;
        }
    }
}
