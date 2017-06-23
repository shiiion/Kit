using System.Resources;
using System.Windows.Media;
using System.Runtime.InteropServices;
using Kit.Graphics.Types;
using Kit.Graphics.Drawing;
using Kit.Core;
using Kit.Core.Delegates;

namespace Kit.Graphics.Components
{
    public class KitTitleBar : KitComponent
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

        public KitTitleBar(Color barColor, TopLevelComponent topParent, VoidDelegate onClose, string pressRes, string releasedRes, string title = "", Vector2 location = default(Vector2))
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

            FadeControl = new AnimationControl(350, 1);

            topParent.Resize += () =>
            {
                Size = new Vector2(topParent.Size.X, 22);
            };

            titleComponent = new KitText(title, "Consolas", 13)
            {
                Location = new Vector2(5),
                Origin = KitAnchoring.LeftCenter,
                Anchor = KitAnchoring.LeftCenter,
                TextColor = Color.FromArgb(barColor.A, 255, 255, 255),
                ComponentDepth = double.MaxValue,
                Opacity = 0
            };


            if (string.IsNullOrWhiteSpace(pressRes))
            {
                closeButton = new KitButton("X", "Consolas", 12, Colors.Black, Colors.LightGray, Colors.White, new Vector2(2, 2), 2)
                {
                    Origin = KitAnchoring.RightCenter,
                    Anchor = KitAnchoring.RightCenter,
                    Location = new Vector2(-4, 0),
                    ComponentDepth = double.MaxValue,
                    Opacity = 0
                };
            }
            else
            {
                closeButton = new KitButton(pressRes, releasedRes, new Vector2(16, 16))
                {
                    Origin = KitAnchoring.RightCenter,
                    Anchor = KitAnchoring.RightCenter,
                    Location = new Vector2(-4, 0),
                    ComponentDepth = double.MaxValue,
                    Opacity = 0
                };
            }
            closeButton.Released += onClose;

            AddChild(titleComponent);
            AddChild(closeButton);
        }

        

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
            Vector2 cursorLoc = InteropFunctions.GetCursorPosition() - topParent.WindowLocation;
            if (topParent.Contains(cursorLoc))
            {
                if (!FadeControl.Animating || FadeControl.AnimationTag.Equals("fadeout") ||
                    (FadeControl.AnimationTag.Equals("fadeouthover") && (time - hoverTime < 3000)))
                {
                    FadeControl.Inverted = false;
                    hoverTime = time;
                    hover = cursorLoc;
                    FadeControl.BeginAnimation(time, "fadein");
                }
                if (!cursorLoc.Equals(hover) || closeButton.IsPressed)
                {
                    hoverTime = time;
                }
                if (time - hoverTime >= 3000 && !FadeControl.AnimationTag.Equals("fadeouthover"))
                {
                    FadeControl.Inverted = true;
                    FadeControl.BeginAnimation(time, "fadeouthover");
                }
            }
            else
            {
                if (FadeControl.AnimationTag.Equals("fadein") || FadeControl.AnimationTag.Equals("fadeouthover"))
                {
                    FadeControl.Inverted = true;
                    FadeControl.BeginAnimation(time, "fadeout");
                }
            }
            FadeControl.StepAnimation(time);

            if (!FadeControl.AnimationOver() && FadeControl.Animating)
            {
                Redraw = true;
            }
            hover = cursorLoc;
            base.OnUpdate();
        }

        protected override bool OnMouseMove(MouseState state, Vector2 start, Vector2 end)
        {
            if (state == (MouseState.Left | MouseState.Down))
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
            if (FadeControl.AnimationOver())
            {
                Redraw = false;
            }
            Opacity = FadeControl.GetGradient();
            titleComponent.Opacity = Opacity;
            closeButton.Opacity = Opacity;
            base.PreDrawComponent(brush);
        }

        protected override void DrawComponent(KitBrush brush)
        {
            if (FadeControl.Animating)
            {
                brush.DrawRoundedRectangle(new Box(topParent.GetLocation(), Size), true, BarColor, 5, 5);
                return;
            }
            else if(Opacity >= 1)
            {
                brush.DrawRoundedRectangle(new Box(topParent.GetLocation(), Size), true, BarColor, 5, 5);
            }
            base.DrawComponent(brush);
        }
    }
}
